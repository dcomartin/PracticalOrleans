using System;
using System.Collections.Generic;
using Botwin;
using Botwin.ModelBinding;
using Botwin.Response;
using Grains;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Orleans;

namespace Web
{
    public class InventoryItemModule : BotwinModule
    {
        public InventoryItemModule(IClusterClient clusterClient)
        {
            Get("/inventory/{Id:Guid}", async (request, response, routeData) =>
            {
                var inventoryItemId = Guid.Parse(routeData.Values["Id"].ToString());
                var grain = clusterClient.GetGrain<IInventoryItemGrain>(inventoryItemId);
                var currentQty = await grain.Quantity();
                await response.WriteAsync(currentQty.ToString());
            });

            Post("/inventory", async (request, response, _) =>
            {
                var inventoryItemId = Guid.NewGuid();
                var grain = clusterClient.GetGrain<IInventoryItemGrain>(inventoryItemId);

                response.StatusCode = 201;
                response.Headers.Add(new KeyValuePair<string, StringValues>("Location", 
                    $"/inventory/{inventoryItemId}"));
            });

            Post("/inventory/{Id:Guid}/increment", async (request, response, routeData) =>
            {
                var inventoryItemId = Guid.Parse(routeData.Values["Id"].ToString());
                var grain = clusterClient.GetGrain<IInventoryItemGrain>(inventoryItemId);

                var body = request.Bind<InventoryRequest>();
                if (body.Quantity >= 0)
                {
                    await grain.Increment(body.Quantity);
                }

                var currentQty = await grain.Quantity();
                await response.WriteAsync(currentQty.ToString());
            });
        }
    }

    public class InventoryRequest
    {
        public int Quantity { get; set; }
    }
}
