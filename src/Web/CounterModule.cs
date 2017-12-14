using System;
using Botwin;
using Grains;
using Microsoft.AspNetCore.Http;
using Orleans;

namespace Web
{
    public class CounterModule : BotwinModule
    {
        private Guid _grainId = Guid.Parse("ff4e9ced-8574-4799-8fa7-06e6b0197ec0");

        public CounterModule(IClusterClient clusterClient)
        {
            Get("/", async (request, response, _) =>
            {
                var counter = clusterClient.GetGrain<ICounterGrain>(_grainId);
                var currentCount = await counter.GetCount();
                await response.WriteAsync(currentCount.ToString());
            });

            Post("/", async (request, response, _) =>
            {
                var counter = clusterClient.GetGrain<ICounterGrain>(_grainId);
                await counter.Increment(1);
                response.StatusCode = 204;
            });
        }
    }
}
