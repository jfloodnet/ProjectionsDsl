using System;

namespace ProjectionsDsl.Core
{
    public interface IInitialiseProjectionState<TState>
    {
        IProjection<TState> Init(Func<TState> init);
    }
}