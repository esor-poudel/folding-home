using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Interface;

namespace DatabaseService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("🚀 Starting DataServer...");

            // Define a TCP binding
            NetTcpBinding tcp = new NetTcpBinding();

            // Create a new service host bound to DataServer
            ServiceHost host = new ServiceHost(typeof(DBServices));

            // Add a service endpoint
            host.AddServiceEndpoint(typeof(DatabaseServiceInterface), tcp,
                "net.tcp://0.0.0.0:8100/DataService");

            // Open the service
            host.Open();
            Console.WriteLine("✅ System Online. Listening on net.tcp://0.0.0.0:8100/DataService");
            Console.WriteLine("Press <Enter> to stop the server...");

            Console.ReadLine();

            // Close the host
            host.Close();
            Console.WriteLine("Server stopped.");
        }
    }
}
