using System;
using System.Linq.Expressions;

namespace ProjectionsDsl.Core
{
    public interface IProjection<TState>
    {
        IProjection<TState> When<TEvent>(Expression<Func<TEvent, TState, TState>> handler, Expression<Func<TEvent, Guid>> id) where TEvent : IEvent;
    }
}