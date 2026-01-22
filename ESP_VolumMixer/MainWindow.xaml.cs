using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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




namespace ESP_VolumMixer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseManager dbManager;
        private CancellationTokenSource RunningCheckAudio;

        public MainWindow()
        {

            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitializeDatabase();
            RunningCheckAudio = new CancellationTokenSource();
            _ = CheckAudio(RunningCheckAudio.Token);
            AppdateComboBox();

        }

        private async Task CheckAudio(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000, token);

                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                foreach (var device in devices)
                {
                    var sessionManager = device.AudioSessionManager;
                    var sessions = sessionManager.Sessions;

                    for (int i = 0; i < sessions.Count; i++)
                    {
                        var session = sessions[i];
                        uint processId = session.GetProcessID;
                        var process = System.Diagnostics.Process.GetProcessById((int)processId);
                        string name = process.ProcessName + ".exe";

                        if (process != null)
                        {
                            var myProcess = new Process
                            {
                                NameProcess = name
                            };
                            dbManager.AddProcess(myProcess);
                            Console.WriteLine($"  Процесс : {name}");
                            AppdateComboBox();



                        }
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            RunningCheckAudio.Cancel();
            RunningCheckAudio.Dispose();
            base.OnClosed(e);
        }

        private void AppdateComboBox()
        {
            List<ComboBox> ListComboBox = new List<ComboBox> { EncodorProcess1, EncodorProcess2, EncodorProcess3, EncodorProcess4, PotenseonetrProcess1, PotenseonetrProcess2, PotenseonetrProcess3, PotenseonetrProcess4, PotenseonetrProcess5 };

            List<Process> processes = dbManager.GetAllProcesses();
            List<Profile> profiles = dbManager.GetAllProfiles();

            object selectedItemProfile = ComboboxProfile.SelectedItem;

            ComboboxProfile.ItemsSource = profiles;
            ComboboxProfile.DisplayMemberPath = "Name";
            ComboboxProfile.SelectedValuePath = "Id";
            if (selectedItemProfile is Profile oldProfile)
            {
                var sameProfile = profiles.FirstOrDefault(p => p.Id == oldProfile.Id);
                if (sameProfile != null)
                {
                    ComboboxProfile.SelectedItem = sameProfile;
                }
            }

            foreach (var ComboBoxNow in ListComboBox)
            {
                object selectedItem = ComboBoxNow.SelectedItem;
                
                ComboBoxNow.ItemsSource = processes;
                ComboBoxNow.DisplayMemberPath = "NameProcess";
                ComboBoxNow.SelectedValuePath = "Id";
                    
                if (selectedItem is Process oldProcess)
                {
                    var sameProcess = processes.FirstOrDefault(p => p.Id == oldProcess.Id);
                    if (sameProcess != null)
                    {
                        ComboBoxNow.SelectedItem = sameProcess;
                        
                    }
                }
            }
        }
    }
}
