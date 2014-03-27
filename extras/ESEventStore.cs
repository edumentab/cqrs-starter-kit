using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using EventStore.ClientAPI;

namespace Edument.CQRS
{
    // An implementation of IEventStore in terms of EventStore, available from
    // http://geteventstore.com/
    public class ESEventStore : IEventStore
    {
        private IEventStoreConnection conn = EventStoreConnection
            .Create(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1113));

        public ESEventStore()
        {
            conn.Connect();
        }

        public IEnumerable LoadEventsFor<TAggregate>(Guid id)
        {
            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice = conn.ReadStreamEventsForward(id.ToString(), nextSliceStart, 200, false);
                foreach (var e in currentSlice.Events)
                    yield return Deserialize(e.Event.EventType, e.Event.Data);
                nextSliceStart = currentSlice.NextEventNumber;
            } while (!currentSlice.IsEndOfStream);
        }

        private object Deserialize(string typeName, byte[] data)
        {
            var ser = new XmlSerializer(Type.GetType(typeName));
            var ms = new MemoryStream(data);
            ms.Seek(0, SeekOrigin.Begin);
            return ser.Deserialize(ms);
        }

        public void SaveEventsFor<TAggregate>(Guid? id, int eventsLoaded, ArrayList newEvents)
        {
            // Establish the aggregate ID to save the events under and ensure they
            // all have the correct ID.
            if (newEvents.Count == 0)
                return;
            Guid aggregateId = id ?? GetAggregateIdFromEvent(newEvents[0]);
            foreach (var e in newEvents)
                if (GetAggregateIdFromEvent(e) != aggregateId)
                    throw new InvalidOperationException(
                        "Cannot save events reporting inconsistent aggregate IDs");

            var expected = eventsLoaded == 0 ? ExpectedVersion.NoStream : eventsLoaded - 1;
            conn.AppendToStream(aggregateId.ToString(), expected, newEvents
                .Cast<dynamic>()
                .Select(e => new EventData(e.Id, e.GetType().AssemblyQualifiedName,
                    false, Serialize(e), null)));
        }

        private Guid GetAggregateIdFromEvent(object e)
        {
            var idField = e.GetType().GetField("Id");
            if (idField == null)
                throw new Exception("Event type " + e.GetType().Name + " is missing an Id field");
            return (Guid)idField.GetValue(e);
        }

        private byte[] Serialize(object obj)
        {
            var ser = new XmlSerializer(obj.GetType());
            var ms = new MemoryStream();
            ser.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
    }
}
