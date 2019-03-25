using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RealEstateFinder.Annotations;

namespace RealEstateFinder.Core
{
    public class Apartment: INotifyPropertyChanged
    {
        // ----- Extracted

        public string Id { get; set; }

        public string Name { get; set; }
        public int Price { get; set; }
        public int Area { get; set; }
        public float Rooms { get; set; }

        public int? Year { get; set; }
        public double? Energy { get; set; }
        public float? Provision { get; set; }
        public float? Hausgeld { get; set; }
        public float? RentIncome { get; set; }
        public bool IsRented { get; set; }

        public string Region { get; set; }
        public string Address { get; set; }

        public List<string> Images { get; set; }

        // ----- Calculated

        public float FullPrice => Price + ( Provision ?? 0 );
        public float PricePerM2 => FullPrice / ( Area > 0 ? Area : 1 );

        public float? PriceToIncomeRatio => FullPrice / RentIncome;

        // ----- User data

        public bool IsNew { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsHidden { get; set; }
        public float PriceComparedToAvegare { get; set; }

        public Apartment()
        {
            Images = new List<string>();
            Region = string.Empty;
            Address = string.Empty;
        }

        // -----

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
