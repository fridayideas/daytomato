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
using Plugin.Geolocator;
using Android.Locations;
using System;
using Android.Util;
using Android.Content;
using Newtonsoft.Json;

namespace DayTomato.Droid.Fragments
{
    class MapFragment : Fragment, IOnMapReadyCallback, GoogleMap.IOnCameraChangeListener, GoogleMap.IOnMarkerClickListener
    {
		private readonly string TAG = "PIN_MAP_FRAGMENT";

		private FloatingActionButton _createPin;
		private Button _selectLocationButton;
		private GoogleMap _map;
		private List<Pin> _pins;
		private LatLng _selectLocation;
		private LatLng _currentLocation;
		private ImageView _selectLocationPin;
		private TextView _estimateAddress;
		private Dictionary<string, List<Pin>> _markerPins;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = inflater.Inflate(Resource.Layout.map_fragment, container, false);

			_pins = new List<Pin>();
			_markerPins = new Dictionary<string, List<Pin>>();
			_createPin = (FloatingActionButton)view.FindViewById(Resource.Id.map_create_pin_fab);
			_selectLocationButton = (Button)view.FindViewById(Resource.Id.map_create_pin_select_button);
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
				MarkerOptions marker = new MarkerOptions();
				marker.SetPosition(new LatLng(pin.Latitude, pin.Longitude));
				marker.SetTitle(pin.Name);
				Marker m = _map.AddMarker(marker);

				if (_markerPins.ContainsKey(m.Id))
				{
					_markerPins[m.Id].Add(pin);

				}
				else
				{
					_markerPins.Add(m.Id, new List<Pin> { pin });
				}

			}
		}

		// Almost like a callback, gets called when the map is loaded
		public void OnMapReady(GoogleMap googleMap)
		{
			_map = googleMap;								// Get the instance of the map
			_map.MapType = GoogleMap.MapTypeNormal;         // Set the type of map to normal
			_map.SetOnCameraChangeListener(this);           // When the user moves the map, this will listen
			_map.SetOnMarkerClickListener(this);

			// Wait for location, should be relatively quick, then move camera
			while (_currentLocation == null)
			{
				_currentLocation = MainActivity.GetLocation();
			}
			CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
			builder.Target(_currentLocation);
			builder.Zoom(18);
			CameraPosition cameraPosition = builder.Build();
			CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
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

			// User can select the location after clicking and the create a pin dialog shows
			_selectLocationButton.Click += async (sender, args) =>
			{
				var fm = FragmentManager;
				FragmentTransaction ft = fm.BeginTransaction();

				//Remove fragment else it will crash as it is already added to backstack
				Fragment prev = fm.FindFragmentByTag("CreatePinDialog");
				if (prev != null)
				{
					ft.Remove(prev);
				}

				ft.AddToBackStack(null);

				// Switch button states
				_selectLocationButton.Visibility = ViewStates.Invisible;
				_selectLocationButton.Enabled = false;
				_createPin.Visibility = ViewStates.Visible;
				_createPin.Enabled = true;
				_selectLocationPin.Visibility = ViewStates.Invisible;
				_estimateAddress.Visibility = ViewStates.Invisible;

				// Reverse geocode coordinates
				string address = await ReverseGeocode(_selectLocation);

				// Create and show the dialog.
				Bundle bundle = new Bundle();
				bundle.PutString("SELECTED_LOCATION", address);
				bundle.PutDouble("SELECTED_LOCATION_LATITUDE", _selectLocation.Latitude);
				bundle.PutDouble("SELECTED_LOCATION_LONGITUDE", _selectLocation.Longitude);

				CreatePinDialogFragment createPinDialogFragment = CreatePinDialogFragment.NewInstance(bundle);
				createPinDialogFragment.DialogClosed += OnDialogClosed;

				//Add fragment
				createPinDialogFragment.Show(fm, "CreatePinDialog");
			};
		}

		public async Task<string> ReverseGeocode(LatLng coordinates)
		{
			// Reverse geocode coordinates
			var geo = new Geocoder(Context);
			var addresses = await geo.GetFromLocationAsync(coordinates.Latitude, coordinates.Longitude, 1);

			string address = "Unknown Address";
			if (addresses.Count > 0)
			{
				address = addresses[0].GetAddressLine(0);
			}
			return address;
		}

		// Event listener, when the dialog is closed, this will get called
		public async void OnDialogClosed(object sender, DialogEventArgs e)
		{
			Account account = MainActivity.GetAccount();
			Pin pin = new Pin
			{
				Type = 0,
				Name = e.Name,
				Rating = e.Rating,
				Description = e.Description,
				Likes = 0,
				Latitude = _selectLocation.Latitude,
				Longitude = _selectLocation.Longitude,
				LinkedAccount = account.Id,
				Reviews = new List<Review>
				{
					new Review(account.Id, e.Review, DateTime.Today)
				},
				CreateDate = DateTime.Today
			};
			_pins.Add(pin);
			CreatePin(pin);
			await MainActivity.dayTomatoClient.CreatePin(pin);
		}

		// When camera has finished moving, update the selected location
		public async void OnCameraChange(CameraPosition position)
		{
			if (_selectLocation != null && _map != null)
			{
				_selectLocation = position.Target;
				_estimateAddress.Text = await ReverseGeocode(_selectLocation);
			}
		}

		public bool OnMarkerClick(Marker marker)
		{
			// Get pins and sort them based on # of likes
			List<Pin> pins = _markerPins[marker.Id];
			pins.Sort(delegate (Pin p1, Pin p2) { return p2.Likes.CompareTo(p1.Likes); });
			string pinData = JsonConvert.SerializeObject(pins);

			Intent intent = new Intent(Context, typeof(ViewPin));
			intent.PutExtra("VIEW_PIN_TITLE", marker.Title);
			intent.PutExtra("VIEW_PIN_DATA", pinData);
			StartActivity(intent);
			return true;
		}
	}
}