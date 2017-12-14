using System.Threading.Tasks;
using Orleans;

namespace Grains
{
    public interface ICounterGrain : IGrainWithGuidKey
    {
        Task Increment(int increment);
        Task<int> GetCount();
    }

    public class Counter : Grain, ICounterGrain
    {
        private int _counter;
        
        public Task Increment(int increment)
        {
            _counter += increment;
            return Task.CompletedTask;
        }

        public Task<int> GetCount()
        {
            return Task.FromResult(_counter);
        }
    }
}
