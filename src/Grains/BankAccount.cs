using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.EventSourcing;

namespace Grains
{
    public abstract class BankAccountEvent
    {
        public decimal Amount { get; set; }
    }

    public class Deposited : BankAccountEvent { }

    public class Withdrawn : BankAccountEvent { }

    public interface IBankAccountGrain : IGrainWithGuidKey
    {
        Task Deposit(decimal amount);
        Task Withdraw(decimal amount);
        Task<decimal> Balance();
    }

    public class BankAccountGrain : JournaledGrain<BankAccountState>, IBankAccountGrain
    {
        private readonly IEventStoreConnection _conn;
        private string _stream;

        const int Defaultport = 1113;

        public BankAccountGrain()
        {
            var settings = ConnectionSettings
                .Create().
                EnableVerboseLogging()
                .UseConsoleLogger();

            _conn = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, Defaultport));
        }

        public override async Task OnActivateAsync()
        {
            _stream = $"{GetType().Name}-{this.GetPrimaryKey()}";

            await _conn.ConnectAsync();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = await _conn.ReadStreamEventsForwardAsync(_stream, nextSliceStart, 200, false);
                foreach (var evnt in currentSlice.Events)
                {
                    base.RaiseEvent(DeserializeEvent(evnt.Event));
                }

                nextSliceStart = (int)currentSlice.NextEventNumber;
                
            } while (!currentSlice.IsEndOfStream);

            await ConfirmEvents();
        }

        public override Task OnDeactivateAsync()
        {
            _conn.Close();
            _conn.Dispose();
            return Task.CompletedTask;
        }

        public async Task Deposit(decimal amount)
        {
            await RaiseEvent(new Deposited
            {
                Amount = amount
            });
        }

        public async Task Withdraw(decimal amount)
        {
            await RaiseEvent(new Withdrawn
            {
                Amount = amount
            });
        }

        public Task<decimal> Balance()
        {
            return Task.FromResult(State.Balance);
        }

        private async Task RaiseEvent(BankAccountEvent evnt)
        {
            base.RaiseEvent(evnt);
            await ConfirmEvents();

            await _conn.AppendToStreamAsync(_stream, Version - 2, ToEventData(Guid.NewGuid(), evnt, new Dictionary<string, object>()));
        }
        
        private static EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    "EventClrType", evnt.GetType().AssemblyQualifiedName
                }
            };
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders));
            var typeName = evnt.GetType().Name;

            return new EventData(eventId, typeName, true, data, metadata);
        }

        private static object DeserializeEvent(RecordedEvent evntData)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(evntData.Metadata)).Property("EventClrType").Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(evntData.Data), Type.GetType((string)eventClrTypeName));
        }
    }

    public class BankAccountState
    {
        public decimal Balance { get; set; }

        public BankAccountState Apply(Deposited evnt)
        {
            Balance += evnt.Amount;
            return this;
        }

        public BankAccountState Apply(Withdrawn evnt)
        {
            Balance -= evnt.Amount;
            return this;
        }
    }
}
