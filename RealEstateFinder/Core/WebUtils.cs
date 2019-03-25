using System.IO;
using System.Net;
using System.Text;

namespace RealEstateFinder.Core
{
    class WebUtils
    {
        public static string GetPage( string url )
        {
            try
            {
                var request = WebRequest.Create( url ) as HttpWebRequest;
                request.Method = WebRequestMethods.Http.Get;
                request.Headers.Add( "Accept-Language", "en-US" );
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Win32)";

                Stream responseStream = request.GetResponse().GetResponseStream();

                if ( responseStream == null )
                    return string.Empty;
                StreamReader reader = new StreamReader( responseStream, Encoding.UTF8 );

                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }
    }
}
