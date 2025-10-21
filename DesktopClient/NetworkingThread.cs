using DesktopClient;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopClient
{
    public class NetworkingThread
    {
        private bool running = true;
        private readonly ServerThread server;
        private readonly string baseUrl = "https://localhost:7194";
        private readonly string endpoint = "/api/client"; // your POST endpoint
        private readonly RestClient client;
        private readonly string myIp;
        private readonly int myPort;
        private readonly string displayName;
        private int jobsCompleted = 0;

        public NetworkingThread(ServerThread server)
        {
            this.server = server;
            client = new RestClient(baseUrl);

            // Simulate current client info (in real world, we’d detect IP)
            myIp = "192.168.1.50";
            myPort = new Random().Next(4000, 5000);
            displayName = "LocalClient";

            RegisterSelf(); // Register this client to WebService
        }

        private async void RegisterSelf()
        {
            try
            {
                // Initialize RestClient
                var client = new RestClient(baseUrl);

                // Create the POST request (just like your working format)
                var request = new RestRequest(endpoint, Method.Post);

                // Prepare JSON body (same structure as your API expects)
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
                    var request = new RestRequest("", Method.Get);
                    var response = await client.ExecuteAsync<List<ClientInfo>>(request);

                    if (response.IsSuccessful && response.Data != null)
                    {
                        Console.WriteLine($"Found {response.Data.Count} clients from server.");
                        foreach (var c in response.Data)
                        {
                            Console.WriteLine($" - {c.DisplayName} @ {c.IPAddress}:{c.Port}");
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
