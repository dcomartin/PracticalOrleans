using System;
using Botwin;
using Botwin.Response;
using Grains;
using Microsoft.AspNetCore.Http;
using Orleans;

namespace Web
{
    public class BankAccountModule : BotwinModule
    {
        public BankAccountModule(IClusterClient clusterClient)
        {
            Get("/bank/account/{AccountId:Guid}", async (request, response, routeData) =>
            {
                var accountId = Guid.Parse(routeData.Values["AccountID"].ToString());
                var grain = clusterClient.GetGrain<IBankAccountGrain>(accountId);
                var balance = await grain.Balance();

                await response.AsJson(balance);
            });

            Post("/bank/account/{AccountId:Guid}/deposit", async (request, response, routeData) =>
            {
                var accountId = Guid.Parse(routeData.Values["AccountID"].ToString());
                var grain = clusterClient.GetGrain<IBankAccountGrain>(accountId);
                await grain.Deposit(100);
                
                response.StatusCode = 204;
            });

            Post("/bank/account/{AccountId:Guid}/withdrawl", async (request, response, routeData) =>
            {
                var accountId = Guid.Parse(routeData.Values["AccountID"].ToString());
                var grain = clusterClient.GetGrain<IBankAccountGrain>(accountId);
                await grain.Withdraw(50);

                response.StatusCode = 204;
            });
        }
    }
}
