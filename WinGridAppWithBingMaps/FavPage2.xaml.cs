using WinGridAppWithBingMaps.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WinGridAppWithBingMaps.Data;
using Windows.UI.Core;
using Windows.Data.Json;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace WinGridAppWithBingMaps
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class FavPage2 : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        bool found;
        String fileName = "favlist.txt";
        ObservableCollection<SampleDataItem> favViewSource;


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


        public FavPage2()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            createFile();

        }

        /// <summary>
        /// Method to create a text file in current local folder
        /// </summary>
        private async void createFile()
        {
            try
            {
                StorageFolder cfolder = ApplicationData.Current.LocalFolder;
                //make sure collision option is set to OpenIfExists so it does not overwrite the existing file
                // after it has been created
                StorageFile cfile = await cfolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            }
            catch (Exception e)
            {
                await ShowDialog("createFile " + e.ToString());
            }
        }
        // Searchbox submitted event
        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            this.Frame.Navigate(typeof(SearchResultsPage1), args.QueryText);
        }

        async Task ShowDialog(string s)
        {
            CoreWindowDialog dialog = new CoreWindowDialog(s);
            await dialog.ShowAsync();
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = await SampleDataSource.GetGroupsAsync();
            this.DefaultViewModel["Groups"] = sampleDataGroups;

            // call this method each time the favourites page is loaded/navigated too
            readStuff();
        }

        /// <summary>
        /// method to read uniqueid saved in text file and 
        /// change the favourites listview source to read in the apropriate 
        /// data linked to that uniqueid in the sampleData.json file
        /// </summary>
        private async void readStuff()
        {
            favViewSource = new ObservableCollection<SampleDataItem>();

            StorageFolder rfolder;
            StorageFile favList = null;
            string content = "[]";

            // get data
            try
            {
                rfolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                favList = await rfolder.GetFileAsync(fileName);

                content = await FileIO.ReadTextAsync(favList);
            }
            catch (FileNotFoundException)
            {
                await ShowDialog("File not Found!");
            }

            // set data
            if (content.Length > 0)
            {
                JsonArray favsArray = JsonArray.Parse(content);
                if (favsArray.Count > 0)
                {
                    foreach (JsonValue id in favsArray)
                    {
                        // TODO: Create an appropriate data model for your problem domain to replace the sample data
                        var item = await SampleDataSource.GetItemAsync(id.GetString());
                        favViewSource.Add(item);
                    }
                    lstFavourites.ItemsSource = favViewSource;// set listview source
                }
            }
        }

        /// <summary>
        /// Takes information from list and converts the string to json file data
        /// </summary>
        /// <param name="favsList">a list parameter</param>
        private async void saveList(List<String> favsList)
        {
            // create json file
            JsonArray jArray = new JsonArray();

            if (favsList.Count > 0)
            {
                foreach (string s in favsList)
                {
                    // convert string to JsonValue
                    JsonValue jsonValue = JsonValue.CreateStringValue(s);
                    jArray.Add(jsonValue);
                }
            }
            string content = jArray.ToString();

            // get link to file
            StorageFolder fFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile fFile = await fFolder.GetFileAsync(fileName);
                // create favourites list json file
                await FileIO.WriteTextAsync(fFile, content);
            }
            catch (FileNotFoundException)
            {
                await ShowDialog("File not found!");
            }
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

        /// <summary>
        /// Save items in favourites listview to a text file.
        /// it saves the items uniqueId from in the sampleData.json file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSaveFlyout_Click(object sender, RoutedEventArgs e)
        {
            // create list to hold data
            List<String> l = new List<String>();

            // point to folder destination of file
            StorageFolder sfolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile sfile = await sfolder.GetFileAsync(fileName);

            try
            {
                if (sfile != null)// if file exists in folder location
                {
                    foreach (var item in lstFavourites.Items)
                    {
                        SampleDataItem sd = (SampleDataItem)item;
                        l.Add(sd.UniqueId);
                    }
                    saveList(l);// save each item in the listview to json
                    await ShowDialog("Favourites saved!");
                }
            }
            catch (FileNotFoundException)
            {
                await ShowDialog("File not Found!");
            }
        }

        /// <summary>
        /// Add button click event to add selected items
        /// in the destinations listview to the favourites listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            found = false;
            // check if items have already been added to the list
            listCheckForDbls();

            try
            {
                if (found == true)// if item already exists in the listview
                {
                    await ShowDialog("1 or more items already added");
                }
                else// else add item/s
                {
                    foreach (var item in lstDestinations.SelectedItems)
                    {
                        //lstFavourites.Items.Add(item);
                        SampleDataItem sdi = (SampleDataItem)item;
                        favViewSource.Add(sdi);
                    }
                    // set listview source
                    lstFavourites.ItemsSource = favViewSource;
                    lstDestinations.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                await ShowDialog("Error!\n" + ex.ToString());
            }

        }

        /// <summary>
        /// Method to check if any of the selected items to add to the
        /// favourites listview are already added
        /// </summary>
        private void listCheckForDbls()
        {
            foreach (var originalItem in lstDestinations.SelectedItems)
            {
                foreach (var copiedItem in lstFavourites.Items)
                {
                    //Check for equality between both items
                    if (originalItem == copiedItem)
                    {
                        found = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// clear button click event to empty out the
        /// favourites listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder fFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile fFile = await fFolder.GetFileAsync(fileName);
            try
            {
                await FileIO.WriteTextAsync(fFile, "");
                favViewSource.Clear();
                lstFavourites.ItemsSource = favViewSource;
            }
            catch (Exception ex)
            {
                await ShowDialog("Error!\n" + ex.ToString());
            }
        }

        /// <summary>
        /// Reading text from a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task<string> readStringFromLocalFile(string filename)
        {
            // reads the contents of file 'filename' in the app's local storage folder and returns it as a string

            // access the local folder
            StorageFolder local = ApplicationData.Current.LocalFolder;
            // open the file 'filename' for reading
            Stream stream = await local.OpenStreamForReadAsync(filename);
            string text;

            // copy the file contents into the string 'text'
            using (StreamReader reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }

        private async void setSaveFlyoutText()
        {
            // read file
            string checkContent = await readStringFromLocalFile(fileName);
            int count = checkContent.Length;
            if (count > 3)
            {
                txtSaveFlyout.Text = "Overwrite existing Favourites List file?";
            }
            else
            {
                txtSaveFlyout.Text = "Save Favourites List to File?";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            setSaveFlyoutText();
        }
    }
}
