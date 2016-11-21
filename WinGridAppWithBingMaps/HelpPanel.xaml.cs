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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WinGridAppWithBingMaps
{
    public sealed partial class HelpPanel : SettingsFlyout
    {
        public HelpPanel()
        {
            this.InitializeComponent();
        }

        private void MenuFlyoutItem_Fav(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = "Select destinations on the left that you would like to visit.\n" +
                "Then click the 'Add Favourites' button to copy the selected destinations to the right.\n" +
                "You can reorder the destinations in the right list by holding down the left-click mouse " +
                "button and dragging the item around and then dropping it where you like.\n" +
                "To save your list of favourite destinations, click the 'Save Favourites' button.";
        }

        private void MenuFlyoutItem_Dir(object sender, RoutedEventArgs e)
        {
            txtInfo.Text = "To calculate the distance of travel, select a starting point on the " +
                "map and then selectect an end point.\n" +
                "To clear map and start again click the 'Refresh' button";
        }
    }
}
