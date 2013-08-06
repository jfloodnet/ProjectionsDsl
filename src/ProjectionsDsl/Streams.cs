﻿using ProjectionsDsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ProjectionsDsl
{
    public class Streams
    {
        public static ConfigureEventSource Configure()
        {
            return new ConfigureEventSource();
        }

        public class ConfigureEventSource
        {
            public ConfigureWriter WithEventSource(IProjectionSource source)
            {
                return new ConfigureWriter(source);
            }

            public class ConfigureWriter
            {
                private IProjectionSource source;

                public ConfigureWriter(IProjectionSource source)
                {
                    this.source = source;
                }

                public ConfigureProjection<TState> WithProjectionWriter<TState>(IDocumentWriter<Guid, TState> writer)
                {
                    return new ConfigureProjection<TState>(this.source, writer);
                }

                public class ConfigureProjection<TState>
                {
                    private IProjectionSource projectionSource;
                    private IDocumentWriter<Guid, TState> writer;

                    public ConfigureProjection(IProjectionSource projectionSource, IDocumentWriter<Guid, TState> writer)
                    {
                        this.projectionSource = projectionSource;
                        this.writer = writer;
                    }
                    public Projection<TState> Build()
                    {
                        return new Projection<TState>(projectionSource, writer);
                    }
                }
            }
        }
    }

    public class Projection<TState>
    {
        public IProjectionSource Source { get; private set; }
        public IDocumentWriter<Guid, TState> Writer { get; private set; }

        public Projection(IProjectionSource projectionSource, IDocumentWriter<Guid, TState> writer)
        {
            this.Source = projectionSource;
            this.Writer = writer;
        }

        public IProjection<TState> InitialiseState(Func<TState> init)
        {
            return new DocumentWriterProjection<TState>(Source, Writer, init);
        }
    }

    public class DocumentWriterProjection<TState> : IProjection<TState>
    {
        private IProjectionSource source;
        private IDocumentWriter<Guid, TState> writer;
        private Func<TState> init;

        public DocumentWriterProjection(IProjectionSource source, IDocumentWriter<Guid, TState> writer, Func<TState> init)
        {
            this.source = source;
            this.writer = writer;
            this.init = init;
        }

        public IProjection<TState> When<TEvent>(Expression<Func<TEvent, TState, TState>> handler, Expression<Func<TEvent, Guid>> id) where TEvent : IEvent
        {
            var idhandler = DelegateAdjuster.CastArgument(id);
            var handlerFunc = DelegateAdjuster.CastArgument(handler);
            source.Events.Where(x => x.GetType() == typeof(TEvent))
                .Subscribe(e =>
                    this.writer.AddOrUpdate(idhandler(e), init, state => handlerFunc(e, state))
                );
            return this;
        }       
    }

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

    public interface IProjectionSource
    {
        Subject<IEvent> Events { get; set; }
    }


    public interface IInitialiseProjectionState<TState>
    {
        IProjection<TState> Init(Func<TState> init);
    }

    public interface IProjection<TState>
    {
        IProjection<TState> When<TEvent>(Expression<Func<TEvent, TState, TState>> handler, Expression<Func<TEvent, Guid>> id) where TEvent : IEvent;
    }

    /// <summary>
    /// This is a sample of strongly-typed document writer interface,
    /// which works good for building simple systems that can be migrated
    /// between cloud and various on-premises implementations.
    /// This interface supports automated view rebuilding (which is
    /// demonstrated in greater detail in Lokad.CQRS project)
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IDocumentWriter<in TKey, TEntity>
    {
        TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists);
        bool TryDelete(TKey key);
    }

    public enum AddOrUpdateHint
    {
        ProbablyExists,
        ProbablyDoesNotExist
    }

}
