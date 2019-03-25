using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RealEstateFinder.Core
{
    class Statistics
    {
        // Has to be singleton to be accessed from list items :(
        public static Statistics Instance = new Statistics();

        public List<Point> AvgM2PricePerYear = new List<Point>();
        public List<Point> AvgM2PricePerYearRented = new List<Point>();

        public Dictionary<string, float> PricePerRegion = new Dictionary<string, float>();
        public Dictionary<string, float> PricePerRegionRented = new Dictionary<string, float>();

        public List<Point> AvgM2PricePerArea = new List<Point>();
        public List<Point> AvgM2PricePerAreaRented = new List<Point>();

        private List<Apartment> list = new List<Apartment>();

        public void Refresh( RequestsModel requests, SearchResultsDatabase searchResultsDatabase, ApartmentsDatabase apartmentsDatabase )
        {
            var searchResults = searchResultsDatabase.SearchResults.FirstOrDefault( it => it.Name == requests.SelectedRequest.Name );

            if ( searchResults == null )
            {
                return;
            }

            list = new List<Apartment>();
            list.AddRange( searchResults.Ids.Select( id =>
            {
                Apartment apartment;
                if ( apartmentsDatabase.Apartments.TryGetValue( id, out apartment ) )
                    return apartment;
                return null;
            } ).Where( it => it != null ) );

            // -----

            // TODO: Very slow, optimize
            foreach ( var apartment in list )
            {
                if ( apartment.Year == null )
                    continue;

                var avgPerM2 = list
                    .Where( a => a.Year != null )
                    .Where( a => a.Region == apartment.Region && Math.Abs( a.Year.Value - apartment.Year.Value ) <= 10 )
                    .Average( a => a.PricePerM2 );

                if ( apartment.PriceComparedToAvegare != apartment.PricePerM2 / avgPerM2 )
                {
                    apartment.PriceComparedToAvegare = apartment.PricePerM2 / avgPerM2;
                    apartment.OnPropertyChanged();
                }
            }

            // -----

            AvgM2PricePerArea = list
                .Where( a => !a.IsRented )
                .Where( a => a.Area >= 10 && a.Area <= 150 )
                .GroupBy( a => a.Area / 5 )
                .OrderBy( g => g.Key )
                .Select( g => new Point( g.Key * 5, g.Average( a => a.PricePerM2 ) ) )
                .ToList();

            AvgM2PricePerAreaRented = list
                .Where( a => a.IsRented )
                .Where( a => a.Area >= 10 && a.Area <= 150 )
                .GroupBy( a => a.Area / 5 )
                .OrderBy( g => g.Key )
                .Select( g => new Point( g.Key * 5, g.Average( a => a.PricePerM2 ) ) )
                .ToList();

            PricePerRegion = list
                .Where( a => !a.IsRented )
                .Where( a => a.Area >= 10 )
                .GroupBy( a => a.Region )
                .ToDictionary( g => g.Key, gg => gg.Average( a => a.PricePerM2 ) );

            PricePerRegionRented = list
                .Where( a => a.IsRented )
                .Where( a => a.Area >= 10 )
                .GroupBy( a => a.Region )
                .ToDictionary( g => g.Key, gg => gg.Average( a => a.PricePerM2 ) );

            AvgM2PricePerYear = list
                .Where( a => !a.IsRented )
                .Where( a => a.Year != null && a.Year >= 1900 && a.Area >= 10 )
                .GroupBy( a => a.Year / 10 )
                .OrderBy( g => g.Key )
                .Select( g => new Point( g.Key.Value * 10, g.Average( i => i.PricePerM2 ) ) )
                .ToList();

            AvgM2PricePerYearRented = list
                .Where( a => a.IsRented )
                .Where( a => a.Year != null && a.Year >= 1900 && a.Area >= 10 )
                .GroupBy( a => a.Year / 10 )
                .OrderBy( g => g.Key )
                .Select( g => new Point( g.Key.Value * 10, g.Average( i => i.PricePerM2 ) ) )
                .ToList();
        }
    }
}
