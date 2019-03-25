using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RealEstateFinder.Core
{
    public class SearchResponse
    {
        public SearchResponseModel searchResponseModel { get; set; }
    }

    public class SearchResponseModel
    {
        [JsonProperty( PropertyName = "resultlist.resultlist" )]
        public ResultList resultList { get; set; }
    }

    public class ResultList
    {
        public Paging paging { get; set; }
        public List<ResultListEntries> resultlistEntries { get; set; }
    }

    public class Paging
    {
        public Page next { get; set; }
        public int pageNumber { get; set; }
        public int numberOfPages { get; set; }
    }

    public class Page
    {
        [JsonProperty( PropertyName = "@xlink.href" )]
        public String href { get; set; }
    }

    public class ResultListEntries
    {
        public List<ResultListEntry> resultlistEntry { get; set; }
    }

    public class ResultListEntry
    {
        public int realEstateId { get; set; }

        [JsonProperty( PropertyName = "resultlist.realEstate" )]
        public RealEstate realEstate { get; set; }

        public List<SimilarObjects> similarObjects { get; set; }
    }

    public class RealEstate
    {
        public string title { get; set; }
        public Address address { get; set; }
        public Price price { get; set; }
        public float livingSpace { get; set; }
        public float numberOfRooms { get; set; }
        public Courtage courtage { get; set; }
    }

    public class Address
    {
        public string quarter { get; set; }
        public Description description { get; set; }
    }

    public class Description
    {
        public string text { get; set; }
    }

    public class Price
    {
        public float value { get; set; }
        public string currency { get; set; }
    }

    public class Courtage
    {
        public string hasCourtage { get; set; }
    }

    public class SimilarObjects
    {
        [JsonConverter( typeof( SingleOrArrayConverter<ResultListEntry> ) )]
        public List<ResultListEntry> similarObject { get; set; }
    }

    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert( Type objectType )
        {
            return ( objectType == typeof( List<T> ) );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            JToken token = JToken.Load( reader );
            if ( token.Type == JTokenType.Array )
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            List<T> list = (List<T>)value;
            if ( list.Count == 1 )
            {
                value = list[0];
            }
            serializer.Serialize( writer, value );
        }

        public override bool CanWrite
        {
            get { return true; }
        }
    }
}
