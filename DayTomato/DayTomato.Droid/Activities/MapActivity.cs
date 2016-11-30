
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Com.Google.Maps.Android.Clustering;
using DayTomato.Models;
using Newtonsoft.Json;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Android.Content.Res;
using Segment;
using Segment.Model;

namespace DayTomato.Droid
{
	[Activity(Label = "MapActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class MapActivity : AppCompatActivity, IOnMapReadyCallback, GoogleMap.IOnCameraChangeListener,
						ClusterManager.IOnClusterItemClickListener
	{

		private readonly static string TAG = "PIN_MAP_FRAGMENT";

		internal IGeolocator Locator { get; set; }

		// Button to create new pin
		private FloatingActionButton _createPin;

		// Resources
		private Button _selectLocationButton;
		private Button _cancelLocationButton;
		private ImageView _selectLocationPin;
		private TextView _estimateAddress;
		private Button _filterButton;
		private AutoCompleteTextView _mapSearch;
		private ArrayAdapter _mapSearchAdapter;
		private string[] _mapSearchPredictions;

		// Filter related
		private bool[] _filterOptions;
		private List<Pin> _filteredPins;

		// Map related
		private GoogleMap _map;
		private List<Pin> _pins;
		private LatLng _selectLocation;
		private LatLng _currentLocation;
		private Dictionary<long, List<Pin>> _markerPins;
		private Dictionary<long, List<LatLng>> _markerPolygons;
		private Dictionary<long, ClusterPin> _markers;
		private ClusterManager _clusterManager;
        private ClusterRenderer _clusterRenderer;
		private const double PolyRadius = 0.0001;

		private bool _createPlaceRequest = false;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.map_activity);
			_pins = new List<Pin>();
			_filteredPins = new List<Pin>();
			_markerPins = new Dictionary<long, List<Pin>>();
			_markerPolygons = new Dictionary<long, List<LatLng>>();
			_markers = new Dictionary<long, ClusterPin>();
			_createPin = (FloatingActionButton)FindViewById(Resource.Id.map_create_pin_fab);
			_selectLocationButton = (Button)FindViewById(Resource.Id.map_create_pin_select_button);
			_cancelLocationButton = (Button)FindViewById(Resource.Id.map_create_pin_cancel_selection);
			_selectLocationPin = (ImageView)FindViewById(Resource.Id.map_create_pin_select_location_pin);
			_estimateAddress = (TextView)FindViewById(Resource.Id.map_fragment_estimate_address);
			_filterButton = (Button)FindViewById(Resource.Id.map_fragment_filter_button);
			_mapSearch = (AutoCompleteTextView)FindViewById(Resource.Id.map_fragment_search);
			_mapSearch.Threshold = 1;

			var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.main_toolbar);
			SetSupportActionBar(toolbar);
			SupportActionBar.Title = "Map";
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);
			SupportActionBar.SetDisplayShowHomeEnabled(true);
			SupportActionBar.SetDefaultDisplayHomeAsUpEnabled(true);

			// Get location
			Locator = CrossGeolocator.Current;
			_currentLocation = new LatLng(0, 0);
			Locator.PositionChanged += (sender, args) =>
			{
				var pos = args.Position;
				_currentLocation = new LatLng(pos.Latitude, pos.Longitude);
			};

