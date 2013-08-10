using EventStore.ClientAPI;
using ProjectionsDsl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ProjectionsDsl.GEventStore
{
    public class GetEventStoreProjectionSource : IProjectionSource 
    {
        private readonly IEventStoreConnection connection;
        private readonly Subject<ResolvedEvent> events = new Subject<ResolvedEvent>();

        public GetEventStoreProjectionSource(IEventStoreConnection connection)
        {
            this.connection = connection;
        }

        public Subject<IEvent> Events
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Subject<ResolvedEvent> SubscribeToAllFrom(Position? position)
        {
            this.connection.SubscribeToAllFrom(position, false, (s, e) => events.OnNext(e), null, HandleSubscriptionDropped, null);
            return events;
        }

        private void HandleSubscriptionDropped(EventStoreCatchUpSubscription subscription, SubscriptionDropReason reason, Exception ex)
        {
            
        }

    }
}
