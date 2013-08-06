using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace ProjectionsDsl.Tests
{
    public class DslTests
    {
        [Theory, AutoMoqData]
        public void ShouldExecuteWhenForEvent(
            FakeProjectionSource source, FakeDocumentWriter writer, TestEvent @event)
        {
            writer.DB.Clear();
            var projection = Streams.Configure()
                 .WithEventSource(source)
                 .WithProjectionWriter(writer)
                 .Build();

            projection.FromAll()
                .Init(() => new ListOfProjectedState())
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

            projection.FromAll()
                .Init(() => new ListOfProjectedState())
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

        public class FakeProjectionSource : IProjectionSource
        {
            public Subject<IEvent> Events { get; set; }
        }

        public class FakeDocumentWriter : IDocumentWriter<Guid, ListOfProjectedState>
        {
            public Dictionary<Guid, ListOfProjectedState> DB = new Dictionary<Guid, ListOfProjectedState>();
            public ListOfProjectedState AddOrUpdate(Guid key, Func<ListOfProjectedState> addFactory, Func<ListOfProjectedState, ListOfProjectedState> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
            {
                ListOfProjectedState document;
                if (DB.ContainsKey(key))
                {
                    document = DB[key];
                    update(document);
                }
                else
                {
                    document = addFactory();
                    DB.Add(key, document);
                }
                return document;
            }

            public bool TryDelete(Guid key)
            {
                DB.Remove(key);
                return true;
            }
        }

        public class NotInterestedInThis : IEvent
        {

        }


        public class ListOfProjectedState
        {
            public List<TestProjectionState> Events = new List<TestProjectionState>();

            public ListOfProjectedState AddState(TestProjectionState state)
            {
                Events.Add(state);
                return this;
            }
        }

        public class TestEvent : IEvent
        {
            public TestEvent(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; set; }
            public string SomeState { get; set; }
        }

        public class TestProjectionState
        {
            public TestProjectionState(string someState)
            {
                SomeState = someState;
            }

            public string SomeState { get; set; }
        }
    }

    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() :
            base(new Fixture().Customize(new AutoMoqCustomization()))
        {

        }
    }

    /// <summary>
    /// Assert (throw exceptions) 
    /// </summary>
    public static class ShouldAssertions
    {
        public static void ShouldContainOnly<T>(this IEnumerable<object> messages) where T : class
        {
            ShouldContainOnly<T>(messages, null);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<object> messages, Func<T, bool> isExpected) where T : class
        {
            Assert.Equal(1, messages.Count());
            ShouldContain(messages, isExpected);
        }

        public static void ShouldContain<T>(this IEnumerable<object> messages)
            where T : class
        {
            ShouldContain<T>(messages, null);
        }

        public static void ShouldContain<T>(this IEnumerable<object> messages, Func<T, bool> isExpected) where T : class
        {
            Assert.True(messages.Any(m =>
            {
                return m is T && (isExpected == null || isExpected(m as T));
            }));
        }

        public static void ShouldBeEmpty(this IEnumerable<object> messages)
        {
            Assert.Empty(messages);
        }
    }
}
