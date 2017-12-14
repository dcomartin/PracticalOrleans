using Botwin;
using Grains;
using Microsoft.AspNetCore.Http;
using Orleans;

namespace Web
{
    public class CounterModule : BotwinModule
    {
        public CounterModule(IClusterClient clusterClient)
        {
            Get("/", async (request, response, _) =>
            {
                var counter = clusterClient.GetGrain<ICounterGrain>("Demo");
                var currentCount = await counter.GetCount();
                await response.WriteAsync(currentCount.ToString());
            });

            Post("/", async (request, response, _) =>
            {
                var counter = clusterClient.GetGrain<ICounterGrain>("Demo");
                await counter.Increment(1);
                response.StatusCode = 204;
            });
        }
    }
}
