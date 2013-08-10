//using System;
//using ProjectionsDsl.Core;
//using System.Reactive.Subjects;

//namespace ProjectionsDsl
//{
//    public class Streams
//    {
//        public static ConfigureEventSource Configure()
//        {
//            return new ConfigureEventSource();
//        }

//        public class ConfigureEventSource
//        {
//            public ConfigureWriter WithEventSource(Subject<IEvent> source)
//            {
//                return new ConfigureWriter(source);
//            }

//            public class ConfigureWriter
//            {
//                private Subject<IEvent> source;

//                public ConfigureWriter(Subject<IEvent> source)
//                {
//                    this.source = source;
//                }

//                public ConfigureProjection<TState> WithProjectionWriter<TState>(IDocumentWriter<Guid, TState> writer)
//                {
//                    return new ConfigureProjection<TState>(this.source, writer);
//                }

//                public class ConfigureCheckPoints<TState>
//                {
//                    private Subject<IEvent> projectionSource;
//                    private IDocumentWriter<Guid, TState> writer;

//                    public ConfigureCheckPoints(Subject<IEvent> projectionSource, IDocumentWriter<Guid, TState> writer)
//                    {
//                        this.projectionSource = projectionSource;
//                        this.writer = writer;
//                    }

//                    public ConfigureProjection<TState> WithCheckPoints(ICheckPointRepository repository)
//                    {
//                        return new ConfigureProjection<TState>(projectionSource, 
//                    }


//                    public class ConfigureProjection<TState>
//                    {
//                        private Subject<IEvent> projectionSource;
//                        private IDocumentWriter<Guid, TState> writer;

//                        public ConfigureProjection(Subject<IEvent> projectionSource, IDocumentWriter<Guid, TState> writer)
//                        {
//                            this.projectionSource = projectionSource;
//                            this.writer = writer;
//                        }
//                        public Projection<TState> Build()
//                        {
//                            return new Projection<TState>(projectionSource, writer);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
