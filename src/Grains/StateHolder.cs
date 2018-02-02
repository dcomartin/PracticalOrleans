using System.Threading.Tasks;
using Orleans;

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
}
