

using Parking.App.Models.Config;

namespace Parking.App.Views.Pages.SettingsPageChilds
{
    /// <summary>
    /// Interaction logic for CameraSettingsPage.xaml
    /// </summary>
    public partial class CameraSettingsPage : Page
    {
        public ObservableCollection<CameraConfigModel> Cameras { get; set; } = new ObservableCollection<CameraConfigModel>();


        public CameraSettingsPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadCameras();

            //if (Settings.Default.Camera_ExitCameraEnable is true)
            //{
            //    exitCamConfigBox.Visibility = Visibility.Visible;
            //}
            if (Settings.Default.Camera_MainCameraEnable is true)
            {
                entranceCameraConfig.Visibility = Visibility.Visible;
                aNPRBox.Visibility = Visibility.Visible;
            }
            LoadData();

            this.Unloaded += Page_Unloaded;
        }
        private void LoadCameras()
        {
            Cameras.Clear();
            foreach (var cam in CameraConfigManager.GetAllCameras())
            {
                Cameras.Add(cam);
            }
        }

        private void AddCamera_Click(object sender, RoutedEventArgs e)
        {
            var newCam = new CameraConfigModel { Name = "New Camera", RTSPUrl = "", IsActive = true };
            Cameras.Add(newCam);
            CameraConfigManager.AddCamera(newCam);
        }

        private void EditCamera_Click(object sender, RoutedEventArgs e)
        {
            if (CameraDataGrid.SelectedItem is CameraConfigModel selectedCamera)
            {
                string oldName = selectedCamera.Name; 

                CameraConfigModel updatedCamera = new CameraConfigModel
                {
                    Name = selectedCamera.Name,  
                    Description = selectedCamera.Description,
                    RTSPUrl = selectedCamera.RTSPUrl,
                    SnapshotUrl = selectedCamera.SnapshotUrl,
                    Username = selectedCamera.Username,
                    Password = selectedCamera.Password,
                    IsActive = selectedCamera.IsActive
                };


                CameraConfigManager.EditCamera(oldName, updatedCamera);
            }
        }


        private void DeleteCamera_Click(object sender, RoutedEventArgs e)
        {
            if (CameraDataGrid.SelectedItem is CameraConfigModel selectedCamera)
            {
                Cameras.Remove(selectedCamera);
                CameraConfigManager.RemoveCamera(selectedCamera.Name);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void Economy_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void Exit_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void Entrance_Config_TextChanged(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void ANPR_Config_Changed(object sender, TextChangedEventArgs e)
        {
            Settings.Default.Save();
        }
        private void ExitCameraToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            //var toggle = sender as ToggleSwitch;
            //if (toggle.IsChecked is true)
            //{
            //    exitCamConfigBox.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    exitCamConfigBox.Visibility = Visibility.Collapsed;
            //}

            Settings.Default.Save();
        }

        private void EnteranceCameraToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleSwitch;
            if (toggle.IsChecked is true)
            {
                entranceCameraConfig.Visibility = Visibility.Visible;
                aNPRBox.Visibility = Visibility.Visible;
            }
            else
            {
                entranceCameraConfig.Visibility = Visibility.Collapsed;
                aNPRBox.Visibility = Visibility.Collapsed;
            }

            Settings.Default.Save();
        }

        private void plateTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Settings.Default.Camera_ANPR_PlateType = (byte)comboBox.SelectedIndex;
                Settings.Default.Save();
            }
        }

        //private void ExitCamConfigBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    Settings.Default.Camera_ExitCameraPass = ExitCamPassTextBox.Text;
        //    Settings.Default.Camera_ExitCameraUser = ExitCamUserTextBox.Text;
        //    Settings.Default.Camera_ExitCameraUrl = ExitCamUrlTextBox.Text;
        //    Settings.Default.Save();
        //}

        private void EntranceCamConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Camera_MainCameraPass = MainCameraPassTextBox.Text;
            Settings.Default.Camera_MainCameraUser = MainCameraUserTextBox.Text;
            Settings.Default.Camera_MainCameraUrl = MainCameraUrlTextBox.Text;
            Settings.Default.Save();
        }

        private void ANPR_CamConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Camera_ANPR_FrameSkip = byte.Parse(ANPR_FrameSkipTextBox.Text);
            Settings.Default.Camera_ANPR_VlcCache = byte.Parse(ANPR_VlcCacheTextBox.Text);
            Settings.Default.Camera_ANPR_LightParameter = byte.Parse(ANPR_LightTextBox.Text);
            Settings.Default.Camera_ANPR_PlateCountInBuffer = byte.Parse(ANPR_PlateCountBox.Text);
            Settings.Default.Camera_ANPR_Cnf = int.Parse(ANPR_Cnf.Text);
            Settings.Default.Save();

        }

        private void Reset_Btn_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Camera_ANPR_FrameSkip = 5;
            Settings.Default.Camera_ANPR_VlcCache = 1000;
            Settings.Default.Camera_ANPR_LightParameter = 15;
            Settings.Default.Camera_ANPR_PlateCountInBuffer = 5;
            Settings.Default.Camera_ANPR_Cnf = 90;
            Settings.Default.Save();
        }
        private void LoadData()
        {
            //ExitCamPassTextBox.Text = Settings.Default.Camera_ExitCameraPass;
            //ExitCamUserTextBox.Text = Settings.Default.Camera_ExitCameraUser;
            //ExitCamUrlTextBox.Text = Settings.Default.Camera_ExitCameraUrl;
            //MainCameraPassTextBox.Text = Settings.Default.Camera_MainCameraPass;
            //MainCameraUserTextBox.Text = Settings.Default.Camera_MainCameraUser;
            //MainCameraUrlTextBox.Text = Settings.Default.Camera_MainCameraUrl;
            //ANPR_FrameSkipTextBox.Text = Settings.Default.Camera_ANPR_FrameSkip.ToString();
            //ANPR_VlcCacheTextBox.Text = Settings.Default.Camera_ANPR_VlcCache.ToString();
            //ANPR_LightTextBox.Text = Settings.Default.Camera_ANPR_LightParameter.ToString();
            //ANPR_PlateCountBox.Text = Settings.Default.Camera_ANPR_PlateCountInBuffer.ToString();
            //ANPR_Cnf.Text = Settings.Default.Camera_ANPR_Cnf.ToString();

        }
    }
}
