using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using PrimeServer;

namespace PrimeClient
{
    class Program
    {
        const int REQUESTS_PER_SECOND = 10000;
        private static long _requestCounter = 0;   
        private static long _responseCounter = 0;  

        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            using var channel = GrpcChannel.ForAddress("http://localhost:50051");
            var client = new PrimeService.PrimeServiceClient(channel);

            var displayTimer = new Timer(DisplayCounters, null, 1000, 1000);

            while (true)
            {
                var stopwatchOverall = new Stopwatch();
                stopwatchOverall.Start();

                Parallel.For(0, REQUESTS_PER_SECOND, async _ => 
                {
                    Interlocked.Increment(ref _requestCounter);  

                    Random rand = new Random();
                    long randomNumber = rand.Next(1, 1001);

                    long requestId = _requestCounter;
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    var request = new PrimeNumber { Number = randomNumber, Id = requestId, Timestamp = timestamp };
                    var response = await client.IsPrimeAsync(request);

                    Interlocked.Increment(ref _responseCounter); 

                    var elapsed = stopwatchOverall.ElapsedMilliseconds;

                    Console.WriteLine($"ID: {requestId}, Number: {randomNumber}, Is Prime: {response.IsPrime}, RTT: {elapsed}ms");
                });

                stopwatchOverall.Stop();

                var sleepTime = 1000 - (int)stopwatchOverall.ElapsedMilliseconds;
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }

                CheckForMissingResponses();
            }
        }

        private static void CheckForMissingResponses()
        {
            if (_requestCounter != _responseCounter)
            {
                long missingResponses = _requestCounter - _responseCounter;
                Console.WriteLine($"Warning: {missingResponses} responses are missing!");
            }
        }

        private static void DisplayCounters(object state)
        {
            Console.WriteLine($"Requests Sent: {_requestCounter}, Responses Received: {_responseCounter}");
        }
    }
}
