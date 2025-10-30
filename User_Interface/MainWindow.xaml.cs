using Microsoft.Win32;
using ServerThread;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows;
using System.Windows.Threading;

namespace User_Interface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static NetworkingThread networkingThread;
        private DispatcherTimer statusTimer;
        public MainWindow()
        {
            InitializeComponent();
            // 1. Register TCP channel for Remoting
            //TcpChannel channel = new TcpChannel(4882);
            //ChannelServices.RegisterChannel(channel, false);

            // 2. Register singleton JobService
            //RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(JobService),
            //    "JobService",
            //    WellKnownObjectMode.Singleton
            //);

            // Get remote JobService reference
            //IJobService jobService = (IJobService)Activator.GetObject(
            //typeof(IJobService),
            //"tcp://192.168.3.1:4883/JobService"
            //  );

            //JobSeeder.SeedJob();
            // 3. Start NetworkingThread
            networkingThread = new NetworkingThread();
            networkingThread.Start();
            LoadClients();
        }

        private async void LoadClients()
        {
            var clients = await networkingThread.GetClientsAsync();
            ClientsListBox.ItemsSource = clients;
        }

        private void FetchJobsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            
            var selectedClient = ClientsListBox.SelectedItem as ClientInfo;
            if (selectedClient == null)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            var jobService = networkingThread.ConnectToJobService(selectedClient.IPAddress, selectedClient.Port);
            //var jobService = networkingThread.ConnectToJobService("127.0.0.1", 4883);
                if (jobService == null)
            {
                MessageBox.Show("Could not connect to the selected client.");
                return;
            }

            var jobs = jobService.GetJobs();
            JobsListBox.ItemsSource = jobs;

            }catch(Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


    }
}
