using System;
using Grains;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
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
                    var config = ClientConfiguration.LocalhostSilo();
                  config.ClusterId = "Test";

                    var builder = new ClientBuilder()
                        .UseConfiguration(config)
                        .ConfigureApplicationParts(parts =>
                            parts.AddApplicationPart(typeof(IInventoryItemGrain).Assembly))
                        .ConfigureLogging(logging => logging.AddConsole());

                    var client = builder.Build();
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
