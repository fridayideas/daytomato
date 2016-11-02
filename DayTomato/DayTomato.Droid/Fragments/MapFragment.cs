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
using Plugin.Geolocator.Abstractions;

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
		private Dictionary<long, List<LatLng>> _markerPolygons;
		private Dictionary<long, ClusterPin> _markers;
		private ClusterManager _clusterManager;
		private const double PolyRadius = 0.0001;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.map_fragment, container, false);
			_pins = new List<Pin>();
			_markerPins = new Dictionary<long, List<Pin>>();
			_markerPolygons = new Dictionary<long, List<LatLng>>();
			_markers = new Dictionary<long, ClusterPin>();
			_createPin = (FloatingActionButton)view.FindViewById(Resource.Id.map_create_pin_fab);
			_selectLocationButton = (Button)view.FindViewById(Resource.Id.map_create_pin_select_button);
			_cancelLocationButton = (Button)view.FindViewById(Resource.Id.map_create_pin_cancel_selection);
			_selectLocationPin = (ImageView)view.FindViewById(Resource.Id.map_create_pin_select_location_pin);
			_estimateAddress = (TextView)view.FindViewById(Resource.Id.map_fragment_estimate_address);

			SetListeners();

            return view;
        }

		// Void here because we don't need to await the OnStart method
		public override void OnStart()
		{
			base.OnStart();
			// If map is not attached to this fragment, get it async
			if (_map == null)
			{
				((SupportMapFragment)ChildFragmentManager.FindFragmentById(Resource.Id.map)).GetMapAsync(this);
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
                var m = new ClusterPin(pin.Coordinate.latitude, pin.Coordinate.longitude) { Title = pin.Name };
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
			_clusterManager.SetAlgorithm(new Com.Google.Maps.Android.Clustering.Algo.PreCachingAlgorithmDecorator
			                             (new Com.Google.Maps.Android.Clustering.Algo.GridBasedAlgorithm()));
			// Map Listeners
			_map.SetOnCameraChangeListener(this);// When the user moves the map, this will listen
			_map.SetOnMarkerClickListener(_clusterManager);

			// Wait for location, should be relatively quick, then move camera
		    ((MainActivity) Activity).Locator.PositionChanged += OnLocationChanged;

            // Get pins
            // TODO can we do this progressively?
		    Task.Run(async () =>
		    {
                _pins = await MainActivity.dayTomatoClient.GetPins();

                // Load pins onto map
                Activity.RunOnUiThread(() =>
                {
                    UpdateMap();
                    _clusterManager.Cluster();
                });
            });
        }

        private void OnLocationChanged(object sender, PositionEventArgs args)
        {
		    ((MainActivity) Activity).Locator.PositionChanged -= OnLocationChanged;

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
            _pins.ForEach(CreatePin);
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
				CreatePinDialog();
			}; 
		}

        private async void CreatePinDialog()
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

        private async Task<string> ReverseGeocode(LatLng coordinates)
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
			sel.Longitude =((ClusterPin)marker).Position.Longitude;

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
				CreatePinDialog();
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
				Type = 0,
				Name = e.Name,
				Rating = e.Rating,
				Description = e.Description,
				Likes = 0,
				Coordinate = new Coordinate(_selectLocation.Latitude, _selectLocation.Longitude),
				LinkedAccount = account.Id,
				Review = e.Review,
				Cost = e.Cost,
				CreateDate = e.CreateDate,
				ImageURL = e.ImageUrl,
				Comments = new List<Comment>()
			};

			pin.Id = await MainActivity.dayTomatoClient.CreatePin(pin);

			_pins.Add(pin);
			CreatePin(pin);

			_clusterManager.Cluster();
		}
	}
}