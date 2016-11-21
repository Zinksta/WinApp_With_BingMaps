using WinGridAppWithBingMaps.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Bing.Maps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace WinGridAppWithBingMaps
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MapPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private readonly string BingMapsKey =
            "Enter your own BingMaps Key2";

        private readonly XNamespace BingMapsNamespace =
            "http://schemas.microsoft.com/search/local/ws/rest/v1";

        private Geolocator locator;

        private uint count;

        MapShapeLayer layer;
        MapPolyline polyline;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MapPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            this.WayPoints = new List<string>();

            this.locator = new Geolocator();

            this.locator.DesiredAccuracy = PositionAccuracy.Default;

            this.locator.PositionChanged += this.GeolocatorPositionChanged;
        }

        //Define a property called WayPoints of type List<string> to contain the list of way-points
        //along the route.
        public List<string> WayPoints
        {
            get;
            set;
        }

        //Add the event handler for the PositionChanged event that will center the map to the current position.
        async private void GeolocatorPositionChanged
            (Geolocator sender, PositionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Location location = new Location(e.Position.Coordinate.Point.Position.Latitude,
                e.Position.Coordinate.Point.Position.Longitude);
                this.Map.SetView(location);
            });
        }

        //Add the event handler for the MapTapped event that will determine the Location on the map
        // that was tapped(or clicked) and then call the GetDirections method to update the directions.
        private void Map_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this.Map);
            Location location;
            if (this.Map.TryPixelToLocation(position, out location))
            {
                Pushpin pin = new Pushpin();
                MapLayer.SetPosition(pin, location);
                pin.Text = (++count).ToString();
                this.Map.Children.Add(pin);
                this.Map.SetView(location);
                this.GetDirections(location);
            }
        }

        //Add the asynchronous GetDirections method to call the Routes Web API to get the
        // directions.The MapPolyline class is then used to draw the route of travel upon the map.
        async private void GetDirections(Location location)
        {
            try
            {
                this.WayPoints.Add(string.Format("{0}, {1}",
                location.Latitude, location.Longitude));
                if (this.WayPoints.Count < 2) return;
                HttpClient client = new HttpClient();
                StringBuilder builder = new StringBuilder("http://dev.virtualearth.net/REST/V1/Routes/Driving?o=xml&");
                for (int index = 0; index < this.WayPoints.Count; index++)
                {
                    builder.Append(
                    string.Format("wp.{0}={1}&", index, this.WayPoints[index]));
                }
                builder.Append("routePathOutput=Points&avoid=minimizeTolls&key=");
                builder.Append(this.BingMapsKey);
                HttpResponseMessage response = await client.GetAsync(builder.ToString());
                response.EnsureSuccessStatusCode();
                Stream stream = await response.Content.ReadAsStreamAsync();
                XDocument document = XDocument.Load(stream);
                var query = from p
                            in document.Descendants(this.BingMapsNamespace + "Point")
                            select new
                            {
                                Latitude = p.Element(this.BingMapsNamespace + "Latitude").Value,
                                Longitude = p.Element(this.BingMapsNamespace + "Longitude").Value
                            };
                layer = new MapShapeLayer();
                polyline = new MapPolyline();
                foreach (var point in query)
                {
                    double latitude, longitude;
                    double.TryParse(point.Latitude, out latitude);
                    double.TryParse(point.Longitude, out longitude);
                    polyline.Locations.Add(new Location(latitude, longitude));
                }
                polyline.Color = Colors.Red;
                polyline.Width = 5;
                layer.Shapes.Add(polyline);
                this.Map.ShapeLayers.Add(layer);
                var distance = (from d in document.Descendants(this.BingMapsNamespace + "TravelDistance")
                                select d).First().Value;
                this.txtDistance.Text =
                string.Format("{0} KM", distance.ToString());
            }
            catch (Exception)
            {
                await ShowDialog("Destination distance cannot be resolved! Please Refresh the map");
            }

        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="Common.SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="Common.NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        // Search box method
        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            this.Frame.Navigate(typeof(SearchResultsPage1), args.QueryText);
        }

        //Dialog method
        async Task ShowDialog(string s)
        {
            CoreWindowDialog dialog = new CoreWindowDialog(s);
            await dialog.ShowAsync();
        }

        // Refresh button click event reloads page to clear all data on map
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.Map.Children.Clear();
            this.Map.ShapeLayers.Clear();
            this.WayPoints.Clear();
            count = 0;
            txtDistance.Text = "";
            Map.Center = new Location(-37.8136, 144.9631);
            Map.ZoomLevel = 13;

        }
    }
}
