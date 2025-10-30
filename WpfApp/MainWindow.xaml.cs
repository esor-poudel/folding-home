using Microsoft.Win32;
using ServerThread;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static NetworkingThread networkingThread;
        private static string clientId = "1";
        private DispatcherTimer statusTimer;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Register TCP channel for Remoting
            TcpChannel channel = new TcpChannel(40);
            ChannelServices.RegisterChannel(channel, false);

            // 2. Register singleton JobService
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(JobService),
                "JobService",
                WellKnownObjectMode.Singleton
            );

            // 3. Start NetworkingThread
            networkingThread = new NetworkingThread(clientId);
            networkingThread.Start();

            // 4. Start status update timer
            statusTimer = new DispatcherTimer();
            statusTimer.Interval = TimeSpan.FromSeconds(1);
            statusTimer.Tick += (s, e) => UpdateJobStatus();
            statusTimer.Start();

            UpdateJobStatus();
        }

        private void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Python files (*.py)|*.py|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                PythonCodeTextBox.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private void SubmitCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string code = PythonCodeTextBox.Text;
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please enter Python code.");
                return;
            }
            string jobId = $"Job_{Guid.NewGuid()}";
            string jobCodeBase64 = NetworkingThread.ToBase64(code);
            string jobHash = NetworkingThread.GetSha256Hash(jobCodeBase64);
            JobService.AddJob(jobId, jobCodeBase64, jobHash);
            MessageBox.Show($"Job submitted: {jobId}");
        }

        private void QueryStatusButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateJobStatus();
        }

        private void UpdateJobStatus()
        {
            IsWorkingText.Text = networkingThread.IsWorking ? "Yes" : "No";
            JobsCompletedText.Text = networkingThread.JobsCompleted.ToString();
        }
    }
}