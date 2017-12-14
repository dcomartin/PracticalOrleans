using System;
using Orleans;
using Orleans.Runtime.Host;

namespace Silo
{
    class Program
    {
        private static SiloHost _siloHost;

        static void Main(string[] args)
        {
            _siloHost = new SiloHost(System.Net.Dns.GetHostName())
            {
                ConfigFileName = "OrleansConfiguration.xml"
            };
            _siloHost.InitializeOrleansSilo();
            var start = _siloHost.StartOrleansSilo();
            if (!start)
            {
                throw new SystemException(String.Format("Failed to start Orleans silo '{0}' as a {1} node",
                    _siloHost.Name, _siloHost.Type));
            }

            Console.WriteLine("Silo is running...");
            Console.ReadLine();
        }
    }
}
