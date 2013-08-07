using System;
using System.Linq.Expressions;
using ProjectionsDsl.Core;

namespace ProjectionsDsl.Common
{
    public class DelegateAdjuster
    {
        public static Func<IEvent, TState, TState> CastArgument<TEvent, TState>(Expression<Func<TEvent, TState, TState>> source) where TEvent : IEvent
        {
            if (typeof(TEvent) == typeof(IEvent))
            {
                return (Func<IEvent, TState, TState>)((Delegate)source.Compile());

            }
            var eventParameter = Expression.Parameter(typeof(IEvent), "source");
            var stateParameter = Expression.Parameter(typeof(TState));
            var converted = Expression.Convert(eventParameter, typeof(TEvent));
            var invocation = Expression.Invoke(
                source,
                converted,
                stateParameter);
            var result = Expression.Lambda<Func<IEvent, TState, TState>>(
                invocation,
                eventParameter, stateParameter);

            return result.Compile();
        }

        public static Func<IEvent, Guid> CastArgument<TEvent>(Expression<Func<TEvent, Guid>> source) where TEvent : IEvent
        {
            if (typeof(TEvent) == typeof(IEvent))
            {
                return (Func<IEvent, Guid>)((Delegate)source.Compile());

            }
            var eventParameter = Expression.Parameter(typeof(IEvent), "source");
            var converted = Expression.Convert(eventParameter, typeof(TEvent));
            var invocation = Expression.Invoke(
                source,
                converted);
            var result = Expression.Lambda<Func<IEvent, Guid>>(
                invocation,
                eventParameter);

            return result.Compile();
        }
    }
}