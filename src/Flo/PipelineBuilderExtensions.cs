using System;
using System.Threading.Tasks;

namespace Flo
{
    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder<TInput, TOutput> Terminate<TInput, TOutput>(
            this PipelineBuilder<TInput, TOutput> builder,
            Func<TInput, TOutput> handler)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            
            return builder.Add((ctx, next) => handler.Invoke(ctx));
        }

        public static PipelineBuilder<TInput, Task> When<TInput>(
            this PipelineBuilder<TInput, Task> builder,
            Func<TInput, bool> predicate,
            Action<PipelineBuilder<TInput, Task>> configurePipeline)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));

            return builder.When(predicate, async (input, innerPipeline, next) =>
            {
                await innerPipeline.Invoke(input);
                await next.Invoke(input);
            },
            configurePipeline);
        }
    }
}