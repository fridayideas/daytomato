using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Support.V4.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System.Collections.Generic;
using DayTomato.Models;
using System.Threading.Tasks;
using Android.Widget;
using Android.Locations;
using System;
using Newtonsoft.Json;
using Com.Google.Maps.Android.Clustering;
using Android.Support.V7.App;

namespace DayTomato.Droid.Fragments
{
    class MapFragment : Fragment, IOnMapReadyCallback, GoogleMap.IOnCameraChangeListener, 
						ClusterManager.IOnClusterItemClickListener
    {
		private readonly static string TAG = "PIN_MAP_FRAGMENT";

		// Button to create new pin
		private FloatingActionButton _createPin;

		// Resources
		private Button _selectLocationButton;
		private Button _cancelLocationButton;
		private ImageView _selectLocationPin;
		private TextView _estimateAddress;

		// Map related
		private GoogleMap _map;
		private List<Pin> _pins;
		private LatLng _selectLocation;
		private LatLng _currentLocation;
		private Dictionary<long, List<Pin>> _markerPins;
		private Dictionary<long, Polygon> _markerPolygons;
		private ClusterManager _clusterManager;

		private const double POLY_RADIUS = 0.0001;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.map_fragment, container, false);

			_pins = new List<Pin>();
			_markerPins = new Dictionary<long, List<Pin>>();
			_markerPolygons = new Dictionary<long, Polygon>();
			_createPin = (FloatingActionButton)view.FindViewById(Resource.Id.map_create_pin_fab);
			_selectLocationButton = (Button)view.FindViewById(Resource.Id.map_create_pin_select_button);
			_cancelLocationButton = (Button)view.FindViewById(Resource.Id.map_create_pin_cancel_selection);
			_selectLocationPin = (ImageView)view.FindViewById(Resource.Id.map_create_pin_select_location_pin);
			_estimateAddress = (TextView)view.FindViewById(Resource.Id.map_fragment_estimate_address);

			SetListeners();

