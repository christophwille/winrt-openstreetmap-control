using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DemoFrontend.Nominatim;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace DemoFrontend
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : DemoFrontend.Common.LayoutAwarePage
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void SearchForLocation_OnClick(object sender, RoutedEventArgs e)
        {
            string searchText = LocationTextBox.Text;

            try
            {
                var geocodeProxy = new NominatimProxy();
                var result = await geocodeProxy.ExecuteQuery(searchText);

                if (result.Count > 0)
                {
                    LocationsListBox.ItemsSource = result;
                }
                else
                {
                    LocationsListBox.ItemsSource = null;
                }
            }
            catch (Exception)
            {
                // TODO in real-world app: notify user of failed search
                LocationsListBox.ItemsSource = null;
            }
        }

        private void LocationsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationsListBox.SelectedItem != null)
            {
                var selectedResult = (GeocodeResult) LocationsListBox.SelectedItem;

                Map.SetView(selectedResult.Latitude, selectedResult.Longitude, 14);
            }
        }
    }
}
