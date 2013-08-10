using System.Linq;
using ProjectionsDsl.Core;
using ProjectionsDsl.Tests.Asserts;
using ProjectionsDsl.Tests.AutoFixture;
using ProjectionsDsl.Tests.Fakes;
using Xunit;
using Xunit.Extensions;
using System.Reactive.Subjects;
using System.Net;
using EventStore.ClientAPI;
using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Moq;
using System.Threading;

namespace ProjectionsDsl.Tests
{
    public class DslTests
    {
        [Theory, AutoMoqData]
        public void WithoutBuilder(
            Subject<IEvent> source, FakeDocumentWriter writer, TestEvent @event)
        {
            writer.DB.Clear();
            new DocumentWriterProjection<ListOfProjectedState>(
                 source, writer, () => new ListOfProjectedState())
                .When<TestEvent>((e, s) =>
                                 s.AddState(
                                     new TestProjectionState(e.SomeState)),
                                 e => e.Id);

            source.OnNext(@event);

            writer.DB.Select(x => x.Value).ShouldContain<ListOfProjectedState>();
        }

        [Theory, AutoMoqData]
        public void ShouldExecuteWhenForEvent(
            Subject<IEvent> source, FakeDocumentWriter writer, TestEvent @event)
        {
            writer.DB.Clear();

            new DocumentWriterProjection<ListOfProjectedState>(
                source, writer, () => new ListOfProjectedState())                
                .When<TestEvent>((e, s) =>
                                 s.AddState(
                                     new TestProjectionState(e.SomeState)),
                                 e => e.Id);

            source.OnNext(@event);

            writer.DB.Select(x => x.Value).ShouldContain<ListOfProjectedState>();
        }

        [Theory, AutoMoqData]
        public void ShouldNotExecuteWhenForEventsNotInterestedIn(
            Subject<IEvent> source, FakeDocumentWriter writer, NotInterestedInThis @event)
        {
            writer.DB.Clear();
   
            new DocumentWriterProjection<ListOfProjectedState>(
                source, writer, () => new ListOfProjectedState())    
                .When<TestEvent>((e, s) =>
                                 s.AddState(
                                     new TestProjectionState(e.SomeState)),
                                 e => e.Id);

            source.OnNext(@event);

            writer.DB.Select(x => x.Value).ShouldBeEmpty();
        }

        //[Theory, AutoMoqData]
        //public void ConfigureProjectionSourceAndWriter(Subject<IEvent> source, FakeDocumentWriter writer)
        //{
        //    var projection = Streams.Configure()
        //                            .WithEventSource(source)
        //                            .WithProjectionWriter(writer)
        //                            .Build();

        //    Assert.IsType(typeof(Subject<IEvent>), projection.Source);
        //    Assert.IsType(typeof(FakeDocumentWriter), projection.Writer);
        //}

        [Theory, AutoMoqData]
        public void EventStoreProjection(Mock<ICheckPointRepository> checkpoint,
            Subject<IEvent> source, FakeDocumentWriter writer, TestEvent @event)
        {
            var settings = EventStore.ClientAPI.ConnectionSettings.Create();
            settings.SetDefaultUserCredentials(new EventStore.ClientAPI.SystemData.UserCredentials("admin", "changeit"));

            var connection = EventStore.ClientAPI.EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, 1113));

            connection.Connect();

            new Projection<ListOfProjectedState>(connection,checkpoint.Object, writer)
                 .FromAll()
                 .InitialiseState(() => new ListOfProjectedState())
                 .When<TestEvent>((e, s) =>
                                  s.AddState(
                                      new TestProjectionState(e.SomeState)),
                                  e => e.Id);

            connection.AppendToStream("test", ExpectedVersion.Any, JsonEventData.Create(Guid.NewGuid(), @event));

            while (true) { Thread.Sleep(500); }
        }

        private static class JsonEventData
        {
            private const string EventClrTypeHeader = "EventClrTypeName";
            public static EventData Create(Guid eventId, object evnt)
            {

                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));
                var metadata = AddEventClrTypeHeaderAndSerializeMetadata(evnt);
                var typeName = evnt.GetType().Name;

                return new EventData(eventId, typeName, true, data, metadata);
            }

            private static readonly JsonSerializerSettings SerializerSettings;
            private static byte[] AddEventClrTypeHeaderAndSerializeMetadata(object evnt)
            {
                var eventHeaders = new Dictionary<string, object>()
                {
                    {EventClrTypeHeader, evnt.GetType().AssemblyQualifiedName}
                };

                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
            }

            static JsonEventData()
            {
                SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            }
        }    
    }
}
