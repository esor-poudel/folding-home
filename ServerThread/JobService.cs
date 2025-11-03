using IronPython.Compiler.Ast;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class JobService: MarshalByRefObject, IJobService
    {
        private readonly Dictionary<string, (string jobCode, string jobHash)> jobs = new Dictionary<string, (string, string)>();
        private readonly object jobsLock = new object();


        public List<string> GetJobs()
        {
            lock(jobsLock)
            {
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

        public void AddJob(string jobId, string jobCode)
        {
            lock(jobsLock)
            {
                string jobCodeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jobCode));
                string Hash = Utils.Cryptography.GetSha256Hash(jobCodeBase64);
                jobs[jobId] = (jobCodeBase64, Hash);
            }
        }
    }
}
