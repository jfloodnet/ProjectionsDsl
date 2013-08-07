using System;
using ProjectionsDsl.Core;

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
}
