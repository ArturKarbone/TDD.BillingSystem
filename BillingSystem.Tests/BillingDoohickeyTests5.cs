using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BillingSystem.Tests
{
    public class BillingDoohickeyTests5
    {
        [Fact]
        public void customer_who_does_not_have_subcriptions_does_not_get_charged()
        {
            var customer = new Customer() { Subscribed = false };

            var thing = TestableBillingProcessor.Create(customer);

            thing.ProcessMonth(2011, 8);

            thing.Charger.Verify(x => x.ChargeCustomer(customer), Times.Never);
        }


        [Fact]
        public void customer_with_subsription_that_is_expired_gets_charged()
        {
            var customer = new Customer() { Subscribed = true };

            var thing = TestableBillingProcessor.Create(customer);

            thing.ProcessMonth(2011, 8);

            thing.Charger.Verify(x => x.ChargeCustomer(customer), Times.Once);
        }


        [Fact]
        public void customer_with_subsription_that_is_current_does_not_get_charged()
        {
            var customer = new Customer() { Subscribed = true, PaidThroughYear = 2021, PaidThroughMonth = 9 };

            var thing = TestableBillingProcessor.Create(customer);

            thing.ProcessMonth(2021, 09);

            thing.Charger.Verify(x => x.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void customer_who_is_current_and_due_to_pay_but_fails_once_is_still_current()
        {
            var customer = new Customer() { Subscribed = true, PaidThroughYear = 2021, PaidThroughMonth = 9 };

            var thing = TestableBillingProcessor.Create(customer);
            thing.Charger
                .Setup(x => x.ChargeCustomer(It.IsAny<Customer>()))
                .Returns(false);

            thing.ProcessMonth(2021, 10);

            Assert.Equal(1, customer.PaymentFailures);
            Assert.True(customer.Subscribed);
        }

        [Fact]
        public void customer_who_is_current_and_due_to_pay_but_fails_maximum_times_is_no_longer_subscribed()
        {
            var customer = new Customer() { Subscribed = true, PaidThroughYear = 2021, PaidThroughMonth = 9 };

            var thing = TestableBillingProcessor.Create(customer);
            thing.Charger
                .Setup(x => x.ChargeCustomer(It.IsAny<Customer>()))
                .Returns(false);

            for (int i = 0; i < BillingProcessor.MAX_FAILURES; i++)
                thing.ProcessMonth(2021, 10);

            Assert.Equal(3, customer.PaymentFailures);
            Assert.False(customer.Subscribed);
        }

        public class BillingProcessor
        {
            public const int MAX_FAILURES = 3;

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
                    if (customer.Subscribed &&
                        customer.PaidThroughYear <= year &&
                        customer.PaidThroughMonth < month)
                    {
                        if (!charger.ChargeCustomer(customer))
                        {
                            customer.PaymentFailures++;
                            if (customer.PaymentFailures >= MAX_FAILURES)
                            {
                                customer.Subscribed = false;
                            }
                        }
                    }
                }
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
        public class Customer
        {
            //maybe this will survive our design
            //what about customers who subscribed today?

            //Is this reaslly customer data? or subscription data?
            public bool Subscribed { get; set; }
            public int PaidThroughYear { get; internal set; }
            public int PaidThroughMonth { get; internal set; }
            public int PaymentFailures { get; set; }


            //what about customers who subscribed today?
        }
    }
}
