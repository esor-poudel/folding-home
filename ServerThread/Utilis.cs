using IronPython.Hosting;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerThread
{
    public class Utils
    {
        public static class Cryptography
        {
          

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



            public static async Task<RestResponse> JobDoneAsync(string ipAddress, int port)
            {
                var jobDone = new { IPAddress = ipAddress, Port = port };
                var client = new RestClient("https://localhost:7194");
                var request = new RestRequest("/api/client/jobdone", Method.Post);
                request.AddJsonBody(jobDone);
                return await client.ExecuteAsync(request);
            }


           


        }
    }
}
