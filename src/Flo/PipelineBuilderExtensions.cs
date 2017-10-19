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
    }
}