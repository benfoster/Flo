using System;
using System.Threading.Tasks;

namespace Flo
{
    public class Pipeline
    {
        public static Func<T, Task<T>> Build<T>(
            Action<PipelineBuilder<T>> configurePipeline,
            Func<Type, object> serviceProvider = null)
        {
            var pipelineBuilder = new PipelineBuilder<T>(serviceProvider);
            configurePipeline(pipelineBuilder);
            return pipelineBuilder.Build();
        }

        public static Func<TIn, Task<TOut>> Build<TIn, TOut>(
            Action<OutputPipelineBuilder<TIn, TOut>> configurePipeline,
            Func<Type, object> serviceProvider = null)
        {
            var pipelineBuilder = new OutputPipelineBuilder<TIn, TOut>(serviceProvider);
            configurePipeline(pipelineBuilder);
            return pipelineBuilder.Build();
        }
    }
}