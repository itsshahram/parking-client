using Parking.App.Models.Dto.Vehicle.VehicleSegment;


namespace Parking.App.Views.Components
{
    /// <summary>
    /// Interaction logic for VehicleSegmentPriceCom.xaml
    /// </summary>
    public partial class VehicleSegmentPriceCom : UserControl
    {
        public VehicleSegmentPriceCom()
        {
            InitializeComponent();
        }
        // Dependency Property برای تنظیم داده‌ها
        public VehicleSegmentPriceListItemModel DataModel
        {
            get { return (VehicleSegmentPriceListItemModel)GetValue(DataModelProperty); }
            set { SetValue(DataModelProperty, value); }
        }

        public static readonly DependencyProperty DataModelProperty =
            DependencyProperty.Register("DataModel", typeof(VehicleSegmentPriceListItemModel), typeof(VehicleSegmentPriceCom), new PropertyMetadata(null));
    }
}
