using IronPython.Hosting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerThread
{
    public class NetworkingThread
    {
        
        public int JobsCompleted { get; private set; }
        public bool IsWorking { get; private set; }
        private string currentClientId;
        private int PollingInterval=5000;
        private readonly Dictionary<string, TcpChannel> hostedChannels = new Dictionary<string, TcpChannel>();
        public JobService LocalJobService { get; private set; }

        public NetworkingThread()
        {
          
        }

        public void Start()
        {
            Task.Run(() => Loop());
        }

        private async Task Loop()
        {
            while (true)
            {
                var clients = new List<ClientInfo>();
                try
                {
                    clients = await GetClientsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching clients: " + ex.Message);
                    Thread.Sleep(PollingInterval);
                    continue;
                }

                foreach (var client in clients)
                {
                    try
                    {
                        if (!hostedChannels.ContainsKey(client.Id))
                        {
                            HostJobServiceForClient(client);
                        }
                        var jobService = ConnectToJobService(client.IPAddress, client.Port);
                        if (jobService == null) continue;
                        var jobs = jobService.GetJobs();
                        if (jobs.Count == 0) continue;

                        foreach (var jobId in jobs)
                        {
                            try
                            {
                                IsWorking = true;
                                var jobCodeBase64 = jobService.DownloadJob(jobId);
                                //var jobHash = jobService.GetJobHash(jobId);

                                //// Verify hash
                                //var computedHash = GetSha256Hash(jobCodeBase64);
                                //if (computedHash != jobHash) continue; // Data corrupted

                                //var jobCode = FromBase64(jobCodeBase64);
                                //var result = ExecutePythonJob(jobCode);
                                //await PostJobResultAsync(client.Id, jobId, result);
                                //jobService.SubmitJobResult(jobId, result);
                                //JobsCompleted++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing job {jobId} for client {client.Id}: {ex.Message}");
                            }
                            finally
                            {
                                IsWorking = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing client {client.Id}: {ex.Message}");
                    }
                }
                Thread.Sleep(PollingInterval);
            }
        }

        public async Task<List<ClientInfo>> GetClientsAsync()
        {
                var client = new RestClient("https://localhost:7194");
                var request = new RestRequest($"/api/client", Method.Get);
                var response = await client.ExecuteAsync<List<ClientInfo>>(request);
                return JsonConvert.DeserializeObject<List<ClientInfo>>(response.Content);

        }

        public IJobService ConnectToJobService(string ip, int port)
        {
            try
            {
                var url = $"tcp://{ip}:{port}/JobService_{port}";
                return (IJobService)Activator.GetObject(typeof(IJobService), url);
            }
            catch { return null; }


            //try
            //{
            //    var binding = new NetTcpBinding();
            //    var endpoint = new EndpointAddress($"net.tcp://{ip}:{port}/JobService");
            //    var channelFactory = new ChannelFactory<IJobService>(binding, endpoint);
            //    return channelFactory.CreateChannel();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"[Error connecting to {ip}:{port}] {ex.Message}");
            //    return null;
            //}
        }

        

        private string ExecutePythonJob(string code)
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromString(code);
            var result = source.Execute(scope);
            return result?.ToString() ?? "";
        }

        private async Task PostJobResultAsync(string clientId, string jobId, string result)
        {
            var client = new RestClient("https://localhost:7194");
            var request = new RestRequest($"/api/jobresult", Method.Post);
            request.AddJsonBody(new { clientId, jobId, result });
            await client.ExecuteAsync(request);
        }

        private async Task<bool> IsClientAlive(ClientInfo client)
        {
            try
            {
                var clientApi = new RestClient("https://localhost:7194");
                var request = new RestRequest($"/api/clients/ping/{client.Id}", Method.Get);
                var response = await clientApi.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch { return false; }
        }

        // Making the Cryptography Traits so it can be used elsewhere if needed
        public static string ToBase64(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetSha256Hash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public void HostJobServiceForClient(ClientInfo client)
        {
            try
            {
                // Only create one channel per process (e.g., main port 4555)
                const int mainPort = 4555;
                if (!hostedChannels.ContainsKey("mainChannel"))
                {
                    var channel = new TcpChannel(mainPort);
                    ChannelServices.RegisterChannel(channel, false);
                    hostedChannels["mainChannel"] = channel;
                    Console.WriteLine($"[INFO] Created main TCP channel on port {mainPort}");
                }

                // Create a separate JobService instance per client
                var localJobService = new JobService();

                // Marshal the JobService with a unique URI per client
                RemotingServices.Marshal(
                    localJobService,
                    $"JobService_{client.Port}",
                    typeof(IJobService)
                );

                Console.WriteLine($"[INFO] Hosted JobService for {client.DisplayName} at /JobService_{client.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not host JobService for {client.DisplayName}: {ex.Message}");
            }
        }

    }

    public class ClientInfo
    {
        public string Id { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string DisplayName { get; set; }

        public int JobsCompleted { get; set; } = 0;

        public DateTime RegisteredAt { get; set; }
    }


 
}
