using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace BillingSystem.Tests
{
    public class BillingDoohickeyTests1
    {
        //brainstorm a code via comments
        //Monthly billing
        //Grace period for missed payments (dunnig status)
        //Not all customers are necessasrily subscribers
        //Iddle customers should be automatically unsubscribed

        //It's a good list to start with

        [Fact]
        public void customer_who_does_not_have_subcriptions_does_not_get_charged()
        {
            //need some source of customers
            //Service for charning customers
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();

            var customer = new Customer();//what does it mean to not have a subcsription?

            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(x => x.ChargeCustomer(customer), Times.Never);
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

        }
    }
}
