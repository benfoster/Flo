using System;
using System.Threading.Tasks;

namespace Flo
{
    public class Pipeline
    {
        public static Func<TInput, Task> Build<TInput>(
            Action<PipelineBuilder<TInput>> configurePipeline,
            Func<Type, object> serviceProvider = null)
        {
            var pipelineBuilder = new PipelineBuilder<TInput>(serviceProvider);
            configurePipeline(pipelineBuilder);
            return pipelineBuilder.Build();
        }

        public static Func<TInput, Task<TOutput>> Build<TInput, TOutput>(
            Action<PipelineBuilder<TInput, TOutput>> configurePipeline,
            Func<Type, object> serviceProvider = null)
        {
            var pipelineBuilder = new PipelineBuilder<TInput, TOutput>(serviceProvider);
            configurePipeline(pipelineBuilder);
            return pipelineBuilder.Build();
        }
    }
}