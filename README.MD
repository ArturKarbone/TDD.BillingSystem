https://app.pluralsight.com/library/courses/play-by-play-wilson-tdd/table-of-contents

TDD is a design process
let the stream of though bleed right into the code

the names are unimportant - getting your thougs out and throwing them around is paramount

let the naming evolve

Doohickey = device (приспособление)


Subcription Billing System


2 projects BillingSystem & BillingSystem.Tests
My understanding is ...

BillingDoohickeyTests

brains storm a code via comments
//Monthly billing
//Grace period for missed payments (dunnig status)
//Not all customers are subscribers
//Iddle customers should be automatically unsubscribed


public class Customer{

}

[Fact]

cusotmer_who_does_not_have_subscription_does_not_get_charged 
//source of customer
//service for charging customers


IcustomerRepository repo = new Mock<>
ICreditCardCharger charger = new Moc<>
var customer = new Customer()// what does it mean to not have a subscription
BillingDoohickey thing = new BillingDoohickey(repo, charger) //Or BillingThing or BillingStuff or BillingSomething and let the name evolve

Doohickey - приспособление 

this.ProcessMonth(2011,8);

charge.Verify(c=>c.ChargeCustomer(custome),Times.Never)




[Fact]

cusotmer_with_subscription_that_is_expired_gets_charged 

if (customer.Subscribed)
Customer Subcribed = true (maybe this will survive our design)


[Fact]

cusotmer_with_subscription_that_is_current_does_not_get_charged 

var thing = CreateBillingSystem(customer)

Rename to BillingProcessor

Favorite pattern = testable object

Testable Billing Processor (publically exposes mocsks)

Current? PaidThroughYear ,PaidThroughMonth
