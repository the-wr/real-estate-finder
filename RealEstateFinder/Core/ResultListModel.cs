using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents.DocumentStructures;

namespace RealEstateFinder.Core
{
    enum Sort
    {
        NONE,
        REGION,
        PRICE,
        PRICE_M2,
        VALUE
    }

    class ResultListModel
    {
        public ObservableCollection<Apartment> Apartments { get; private set; }

        public Sort SortBy { get; set; }

        public ResultListModel()
        {
            SortBy = Sort.NONE;
            Apartments = new ObservableCollection<Apartment>();
        }

        public void Refresh( RequestsModel requests, SearchResultsDatabase searchResultsDatabase, ApartmentsDatabase apartmentsDatabase )
        {
            Apartments.Clear();

            var searchResults =
                searchResultsDatabase.SearchResults.FirstOrDefault( it => it.Name == requests.SelectedRequest.Name );

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
            } ).Where( it => it != null ).Where( it => !requests.SelectedRequest.ExcludedRegions.Contains( it.Region ) ) );

            if ( SortBy == Sort.VALUE )
            {
                list = list.OrderBy( a => a.PriceComparedToAvegare ).ThenBy( a => a.Region ).ThenBy( a => a.IsRented ).ThenBy( a => a.FullPrice ).ToList();
            }
            else if ( SortBy == Sort.PRICE )
            {
                list.Sort( ( i1, i2 ) => i1.FullPrice.CompareTo( i2.FullPrice ) );
            }
            else if ( SortBy == Sort.PRICE_M2 )
            {
                list.Sort( ( i1, i2 ) => i1.PricePerM2.CompareTo( i2.PricePerM2 ) );
            }
            else if ( SortBy == Sort.REGION )
            {
                list = list.OrderBy( a => a.Region ).ThenBy( a => a.FullPrice ).ToList();
            }

            list.ForEach( Apartments.Add );
        }
    }
}
