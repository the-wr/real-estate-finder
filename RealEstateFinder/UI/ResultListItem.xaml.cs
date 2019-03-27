using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using RealEstateFinder.Core;

namespace RealEstateFinder.UI
{
    /// <summary>
    /// Interaction logic for ResultListItem.xaml
    /// </summary>
    public partial class ResultListItem : UserControl
    {
        private Apartment data;

        public ResultListItem()
        {
            InitializeComponent();
        }

        public Apartment Apartment
        {
            get { return data; }
            set
            {
                if ( data != null )
                {
                    data.PropertyChanged -= OnPropertyChanged;
                }

                data = value;

                if ( data != null )
                {
                    data.PropertyChanged += OnPropertyChanged;
                }

                UpdateUI();
            }
        }

        private void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if ( data.IsFavorite )
            {
                rectStatus.Fill = Brushes.Gold;
            }
            else if ( data.IsNew )
            {
                rectStatus.Fill = Brushes.LightGreen;
            }
            else if ( data.IsHidden )
            {
                rectStatus.Fill = Brushes.IndianRed;
            }
            else
            {
                rectStatus.Fill = Brushes.Transparent;
            }

            tbRegion.Text = data.Region;
            //tbName.Text = data.Name;
            tbRooms.Text = data.Rooms.ToString();
            tbArea.Text = data.Area.ToString();
            tbPrice.Text = ( (int)( data.FullPrice / 1000 ) ).ToString();
            tbPricePerM2.Text = ( (int)data.PricePerM2 ).ToString();

            tbYear.Text = data.Year?.ToString() ?? "";
            tbIncome.Text = data.RentIncome?.ToString() ?? "";
            tbPriceToIncome.Text = data.PriceToIncomeRatio?.ToString() ?? "";
            tbRented.Text = data.IsRented ? "Rented" : "";
            tbPriceToAverage.Text = $"{data.PriceComparedToAvegare:0.00}";

            if ( data.FullPrice <= 100000 )
                tbPrice.Background = Grades.VeryGood;
            else if ( data.FullPrice <= 200000 )
                tbPrice.Background = Grades.Good;
            else if ( data.FullPrice <= 300000 )
                tbPrice.Background = Grades.Average;
            else if ( data.FullPrice <= 400000 )
                tbPrice.Background = Grades.Bad;
            else
                tbPrice.Background = Grades.VeryBad;

            if ( data.PriceComparedToAvegare <= 0.1 )
                tbPriceToAverage.Background = Brushes.Transparent;
            else if ( data.PriceComparedToAvegare <= 0.7 )
                tbPriceToAverage.Background = Grades.VeryGood;
            else if ( data.PriceComparedToAvegare <= 0.95 )
                tbPriceToAverage.Background = Grades.Good;
            else if ( data.PriceComparedToAvegare <= 1.05 )
                tbPriceToAverage.Background = Grades.Average;
            else if ( data.PriceComparedToAvegare <= 1.3 )
                tbPriceToAverage.Background = Grades.Bad;
            else
                tbPriceToAverage.Background = Grades.VeryBad;
        }

        // -----

        public static DependencyProperty ApartmentProperty =
             DependencyProperty.Register( "Apartment", typeof( Apartment ), typeof( ResultListItem ),
                 new FrameworkPropertyMetadata( default( Apartment ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ApartmentValuePropertyChangedCallback ) );

        private static void ApartmentValuePropertyChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( d as ResultListItem ).Apartment = e.NewValue as Apartment;
        }
    }
}
