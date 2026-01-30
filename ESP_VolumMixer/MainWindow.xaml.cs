using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
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
using System.Xml.Linq;




namespace ESP_VolumMixer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseManager dbManager;
        private CancellationTokenSource RunningCheckAudio;
        private CancellationTokenSource RunningConnektedPort;

        public MainWindow()
        {

            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitializeDatabase();
            RunningCheckAudio = new CancellationTokenSource();
            _ = CheckAudio(RunningCheckAudio.Token);
            Task.Run(() => ConnektedPort());
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
                            //Console.WriteLine($"  Процесс : {name}");
                            AppdateComboBox();
                        }
                    }
                }
            }
        }

        String portValidate = "COM1";

        private async Task ConnektedPort()
        {
            while (true)
            {
                try
                {
                    using (SerialPort port = new SerialPort(portValidate, 9600))
                    {
                        port.ReadTimeout = 600;
                        port.Open();
                        string data = port.ReadLine();
                        Console.WriteLine(port.ReadLine());
                    }
                }
                catch
                {
                    portValidate = PortSearch();
                }
            }
        }

        String PortSearch()
        {
            Console.WriteLine("Поиск порта:");
            foreach (string portName in SerialPort.GetPortNames())
            {
                try
                {
                    Console.WriteLine(portName);
                    using (SerialPort port = new SerialPort(portName, 9600))
                    {
                        port.ReadTimeout = 600;
                        port.Open();
                        string data = port.ReadLine();
                        port.Close();
                        if (data.Substring(0, 11) == "VolumMixer-")
                        {
                            return portName;
                        }
                    }
                }
                catch
                { Console.WriteLine("Не тот"); }
            }
            Console.WriteLine("Не найден нужный порт");
            return "COM1";
        }

        protected override void OnClosed(EventArgs e)
        {
            RunningCheckAudio.Cancel();
            RunningCheckAudio.Dispose();
            base.OnClosed(e);
        }



        private void AppdateComboBox()
        {
            List<ComboBox> ListComboBox = new List<ComboBox> {PotenseonetrProcess1, PotenseonetrProcess2, PotenseonetrProcess3, PotenseonetrProcess4, PotenseonetrProcess5, EncodorProcess1, EncodorProcess2, EncodorProcess3};
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

        private void ClickSaveProfile(object sender, RoutedEventArgs e)
        {
            List<ComboBox> ListComboBox = new List<ComboBox> {PotenseonetrProcess1, PotenseonetrProcess2, PotenseonetrProcess3, PotenseonetrProcess4, PotenseonetrProcess5, EncodorProcess1, EncodorProcess2, EncodorProcess3};
            var selectedProfile = ComboboxProfile.SelectedItem as Profile;
            string StringReadCombobox = "";
            foreach (var ComboBoxNow in ListComboBox)
            {
                var selectedComboBox = ComboBoxNow.SelectedItem as Process;
                if (selectedComboBox != null)
                {
                    StringReadCombobox += selectedComboBox.Id.ToString() + "| ";
                }
                else
                {
                    StringReadCombobox += "Null| ";
                }

            }

            if (selectedProfile != null) { 
                var updatedProfileProcess = new Profile
                {
                    Id = selectedProfile.Id,
                    ProcessIds = StringReadCombobox
                };
                dbManager.UpdateProfile(updatedProfileProcess);
            }
        }

        private void ProcessFromProfile(object sender, SelectionChangedEventArgs e)
        {
            List<ComboBox> ListComboBox = new List<ComboBox> {PotenseonetrProcess1, PotenseonetrProcess2, PotenseonetrProcess3, PotenseonetrProcess4, PotenseonetrProcess5, EncodorProcess1, EncodorProcess2, EncodorProcess3 };
            List<Process> processes = dbManager.GetAllProcesses();
            var selectedProfile = ComboboxProfile.SelectedItem as Profile;
            if (selectedProfile != null) { 
                string[] ProcessNowProfile = selectedProfile.ProcessIds.Split(new[] { "| " }, StringSplitOptions.None);

                for (var ComboBoxNow = 0; ComboBoxNow < ListComboBox.Count; ComboBoxNow++)
                {
                    ListComboBox[ComboBoxNow].ItemsSource = processes;
                    ListComboBox[ComboBoxNow].DisplayMemberPath = "NameProcess";
                    ListComboBox[ComboBoxNow].SelectedValuePath = "Id";
                    if (ProcessNowProfile[ComboBoxNow] != null)
                    {
                        int processId = int.Parse(ProcessNowProfile[ComboBoxNow]);
                        ListComboBox[ComboBoxNow].SelectedValue = processId;
                    }
                }
            }
        }
    }
}
