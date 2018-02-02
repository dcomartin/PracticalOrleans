using System;
using System.Threading.Tasks;
using Grains;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Orleans;
using Orleans.Runtime.Configuration;
using Polly;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DemoOrleansClient.ClusterClient = Policy<IClusterClient>
                .Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                })
                .Execute(() =>
                {
                    var config = ClientConfiguration.LocalhostSilo(30000);
                    var client = ClientBuilder.CreateDefault()
                        .UseConfiguration(config)
                        .Build();
                    
                    client.Connect().Wait();

                    return client;
                });

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
