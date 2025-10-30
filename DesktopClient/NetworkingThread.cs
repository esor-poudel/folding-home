using DesktopClient;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopClient
{
    public class NetworkingThread
    {
        private bool running = true;
        private readonly ServerThread server;
        private readonly string baseUrl = "https://localhost:7194";
        private readonly string endpoint = "/api/client"; 
        private readonly RestClient client;
        private readonly string myIp;
        private readonly int myPort;
        private readonly string displayName;
        private int jobsCompleted = 0;
        public int MyPort => myPort;

        public NetworkingThread(ServerThread server)
        {
            this.server = server;
            client = new RestClient(baseUrl);

            // Simulate current client info (in real world, we’d detect IP)
            myIp = "192.168.1.50";
            myPort = new Random().Next(4000, 5000);
            displayName = "LocalClient";

            RegisterSelf(); // Register this client to WebService

            server.JobCompleted += async () => await NotifyJobCompletedAsync();
        }

        private async void RegisterSelf()
        {
            try
            {
                // Initialize RestClient
                var client = new RestClient(baseUrl);

                // Create the POST request to the webservice endpoint
                var request = new RestRequest(endpoint, Method.Post);

                // Prepare JSON body 
                var body = new
                {
                    IPAddress = myIp,
                    Port = myPort,
                    DisplayName = displayName,
                    JobsCompleted = jobsCompleted,
                    RegisteredAt = DateTime.UtcNow
                };

                // Add the JSON body
                request.AddJsonBody(body);

                // Execute the POST request
                var response = await client.ExecuteAsync(request);

                // Debug info
                Console.WriteLine($"Response status: {response?.StatusCode}");
                Console.WriteLine($"Response content: {response?.Content}");

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Failed to register client: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
            }
        }

        public void Start()
        {
            Task.Run(() => Run());
        }

        private async Task Run()
        {
            while (running)
            {
                try
                {
                    // Initialize RestClient
                    var client = new RestClient(baseUrl);

                    // Create the POST request to the webservice endpoint
                    var request = new RestRequest(endpoint, Method.Get);

                    var response = await client.ExecuteAsync<List<ClientInfo>>(request);

                    if (response.IsSuccessful && response.Data != null)
                    {
                        Console.WriteLine($"Found {response.Data.Count} clients from server.");
                        foreach (var c in response.Data)
                        {
                           if (c.IPAddress == myIp && c.Port == myPort)
                            {
                               continue; // Skip self
                            }

                          
                            try
                            {
                                var http = new HttpClient();
                                var joburl = $"http://{c.IPAddress}:{c.Port}/getjob";
                                var jobDto = await http.GetFromJsonAsync<JobTransferDto>(joburl);

                                if (jobDto != null)
                                {
                                    // decode and verify the job first before executing
                                    string decodedJob = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jobDto.Base64Job));
                                    string computedHash = ComputeSHA256(decodedJob);
                                    if (computedHash == jobDto.Sha256Hash)
                                    {
                                        // Execute the job
                                        string result = ExecutePythonJob(decodedJob);

                                        // Submit the result back to Client A
                                        var submitUrl = $"http://{c.IPAddress}:{c.Port}/submit";
                                        await http.PostAsJsonAsync(submitUrl, result);

                                        // Notify the web service (job count incremented)
                                        await NotifyJobCompletedAsync();
                                    }
                                    else
                                    {
                                        Console.WriteLine("Hash verification failed. Job not executed.");
                                    }


                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error communicating with client {c.IPAddress}:{c.Port} - {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch clients list.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Networking error: {ex.Message}");
                }

                Thread.Sleep(8000); // Check every 8 seconds
            }
        }

        public void Stop()
        {
            running = false;
        }
    private string ExecutePythonJob(string job)
        {
            try
            {
                var engine = IronPython.Hosting.Python.CreateEngine();
                var scope = engine.CreateScope();
                var source = engine.CreateScriptSourceFromString(job);
                var result = source.Execute(scope);
                return result?.ToString() ?? "null";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private static string ComputeSHA256(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }


        private async Task NotifyJobCompletedAsync()
        {
            try
            {
                var request = new RestRequest(endpoint + "/jobdone", Method.Post);
                var body = new
                {
                    IPAddress = myIp,
                    Port = myPort,
                    DisplayName = displayName,
                    JobsCompleted = ++jobsCompleted,
                    CompletedAt = DateTime.UtcNow
                };
                request.AddJsonBody(body);
                var response = await client.ExecuteAsync(request);
                Console.WriteLine($"Job completion notified: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to notify job completion: {ex.Message}");
            }
        }

    }




    // Temporary model class to map JSON results
    public class ClientInfo
    {
        public int Id { get; set; }
        public required string IPAddress { get; set; }
        public int Port { get; set; }
        public string ? DisplayName { get; set; }
        public int JobsCompleted { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

 }
