using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reactive.Subjects;
using System.Text;

namespace ProjectionsDsl.Core
{
    public class Projection<TState>
    {
        private readonly IEventStoreConnection connection;
        private readonly ICheckPointRepository checkpoint;
        public Subject<IEvent> Source { get; private set; }
        public IDocumentWriter<Guid, TState> Writer { get; private set; }

        public Projection(
            IEventStoreConnection connection, 
            ICheckPointRepository checkpoint,  
            IDocumentWriter<Guid, TState> writer)
        {
            this.connection = connection;
            this.Writer = writer;
            this.checkpoint = checkpoint;
            this.Source = new Subject<IEvent>();
        }

        public IInitialiseProjectionState<TState> FromAll()
        {
            RecoverSubscription();
            return new Initialiser<TState>(this.Source, this.Writer);
        }

        private void RecoverSubscription()
        {
            this.connection.SubscribeToAllFrom(
                this.checkpoint.GetLastPosition(),
                false, HandleEventAppeared, null, HandleDrop);
        }

        private void HandleDrop(
            EventStoreCatchUpSubscription subscription, 
            SubscriptionDropReason reason, 
            Exception ex)
        {
            RecoverSubscription();
        }

        private void HandleEventAppeared(
            EventStoreCatchUpSubscription subscription, 
            ResolvedEvent @event)
        {
            IEvent theEvent;
            if (TryDeserialiseEvent(@event, out theEvent))
            {
                Source.OnNext(theEvent);
                this.checkpoint.StorePosition(@event.OriginalPosition.Value);
            }
        }


        private static bool TryDeserialiseEvent(ResolvedEvent resolvedEvent, out IEvent outEvent)
        {
            outEvent = null;

            try
            {
                outEvent = DeserializeEvent(resolvedEvent.OriginalEvent.Metadata, resolvedEvent.OriginalEvent.Data);                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deserializes the event from the raw GetEventStore event to my event.
        /// Took this from a gist that James Nugent posted on the GetEventStore forumns.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IEvent DeserializeEvent(byte[] metadata, byte[] data)
        {
            var property = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property("EventClrTypeName");
            return (IEvent)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)property));
        }

        public class Initialiser<TState> : IInitialiseProjectionState<TState>
        {
            public Subject<IEvent> Source { get; private set; }
            public IDocumentWriter<Guid, TState> Writer { get; private set; }

            public Initialiser(Subject<IEvent> projectionSource, IDocumentWriter<Guid, TState> writer)
            {
                this.Source = projectionSource;
                this.Writer = writer;
            }

            public IProjection<TState> InitialiseState(Func<TState> init)
            {
                return new DocumentWriterProjection<TState>(Source, Writer, init);
            }
        }
    }
}