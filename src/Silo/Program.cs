using System;
using System.Threading.Tasks;
using Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using Orleans.Storage;

namespace Silo
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var config = ClusterConfiguration.LocalhostPrimarySilo();
            config.AddMemoryStorageProvider();

            var builder = new SiloHostBuilder()
                .UseConfiguration(config)
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(IInventoryItemGrain).Assembly)
                        .WithReferences())
                //.ConfigureLogging(logging => logging.AddConsole())
                ;

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
