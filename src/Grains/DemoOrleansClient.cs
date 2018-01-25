using Orleans;

namespace Grains
{
    public static class DemoOrleansClient
    {
        public static IClusterClient ClusterClient { get; set; }
    }
}
