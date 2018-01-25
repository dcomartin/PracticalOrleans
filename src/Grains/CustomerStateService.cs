using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Providers;

namespace Grains
{
    public interface IStateHolderGrain<T> : IGrainWithGuidKey
    {
        Task<T> GetItem();
        Task<T> SetItem(T obj);
    }

    public class StateHolder<T>
    {
        public StateHolder() : this(default(T))
        {
        }

        public StateHolder(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }

    public abstract class StateHolderGrain<T> : Grain<StateHolder<T>>,
        IStateHolderGrain<T>
    {
        public Task<T> GetItem()
        {
            return Task.FromResult(State.Value);
        }

        public async Task<T> SetItem(T item)
        {
            State.Value = item;
            await WriteStateAsync();

            return State.Value;
        }
    }

    public interface ICustomerGrain : IStateHolderGrain<CustomerState> { }

    [StorageProvider(ProviderName = "OrleansStorage")]
    public class CustomerGrain : StateHolderGrain<CustomerState>, ICustomerGrain {}

    public class CustomerState
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

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
