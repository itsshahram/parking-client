using Parking.Domain.General;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Parking.App.ViewModels.Windows
{
    public class AddLicensePlateViewModel : INotifyPropertyChanged
    {
        private readonly PersianCalendar _pc = new PersianCalendar();

        public AddLicensePlateViewModel()
        {
            var now = DateTime.Now;

            StartYear = _pc.GetYear(now).ToString();
            StartMonth = _pc.GetMonth(now).ToString("00");
            StartDay = _pc.GetDayOfMonth(now).ToString("00");
            StartHour = now.Hour.ToString("00");
            StartMinute = now.Minute.ToString("00");

            SetEndDate(now.AddMonths(1));
        }

        private void SetEndDate(DateTime date)
        {
            EndYear = _pc.GetYear(date).ToString();
            EndMonth = _pc.GetMonth(date).ToString("00");
            EndDay = _pc.GetDayOfMonth(date).ToString("00");
            EndHour = date.Hour.ToString("00");
            EndMinute = date.Minute.ToString("00");
            EndDate = date;
        }

        private DateTime _startDate = DateTime.Now;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                    UpdateEndDateBasedOnGroupDate();
                }
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }

        public string StartYear { get; set; }
        public string StartMonth { get; set; }
        public string StartDay { get; set; }
        public string StartHour { get; set; }
        public string StartMinute { get; set; }

        public string EndYear { get; set; }
        public string EndMonth { get; set; }
        public string EndDay { get; set; }
        public string EndHour { get; set; }
        public string EndMinute { get; set; }

        private string? _name;
        public string? Name { get => _name; set => SetField(ref _name, value); }

        private string? _description;
        public string? Description { get => _description; set => SetField(ref _description, value); }

        private short _discountPercent;
        public short DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                SetField(ref _discountPercent, value);
            }
        }

        private LicensePlateGroupDate _groupDate;
        public LicensePlateGroupDate GroupDate
        {
            get => _groupDate;
            set
            {
                if (SetField(ref _groupDate, value))
                {
                    IsOptionalSelected = value == LicensePlateGroupDate.Optional;
                    UpdateEndDateBasedOnGroupDate();
                }
            }
        }

        private bool _isOptionalSelected;
        public bool IsOptionalSelected
        {
            get => _isOptionalSelected;
            set => SetField(ref _isOptionalSelected, value);
        }

        private void UpdateEndDateBasedOnGroupDate()
        {
            if (GroupDate == LicensePlateGroupDate.Optional)
            {
                EndDate = null;
                return;
            }

            int monthsToAdd = GroupDate switch
            {
                LicensePlateGroupDate.OneMonth => 1,
                LicensePlateGroupDate.ThreeMonth => 3,
                LicensePlateGroupDate.SixMonth => 6,
                _ => 1
            };

            SetEndDate(StartDate.AddMonths(monthsToAdd));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
