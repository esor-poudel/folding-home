using ServerThread;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
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
    /// Interaction logic for JobSubmissionForm.xaml
    /// </summary>
    public partial class JobSubmissionForm : Window
    {
        private static NetworkingThread networkingThread;
        public JobSubmissionForm(NetworkingThread sharedThread)
        {
            InitializeComponent();
            networkingThread = sharedThread;
        }

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Python Files (*.py)|*.py"
            };

            if (dialog.ShowDialog() == true)
            {
                txtPythonCode.Text = File.ReadAllText(dialog.FileName);
            }

        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string jobTitle = JobTitleTextBox.Text;
            string code = txtPythonCode.Text.Trim();
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(jobTitle))
            {
                MessageBox.Show("Please enter a job title and Python code before submitting.",
                                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var jobService = networkingThread.LocalJobService;
            //var jobService = networkingThread.ConnectToJobService("127.0.0.1", 4883);
            if (jobService == null)
            {
                MessageBox.Show("Could not connect to the selected client.");
                return;
            }


            // Submit the job 

            string jobCodeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
            string jobHash = ServerThread.NetworkingThread.GetSha256Hash(jobCodeBase64);
            jobService.AddJob(jobTitle, jobCodeBase64, jobHash);
            MessageBox.Show("Job submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        

        private void btnFetchJobs_Click(object sender, RoutedEventArgs e)
        {
            var browser = new ClientJobBrowser();
            browser.Show();
            this.Close();
        }
    }
}


