using System;

namespace ProjectionsDsl.Core
{
    public interface IInitialiseProjectionState<TState>
    {
        IProjection<TState> InitialiseState(Func<TState> init);
    }
}