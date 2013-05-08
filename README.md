# Edument CQRS and Intentful Testing Starter Kit

## What is this?

A bunch of C# code to help you get started with writing **intentful tests**
for a domain, expressing it as **commands, events and exceptions**. These
ideas are often associated with the CQRS pattern.

Here's an example of how a test might looks:

    [Test]
    public void VoucherGivenAfter5000Points()
    {
        Test(
            Given(new PointsAwarded
            {
                MemberId = dummyMember,
                Points = 4000,
                AwardDate = awardDate1
            }),
            When(new AwardPoints
            {
                MemberId = dummyMember,
                Points = 1500,
                AwardDate = awardDate2
            }),
            Then(new PointsAwarded
            {
                MemberId = dummyMember,
                Points = 1500,
                AwardDate = awardDate2
            },
            new VoucherSent
            {
                MemberId = dummyMember,
                PointValue = 5000
            }));
    }

The approach taken draws on:

* **Behavior Driven Development**, for the given/when/then form of the tests
* **Domain Driven Design**, both in terms of using the aggregate pattern, but
  far more importantly because the *verb focus* this approach encourages helps
  better capture domain language
* **Functional Programming**, since commands and events are immutable, and the
  handling of commands is expressed in a pure way (but don't worry if these
  terms confuse you; you can still use this stuff!)

If you use this approach, you may also want to consider:

* **Event Sourcing**, which provides a persistence approach that fits well
  with a system built in terms of events
* **Command Query Responsibility Segregation**, because event sourcing is
  often great for our (command-centric) domain logic, but less good for
  queries

## So, it's a framework?

No. You take the code, copy it into your project, and massage it to fit your
needs. Not using part of it? **Just Delete It.** Wish something worked a little
differently? **Just Change It.**

The idea is to give you a head start, saving you from writing &mdash; or working
out how to write &mdash; this code yourself. In every non-trivial system where we've
used this code, we've done some kind of modification to it. We expect you will
experience the same need, and follow the same path.

## So, how do I get started?

Clone the repository, then take a look at the [tutorial](http://cqrs.nu/tutorial),
which walks through the [sample application](https://github.com/edumentab/cqrs-starter-kit/tree/master/sample-app).

You will need a C# compiler. The starter kit uses language features from C# 3,
however the sample application uses ASP.NET MVC 4, which needs at least Visual
Studio 2010.

You also need NUnit installed, and may need to correct the reference to it
when first loading the project.

You may also find our [DDD and CQRS FAQ](http://cqrs.nu/) useful.

## I want to do CQRS; is this for me?

Whoa there! Let's try and clear up what we mean by CQRS. Its literal meaning
is separating the parts of our system that read (queries) from those that
write (commands). This may simply mean that reads and writes take place on
different objects, or it may also imply that we use different data stores for
reads and writes.

For this literal sense of CQRS, you should not start out by saying you want to
do CQRS. Instead, you should arrive at it because it helps you to deliver on
some larger architectural goal.

* **Wrong:** I want to do CQRS because I heard it's the modern thing!
* **Right:** I want to work in terms of commands/events for my domain logic,
  because modeling and writing tests this way will help me deliver a system
  that meets the customer's needs. I want to persist using an event store,
  but that doesn't meet my querying needs. Therefore, I need to also have a
  relational database. This means my reads and writes need to happen through
  different code paths, which means I will be doing CQRS.

By now, however, CQRS has come to *imply* a lot of things besides the literal
separation of reads and writes. It evokes ideas of domain events, event
sourcing, BDD style testing, and a different way of approaching Domain Driven
Design. If you feel those kinds of ideas will be helpful to you in building a
system, then yes, the code we're offering here may serve as a good starting
point.

## Support and training

Edument offers commercial support and training on Domain Driven Design, CQRS,
and software architecture in general. [Contact us](http://www.edument.se/) for
more details.

## Who did this?

This code was built by Jonathan Worthington and Carl Mäsak as part of their
work at [Edument](http://www.edument.se/).
