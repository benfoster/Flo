using System;
using System.Threading.Tasks;

namespace Flo
{
    public class OutputPipelineBuilder<TIn, TOut> 
        : Builder<TIn, TOut, OutputPipelineBuilder<TIn, TOut>>
    {       
        private static Func<TOut, bool> DefaultContinuation = output => output == null;

        public OutputPipelineBuilder(Func<Type, object> serviceProvider = null) 
            : base(input => Task.FromResult(default(TOut)), serviceProvider)
        {
        }

        public OutputPipelineBuilder<TIn, TOut> When(
            Func<TIn, bool> predicate,
            Action<OutputPipelineBuilder<TIn, TOut>> configurePipeline,
            Func<TOut, bool> continueIf = null)
        {
            return When(predicate, async (input, innerPipeline, next) =>
            {
                var result = await innerPipeline.Invoke(input);
                
                // Continue on to the parent pipeline if the continueIf output predicate matches
                if ((continueIf ?? DefaultContinuation).Invoke(result))                
                    return await next.Invoke(input);

                return result;
            },
            configurePipeline);
        }

        public OutputPipelineBuilder<TIn, TOut> When(
            Func<TIn, Task<bool>> predicate,
            Action<OutputPipelineBuilder<TIn, TOut>> configurePipeline,
            Func<TOut, bool> continueIf = null)
        {
            return When(predicate, async (input, innerPipeline, next) =>
            {
                var result = await innerPipeline.Invoke(input);

                // Continue on to the parent pipeline if the continueIf output predicate matches
                if ((continueIf ?? DefaultContinuation).Invoke(result))
                    return await next.Invoke(input);

                return result;
            },
            configurePipeline);
        }

        protected override OutputPipelineBuilder<TIn, TOut> CreateBuilder() => new OutputPipelineBuilder<TIn, TOut>(ServiceProvider);
    }
}