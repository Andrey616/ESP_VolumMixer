using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;




namespace ESP_VolumMixer
{
    
    public partial class MainWindow : Window
    {
        private DatabaseManager dbManager;
        private CancellationTokenSource RunningCheckAudio;
        private CancellationTokenSource RunningConnektedPort;
        List<string> NameProcessInComboboxes = new List<string> { "", "", "", "", "", "", "", "" };

        public MainWindow()
        {

            InitializeComponent();
            dbManager = new DatabaseManager();
            dbManager.InitializeDatabase();
            RunningCheckAudio = new CancellationTokenSource();
            _ = CheckAudio(RunningCheckAudio.Token);
            _ = Task.Run(() => ConnektedPort());
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
                        string name = process.ProcessName;

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

        string port = "COM1";

        bool Check(string p)
        {
            try
            {
                using (var sp = new SerialPort(p, 115200))
                {
                    sp.ReadTimeout = 200;
                    sp.Open();
                    return sp.ReadLine().StartsWith("VolumMixer-");
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task ConnektedPort()
        {
            while (true)
            {
                if (!Check(port))
                {
                    port = SerialPort.GetPortNames().Where(Check).FirstOrDefault() ?? "COM1";
                    if (port == "COM1") { continue; }
                }

                try
                {
                    using (var sp = new SerialPort(port, 115200))
                    {
                        sp.ReadTimeout = 1000;
                        sp.Open();
                        while (true)
                        {
                            try { UppVolumProc(sp.ReadLine()); }
                            catch (IOException) { break; }
                        }
                    }
                }
                catch { }

                Console.WriteLine("Переподключение...");
                Thread.Sleep(3000);
            }
        }

        
        
        void UppVolumProc(String Data)
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            int[] VolumList = Array.ConvertAll(Data.Split(new[] { "VolumMixer-", "| " }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
            foreach (var device in devices)
            {
                var sessionManager = device.AudioSessionManager;
                var sessions = sessionManager.Sessions;

                for (int i = 0; i < sessions.Count; i++)
                {
                    var session = sessions[i];
                    uint processId = session.GetProcessID;
                    var process = System.Diagnostics.Process.GetProcessById((int)processId);
                    string name = process.ProcessName;


                    if (NameProcessInComboboxes.Contains(process.ProcessName))
                    {
                        for (int idProList = 0; idProList < NameProcessInComboboxes.Count; idProList++)
                        {
                            if (process.ProcessName == NameProcessInComboboxes[idProList])
                            {
                                session.SimpleAudioVolume.Volume = VolumList[idProList] / 100f;
                            }
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
            ClickSaveProfile(null, null);
            List<ComboBox> ListComboBox = new List<ComboBox>{PotenseonetrProcess1, PotenseonetrProcess2, PotenseonetrProcess3,PotenseonetrProcess4, PotenseonetrProcess5, EncodorProcess1, EncodorProcess2, EncodorProcess3};

            List<Process> processes = dbManager.GetAllProcesses();
            List<Profile> profiles = dbManager.GetAllProfiles();

            int? selectedProfileId = null;
            if (ComboboxProfile.SelectedItem is Profile selectedProfile)
            {
                selectedProfileId = selectedProfile.Id;
            }
            else if (ComboboxProfile.SelectedValue != null)
            {
                selectedProfileId = ComboboxProfile.SelectedValue as int?;
            }

            var selectedProcessIds = new Dictionary<ComboBox, int?>();
            foreach (var comboBox in ListComboBox)
            {
                int? processId = null;
                if (comboBox.SelectedItem is Process selectedProcess)
                {
                    processId = selectedProcess.Id;
                }
                else if (comboBox.SelectedValue != null)
                {
                    processId = comboBox.SelectedValue as int?;
                }
                selectedProcessIds[comboBox] = processId;
            }

            ComboboxProfile.ItemsSource = profiles;
            ComboboxProfile.DisplayMemberPath = "Name";
            ComboboxProfile.SelectedValuePath = "Id";

            if (selectedProfileId.HasValue)
            {
                ComboboxProfile.SelectedValue = selectedProfileId.Value;
            }

            foreach (var comboBox in ListComboBox)
            {
                comboBox.ItemsSource = processes;
                comboBox.DisplayMemberPath = "NameProcess";
                comboBox.SelectedValuePath = "Id";

                if (selectedProcessIds.TryGetValue(comboBox, out int? processId) && processId.HasValue)
                {
                    comboBox.SelectedValue = processId.Value;
                }
            }
        }

        private void ClickSaveProfile(object sender, RoutedEventArgs e)
        {
            List<ComboBox> ListComboBox = new List<ComboBox> {PotenseonetrProcess1, PotenseonetrProcess2, PotenseonetrProcess3, PotenseonetrProcess4, PotenseonetrProcess5, EncodorProcess1, EncodorProcess2, EncodorProcess3};
            var selectedProfile = ComboboxProfile.SelectedItem as Profile;
            string StringReadCombobox = "";
            for (int ComboBoxNow = 0; ComboBoxNow < ListComboBox.Count(); ComboBoxNow++) 
                {
                    var selectedComboBox = ListComboBox[ComboBoxNow].SelectedItem as Process;
                if (selectedComboBox != null)
                {
                    StringReadCombobox += selectedComboBox.Id.ToString() + "| ";
                    NameProcessInComboboxes[ComboBoxNow] = selectedComboBox.NameProcess.ToString();
                    
                    
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
