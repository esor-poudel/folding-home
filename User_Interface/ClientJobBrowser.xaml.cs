using ServerThread;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace User_Interface
{
    /// <summary>
    /// Interaction logic for ClientJobBrowser.xaml
    /// </summary>
    public partial class ClientJobBrowser : Window
    {
        private NetworkingThread networkingThread = new NetworkingThread();
        private string selectedJobId;
        private ClientInfo selectedClient;
        private System.Windows.Threading.DispatcherTimer refreshTimer;
        public ClientJobBrowser()
        {
            InitializeComponent();
            LoadClients();
            refreshTimer = new System.Windows.Threading.DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(3); // refresh every 5 seconds
            refreshTimer.Tick += (s, e) => LoadClients();
            refreshTimer.Start();
        }
        private async void LoadClients()
        {
            var clients = await networkingThread.GetClientsAsync();
            ClientsDataGrid.ItemsSource = null;
            ClientsDataGrid.ItemsSource = clients;
        }

        private void FetchJobs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedClient = ClientsDataGrid.SelectedItem as ClientInfo;
                if (selectedClient == null)
                {
                    MessageBox.Show("Please select a client.");
                    return;
                }

                var jobService = networkingThread.ConnectToJobService(selectedClient.IPAddress, selectedClient.Port);
                if (jobService == null)
                {
                    MessageBox.Show("Could not connect to the selected client.");
                    return;
                }

                var jobs = jobService.GetJobs(); // List<string> of job IDs
                JobsDataGrid.ItemsSource = jobs;

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void JobsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedJobId = JobsDataGrid.SelectedItem as string;
            selectedClient = ClientsDataGrid.SelectedItem as ClientInfo;

            if (selectedClient == null || string.IsNullOrEmpty(selectedJobId))
            {
                JobCodeTextBox.Text = "";
                return;
            }

            var jobService = networkingThread.ConnectToJobService(selectedClient.IPAddress, selectedClient.Port);
            if (jobService == null)
            {
                MessageBox.Show("Could not connect to the selected client.");
                return;
            }

            var jobCodeBase64 = jobService.DownloadJob(selectedJobId);
            var jobCode = Utils.Cryptography.FromBase64(jobCodeBase64);
            JobCodeTextBox.Text = jobCode;
        }

        private async void ExecuteJob_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClient == null || string.IsNullOrEmpty(selectedJobId) || string.IsNullOrEmpty(JobCodeTextBox.Text))
            {
                MessageBox.Show("Please select a job and ensure job code is loaded.");
                return;
            }

            // Execute the job code using IronPython
            string result;
            try
            {
                result = networkingThread.ExecutePythonJob(JobCodeTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing job: {ex.Message}");
                return;
            }

            // Submit the result
            var jobService = networkingThread.ConnectToJobService(selectedClient.IPAddress, selectedClient.Port);
            jobService.SubmitJobResult(selectedJobId, result);

            // Notify the Web server Thats Job done and increase the count of the jobdone 

            await Utils.Cryptography.JobDoneAsync(selectedClient.IPAddress, selectedClient.Port);

            MessageBox.Show("Result submitted!\nResult: " + result, "Success");
            JobCodeTextBox.Text = "";
            JobsDataGrid.SelectedItem = null;
        }


    }
}
