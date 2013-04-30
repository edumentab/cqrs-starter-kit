using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Edument.CQRS
{
    /// <summary>
    /// Provides infrastructure for a set of tests on a given command handler
    /// and aggregate.
    /// </summary>
    /// <typeparam name="TCommandHandler"></typeparam>
    /// <typeparam name="TAggregate"></typeparam>
    public class BDDTest<TCommandHandler, TAggregate>
        where TCommandHandler : new()
        where TAggregate : Aggregate, new()
    {
        private TCommandHandler sut;

        [SetUp]
        public void BDDTestSetup()
        {
            sut = new TCommandHandler();
        }

        protected void Test(IEnumerable given, Func<TAggregate, object> when, Action<object> then)
        {
            then(when(ApplyEvents(new TAggregate(), given)));
        }

        protected IEnumerable Given(params object[] events)
        {
            return events;
        }

        protected Func<TAggregate, object> When<TCommand>(TCommand command)
        {
            return agg =>
            {
                try
                {
                    return DispatchCommand(_ => agg, command).Cast<object>().ToArray();
                }
                catch (Exception e)
                {
                    return e;
                }
            };
        }

        protected Action<object> Then(params object[] expectedEvents)
        {
            return got =>
            {
                var gotEvents = got as object[];
                if (gotEvents != null)
                {
                    if (gotEvents.Length == expectedEvents.Length)
                        for (var i = 0; i < gotEvents.Length; i++)
                            if (gotEvents[i].GetType() == expectedEvents[i].GetType())
                                Assert.AreEqual(Serialize(expectedEvents[i]), Serialize(gotEvents[i]));
                            else
                                Assert.Fail(string.Format(
                                    "Incorrect event in results; expected a {0} but got a {1}",
                                    expectedEvents[i].GetType().Name, gotEvents[i].GetType().Name));
                    else if (gotEvents.Length < expectedEvents.Length)
                        Assert.Fail(string.Format("Expected event(s) missing: {0}",
                            expectedEvents.Select(e => e.GetType().Name)
                                .Except(gotEvents.Select(e => e.GetType().Name))));
                    else
                        Assert.Fail(string.Format("Unexpected event(s) emitted: {0}",
                           gotEvents.Select(e => e.GetType().Name)
                               .Except(expectedEvents.Select(e => e.GetType().Name))));
                }
                else if (got is CommandHandlerNotDefiendException)
                    Assert.Fail((got as Exception).Message);
                else
                    Assert.Fail("Expected events, but got exception {0}",
                        got.GetType().Name);
            };
        }

        protected Action<object> ThenFailWith<TException>()
        {
            return got =>
            {
                if (got is TException)
                    Assert.Pass("Got correct exception type");
                else if (got is CommandHandlerNotDefiendException)
                    Assert.Fail((got as Exception).Message);
                else if (got is Exception)
                    Assert.Fail(string.Format(
                        "Expected exception {0}, but got exception {1}",
                        typeof(TException).Name, got.GetType().Name));
                else
                    Assert.Fail(string.Format(
                        "Expected exception {0}, but got event result",
                        typeof(TException).Name));
            };
        }

        private IEnumerable DispatchCommand<TCommand>(Func<Guid, TAggregate> al, TCommand c)
        {
            var handler = sut as IHandleCommand<TCommand, TAggregate>;
            if (handler == null)
                throw new CommandHandlerNotDefiendException(string.Format(
                    "Command handler {0} does not yet handle command {1}",
                    sut.GetType().Name, c.GetType().Name));
            return handler.Handle(al, c);
        }

        private TAggregate ApplyEvents(TAggregate agg, IEnumerable events)
        {
            agg.ApplyEvents(events);
            return agg;
        }

        private string Serialize(object obj)
        {
            var ser = new XmlSerializer(obj.GetType());
            var ms = new MemoryStream();
            ser.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return new StreamReader(ms).ReadToEnd();
        }

        private class CommandHandlerNotDefiendException : Exception
        {
            public CommandHandlerNotDefiendException(string msg) : base(msg) { }
        }
    }
}
