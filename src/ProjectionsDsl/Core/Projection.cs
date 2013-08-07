using System;

namespace ProjectionsDsl.Core
{
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
}