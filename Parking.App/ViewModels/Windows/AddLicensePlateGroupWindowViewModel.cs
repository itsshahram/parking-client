using Parking.Domain.General;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Parking.App.ViewModels.Windows
{
    public class AddLicensePlateGroupViewModel : INotifyPropertyChanged
    {
        private readonly PersianCalendar _pc = new PersianCalendar();

        public AddLicensePlateGroupViewModel()
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

        private string? _endDateString;
        public string? EndDateString
        {
            get => _endDateString;
            set { _endDateString = value; OnPropertyChanged(nameof(EndDateString)); }
        }
        public string StartYear { get; set; }
        public string StartMonth { get; set; }
        public string StartDay { get; set; }
        public string StartHour { get; set; }
        public string StartMinute { get; set; }

        private string? _endYear;
        public string? EndYear
        {
            get => _endYear;
            set { SetField(ref _endYear, value); UpdateEndDateFromFields(); }
        }

        private string? _endMonth;
        public string? EndMonth
        {
            get => _endMonth;
            set { SetField(ref _endMonth, value); UpdateEndDateFromFields(); }
        }

        private string? _endDay;
        public string? EndDay
        {
            get => _endDay;
            set { SetField(ref _endDay, value); UpdateEndDateFromFields(); }
        }

        private string? _endHour;
        public string? EndHour
        {
            get => _endHour;
            set { SetField(ref _endHour, value); UpdateEndDateFromFields(); }
        }

        private string? _endMinute;
        public string? EndMinute
        {
            get => _endMinute;
            set { SetField(ref _endMinute, value); UpdateEndDateFromFields(); }
        }

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

        private void UpdateEndDateFromFields()
        {
            if (int.TryParse(EndYear, out int year) &&
                int.TryParse(EndMonth, out int month) &&
                int.TryParse(EndDay, out int day) &&
                int.TryParse(EndHour, out int hour) &&
                int.TryParse(EndMinute, out int minute))
            {
                try
                {
                    EndDate = _pc.ToDateTime(year, month, day, hour, minute, 0, 0);
                    EndDateString = EndDate?.ToShamsi();
                }
                catch
                {
                    EndDate = null;
                    EndDateString = null;
                }
            }
            else
            {
                EndDate = null;
                EndDateString = null;
            }
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
        public bool Validate(out string errorMessage)
        {
            // Name validation
            if (string.IsNullOrWhiteSpace(Name))
            {
                errorMessage = "نام گروه نمی‌تواند خالی باشد.";
                return false;
            }

            // Discount validation
            if (DiscountPercent < 0 || DiscountPercent > 100)
            {
                errorMessage = "درصد تخفیف باید بین 0 تا 100 باشد.";
                return false;
            }

            // Start date validation
            if (!IsValidDate(StartYear, StartMonth, StartDay, StartHour, StartMinute))
            {
                errorMessage = "تاریخ شروع نامعتبر است.";
                return false;
            }

            // End date validation if optional is selected
            if (IsOptionalSelected)
            {
                if (!IsValidDate(EndYear, EndMonth, EndDay, EndHour, EndMinute))
                {
                    errorMessage = "تاریخ پایان اختیاری نامعتبر است.";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool IsValidDate(string? yearStr, string? monthStr, string? dayStr, string? hourStr, string? minuteStr)
        {
            if (!int.TryParse(yearStr, out int year)) return false;
            if (!int.TryParse(monthStr, out int month)) return false;
            if (!int.TryParse(dayStr, out int day)) return false;
            if (!int.TryParse(hourStr, out int hour)) return false;
            if (!int.TryParse(minuteStr, out int minute)) return false;

            if (month < 1 || month > 12) return false;
            if (day < 1 || day > GetMaxDayOfShamsiMonth(year, month)) return false;
            if (hour < 0 || hour > 23) return false;
            if (minute < 0 || minute > 59) return false;

            return true;
        }

        private int GetMaxDayOfShamsiMonth(int year, int month)
        {
            if (month <= 6) return 31;
            if (month <= 11) return 30;
            return _pc.IsLeapYear(year) ? 30 : 29;
        }


        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
