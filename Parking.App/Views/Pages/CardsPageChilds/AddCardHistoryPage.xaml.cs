using Azure.Core;
using Parking.App.Models.Dto.Card;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Parking.App.Views.Pages.CardsPageChilds
{
    /// <summary>
    /// Interaction logic for AddCardHistoryPage.xaml
    /// </summary>
    public partial class AddCardHistoryPage : Page
    {
        private readonly ILogger<AddCardHistoryPage>? _logger;
        private readonly IParkingService? _parkingService;
        public AddCardHistoryPageViewModel ViewModel { get; set; }
        public AddCardHistoryPage()
        {
            _logger = App.GetService<ILogger<AddCardHistoryPage>>();
            _parkingService = App.GetService<IParkingService>();
            ViewModel = new AddCardHistoryPageViewModel();
            DataContext = ViewModel;
            ViewModel.Items = new ObservableCollection<AddCardItemModel>(_parkingService.SearchInCardHistory(null, null, null, null, null, null, null, 1, 10).Result);
            InitializeComponent();
            SetDefaultParameter();




        }
        private void SetDefaultParameter()
        {
            var yesterdayTime = DateTime.Now.AddDays(-90).ToShamsi().Split(" / ");
            StartYearTextBox.Text = yesterdayTime[0];
            StartMountTextBox.Text = yesterdayTime[1];
            StartDayTextBox.Text = yesterdayTime[2];

            var nowTime = DateTime.Now.ToShamsi().Split(" / ");
            EndYearTextBox.Text = nowTime[0];
            EndMountTextBox.Text = nowTime[1];
            EndDayTextBox.Text = nowTime[2];
        }

        private void Pagination_PageChanged(object sender, int newPage)
        {
            ViewModel.CurrentPage = newPage;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string plate = "";
            if (leftNumbersNumberTextBox.Text != null)
            {
                if (leftNumbersNumberTextBox.Text.Length > 0)
                {
                    plate += $"{leftNumbersNumberTextBox.Text}";
                }
            }
            var plateChar = plateCharacter.Text;
            if (plateChar?.Length > 0)
            {
                plate += $"_{plateChar?.ConvertFaCharToEnCharIndex()}";
                if (rightNumbersNumberTextBox.Text != null)
                {
                    plate += $"_{rightNumbersNumberTextBox.Text}";
                    if (irNumberTextBox.Text != null && irNumberTextBox.Text.Length == 2)
                        plate += $"_IR{irNumberTextBox.Text}";
                }
            }
            if (plate.Length > 1)
            {
                ViewModel.LicensePlate = plate;
            }
            

            ViewModel.Items = new ObservableCollection<AddCardItemModel>(_parkingService.SearchInCardHistory(ViewModel.FullName,(ViewModel.CardUidTextBox!=null && ViewModel.CardUidTextBox.Length>1) ?long.Parse(ViewModel.CardUidTextBox):null, ViewModel.LicensePlate, null,ViewModel.StartTime, ViewModel.EndTime, null, 1, 10).Result);
            HistoryDataGrid.ItemsSource = ViewModel.Items;
        }

        private void EndTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EndYearTextBox != null && EndMountTextBox != null && EndDayTextBox != null)
            {
                if (EndYearTextBox.Text != null && EndYearTextBox.Text.Length == 4)
                {
                    if (EndMountTextBox.Text != null && EndMountTextBox.Text.Length == 2)
                    {
                        if (EndDayTextBox.Text != null && EndDayTextBox.Text.Length == 2)
                        {

                            DateTime end = DateConvertor.ShamsiToDateTime(
                                int.Parse(EndYearTextBox.Text.ToString()),
                                int.Parse(EndMountTextBox.Text.ToString()),
                                int.Parse(EndDayTextBox.Text.ToString())
                                );
                            ViewModel.EndTime = end;
                        }
                    }
                }
            }

        }
        private void StartTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
           
            if (StartYearTextBox!=null && StartMountTextBox != null && StartDayTextBox != null)
            {
                if (StartYearTextBox.Text != null && StartYearTextBox.Text.Length == 4)
                {
                    if (StartMountTextBox.Text != null && StartMountTextBox.Text.Length == 2)
                    {
                        if (StartDayTextBox.Text != null && StartDayTextBox.Text.Length == 2)
                        {
                            DateTime start = DateConvertor.ShamsiToDateTime(
                                            int.Parse(StartYearTextBox.Text.ToString()),
                                            int.Parse(StartMountTextBox.Text.ToString()),
                                            int.Parse(StartDayTextBox.Text.ToString())
                                            );

                            ViewModel.StartTime = start;
                        }
                    }
                }
            }

        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Items = new ObservableCollection<AddCardItemModel>(_parkingService.SearchInCardHistory(null, null, null, null, null, null, null, 1, 10).Result);
            HistoryDataGrid.ItemsSource = ViewModel.Items;
            StartYearTextBox.Text = null;
            StartMountTextBox.Text = null;
            StartDayTextBox.Text = null;

            EndYearTextBox.Text = null;
            EndMountTextBox.Text = null;
            EndDayTextBox.Text = null;

            FullNameTextBox.Text = null;

            CardUidTextBox.Text = null;

            leftNumbersNumberTextBox.Text = null;
            plateCharacter.Text = null;
            rightNumbersNumberTextBox.Text = null;
            irNumberTextBox.Text = null;


        }
    }
}
