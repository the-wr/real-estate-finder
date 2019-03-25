using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RealEstateFinder.Core;

namespace RealEstateFinder.UI
{
    /// <summary>
    /// Interaction logic for RegionFilterDialog.xaml
    /// </summary>
    public partial class RegionFilterDialog : Window
    {
        public RegionFilterDialog()
        {
            InitializeComponent();

            btnOk.Click += ( sender, obj ) =>
            {
                DialogResult = true;
                Close();
            };

            btnCancel.Click += ( sender, obj ) =>
            {
                DialogResult = false;
                Close();
            };
        }

        public void SetRequest( Request request, SearchResultsDatabase searchResultsDatabase, ApartmentsDatabase apartmentsDatabase )
        {
            var searchResults = searchResultsDatabase.SearchResults.FirstOrDefault( it => it.Name == request.Name );

            if ( searchResults == null )
            {
                return;
            }

            var list = new List<Apartment>();
            list.AddRange( searchResults.Ids.Select( id =>
            {
                Apartment apartment;
                if ( apartmentsDatabase.Apartments.TryGetValue( id, out apartment ) )
                    return apartment;
                return null;
            } ).Where( it => it != null ) );

            list.GroupBy( it => it.Region ).OrderBy( g => g.Key ).ToList().ForEach( it =>
            {
                var cb = new CheckBox() { Content = $"{it.Key} ({it.Count()})", IsChecked = !request.ExcludedRegions.Contains( it.Key ) };
                cb.Checked += delegate { request.ExcludedRegions.Remove( it.Key ); };
                cb.Unchecked += delegate { request.ExcludedRegions.Add( it.Key ); };
                lvItems.Items.Add( cb );
            }
            );
        }
    }
}
