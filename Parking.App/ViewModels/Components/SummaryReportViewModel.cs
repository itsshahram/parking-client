using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Parking.App.ViewModels.Components
{
    public class SummaryReportViewModel
    {
        private TicketSummaryReportModel? _report;

        public TicketSummaryReportModel? Report
        {
            get => _report;
            set
            {
                _report = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
