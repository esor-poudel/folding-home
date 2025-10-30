using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    public class JobService: MarshalByRefObject, IJobService
    {
        private readonly Dictionary<string, (string jobCode, string jobHash)> jobs = new Dictionary<string, (string, string)>();
        // Use a thread-safe collection instead of a regular dictionary
        //private static readonly ConcurrentDictionary<string, (string jobCode, string jobHash)> jobs
        //    = new ConcurrentDictionary<string, (string, string)>();


        private readonly object jobsLock = new object();
        public List<string> GetJobs()
        {
            lock(jobsLock)
            {
            Console.WriteLine("GetJobs called. Current jobs: " + string.Join(",", jobs.Keys));
            return jobs.Keys.ToList();
            }
            
        }
        public string DownloadJob(string jobId)
        {
            lock(jobsLock)
            {

            if (jobs.ContainsKey(jobId))
            {
                return jobs[jobId].jobCode;
            }
            throw new Exception("Job not found");
            }
        }

        public string GetJobHash(string jobId)
        {
            lock(jobsLock)
            {
            if (jobs.ContainsKey(jobId))
            {
                return jobs[jobId].jobHash;
            }
            throw new Exception("Job not found");
            }
        }

        public void SubmitJobResult(string jobId, string result)
        {
            lock(jobsLock)
            {
            jobs.Remove(jobId);
            Console.WriteLine($"Job {jobId} completed with result: {result}");
            }   

        }

        public void AddJob(string jobId, string jobCode, string jobHash)
        {
            lock(jobsLock)
            {
            jobs[jobId] = (jobCode, jobHash);
            }
        }
    }
}
