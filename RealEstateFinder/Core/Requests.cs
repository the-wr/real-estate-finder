using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RealEstateFinder.Core
{
    public class Request
    {
        public string Name { get; set; }
        public string SearchUrl { get; set; }
        public List<string> ExcludedRegions { get; set; }

        public Request()
        {
            ExcludedRegions = new List<string>();
        }
    }

    class RequestsModel
    {
        public List<Request> Requests { get; private set; }
        public Request SelectedRequest { get; set; }

        public RequestsModel()
        {
            Directory.CreateDirectory( "Data" );
            Load();
        }

        public void Save()
        {
            using ( var sw = new StreamWriter( "Data\\requests.xml" ) )
                new XmlSerializer( Requests.GetType() ).Serialize( sw, Requests );
        }

        private void Load()
        {
            try
            {
                using ( var sw = new StreamReader( "Data\\requests.xml" ) )
                    Requests = new XmlSerializer( typeof( List<Request> ) ).Deserialize( sw ) as List<Request>;

                SelectedRequest = Requests[0];
            }
            catch ( Exception )
            {
                Requests = new List<Request>();
                SelectedRequest = null;
            }
        }
    }
}
