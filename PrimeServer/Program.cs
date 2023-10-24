using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using PrimeServer;
using System.Collections.Concurrent;

namespace PrimeServer
{
    class Program
    {
        const string Host = "0.0.0.0";
        const int Port = 50051;

        static async Task Main(string[] args)
        {
            var server = new Server
            {
                Services = { PrimeService.BindService(new PrimeServiceImpl()) },
                Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
            };
            server.Start();

            var displayTimer = new Timer(DisplayStats, null, 1000, 1000);

            Console.WriteLine($"Server listening on port {Port}");
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            displayTimer.Dispose();
            await server.ShutdownAsync();
        }

        private static void DisplayStats(object? state)  
        {
            var topPrimes = PrimeServiceImpl.GetTopPrimes();
            Console.WriteLine($"Total Messages Received: {PrimeServiceImpl.TotalMessages}");
            Console.WriteLine("Top 10 Prime Numbers:");
            foreach (var prime in topPrimes)
            {
                Console.WriteLine($"Number: {prime.Key}, Requests: {prime.Value}");
            }
        }
    }

    public class PrimeServiceImpl : PrimeService.PrimeServiceBase
    {
        private static ConcurrentDictionary<long, int> primeStats = new ConcurrentDictionary<long, int>();
        private static long _totalMessages = 0;  

        public static long TotalMessages 
        { 
            get { return _totalMessages; } 
        }

        public override Task<PrimeResponse> IsPrime(PrimeNumber request, ServerCallContext context)
        {
            bool result = IsPrimeNumber(request.Number);

            Interlocked.Increment(ref _totalMessages);

            if (result)
            {
                primeStats.AddOrUpdate(request.Number, 1, (key, oldValue) => oldValue + 1);
            }

            return Task.FromResult(new PrimeResponse { IsPrime = result });
        }

        public static IEnumerable<KeyValuePair<long, int>> GetTopPrimes()
        {
            return primeStats.OrderByDescending(p => p.Value).Take(10);
        }

        private bool IsPrimeNumber(long number)
        {
            if (number <= 1) return false;
            if (number <= 3) return true;

            if (number % 2 == 0 || number % 3 == 0) return false;

            long i = 5;
            while (i * i <= number)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
                i += 6;
            }

            return true;
        }
    }
}
