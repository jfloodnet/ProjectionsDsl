using System.Reactive.Subjects;

namespace ProjectionsDsl.Core
{
    public interface IProjectionSource
    {
        Subject<IEvent> Events { get; set; }
    }
}