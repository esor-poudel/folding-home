using DesktopClient.Server;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopClient
{
    public partial class MainWindow : Window
    {
        private ServerThread server;
        private NetworkingThread network;
        private bool isWorking = false;
        private int jobsDone = 0;
        private JobApiHost? _apiHost;


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public MainWindow()
        {
            AllocConsole();
            InitializeComponent();

            server = new ServerThread();
            network = new NetworkingThread(server);

            int myPort = network.MyPort;
            _apiHost = new JobApiHost(server, myPort);
            _apiHost.Start();



            // Start server and networking
            // comment this line to get the job remotely 
            server.Start();
            network.Start();

            StatusText.Text = "Client started and registered.";
        }

      

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string jobCode = JobInputBox.Text.Trim();
            if (string.IsNullOrEmpty(jobCode))
            {
                MessageBox.Show("Please enter some Python code to execute!");
                return;
            }
            // encode the job to base64
            string base64Job = Convert.ToBase64String(Encoding.UTF8.GetBytes(jobCode));
            //  // Compute SHA256 hash of the original job code
            string jobHash = ComputeSHA256(jobCode);

            server.AddJob(new JobItem
            {
                Base64Job = base64Job,
                Sha256Hash = jobHash
            });

            JobInputBox.Clear();
            StatusText.Text = "Job added to queue.";
        }

        private static string ComputeSHA256(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        protected override void OnClosed(EventArgs e)
        {
            server.Stop();
            network.Stop();
            base.OnClosed(e);
        }
    }
}
