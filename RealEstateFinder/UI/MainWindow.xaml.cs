using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RealEstateFinder.Core;

namespace RealEstateFinder.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RequestsModel requests;
        private Scanner scanner = new Scanner();

        private SearchResultsDatabase searchResultsDatabase;
        private ApartmentsDatabase apartmentsDatabase;
        private ResultListModel resultListModel = new ResultListModel();

        private bool muteRequestChanged = false;

        public MainWindow()
        {
            InitializeComponent();

            requests = new RequestsModel();
            searchResultsDatabase = new SearchResultsDatabase();
            apartmentsDatabase = new ApartmentsDatabase();

            scanner.ScanCompleted += OnScanCompleted;
            scanner.ProgressChanged += OnScanProgressChanged;
            scanner.ListRetrieved += OnScanerListRetrieved;

            UpdateRequestList();

            cbRequest.SelectionChanged += OnRequestSelectionChanged;
            btnAddNewRequest.Click += OnButtonNewRequestClicked;
            btnScan.Click += OnButtonScanClicked;

            lvItems.DataContext = resultListModel;
            lvItems.SelectionChanged += OnResultListSelectionChanged;

            header.btnPriceToAverage.Click += OnHeaderSortByValueClicked;
            header.btnPrice.Click += OnHeaderSortByPriceClicked;
            header.btnPricePerM2.Click += OnHeaderSortByPricePerM2Clicked;
            header.btnRegion.Click += OnHeaderSortByRegionClicked;
            header.btnFilterRegion.Click += OnHeaderFilterRegionClicked;
        }

        // -----

        private void OnRequestSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if ( muteRequestChanged )
                return;

            requests.SelectedRequest =
                requests.Requests.FirstOrDefault( it => it.Name == cbRequest.SelectedItem.ToString() );

            PopulateResultList();
            UpdateGraphs();
        }

        private void OnButtonNewRequestClicked( object sender, RoutedEventArgs e )
        {
            var dialog = new NewRequestDialog() { Owner = this };
            if ( dialog.ShowDialog() == true &&
                !string.IsNullOrWhiteSpace( dialog.tbName.Text ) &&
                !string.IsNullOrWhiteSpace( dialog.tbRequest.Text ) )
            {
                requests.Requests.Add( new Request() { Name = dialog.tbName.Text, SearchUrl = dialog.tbRequest.Text } );
                requests.SelectedRequest = requests.Requests.Last();

                requests.Save();
                UpdateRequestList();
            }
        }

        private void OnButtonScanClicked( object sender, RoutedEventArgs e )
        {
            if ( requests.SelectedRequest == null )
                return;

            foreach ( var apartment in apartmentsDatabase.Apartments )
            {
                apartment.Value.IsNew = false;
            }

            btnScan.IsEnabled = false;
            scanner.StartScan( requests.SelectedRequest );
        }

        private void OnResultListSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            apartmentDetails.SetApartment( requests.SelectedRequest, lvItems.SelectedItem as Apartment );
        }

        private void OnHeaderSortByValueClicked( object sender, RoutedEventArgs e )
        {
            resultListModel.SortBy = Sort.VALUE;
            PopulateResultList();
        }

        private void OnHeaderSortByRegionClicked( object sender, RoutedEventArgs e )
        {
            resultListModel.SortBy = Sort.REGION;
            PopulateResultList();
        }

        private void OnHeaderSortByPricePerM2Clicked( object sender, RoutedEventArgs e )
        {
            resultListModel.SortBy = Sort.PRICE_M2;
            PopulateResultList();
        }

        private void OnHeaderSortByPriceClicked( object sender, RoutedEventArgs e )
        {
            resultListModel.SortBy = Sort.PRICE;
            PopulateResultList();
        }

        private void OnHeaderFilterRegionClicked( object sender, RoutedEventArgs e )
        {
            var dialog = new RegionFilterDialog { Owner = this };
            dialog.SetRequest( requests.SelectedRequest, searchResultsDatabase, apartmentsDatabase );
            if ( dialog.ShowDialog() == true )
            {
                requests.Save();
                PopulateResultList();
            }
        }

        // -----

        private void OnScanCompleted()
        {
            OnScanProgressChanged( 1, 1, "Done." );
            btnScan.IsEnabled = true;

            searchResultsDatabase.Save();
            apartmentsDatabase.Save();

            UpdateGraphs();
        }

        private void OnScanProgressChanged( int arg1, int arg2, string arg3 )
        {
            progressScan.Maximum = arg2;
            progressScan.Value = arg1;
            tbScanStatus.Text = arg3;
        }

        private void OnScanerListRetrieved( List<Apartment> apartments )
        {
            var searchResults = searchResultsDatabase.SearchResults.FirstOrDefault( it => it.Name == requests.SelectedRequest.Name );
            if ( searchResults == null )
            {
                searchResults = new SearchResult { Name = requests.SelectedRequest.Name, Ids = new List<string>() };
                searchResultsDatabase.SearchResults.Add( searchResults );
            }

            var oldResults = new HashSet<string>( searchResults.Ids );
            var added = 0;

            var toScan = new List<Apartment>();

            apartments.ForEach( it =>
            {
                if ( !oldResults.Contains( it.Id ) )
                {
                    searchResults.Ids.Add( it.Id );
                    added++;

                    if ( oldResults.Count != 0 )
                    {
                        it.IsNew = true;    // Only mark if the initial list was non-empty
                    }
                }

                if ( !apartmentsDatabase.Apartments.ContainsKey( it.Id ) )
                {
                    apartmentsDatabase.Apartments[it.Id] = it;
                    toScan.Add( it );
                }
                else
                {
                    apartmentsDatabase.Apartments[it.Id].Price = it.Price;
                }
            } );

            searchResultsDatabase.Save();
            apartmentsDatabase.Save();

            PopulateResultList();
            UpdateGraphs();

            scanner.StartParseExposes( requests.SelectedRequest, toScan );
        }

        // -----

        private void UpdateRequestList()
        {
            muteRequestChanged = true;
            cbRequest.Items.Clear();

            requests.Requests.ForEach( r => { cbRequest.Items.Add( r.Name ); } );

            if ( requests.SelectedRequest != null )
            {
                cbRequest.SelectedItem = requests.SelectedRequest.Name;
            }

            muteRequestChanged = false;

            PopulateResultList();
            UpdateGraphs();
        }

        private void PopulateResultList()
        {
            resultListModel.Refresh( requests, searchResultsDatabase, apartmentsDatabase );
        }

        private void UpdateGraphs()
        {
            Statistics.Instance.Refresh( requests, searchResultsDatabase, apartmentsDatabase );

            var plotModel = new PlotModel { Title = "m2 price per total area" };
            var series = new LineSeries() { Title = "Not rented" };
            var seriesRented = new LineSeries() {Title = "Rented"};
            Statistics.Instance.AvgM2PricePerArea.ForEach( p => series.Points.Add( new DataPoint( p.X, p.Y ) ) );
            Statistics.Instance.AvgM2PricePerAreaRented.ForEach( p => seriesRented.Points.Add( new DataPoint( p.X, p.Y ) ) );
            plotModel.Series.Add( series );
            plotModel.Series.Add( seriesRented );
            plotModel.Axes.Add( new LinearAxis() { Minimum = 0 } );
            plot1.Model = plotModel;

            var pm2 = new PlotModel { Title = "m2 price per region" };
            var s2 = new BarSeries() { LabelFormatString = "{0:0}" };
            var s2r = new BarSeries() { LabelFormatString = "{0:0}" };
            var list2 = new List<BarItem>();
            var list2r = new List<BarItem>();
            var axes2 = new List<string>();
            Statistics.Instance.PricePerRegion.ToList()
                .OrderBy( kvp => kvp.Value )
                .ToList()
                .ForEach( p =>
                {
                    list2.Add( new BarItem(p.Value) );
                    axes2.Add( p.Key.Substring( 0, Math.Min( 20, p.Key.Length ) ) );
                    float rented;
                    if ( Statistics.Instance.PricePerRegionRented.TryGetValue( p.Key, out rented ) )
                    {
                        list2r.Add( new BarItem( rented ) );
                    }
                    else
                    {
                        list2r.Add( new BarItem( 0 ) );
                    }
                } );
            s2.ItemsSource = list2;
            s2r.ItemsSource = list2r;
            pm2.Series.Add( s2 );
            pm2.Series.Add( s2r );
            pm2.Axes.Add( new CategoryAxis() { Position = AxisPosition.Left, ItemsSource = axes2} );
            plot2.Model = pm2;
            plot2.Height = 100 + list2.Count * 40;

            var pm3 = new PlotModel { Title = "m2 price per year" };
            var s3 = new LineSeries() { Title = "Not rented"};
            var s3r = new LineSeries() { Title = "Rented" };
            Statistics.Instance.AvgM2PricePerYear.ForEach( p => s3.Points.Add( new DataPoint( p.X, p.Y ) ) );
            Statistics.Instance.AvgM2PricePerYearRented.ForEach( p => s3r.Points.Add( new DataPoint( p.X, p.Y ) ) );
            pm3.Series.Add( s3 );
            pm3.Series.Add( s3r );
            pm3.Axes.Add( new LinearAxis() { Minimum = 0 } );
            plot3.Model = pm3;
        }
    }
}
