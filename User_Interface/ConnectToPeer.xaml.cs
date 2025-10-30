using RestSharp;
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
    /// Interaction logic for ConnectToPeer.xaml
    /// </summary>
    public partial class ConnectToPeer : Window
    {
        public static NetworkingThread SharedNetworkingThread { get; private set; }

        public ConnectToPeer()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string displayName = NameTextBox.Text.Trim();
            string ipAddress = IPAddressTextBox.Text.Trim();

            if (string.IsNullOrEmpty(displayName))
            {
                MessageBox.Show("Please enter a display name.", "Missing Name",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                MessageBox.Show("Please enter an IP address.", "Missing IP",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                MessageBox.Show("Please enter a valid port number.", "Invalid Port",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var clientInfo = new
                {
                    ipAddress = ipAddress,
                    port = port,
                    displayName = displayName,
                    jobsCompleted = 0,
                    registeredAt = DateTime.UtcNow
                };
                var client = new RestClient("https://localhost:7194");
                var request = new RestRequest("/api/client", Method.Post);

                request.AddJsonBody(clientInfo);

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    MessageBox.Show($"Failed to register peer on web service.\n\n" +
                                    $"Status: {response.StatusCode}\nResponse: {response.Content}",
                                    "Web Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Create and start networking thread (host the JobService)
                SharedNetworkingThread = new NetworkingThread();
                SharedNetworkingThread.HostJobServiceForClient(new ServerThread.ClientInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    IPAddress = ipAddress,
                    Port = port,
                    DisplayName = displayName,
                    RegisteredAt = DateTime.UtcNow
                });

                MessageBox.Show($"Peer '{displayName}' hosted at {ipAddress}:{port}",
                                "Connected", MessageBoxButton.OK, MessageBoxImage.Information);

                // Open next window (for example JobSubmissionForm)
                var jobForm = new JobSubmissionForm(SharedNetworkingThread);
                jobForm.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to host peer.\n\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