            return view;
        }

		// Void here because we don't need to await the OnStart method
		public override async void OnStart()
		{
			base.OnStart();
			await InitMapFragment();				// Wait for map to initialize
		}

		private async Task InitMapFragment()
		{
			// Get the pins
			_pins = await MainActivity.dayTomatoClient.GetPins();
			// If map is not attached to this fragment, get it async
			if (_map == null)
			{
				((SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map)).GetMapAsync(this);
			}
		}

		// Can only be called if map is ready!
		private void CreatePin(Pin pin)
		{
			if (_map != null)
			{
				// If a marker already exists within a certain diameter
				// Then do not create a new marker, rather put it in dict
				var stack = false; 
				var markerId = 0L;
				var coordinate = new LatLng(pin.Latitude, pin.Longitude);

				// Look at each polygon in all the polygons
				foreach(var p in _markerPolygons)
				{
					// If the point is in the polygon, then we have to stack
					if(PolyUtil.containsLocation(coordinate, new List<LatLng>(p.Value.Points), false))
					{
						stack = true;
						markerId = p.Key;
						break;
					}
			  	}
				// If not stacking, create a new pin and new polygon
				if (!stack)
				{
					var polyOpt = new PolygonOptions()
					.Add(new LatLng(pin.Latitude - POLY_RADIUS, pin.Longitude - POLY_RADIUS),
						 new LatLng(pin.Latitude - POLY_RADIUS, pin.Longitude + POLY_RADIUS),
						 new LatLng(pin.Latitude + POLY_RADIUS, pin.Longitude + POLY_RADIUS),
						 new LatLng(pin.Latitude + POLY_RADIUS, pin.Longitude - POLY_RADIUS))
					.Visible(false);

					var poly = _map.AddPolygon(polyOpt);
					var m = new ClusterPin(pin.Latitude, pin.Longitude);
					m.Title = pin.Name;
					_clusterManager.AddItem(m);
					_clusterManager.Cluster();

					// Add new pin
					_markerPins.Add(m.Id, new List<Pin> { pin });
					_markerPolygons[m.Id] = poly;
				}
				// Otherwise, just add a new pin at the same marker
				else 
				{
					_markerPins[markerId].Add(pin);
				}

			}
		}

		// Almost like a callback, gets called when the map is loaded
		public void OnMapReady(GoogleMap googleMap)
		{
			// Initialize map
			_map = googleMap;								// Get the instance of the map
			_map.MapType = GoogleMap.MapTypeNormal;         // Set the type of map to normal
			_map.MyLocationEnabled = true;

			// Clustering
			_clusterManager = new ClusterManager(Context, _map);
			_clusterManager.SetOnClusterItemClickListener(this);

			// Map Listeners
			_map.SetOnCameraChangeListener(this);// When the user moves the map, this will listen
			_map.SetOnMarkerClickListener(_clusterManager);

			// Wait for location, should be relatively quick, then move camera
			while (_currentLocation == null)
			{
				_currentLocation = MainActivity.GetLocation();
			}
			var builder = CameraPosition.InvokeBuilder();
			builder.Target(_currentLocation);
			builder.Zoom(18);
			var cameraPosition = builder.Build();
			var cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
			_map.MoveCamera(cameraUpdate);

			// Load pins onto map
			UpdateMap();
		}

		// Update pins on map when view changes
		private void UpdateMap()
		{
			// Load pins onto map
			for (int i = 0; i < _pins.Count; ++i)
			{
				CreatePin(_pins[i]);
			}
		}

		private void SetListeners()
		{
			// Allows the user to select a location on the map
			_createPin.Click += async (sender, args) =>
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
				Location curr = new Location("Current");
				Location sel = new Location("Selected");
				curr.Latitude = _currentLocation.Latitude;
				curr.Longitude = _currentLocation.Longitude;
				sel.Latitude = _selectLocation.Latitude;
				sel.Longitude = _selectLocation.Longitude;

				if (curr.DistanceTo(sel) > 100 && (MainActivity.GetAccount().Seeds < 200))
				{
					AlertDialog.Builder alert = new AlertDialog.Builder(Context);
					alert.SetTitle("You're too far away!");
					alert.SetMessage("You need to be within 100m of placing a new pin. " +
					                 "Collect more seeds to remove this restriction! ");
					alert.SetPositiveButton("OK", (senderAlert, args) => {});

					Android.App.Dialog dialog = alert.Create();
					dialog.Show();
				}
				else
				{
					CreatePinDialog();
				}
			}; 
		}

		async void CreatePinDialog()
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

			// Switch button states
			_selectLocationButton.Visibility = ViewStates.Invisible;
			_selectLocationButton.Enabled = false;
			_cancelLocationButton.Visibility = ViewStates.Invisible;
			_cancelLocationButton.Enabled = false;
			_createPin.Visibility = ViewStates.Visible;
			_createPin.Enabled = true;
			_selectLocationPin.Visibility = ViewStates.Invisible;
			_estimateAddress.Visibility = ViewStates.Invisible;

			var place = await MainActivity.dayTomatoClient.GetPlace(_selectLocation.Latitude, _selectLocation.Longitude);

			// Reverse geocode coordinates
			var address = await ReverseGeocode(_selectLocation);

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutString("SELECTED_LOCATION", address);
			bundle.PutDouble("SELECTED_LOCATION_LATITUDE", _selectLocation.Latitude);
			bundle.PutDouble("SELECTED_LOCATION_LONGITUDE", _selectLocation.Longitude);
			bundle.PutByteArray("SELECTED_LOCATION_IMAGE", place.Image);
			bundle.PutString("SELECTED_LOCATION_NAME", place.Name);
			bundle.PutString("SELECTED_LOCATION_DESCRIPTION", place.Description);

			var createPinDialogFragment = CreatePinDialogFragment.NewInstance(bundle);
			createPinDialogFragment.CreatePinDialogClosed += OnCreatePinDialogClosed;

			//Add fragment
			createPinDialogFragment.Show(fm, "CreatePinDialog");
		}

		public async Task<string> ReverseGeocode(LatLng coordinates)
		{
			// Reverse geocode coordinates
			var geo = new Geocoder(Context);
			var addresses = await geo.GetFromLocationAsync(coordinates.Latitude, coordinates.Longitude, 1);

			var address = "Unknown Address";
			if (addresses.Count > 0)
			{
				address = addresses[0].GetAddressLine(0);
			}
			return address;
		}

		// Event listener, when the dialog is closed, this will get called
		public async void OnCreatePinDialogClosed(object sender, CreatePinDialogEventArgs e)
		{
			var account = MainActivity.GetAccount();
			var pin = new Pin
			{
				Type = 0,
				Name = e.Name,
				Rating = e.Rating,
				Description = e.Description,
				Likes = 0,
				Latitude = _selectLocation.Latitude,
				Longitude = _selectLocation.Longitude,
				LinkedAccount = account.Id,
				Review = e.Review,
				CreateDate = DateTime.Today
			};
			_pins.Add(pin);
			CreatePin(pin);
			await MainActivity.dayTomatoClient.CreatePin(pin);
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

			// Switch button states
			_selectLocationButton.Visibility = ViewStates.Invisible;
			_selectLocationButton.Enabled = false;
			_cancelLocationButton.Visibility = ViewStates.Invisible;
			_cancelLocationButton.Enabled = false;
			_createPin.Visibility = ViewStates.Invisible;
			_createPin.Enabled = false;
			_selectLocationPin.Visibility = ViewStates.Invisible;
			_estimateAddress.Visibility = ViewStates.Invisible;

			// Create and show the dialog.
			var bundle = new Bundle();
			bundle.PutString("VIEW_PIN_TITLE", ((ClusterPin)marker).Title);
			bundle.PutString("VIEW_PIN_DATA", pinData);

			var viewPinDialogFragment = ViewPinDialogFragment.NewInstance(bundle);
			viewPinDialogFragment.ViewPinDialogClosed += OnViewPinDialogClosed;

			//Add fragment
			viewPinDialogFragment.Show(fm, "ViewPinDialog");
			return true;
		}

		// Event listener, when the dialog is closed, this will get called
		public void OnViewPinDialogClosed(object sender, ViewPinDialogEventArgs e)
		{
			if (e.Create)
			{
				CreatePinDialog();
			}

			// Switch button states
			_createPin.Visibility = ViewStates.Visible;
			_createPin.Enabled = true;
		}
	}
}