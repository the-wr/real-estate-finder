using System.Windows;

namespace RealEstateFinder.UI
{
    /// <summary>
    /// Interaction logic for NewRequestDialog.xaml
    /// </summary>
    public partial class NewRequestDialog : Window
    {
        public NewRequestDialog()
        {
            InitializeComponent();

            Loaded += ( sender, args ) => { tbName.Focus(); };

            btnOk.Click += ( sender, obj ) =>
            {
                DialogResult = true;
                Close();
            };

            btnCancel.Click += ( sender, obj ) =>
            {
                DialogResult = false;
                Close();
            };
        }
    }
}
