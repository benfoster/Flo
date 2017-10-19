using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flo
{   
    public class PipelineBuilder<TInput, TOutput>
    {
        private readonly List<Func<Func<TInput, TOutput>, Func<TInput, TOutput>>> handlers
            = new List<Func<Func<TInput, TOutput>, Func<TInput, TOutput>>>();

        private Func<TInput, TOutput> _innerHandler = input => default(TOutput);

        public PipelineBuilder<TInput, TOutput> WithFinalHandler(Func<TInput, TOutput> innerHandler)
        {
            _innerHandler = innerHandler;
            return this;
        }

        public PipelineBuilder<TInput, TOutput> Add(Func<TInput, Func<TInput, TOutput>, TOutput> handler)
        {
            handlers.Add(next => ctx => handler.Invoke(ctx, next));
            return this;
        }

        public PipelineBuilder<TInput, TOutput> When(
            Func<TInput, bool> predicate,
            Func<TInput, Func<TInput, TOutput>, Func<TInput, TOutput>, TOutput> handler,
            Action<PipelineBuilder<TInput, TOutput>> configurePipeline)
        {
            return Add((input, next) =>
            {
                if (predicate.Invoke(input))
                {
                    var builder = new PipelineBuilder<TInput, TOutput>()
                        .WithFinalHandler(_innerHandler);
                    
                    configurePipeline(builder);
                    return handler.Invoke(input, builder.Build(), next);
                }

                return next.Invoke(input);
            });
        }

        public PipelineBuilder<TInput, TOutput> Project<TProjectedInput, TProjectedOutput>(
            Func<TInput, Func<TProjectedInput, TProjectedOutput>, Func<TInput, TOutput>, TOutput> handler,
            Action<PipelineBuilder<TProjectedInput, TProjectedOutput>> configurePipeline)
        {
            return Add((input, next) =>
            {
                var builder = new PipelineBuilder<TProjectedInput, TProjectedOutput>();
                configurePipeline(builder);
                return handler.Invoke(input, builder.Build(), next);
            });
        }

        public Func<TInput, TOutput> Build()
        {
            Func<TInput, TOutput> pipeline = _innerHandler;
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                var handler = handlers[i];
                pipeline = handler.Invoke(pipeline);
            }

            return pipeline;
        }
    }
}