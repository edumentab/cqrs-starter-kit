using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Edument.CQRS
{
    /// <summary>
    /// Aggregate base class, which factors out some common infrastructure that
    /// all aggregates have (ID and event application).
    /// </summary>
    public class Aggregate
    {
        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        public int EventsLoaded { get; private set; }

        /// <summary>
        /// The unique ID of the aggregate.
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        /// Enuerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="events"></param>
        public void ApplyEvents(IEnumerable events)
        {
            foreach (var e in events)
                GetType().GetMethod("ApplyOneEvent")
                    .MakeGenericMethod(e.GetType())
                    .Invoke(this, new object[] { e });
        }

        /// <summary>
        /// Applies a single event to the aggregate.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="ev"></param>
        public void ApplyOneEvent<TEvent>(TEvent ev)
        {
            var applier = this as IApplyEvent<TEvent>;
            if (applier == null)
                throw new InvalidOperationException(string.Format(
                    "Aggregate {0} does not know how to apply event {1}",
                    GetType().Name, ev.GetType().Name));
            applier.Apply(ev);
            EventsLoaded++;
        }
    }
}
