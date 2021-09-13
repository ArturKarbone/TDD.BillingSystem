using Moq;
using System;
using System.Collections.Generic;
using Xunit;
using static BillingSystem.Tests.BillingProcessorTests6;

namespace BillingSystem.Tests
{
    public class BillingProcessorTests6
    {
        public class Annual
        {

        }

        public class Monthly
        {
            [Fact]
            public void customer_with_subsription_that_is_expired_gets_charged()
            {
                var subscription = new MonthlySubscription();
                var customer = new Customer() { Subscription = subscription };

                var thing = TestableBillingProcessor.Create(customer);

                thing.ProcessMonth(2011, 8);

                thing.Charger.Verify(x => x.ChargeCustomer(customer), Times.Once);
            }


            [Fact]
            public void customer_with_subsription_that_is_current_does_not_get_charged()
            {
                var customer = new Customer() { Subscription = new MonthlySubscription { PaidThroughYear = 2021, PaidThroughMonth = 9 } };

                var thing = TestableBillingProcessor.Create(customer);

                thing.ProcessMonth(2021, 09);

                thing.Charger.Verify(x => x.ChargeCustomer(customer), Times.Never);
            }

            [Fact]
            public void customer_who_is_current_and_due_to_pay_but_fails_once_is_still_current()
            {
                var customer = new Customer() { Subscription = new MonthlySubscription { PaidThroughYear = 2021, PaidThroughMonth = 9 } };

                var thing = TestableBillingProcessor.Create(customer);
                thing.Charger
                    .Setup(x => x.ChargeCustomer(It.IsAny<Customer>()))
                    .Returns(false);

                thing.ProcessMonth(2021, 10);

                Assert.Equal(1, customer.Subscription.PaymentFailures);
                Assert.True(customer.Subscription.IsCurrent);
            }

            [Fact]
            public void customer_who_is_current_and_due_to_pay_but_fails_maximum_times_is_no_longer_subscribed()
            {
                var customer = new Customer() { Subscription = new MonthlySubscription { PaidThroughYear = 2021, PaidThroughMonth = 9 } };

                var thing = TestableBillingProcessor.Create(customer);
                thing.Charger
                    .Setup(x => x.ChargeCustomer(It.IsAny<Customer>()))
                    .Returns(false);

                for (int i = 0; i < Subscription.MAX_FAILURES; i++)
                    thing.ProcessMonth(2021, 10);

                Assert.Equal(3, customer.Subscription.PaymentFailures);
                Assert.False(customer.Subscription.IsCurrent);
            }
        }

        public class NoSubscription
        {
            [Fact]
            public void customer_who_does_not_have_subcriptions_does_not_get_charged()
            {
                var customer = new Customer() { };

                var thing = TestableBillingProcessor.Create(customer);

                thing.ProcessMonth(2011, 8);

                thing.Charger.Verify(x => x.ChargeCustomer(customer), Times.Never);
            }
        }






        public class BillingProcessor
        {

            private ICustomerRepository repo;
            private ICreditCardCharger charger;

            public BillingProcessor(ICustomerRepository repo, ICreditCardCharger charger)
            {
                this.repo = repo;
                this.charger = charger;
            }

            internal void ProcessMonth(int year, int month)
            {
                var custoners = repo.GetAll();

                foreach (var customer in custoners)
                {
                    if (NeedsBilling(year, month, customer))
                    {
                        var charged = charger.ChargeCustomer(customer);
                        customer.Subscription.RecordChargeResults(charged);
                    }
                }
            }

            private static bool NeedsBilling(int year, int month, Customer customer)
            {
                if (null != customer.Subscription)
                    return customer.Subscription.NeedsBiling(year, month);

                return false;

            }
        }


        //Testable object pattern
        public class TestableBillingProcessor : BillingProcessor
        {
            public Mock<ICustomerRepository> Repo { get; }
            public Mock<ICreditCardCharger> Charger { get; }

            public TestableBillingProcessor(Mock<ICustomerRepository> repo, Mock<ICreditCardCharger> charger) : base(repo.Object, charger.Object)
            {
                this.Repo = repo;
                this.Charger = charger;
            }

            public static TestableBillingProcessor Create(params Customer[] customers)
            {
                var repo = new Mock<ICustomerRepository>();
                var charger = new Mock<ICreditCardCharger>();

                repo
                 .Setup(x => x.GetAll())
                 .Returns(customers);

                TestableBillingProcessor thing = new TestableBillingProcessor(repo, charger);

                return thing;
            }
        }
        public interface ICreditCardCharger
        {
            bool ChargeCustomer(Customer customer);
        }

        public interface ICustomerRepository
        {
            IEnumerable<Customer> GetAll();
        }

        //tests & domain are at the same file (at least to start with)

        public abstract class Subscription
        {
            public const int MAX_FAILURES = 3;
            public abstract bool IsRecurring { get; }
            public bool IsCurrent => PaymentFailures < MAX_FAILURES;
            public int PaymentFailures { get; set; }
            public abstract bool NeedsBiling(int year, int month);

            //default implementation
            public virtual void RecordChargeResults(bool charged)
            {
                if (!charged)
                {
                    PaymentFailures++;
                }
            }
        }

        public class AnnualSubscription : Subscription
        {
            public override bool IsRecurring => throw new NotImplementedException();

            public override bool NeedsBiling(int year, int month)
            {
                throw new NotImplementedException();
            }
        }
        public class MonthlySubscription : Subscription
        {
            public override bool IsRecurring => throw new NotImplementedException();

            public int PaidThroughYear { get; internal set; }
            public int PaidThroughMonth { get; internal set; }

            public override bool NeedsBiling(int year, int month)
            {
                return PaidThroughYear <= year && PaidThroughMonth < month;
            }
        }
        public class Customer
        {
            public Subscription Subscription { get; set; }
            //maybe this will survive our design
            //what about customers who subscribed today?
        }
    }
}