			SetFilterOptions();
			SetListeners();
		}

		private void SetFilterOptions()
		{
			// 0: General
			// 1: Food
			// 2: POI
			// 3: Shopping
			// 4: Outdoors
			// 5: Cultural
			// 6: Kids
			// 7: Walking
			// 8: Biking
			// 9: Driving
			// 10: Budget

			_filterOptions = new bool[11];
			for (var i = 0; i < _filterOptions.Length; i++)
			{
				_filterOptions[i] = true;
			}
		}

		// Void here because we don't need to await the OnStart method
		protected override void OnStart()
		{
			base.OnStart();
			// If map is not attached to this fragment, get it async
			if (_map == null)
			{
				//((MapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map)).GetMapAsync(this);
				Android.Gms.Maps.MapFragment mapFrag = (Android.Gms.Maps.MapFragment)FragmentManager.FindFragmentById(Resource.Id.map);
				mapFrag.GetMapAsync(this);
			}
		}

		// Can only be called if map is ready!
		private void CreatePin(Pin pin)
		{
			// If a marker already exists within a certain diameter
			// Then do not create a new marker, rather put it in dict
			var stack = false;
			var markerId = 0L;
			var coordinate = new LatLng(pin.Coordinate.latitude, pin.Coordinate.longitude);

			// Look at each polygon in all the polygons
			foreach (var p in _markerPolygons)
			{
				// If the point is in the polygon, then we have to stack
				if (PolyUtil.containsLocation(coordinate, new List<LatLng>(p.Value), false))
				{
					stack = true;
					markerId = p.Key;
					break;
				}
			}

			// If not stacking, create a new pin and new polygon
			if (!stack)
			{
				//var polyOpt = new PolygonOptions()
				//.Add(new LatLng(pin.Latitude - POLY_RADIUS, pin.Longitude - POLY_RADIUS),
				//     new LatLng(pin.Latitude - POLY_RADIUS, pin.Longitude + POLY_RADIUS),
				//     new LatLng(pin.Latitude + POLY_RADIUS, pin.Longitude + POLY_RADIUS),
				//     new LatLng(pin.Latitude + POLY_RADIUS, pin.Longitude - POLY_RADIUS))
				//.Visible(false);
				var points = new List<LatLng>()
				{
					new LatLng(pin.Coordinate.latitude - PolyRadius, pin.Coordinate.longitude - PolyRadius),
					new LatLng(pin.Coordinate.latitude - PolyRadius, pin.Coordinate.longitude + PolyRadius),
					new LatLng(pin.Coordinate.latitude + PolyRadius, pin.Coordinate.longitude + PolyRadius),
					new LatLng(pin.Coordinate.latitude + PolyRadius, pin.Coordinate.longitude - PolyRadius)
				};

                var m = new ClusterPin(pin.Coordinate.latitude, pin.Coordinate.longitude, pin.Name);
                if (pin.Type == 0)
                {
                    pin.Type = pin.GuessType(pin.Description);
                }
                // Set the custom icon for pins of type 1-4
                if (pin.Type == 1){ m = setIcon(pin.Rating, m, "T");} // Food
                else if (pin.Type == 2) { m = setIcon(pin.Rating, m, "Bi"); } // POI
                else if (pin.Type == 3) { m = setIcon(pin.Rating, m, "P"); } // Shopping
                else if (pin.Type == 4) { m = setIcon(pin.Rating, m, "B"); } // Outdoor

                _clusterManager.AddItem(m);

				// Add new pin
				_markers.Add(m.Id, m);
				_markerPins.Add(m.Id, new List<Pin> { pin });
				_markerPolygons[m.Id] = points;
			}
			// Otherwise, just add a new pin at the same marker
			else
			{
				_markerPins[markerId].Add(pin);
			}
		}

        private ClusterPin setIcon(float rating, ClusterPin m, string file)
        {
            if (rating < 2) {
                file = file + "Pin1";
                try
                {
                    m.iconResId = (int)typeof(Resource.Drawable).GetField(file).GetValue(null);
                }catch(Exception ex)
                {
                    Log.Error("icon error", ex.Message);
                }
                return m;
            } else if (rating >= 2 && rating <= 4)
            {
                file = file + "Pin2";
                try
                {
                    m.iconResId = (int)typeof(Resource.Drawable).GetField(file).GetValue(null);
                }
                catch (Exception ex)
                {
                    Log.Error("icon error", ex.Message);
                }
                return m;
            }
            file = file + "Pin3";
            try
            {
                m.iconResId = (int)typeof(Resource.Drawable).GetField(file).GetValue(null);
            }
            catch (Exception ex)
            {
                Log.Error("icon error", ex.Message);
            }
            return m;
        }

		// Almost like a callback, gets called when the map is loaded
		public void OnMapReady(GoogleMap googleMap)
		{
			// Initialize map
			_map = googleMap;                               // Get the instance of the map
			_map.MapType = GoogleMap.MapTypeNormal;         // Set the type of map to normal
			_map.MyLocationEnabled = true;

			// Clustering
			_clusterManager = new ClusterManager(this, _map);
            _clusterRenderer = new ClusterRenderer(this, _map, _clusterManager);
            _clusterManager.SetOnClusterItemClickListener(this);
			_clusterManager.SetAlgorithm(new Com.Google.Maps.Android.Clustering.Algo.PreCachingAlgorithmDecorator
										 (new Com.Google.Maps.Android.Clustering.Algo.GridBasedAlgorithm()));
			// Map Listeners
			_map.SetOnCameraChangeListener(this);// When the user moves the map, this will listen
			_map.SetOnMarkerClickListener(_clusterManager);

			// Wait for location, should be relatively quick, then move camera
			Locator.PositionChanged += OnLocationChanged;

			// Get pins
			// TODO can we do this progressively?
			Task.Run(async () =>
			{
				_pins = await MainActivity.dayTomatoClient.GetPins();
				_filteredPins = new List<Pin>(_pins);

				// Load pins onto map
				RunOnUiThread(() =>
				{
					UpdateMap();
					_clusterManager.Cluster();
				});
			});

			_createPlaceRequest = Intent.GetBooleanExtra("CREATE_PLACE_REQUEST", false);
			if (_createPlaceRequest)
			{
				CreatePin();
			}
		}

		private void OnLocationChanged(object sender, PositionEventArgs args)
		{
			Locator.PositionChanged -= OnLocationChanged;

			var pos = args.Position;
			_currentLocation = new LatLng(pos.Latitude, pos.Longitude);
			var builder = CameraPosition.InvokeBuilder();
			builder.Target(_currentLocation);
			builder.Zoom(15);
			var cameraPosition = builder.Build();
			var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
			_map.AnimateCamera(cameraUpdate);
		}

		// Update pins on map when view changes
		private void UpdateMap()
		{
			// Load pins onto map
			// TODO culling by area/viewport
			_filteredPins.ForEach(CreatePin);
		}

		private async void CreatePin()
		{
			// Switch button states
			_selectLocationButton.Visibility = ViewStates.Visible;
			_selectLocationButton.Enabled = true;
			_cancelLocationButton.Visibility = ViewStates.Visible;
			_cancelLocationButton.Enabled = true;
			_createPin.Visibility = ViewStates.Invisible;
			_createPin.Enabled = false;
			_selectLocationPin.Visibility = ViewStates.Visible;
			_estimateAddress.Visibility = ViewStates.Visible;

			// Get currently centered location
			if (_selectLocation == null)
			{
				_selectLocation = _map.CameraPosition.Target;
				_estimateAddress.Text = await ReverseGeocode(_selectLocation);
			}
		}

		private void SetListeners()
		{
			// Allows the user to select a location on the map
			_createPin.Click += (sender, e) => 
			{
				CreatePin();
			};

			_cancelLocationButton.Click += (sender, e) =>
			{
				// Switch button states
				_selectLocationButton.Visibility = ViewStates.Invisible;
				_selectLocationButton.Enabled = false;
				_cancelLocationButton.Visibility = ViewStates.Invisible;
				_cancelLocationButton.Enabled = false;
				_createPin.Visibility = ViewStates.Visible;
				_createPin.Enabled = true;
				_selectLocationPin.Visibility = ViewStates.Invisible;
				_estimateAddress.Visibility = ViewStates.Invisible;
			};

			// User can select the location after clicking and the create a pin dialog shows
			_selectLocationButton.Click += (sender, e) =>
			{
				CreateNewPinDialog();
			};

			_filterButton.Click += (sender, e) =>
			{
				FilterDialog();
			};

			_mapSearch.TextChanged += async (sender, e) =>
			{
				try
				{
					_mapSearchPredictions = await MainActivity.googleClient.PredictPlaces(e.Text.ToString(),
																						 _currentLocation.Latitude,
																						 _currentLocation.Longitude);
					_mapSearchAdapter = new ArrayAdapter(this,
														 Android.Resource.Layout.SimpleDropDownItem1Line,
														 _mapSearchPredictions);
					_mapSearch.Adapter = _mapSearchAdapter;
				}
				catch (Exception ex)
				{
					Log.Error(TAG, ex.Message);
				}
			};

			_mapSearch.ItemClick += async (Sender, e) =>
			{
				try
				{
					var imm = (InputMethodManager)this.GetSystemService(Android.Content.Context.InputMethodService);
					imm.HideSoftInputFromWindow(_mapSearch.WindowToken, 0);

					if (_mapSearch.Text != string.Empty)
					{
						Coordinate coords = await MainActivity.googleClient.Geocode(_mapSearch.Text);
						UpdateCameraPosition(new LatLng(coords.latitude, coords.longitude));
						_mapSearch.Text = "";
						_mapSearch.ClearFocus();
					}
				}
				catch (Exception ex)
				{
					Log.Error(TAG, ex.Message);
				}
			};
		}

		private void FilterDialog()
		{
			var fm = FragmentManager;
			var ft = fm.BeginTransaction();

			//Remove fragment else it will crash as it is already added to backstack
			var prev = fm.FindFragmentByTag("FilterDialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutBooleanArray("FILTER_OPTIONS", _filterOptions);

			var filterDialogFragment = FilterDialogFragment.NewInstance(bundle);
			filterDialogFragment.FilterDialogClosed += OnFilterDialogClosed;

			//Add fragment
			filterDialogFragment.Show(fm, "FilterDialog");
		}

		private async void CreateNewPinDialog()
		{
			var fm = FragmentManager;
			var ft = fm.BeginTransaction();

			//Remove fragment else it will crash as it is already added to backstack
			var prev = fm.FindFragmentByTag("CreatePinDialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			SwitchLocationButtonState(true);

			// Reverse geocode coordinates
			var address = await ReverseGeocode(_selectLocation);

			var place = await MainActivity.googleClient.GetPlace(_selectLocation.Latitude, _selectLocation.Longitude);

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutString("SELECTED_LOCATION", address);
			bundle.PutDouble("SELECTED_LOCATION_LATITUDE", _selectLocation.Latitude);
			bundle.PutDouble("SELECTED_LOCATION_LONGITUDE", _selectLocation.Longitude);
			bundle.PutByteArray("SELECTED_LOCATION_IMAGE", place.Image);
			bundle.PutString("SELECTED_LOCATION_NAME", place.Name);
			bundle.PutInt("SELECTED_LOCATION_TYPE", place.PlaceType);
			bundle.PutString("SELECTED_LOCATION_DESCRIPTION", place.Description);

			var createPinDialogFragment = CreatePinDialogFragment.NewInstance(bundle);
			createPinDialogFragment.CreatePinDialogClosed += OnCreatePinDialogClosed;

			//Add fragment
			createPinDialogFragment.Show(fm, "CreatePinDialog");
		}

		private async void CreatePinDialog(Pin p)
		{
			var fm = FragmentManager;
			var ft = fm.BeginTransaction();

			//Remove fragment else it will crash as it is already added to backstack
			var prev = fm.FindFragmentByTag("CreatePinDialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			SwitchLocationButtonState(true);

			// Reverse geocode coordinates
			var address = await ReverseGeocode(new LatLng(p.Coordinate.latitude, p.Coordinate.longitude));

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutString("SELECTED_LOCATION", address);
			bundle.PutDouble("SELECTED_LOCATION_LATITUDE", p.Coordinate.latitude);
			bundle.PutDouble("SELECTED_LOCATION_LONGITUDE", p.Coordinate.longitude);
			bundle.PutByteArray("SELECTED_LOCATION_IMAGE",
								await MainActivity.dayTomatoClient.GetImageBitmapFromUrlAsync(p.ImageURL));
			bundle.PutString("SELECTED_LOCATION_NAME", p.Name);
			bundle.PutInt("SELECTED_LOCATION_TYPE", p.Type);
			bundle.PutString("SELECTED_LOCATION_DESCRIPTION", p.Description);

			var createPinDialogFragment = CreatePinDialogFragment.NewInstance(bundle);
			createPinDialogFragment.CreatePinDialogClosed += OnCreatePinDialogClosed;

			//Add fragment
			createPinDialogFragment.Show(fm, "CreatePinDialog");
		}

		private async Task<string> ReverseGeocode(LatLng coordinates)
		{
			// Reverse geocode coordinates
			var geo = new Geocoder(this);
			var addresses = await geo.GetFromLocationAsync(coordinates.Latitude, coordinates.Longitude, 1);

			var address = "Unknown Address";
			if (addresses.Count > 0)
			{
				address = addresses[0].GetAddressLine(0);
			}
			return address;
		}

		// When camera has finished moving, update the selected location
		public async void OnCameraChange(CameraPosition position)
		{
			_clusterManager.OnCameraChange(position);
			if (_selectLocation != null && _map != null)
			{
				_selectLocation = position.Target;
				_estimateAddress.Text = await ReverseGeocode(_selectLocation);
			}
		}

		public bool OnClusterItemClick(Java.Lang.Object marker)
		{
			// Get pins and sort them based on # of likes
			var pins = _markerPins[((ClusterPin)marker).Id];

			// Give seeds to all users if they are near!
			var curr = new Location("Current");
			var sel = new Location("Selected");
			curr.Latitude = _currentLocation.Latitude;
			curr.Longitude = _currentLocation.Longitude;
			sel.Latitude = ((ClusterPin)marker).Position.Latitude;
			sel.Longitude = ((ClusterPin)marker).Position.Longitude;

			// If they are within 300 meters
			if (curr.DistanceTo(sel) < 300)
			{
				var parts = pins.Count;
				foreach (var p in pins)
				{
					MainActivity.UpdateAccount(p.LinkedAccount, (double)(1 / parts), 0);
				}
			}

			pins.Sort(delegate (Pin p1, Pin p2) { return p2.Likes.CompareTo(p1.Likes); });
			var pinData = JsonConvert.SerializeObject(pins);

			var fm = FragmentManager;
			var ft = fm.BeginTransaction();

			//Remove fragment else it will crash as it is already added to backstack
			var prev = fm.FindFragmentByTag("ViewPinDialog");
			if (prev != null)
			{
				ft.Remove(prev);
			}

			ft.AddToBackStack(null);

			SwitchLocationButtonState(false);

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutString("VIEW_PIN_TITLE", ((ClusterPin)marker).Title);
			bundle.PutString("VIEW_PIN_DATA", pinData);
			bundle.PutLong("VIEW_PIN_MARKER", ((ClusterPin)marker).Id);

			var viewPinDialogFragment = ViewPinDialogFragment.NewInstance(bundle);
			viewPinDialogFragment.ViewPinDialogClosed += OnViewPinDialogClosed;

			//Add fragment
			viewPinDialogFragment.Show(fm, "ViewPinDialog");
			return true;
		}

		// Event listener, when the viewpin dialog is closed, this will get called
		public async void OnViewPinDialogClosed(object sender, ViewPinDialogEventArgs e)
		{
			// To fix issue when pressing "Add" in ViewPin was crashing
			_selectLocation = _markers[e.MarkerId].Position;

			if (e.Create)
			{
				CreatePinDialog(_markerPins[e.MarkerId][0]);
			}
			if (e.Delete)
			{
				_pins = await MainActivity.dayTomatoClient.GetPins();
				_markerPins.Remove(e.MarkerId);
				_markerPolygons.Remove(e.MarkerId);
				_clusterManager.RemoveItem(_markers[e.MarkerId]);
				_markers.Remove(e.MarkerId);
				_clusterManager.Cluster();
			}
			if (e.Update.Count > 0)
			{
				foreach (var t in e.Update)
				{
					// This really sucks, but I cannot figure out why its creating multiple of the same pins here
					_markerPins[e.MarkerId].RemoveAll(p => p.Id.Equals(t.Id));
					var r1 = _pins.FindIndex(p => p.Id.Equals(t.Id));
					_pins.RemoveAt(r1);
					_pins.Add(t);
					_markerPins[e.MarkerId].Add(t);
					await MainActivity.dayTomatoClient.UpdatePin(t);
				}
				_clusterManager.Cluster();
			}

			// Switch button states
			_createPin.Visibility = ViewStates.Visible;
			_createPin.Enabled = true;
		}

		// Event listener, when the createpin dialog is closed, this will get called
		private async void OnCreatePinDialogClosed(object sender, CreatePinDialogEventArgs e)
		{
			var account = MainActivity.GetAccount();
			var pin = new Pin
			{
				Type = e.PinType,
				Name = e.Name,
				Rating = e.Rating,
				Description = e.Description,
				Likes = 0,
				Coordinate = new Coordinate(_selectLocation.Latitude, _selectLocation.Longitude),
				LinkedAccount = account.Id,
				Username = account.Username,
				Review = e.Review,
				Cost = e.Cost,
				CreateDate = e.CreateDate,
				ImageURL = e.ImageUrl,
				Comments = new List<Comment>(),
				LikedBy = new List<string>(),
				DislikedBy = new List<string>()
			};

			pin.Id = await MainActivity.dayTomatoClient.CreatePin(pin);
            Analytics.Client.Track(account.AnalyticsId, "Pin created", new Properties() {
                    { "Pin name", pin.Name }
                }, new Options().SetIntegration("all", true));

            _pins.Add(pin);
			CreatePin(pin);

			_clusterManager.Cluster();

			if (_createPlaceRequest)
			{
				Intent returnIntent = new Intent();
				returnIntent.PutExtra("CREATE_PLACE_RESULT", pin.Id);
				SetResult(Result.Ok, returnIntent);
				Finish();
			}
		}

		private bool IsInFilter(Pin p)
		{
			for (int i = 0; i < _filterOptions.Length; ++i)
			{
				if (_filterOptions[i] == true && p.Type == i)
					return true;
			}
			return false;
		}

		private void OnFilterDialogClosed(object sender, FilterDialogEventArgs e)
		{
			/*
			 * 1. go through all pins:
			 * 2.   if the pin does not pass filter
			 * 3.     remove it from filteredPins
			 * 4.   else if filterPins doesnt have pin
			 * 5.     add it to filterPins
			 * 6. update map with filtered pins
			 */

			_filterOptions = e.FilterOptions;
			if (e.Filter)
			{
				foreach (var p in _pins)
				{
					// Remove
					int index = _filteredPins.FindIndex((t => t.Id.Equals(p.Id)));
					if (!IsInFilter(p) && index != -1)
					{
						_filteredPins.RemoveAt(index);
					}
					// Add
					else if (index == -1)
					{
						_filteredPins.Add(p);
					}
				}

				_markers = new Dictionary<long, ClusterPin>();
				_markerPins = new Dictionary<long, List<Pin>>();
				_markerPolygons = new Dictionary<long, List<LatLng>>();
				_clusterManager.ClearItems();

				UpdateMap();
				_clusterManager.Cluster();
				RefreshMap();
			}
		}

		private void UpdateCameraPosition(LatLng position)
		{

			CameraPosition cameraPosition = new CameraPosition.Builder()
			.Target(position)
			.Zoom(20)
			.Bearing(0)
			.Tilt(45)
			.Build();
			_map.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));

			//CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(position, 20);


			//_map.AnimateCamera(cameraUpdate);
		}

		private void RefreshMap()
		{
			_clusterManager.OnCameraChange(_map.CameraPosition);
		}

		private void SwitchLocationButtonState(bool createPin)
		{
			// Switch button states
			_selectLocationButton.Visibility = ViewStates.Invisible;
			_selectLocationButton.Enabled = false;
			_cancelLocationButton.Visibility = ViewStates.Invisible;
			_cancelLocationButton.Enabled = false;
			_createPin.Visibility = createPin ? ViewStates.Visible : ViewStates.Invisible;
			_createPin.Enabled = createPin;
			_selectLocationPin.Visibility = ViewStates.Invisible;
			_estimateAddress.Visibility = ViewStates.Invisible;
		}

		public override void OnBackPressed()
		{
			base.OnBackPressed();
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					Finish();
					return true;

				default:
					return base.OnOptionsItemSelected(item);
			}
		}
	}
}

