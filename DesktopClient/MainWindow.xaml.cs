using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace DesktopClient
{
    public partial class MainWindow : Window
    {
        private ServerThread server;
        private NetworkingThread network;
        private bool isWorking = false;
        private int jobsDone = 0;

        public MainWindow()
        {
            InitializeComponent();

            server = new ServerThread();
            network = new NetworkingThread(server);

            // Subscribe to output event
            server.JobOutputReceived += OnJobOutputReceived;

            // Start server and networking
            server.Start();
            network.Start();

            StatusText.Text = "Client started and registered.";
        }

        private void OnJobOutputReceived(string output)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = output;
                jobsDone++;
            });
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string jobCode = JobInputBox.Text.Trim();
            if (string.IsNullOrEmpty(jobCode))
            {
                MessageBox.Show("Please enter some Python code to execute!");
                return;
            }

            server.AddJob(jobCode);
            JobInputBox.Clear();
            StatusText.Text = "Job added to queue.";
        }

        protected override void OnClosed(EventArgs e)
        {
            server.Stop();
            network.Stop();
            base.OnClosed(e);
        }
    }
}
