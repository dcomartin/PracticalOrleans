using System;
using System.Collections.Generic;
using Botwin;
using Botwin.Response;
using Grains;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Orleans;

namespace Web
{
    public class CustomerModule : BotwinModule
    {
        public CustomerModule()
        {
            Get("/customers/{customerId:Guid}", async (request, response, routeData) =>
            {
                var customer = await CustomerStateService.GetCustomer(Guid.Parse(routeData.Values["customerId"].ToString()));
                await response.Negotiate(customer);
            });

            Post("/customers", async (request, response, _) =>
            {
                var customerId = Guid.NewGuid();
                await CustomerStateService.CreateCustomer(customerId, "CodeOpinion");

                response.StatusCode = 201;
                response.Headers.Add(new KeyValuePair<string, StringValues>("Location", $"/customers/{customerId}"));
            });
        }
    }
}
