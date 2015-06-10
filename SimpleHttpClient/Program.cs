using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleHttpClient
{
    class Program
    {
        const int NumIterations = 200;
        static void Main(string[] args)
        {
            //MakeRequest(shared: true);
            MakeRequest(shared: false);
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                Proxy = new WebProxy("http://127.0.0.1"),
                UseProxy = false,
            };

            HttpClient client = new HttpClient(handler);
            return client;
        }

        static void MakeRequest(bool shared)
        {
            var sharedHttpClient = CreateHttpClient();
            List<double> latencies = new List<double>();

            double avg = 0;
            int i = 0;

            for (i = 0; i < NumIterations; i++)
            {
                string str = string.Empty;
                if (shared)
                {
                    str = HTTP_GET(sharedHttpClient).Result;
                }
                else
                {
                    var client = CreateHttpClient();
                    str = HTTP_GET(client).Result;
                }

                double latency = double.Parse(str);
                latencies.Add(latency);

                avg += latency;
                Console.WriteLine(str);
                Thread.Sleep(500);
            }

            Console.WriteLine("50th percentile:" + Percentile(latencies.ToArray(), 0.5));
            Console.WriteLine("95th percentile:" + Percentile(latencies.ToArray(), 0.95));
            Console.WriteLine("99th percentile:" + Percentile(latencies.ToArray(), 0.99));
            Console.WriteLine("Average:" + avg / i);
        }

        static async Task<string> HTTP_GET(HttpClient client)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("<user>:<password>")));
            using (var response = client.GetAsync("https://<site>.scm.azurewebsites.net/api/vfs/site/repository/package.json").Result)

                if (response.IsSuccessStatusCode)
                {
                    HttpContent content = response.Content;
                    // ... Read the string.
                    string result = await content.ReadAsStringAsync();
                    sw.Stop();

                    return sw.ElapsedMilliseconds.ToString();
                }
                else
                {
                    return "Failed:" + response.StatusCode.ToString();
                }
        }

        public static double Percentile(double[] sequence, double excelPercentile)
        {
            Array.Sort(sequence);
            int N = sequence.Length;
            double n = (N - 1) * excelPercentile + 1;

            if (n == 1d) return sequence[0];
            else if (n == N) return sequence[N - 1];
            else
            {
                int k = (int)n;
                double d = n - k;
                return sequence[k - 1] + d * (sequence[k] - sequence[k - 1]);
            }
        }
    }
}
