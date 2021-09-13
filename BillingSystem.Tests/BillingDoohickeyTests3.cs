using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BillingSystem.Tests
{
    public class BillingDoohickeyTests3
    { 

        //in/out/process

        [Fact]
        public void customer_who_does_not_have_subcriptions_does_not_get_charged()
        {
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();

            var customer = new Customer() { Subscribed = false };

            repo
             .Setup(x => x.GetAll())
             .Returns(new List<Customer> { customer });

            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(x => x.ChargeCustomer(customer), Times.Never);
        }


        [Fact]
        public void customer_with_subsription_that_is_expired_gets_charged()
        {
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();

            var customer = new Customer() { Subscribed = true };

            repo
                .Setup(x => x.GetAll())
                .Returns(new List<Customer> { customer });

            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(x => x.ChargeCustomer(customer), Times.Once);
        }


        [Fact]
        public void customer_with_subsription_that_is_current_does_not_get_charged()
        {
            //here we see something that repeats three times. Time to refator it.
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();

            var customer = new Customer() { Subscribed = true };

            repo
                .Setup(x => x.GetAll())
                .Returns(new List<Customer> { customer });

            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(x => x.ChargeCustomer(customer), Times.Once);
        }

        public class BillingDoohickey
        {
            private ICustomerRepository repo;
            private ICreditCardCharger charger;

            public BillingDoohickey(ICustomerRepository repo, ICreditCardCharger charger)
            {
                this.repo = repo;
                this.charger = charger;
            }

            internal void ProcessMonth(int year, int month)
            {
                var custoners = repo.GetAll();

                foreach (var customer in custoners)
                {
                    if (customer.Subscribed)
                    {
                        charger.ChargeCustomer(customer);
                    }
                }
            }
        }

        public interface ICreditCardCharger
        {
            void ChargeCustomer(Customer customer);
        }

        public interface ICustomerRepository
        {
            IEnumerable<Customer> GetAll();
        }

        //tests & domain are at the same file (at least to start with)
        public class Customer
        {
            //maybe this will survive our design
            public bool Subscribed { get; set; }
        }
    }
}
