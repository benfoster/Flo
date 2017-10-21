using System;
using System.Collections.Generic;

namespace Flo
{
    /// <summary>
    /// Provides linking and building functionality for chain of responsibility pipelines
    /// </summary>
    public abstract class Builder<TInput, TOutput>
    {
        private readonly List<Func<Func<TInput, TOutput>, Func<TInput, TOutput>>> _handlers
            = new List<Func<Func<TInput, TOutput>, Func<TInput, TOutput>>>();

        protected Func<TInput, TOutput> InnerHandler { get; set; }

        public Builder(Func<TInput, TOutput> innerHandler)
        {
            InnerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        }

        protected virtual void AddHandler(Func<TInput, Func<TInput, TOutput>, TOutput> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers.Add(next => input => handler.Invoke(input, next));
        }

        public virtual Func<TInput, TOutput> Build()
        {
            Func<TInput, TOutput> pipeline = InnerHandler;
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                var handler = _handlers[i];
                pipeline = handler.Invoke(pipeline);
            }

            return pipeline;
        }
    }
}