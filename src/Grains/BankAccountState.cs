using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace Grains
{
    [Serializable]
    public class Deposited : BankAccountEvent { }

    [Serializable]
    public class Withdrawn : BankAccountEvent { }

    [Serializable]
    public class BankAccountState
    {
        public decimal Balance { get; set; }

        public BankAccountState Apply(Deposited evnt)
        {
            Balance += evnt.Amount;
            return this;
        }

        public BankAccountState ApplyEvent(Withdrawn evnt)
        {
            Balance -= evnt.Amount;
            return this;
        }
    }

    [Serializable]
    public class BankAccountEvent
    {
        public decimal Amount;
    }

    public interface IBankAccountGrain : IGrainWithStringKey
    {
        Task Deposit();
        Task Withdrawl();
        Task<decimal> Balance();
    }

    [StorageProvider(ProviderName = "OrleansStorage")]
    public class BankAccountGrain : JournaledGrain<BankAccountState>, IBankAccountGrain
    {
        public Task Deposit()
        {
            RaiseEvent(new Deposited
            {
                Amount = 100
            });
            return ConfirmEvents();
        }

        public Task Withdrawl()
        {
            RaiseEvent(new Withdrawn
            {
                Amount = 50
            });
            return ConfirmEvents();
        }

        public Task<decimal> Balance()
        {
            return Task.FromResult(State.Balance);
        }
    }
}
