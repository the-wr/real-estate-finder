using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace RealEstateFinder.Core
{
    class Scanner
    {
        public event Action<int, int, string> ProgressChanged;
        public event Action ScanCompleted;

        public event Action<List<Apartment>> ListRetrieved;
        public event Action<Apartment> ApartmentDataRetrieved;

        private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

        public void StartScan( Request request )
        {
            var thread = new Thread( () =>
            {
                var apartments = new List<Apartment>();

                var parseResult = new ParseResult() { CurrentPage = 0, TotalPages = 1, NextUrl = request.SearchUrl };

                while ( parseResult.CurrentPage != parseResult.TotalPages )
                {
                    ReportProgress( parseResult.CurrentPage, parseResult.TotalPages, $"Fetching list page {parseResult.CurrentPage}/{parseResult.TotalPages}..." );
                    parseResult = Parser.ParseListPage( parseResult.NextUrl );

                    if ( parseResult.Apartments != null )
                    {
                        apartments.AddRange( parseResult.Apartments );
                    }
                }

                dispatcher.BeginInvoke( (Action)( () => { ListRetrieved?.Invoke( apartments ); } ) );
            } );
            thread.Start();
        }

        public void StartParseExposes( Request request, List<Apartment> apartments )
        {
            var thread = new Thread( () =>
            {
                var host = request.SearchUrl.Split( '/' ).Take( 3 ).Aggregate( "", ( acc, s ) => acc + s + "/" ).TrimEnd( '/' );

                var visited = new HashSet<string>();
                int count = 0;
                foreach ( var apartment in apartments )
                {
                    count++;
                    ReportProgress( count, apartments.Count, $"Parsing expose {count}/{apartments.Count}..." );
                    if ( visited.Contains( apartment.Id ) )
                        continue;

                    visited.Add( apartment.Id );
                    var url = host + "/expose/" + apartment.Id;

                    if ( Parser.ParseExpose( url, apartment ) )
                    {
                        dispatcher.BeginInvoke( (Action)( () => { apartment.OnPropertyChanged(); } ) );
                    }
                }

                dispatcher.BeginInvoke( (Action)( () => { ScanCompleted?.Invoke(); } ) );
            } );
            thread.Start();
        }

        private void ReportProgress( int current, int total, string messafe )
        {
            dispatcher.BeginInvoke( (Action)( () => { ProgressChanged?.Invoke( current, total, messafe ); } ) );
        }
    }
}
