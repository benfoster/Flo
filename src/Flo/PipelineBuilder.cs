using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flo
{
    public class PipelineBuilder<T> : Builder<T, T, PipelineBuilder<T>> // : Builder<T, Task<T>>
    {
        public PipelineBuilder(Func<Type, object> serviceProvider = null)
            : base(input => Task.FromResult(input), serviceProvider)
        {
            InnerHandler = input => Task.FromResult(input);
        }

        public PipelineBuilder<T> When(
            Func<T, bool> predicate,
            Action<PipelineBuilder<T>> configurePipeline)
        {
            return When(predicate, async (input, innerPipeline, next) =>
            {
                input = await innerPipeline.Invoke(input);
                return await next.Invoke(input);
            },
            configurePipeline);
        }

        public PipelineBuilder<T> When(
            Func<T, Task<bool>> predicate,
            Action<PipelineBuilder<T>> configurePipeline)
        {
            return When(predicate, async (input, innerPipeline, next) =>
            {
                input = await innerPipeline.Invoke(input);
                return await next.Invoke(input);
            },
            configurePipeline);
        }

        protected override PipelineBuilder<T> CreateBuilder() => new PipelineBuilder<T>(ServiceProvider);
    }
}