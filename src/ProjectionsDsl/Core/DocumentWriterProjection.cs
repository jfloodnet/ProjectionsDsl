using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using ProjectionsDsl.Common;

namespace ProjectionsDsl.Core
{
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
}