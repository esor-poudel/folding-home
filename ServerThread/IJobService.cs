using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    [ServiceContract]
    public interface IJobService
    {
        [OperationContract]
        List<string> GetJobs();
        [OperationContract]
        string DownloadJob(string jobId);
        [OperationContract]
        string GetJobHash(string jobId);
        [OperationContract]
        void SubmitJobResult(string jobId, string result);
        [OperationContract]
        void AddJob(string jobTitle, string jobCodeBase64);

    }
}
