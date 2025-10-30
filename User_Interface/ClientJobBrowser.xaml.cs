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
        public ClientJobBrowser()
        {
            InitializeComponent();
            LoadClients();
        }
        private async void LoadClients()
        {
            var clients = await networkingThread.GetClientsAsync();
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
