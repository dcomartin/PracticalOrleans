using System;
using System.Threading.Tasks;
using Orleans.Providers;

namespace Grains
{
    public class CustomerState
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public interface ICustomerGrain : IStateHolderGrain<CustomerState> { }

    [StorageProvider(ProviderName = "OrleansStorage")]
    public class CustomerGrain : StateHolderGrain<CustomerState>, ICustomerGrain {}
    
    public static class CustomerStateService
    {

        public static async Task CreateCustomer(Guid id, string name)
        {
            var state = new CustomerState
            {
                Id = id,
                Name = name
            };
            var grain = DemoOrleansClient.ClusterClient.GetGrain<ICustomerGrain>(id);
            await grain.SetItem(state);
        }

        public static async Task<CustomerState> GetCustomer(Guid id)
        {
            var grain = DemoOrleansClient.ClusterClient.GetGrain<ICustomerGrain>(id);
            return await grain.GetItem();
        }
    }
}
