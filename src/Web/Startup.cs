using System;
using Botwin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime.Configuration;
using Polly;

namespace Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBotwin();
            services.AddSingleton(provider =>
            {
                return Policy<IClusterClient>
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
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseBotwin();
        }
    }
}
