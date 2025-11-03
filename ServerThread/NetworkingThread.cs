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

        public string MyIp { get; private set; }
        public int MyPort { get; private set; }
        public bool IsWorking { get; private set; }
        private int PollingInterval=5000;
        private readonly Dictionary<int, TcpChannel> hostedChannels = new Dictionary<int, TcpChannel>();
        public JobService LocalJobService { get; private set; }

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

                        if (!await IsClientAlive(client)) continue;

                        foreach (var jobId in jobs)
                        {
                            try
                            {
                                IsWorking = true;
                                //var jobCodeBase64 = jobService.DownloadJob(jobId);
                                //var jobHash = jobService.GetJobHash(jobId);

                                //// Verify hash
                                //var computedHash = GetSha256Hash(jobCodeBase64);
                                //if (computedHash != jobHash) continue; // Data corrupted

                                //var jobCode = FromBase64(jobCodeBase64);
                                //var result = ExecutePythonJob(jobCode);
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
                var url = $"tcp://{ip}:{port}/JobService";
                return (IJobService)Activator.GetObject(typeof(IJobService), url);
            }
            catch { return null; }
        }

        

        public string ExecutePythonJob(string code)
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            var source = engine.CreateScriptSourceFromString(code);
            var result = source.Execute(scope);
            return result?.ToString() ?? "";
        }

      

        private async Task<bool> IsClientAlive(ClientInfo client)
        {
            try
            {
                var clientApi = new RestClient("https://localhost:7194");
                var request = new RestRequest($"/api/client/ping/{client.Id}", Method.Get);
                var response = await clientApi.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch { return false; }
        }


        public void HostJobServiceForClient(ClientInfo client)
        {
            MyIp = client.IPAddress;
            MyPort = client.Port;
            try
            {
                var channel = new TcpChannel(client.Port);
                ChannelServices.RegisterChannel(channel, false);
                LocalJobService = new JobService();
                RemotingServices.Marshal(LocalJobService, "JobService");
                hostedChannels[client.Id] = channel;
                Console.WriteLine($"[INFO] Hosted JobService for {client.DisplayName} on port {client.Port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not host JobService for {client.DisplayName}: {ex.Message}");
            }
        }
    }

    public class ClientInfo
    {
        public int Id { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string DisplayName { get; set; }
        public int JobsCompleted { get; set; } = 0;
        public DateTime RegisteredAt { get; set; }
    }



}
