using System.Threading.Tasks;
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
        public Task Deposit(decimal amount)
        {
            RaiseEvent(new Deposited
            {
                Amount = amount
            });
            return ConfirmEvents();
        }

        public Task Withdraw(decimal amount)
        {
            RaiseEvent(new Withdrawn
            {
                Amount = amount
            });
            return ConfirmEvents();
        }

        public Task<decimal> Balance()
        {
            return Task.FromResult(State.Balance);
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
