using System;
using System.Collections.Generic;
using System.Threading;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace DesktopClient
{
    public class ServerThread
    {
        private readonly Queue<string> jobQueue = new();
        private readonly object locker = new();
        private bool running = true;
        private int jobsCompleted = 0;

        public event Action<string>? JobOutputReceived;

        public void AddJob(string jobCode)
        {
            lock (locker)
            {
                jobQueue.Enqueue(jobCode);
            }
        }

        public string? GetNextJob()
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

        private void RunJobs()
        {
            while (running)
            {
                string? job = GetNextJob();
                if (job != null)
                {
                    try
                    {
                        var engine = Python.CreateEngine();
                        var scope = engine.CreateScope();
                        var source = engine.CreateScriptSourceFromString(job);

                        var result = source.Execute(scope);
                        jobsCompleted++;

                        JobOutputReceived?.Invoke($"✅ Job completed. Result: {result}");
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
    }
}
