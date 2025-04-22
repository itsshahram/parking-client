

namespace Parking.App.Views.Components
{
    /// <summary>
    /// Interaction logic for SeizedPlateItemComponent.xaml
    /// </summary>
    public partial class SeizedPlateItemComponent : UserControl
    {
        public SeizedPlateItemComponent()
        {
            InitializeComponent();
        }
        public SeizedLicensePlateModel DataModel
        {
            get { return (SeizedLicensePlateModel)GetValue(DataModelProperty); }
            set { SetValue(DataModelProperty, value); }
        }

        public static readonly DependencyProperty DataModelProperty =
            DependencyProperty.Register("DataModel", typeof(SeizedLicensePlateModel), typeof(SeizedPlateItemComponent), new PropertyMetadata(null));


    }
}
