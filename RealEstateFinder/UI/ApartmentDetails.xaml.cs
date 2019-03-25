using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RealEstateFinder.Core;

namespace RealEstateFinder.UI
{
    /// <summary>
    /// Interaction logic for AppartmentDetails.xaml
    /// </summary>
    public partial class ApartmentDetails : UserControl
    {
        private Request request;
        private Apartment apartment;

        public ApartmentDetails()
        {
            InitializeComponent();

            btnRefresh.Click += OnRefreshClicked;
            btnOpenMap.Click += OnOpenMapClicked;
            btnOpenSite.Click += OnOpenSiteClicked;
            btnFavorite.Click += OnFavoriteClicked;
        }

        public void SetApartment( Request request, Apartment apartment )
        {
            this.request = request;
            this.apartment = apartment;

            if ( apartment != null )
            {
                tbName.Text = apartment.Name;
                tbAddress.Text = apartment.Address;
            }
        }

        private void OnRefreshClicked( object sender, RoutedEventArgs e )
        {
            if ( request == null || apartment == null )
                return;

            var host = request.SearchUrl.Split( '/' ).Take( 3 ).Aggregate( "", ( acc, s ) => acc + s + "/" ).TrimEnd( '/' );
            var url = host + "/expose/" + apartment.Id;

            btnRefresh.IsEnabled = false;
            var dispatcher = Dispatcher.CurrentDispatcher;

            var thread = new Thread( () =>
            {
                Parser.ParseExpose( url, apartment );
                dispatcher.BeginInvoke( (Action)( () =>
               {
                   btnRefresh.IsEnabled = true;
                   SetApartment( request, apartment );
                   apartment.OnPropertyChanged();
               } ) );
            } );
            thread.Start();
        }

        private void OnOpenSiteClicked( object sender, RoutedEventArgs e )
        {
            if ( request == null || apartment == null )
                return;

            var host = request.SearchUrl.Split( '/' ).Take( 3 ).Aggregate( "", ( acc, s ) => acc + s + "/" ).TrimEnd( '/' );
            var url = host + "/expose/" + apartment.Id;
            Process.Start( url );
        }

        private void OnOpenMapClicked( object sender, RoutedEventArgs e )
        {
            if ( request == null || apartment == null )
                return;

            var q = apartment.Address.Replace( " ", "+" );
            Process.Start( $"https://www.google.de/maps/search/{q}" );
        }

        private void OnFavoriteClicked( object sender, RoutedEventArgs e )
        {
            if ( request == null || apartment == null )
                return;

            apartment.IsFavorite = !apartment.IsFavorite;
            apartment.OnPropertyChanged();
        }
    }
}
