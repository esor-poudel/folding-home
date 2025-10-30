using ServerThread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    internal class Program
    {

        static void Main(string[] args)
        {

            //int port = 4532;

            //// If a port number is provided as argument
            //if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
            //{
            //    port = parsedPort;
            //}

            //Console.WriteLine($"[INFO] Job Service starting on TCP port {port}...");

            //// 1. Register TCP channel first
            //TcpChannel channel = new TcpChannel(port);
            //ChannelServices.RegisterChannel(channel, false);

            //// 2. Register singleton service
            //RemotingConfiguration.RegisterWellKnownServiceType(
            //    typeof(JobService),
            //    "JobService",
            //    WellKnownObjectMode.Singleton
            //);

            //var jobService = (IJobService)Activator.GetObject(
            //    typeof(IJobService),
            //    $"tcp://127.0.0.1:{port}/JobService"
            //    );

            //// 3. Seed jobs
            //string python1 = "print('Hello from Job1')";
            //string jobCode1 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(python1));
            //string hash1 = NetworkingThread.GetSha256Hash(jobCode1);

            //string python2 = "print('Processing Job2')";
            //string jobCode2 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(python2));
            //string hash2 = NetworkingThread.GetSha256Hash(jobCode2);

            //// **Add jobs AFTER channel & singleton are ready**
            //jobService.AddJob("Job1", jobCode1, hash1);
            //jobService.AddJob("Job2", jobCode2, hash2);

            //Console.WriteLine("Jobs Added");

            ///// server  4883 le yo deuita job haleko ho 

            //// 4. Start networking thread
            ////NetworkingThread networkingThread = new NetworkingThread("1");
            ////networkingThread.Start();

            //Console.WriteLine("System Online. Press Enter to exit.");
            //Console.ReadLine();




        }
    }
}


