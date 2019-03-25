using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RealEstateFinder.Core
{
    class ParseResult
    {
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string NextUrl { get; set; }
        public List<Apartment> Apartments { get; set; }
    }

    class Parser
    {
        private static string[] RENTED = { "vermietet", "kapital", "invest" };

        public static ParseResult ParseListPage( string url )
        {
            var host = url.Split( '/' ).Take( 3 ).Aggregate( "", ( acc, s ) => acc + s + "/" ).TrimEnd( '/' );
            var response = WebUtils.GetPage( url );
            if ( response == null )
            {
                return new ParseResult();
            }

            var lines = response.Split( '\n' );
            var json = lines.FirstOrDefault( l => l.Contains( "resultListModel:" ) );
            if ( json == null )
            {
                return new ParseResult();
            }

            json = json.Replace( "resultListModel:", "" ).TrimEnd( ',' );
            var responseObject = JsonConvert.DeserializeObject<SearchResponse>( json );

            var resultList = responseObject.searchResponseModel.resultList;
            var entries = resultList.resultlistEntries[0].resultlistEntry;

            var apartments = new List<Apartment>();
            foreach ( var entry in entries )
            {
                var apartment = EntryToApartment( entry );
                apartments.Add( apartment );

                entry.similarObjects?.ForEach( it => { it.similarObject.ForEach( ent => { apartments.Add( EntryToApartment( ent ) ); } ); } );
            }

            return new ParseResult()
            {
                TotalPages = resultList.paging.numberOfPages,
                CurrentPage = resultList.paging.pageNumber,
                NextUrl = resultList.paging.next != null ?
                          host + resultList.paging.next.href : null,
                Apartments = apartments
            };
        }

        public static bool ParseExpose( string url, Apartment apartment )
        {
            var response = WebUtils.GetPage( url );
            if ( response == null )
                return false;

            var yearStart1 = response.IndexOf( "is24qa-baujahr grid-item three-fifths" );
            if ( yearStart1 > 0 )
            {
                var yearStr = response.Substring( yearStart1 + 39, 4 );

                int year;
                if ( int.TryParse( yearStr, out year ) )
                {
                    apartment.Year = year;
                }
                else
                {
                    var yearStart = response.IndexOf( "&constructionYear=" );
                    if ( yearStart > 0 )
                    {
                        yearStr = response.Substring( yearStart + 18, 4 );
                        apartment.Year = int.Parse( yearStr );
                    }
                }
            }
            else
            {
                var yearStart = response.IndexOf( "&constructionYear=" );
                if ( yearStart > 0 )
                {
                    var yearStr = response.Substring( yearStart + 18, 4 );
                    apartment.Year = int.Parse( yearStr );
                }
            }

            var rentedStart = response.IndexOf( "\"obj_rented\"" );
            if ( rentedStart > 0 )
            {
                if ( response[rentedStart + 14] == 'y' )
                {
                    apartment.IsRented = true;
                }
            }

            var nameLower = apartment.Name.ToLower();
            if ( RENTED.Any( it => nameLower.Contains( it ) ) )
            {
                apartment.IsRented = true;
            }

            return true;
        }

        public static Apartment EntryToApartment( ResultListEntry entry )
        {
            return new Apartment()
            {
                Id = entry.realEstateId.ToString(),

                Name = entry.realEstate.title.Replace( "\n", "" ),
                Price = (int)entry.realEstate.price.value,
                Area = (int)entry.realEstate.livingSpace,
                Rooms = entry.realEstate.numberOfRooms,

                Address = entry.realEstate.address.description.text,
                Region = entry.realEstate.address.quarter,

                Provision = entry.realEstate.courtage?.hasCourtage?.ToUpper() == "YES" ? entry.realEstate.price.value * 0.0714f : 0f
            };
        }
    }
}
