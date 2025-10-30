using IronPython.Hosting;
using System.Windows.Controls;



namespace DesktopClient
{
    public class ServerThread
    {
        // Adding the JObItem queue 
        private readonly Queue<JobItem> jobQueue = new();
        private readonly object locker = new();
        private bool running = true;
        public event Action<string>? JobOutputReceived;
        public event Action? JobCompleted;

     
        public void AddJob(JobItem job)
        {
            lock (locker)
            {
                jobQueue.Enqueue(job);
            }
        }

        // Update AddJob to match the new queue type (no change needed here)

        // Update GetNextJob to return JobItem? instead of string?
        public JobItem? GetNextJob()
        {
            lock (locker)
            {
                if (jobQueue.Count > 0)
                {
                    return jobQueue.Dequeue();
                }
                return null;
            }
        }

        public void Start()
        {
            Thread worker = new(() => RunJobs());
            worker.IsBackground = true;
            worker.Start();
        }

        // Update RunJobs to handle JobItem and extract the Base64Job string
        private void RunJobs()
        {
            while (running)
            {
                JobItem? job = GetNextJob();
                if (job != null)
                {
                    try
                    {
                        // decode base64job to string first before executing
                        var jobCode = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(job.Base64Job));
                        // verfiy jobsha256 hash

                        var computedHash = ComputeSHA256(jobCode);
                        if (!string.Equals(computedHash, job.Sha256Hash, StringComparison.OrdinalIgnoreCase))
                        {
                            JobOutputReceived?.Invoke("❌ Job hash verification failed. Job skipped.");
                            continue;
                        }

                        var engine = Python.CreateEngine();
                        var scope = engine.CreateScope();
                        var source = engine.CreateScriptSourceFromString(jobCode);
                        var result = source.Execute(scope);
                        JobOutputReceived?.Invoke($"✅ Job completed. Result: {result}");
                        JobCompleted?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        JobOutputReceived?.Invoke($"❌ Error executing job: {ex.Message}");
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            running = false;
        }

        private static string ComputeSHA256(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
