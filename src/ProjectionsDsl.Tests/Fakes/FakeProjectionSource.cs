using System.Reactive.Subjects;
using ProjectionsDsl.Core;

namespace ProjectionsDsl.Tests.Fakes
{
    public class FakeProjectionSource : IProjectionSource
    {
        public Subject<IEvent> Events { get; set; }
    }
}