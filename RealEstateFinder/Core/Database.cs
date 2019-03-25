using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace RealEstateFinder.Core
{
    public class SearchResult
    {
        public string Name { get; set; }
        public List<string> Ids { get; set; }
    }

    public class SearchResultsDatabase
    {
        public List<SearchResult> SearchResults { get; private set; }

        public SearchResultsDatabase()
        {
            Load();
        }

        public void Save()
        {
            using ( var sw = new StreamWriter( "Data\\searchResults.xml" ) )
                new XmlSerializer( SearchResults.GetType() ).Serialize( sw, SearchResults );
        }

        private void Load()
        {
            try
            {
                using ( var sw = new StreamReader( "Data\\searchResults.xml" ) )
                    SearchResults = new XmlSerializer( typeof( List<SearchResult> ) ).Deserialize( sw ) as List<SearchResult>;
            }
            catch ( Exception )
            {
                SearchResults = new List<SearchResult>();
            }
        }
    }

    public class ApartmentsDatabase
    {
        public Dictionary<string, Apartment> Apartments { get; set; }

        public ApartmentsDatabase()
        {
            Load();
        }

        public void Save()
        {
            var apartments = Apartments.Values.ToList();

            using ( var sw = new StreamWriter( "Data\\apartments.xml" ) )
                new XmlSerializer( apartments.GetType() ).Serialize( sw, apartments );
        }

        private void Load()
        {
            try
            {
                using ( var sw = new StreamReader( "Data\\apartments.xml" ) )
                {
                    var apartments = new XmlSerializer( typeof (List<Apartment>) ).Deserialize( sw ) as List<Apartment>;

                    Apartments = apartments.ToDictionary( a => a.Id );
                }
            }
            catch ( Exception )
            {
                Apartments = new Dictionary<string, Apartment>();
            }
        }
    }
}
