using System.Linq;
using ProjectionsDsl.Core;
using ProjectionsDsl.Tests.Asserts;
using ProjectionsDsl.Tests.AutoFixture;
using ProjectionsDsl.Tests.Fakes;
using Xunit;
using Xunit.Extensions;

namespace ProjectionsDsl.Tests
{
    public class DslTests
    {
        [Theory, AutoMoqData]
        public void WithoutBuilder(
            FakeProjectionSource source, FakeDocumentWriter writer, TestEvent @event)
        {
            writer.DB.Clear();
            new Projection<ListOfProjectedState>(source, writer)
                .InitialiseState(() => new ListOfProjectedState())
                .When<TestEvent>((e, s) =>
                                 s.AddState(
                                     new TestProjectionState(e.SomeState)),
                                 e => e.Id);

            source.Events.OnNext(@event);

            writer.DB.Select(x => x.Value).ShouldContain<ListOfProjectedState>();
        }

        [Theory, AutoMoqData]
        public void ShouldExecuteWhenForEvent(
            FakeProjectionSource source, FakeDocumentWriter writer, TestEvent @event)
        {
            writer.DB.Clear();
            var projection = Streams.Configure()
                                    .WithEventSource(source)
                                    .WithProjectionWriter(writer)
                                    .Build();

            projection
                .InitialiseState(() => new ListOfProjectedState())
                .When<TestEvent>((e, s) =>
                                 s.AddState(
                                     new TestProjectionState(e.SomeState)),
                                 e => e.Id);

            source.Events.OnNext(@event);

            writer.DB.Select(x => x.Value).ShouldContain<ListOfProjectedState>();
        }

        [Theory, AutoMoqData]
        public void ShouldNotExecuteWhenForEventsNotInterestedIn(
            FakeProjectionSource source, FakeDocumentWriter writer, NotInterestedInThis @event)
        {
            writer.DB.Clear();
            var projection = Streams.Configure()
                                    .WithEventSource(source)
                                    .WithProjectionWriter(writer)
                                    .Build();

            projection
                .InitialiseState(() => new ListOfProjectedState())
                .When<TestEvent>((e, s) =>
                                 s.AddState(
                                     new TestProjectionState(e.SomeState)),
                                 e => e.Id);

            source.Events.OnNext(@event);

            writer.DB.Select(x => x.Value).ShouldBeEmpty();
        }

        [Fact]
        public void ConfigureProjectionSourceAndWriter()
        {
            var projection = Streams.Configure()
                                    .WithEventSource(new FakeProjectionSource())
                                    .WithProjectionWriter(new FakeDocumentWriter())
                                    .Build();

            Assert.IsType(typeof(FakeProjectionSource), projection.Source);
            Assert.IsType(typeof(FakeDocumentWriter), projection.Writer);
        }
    }
}
