using System;
using System.Threading.Tasks;

namespace Flo
{
    public class Pipeline
    {
        public static Func<TInput, TOutput> Build<TInput, TOutput>(Action<PipelineBuilder<TInput, TOutput>> configurePipeline)
        {
            var pipelineBuilder = new PipelineBuilder<TInput, TOutput>();
            configurePipeline(pipelineBuilder);
            return pipelineBuilder.Build();
        }
    }
}