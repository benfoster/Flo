using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;

namespace Flo.Tests
{
    class describe_Pipeline_Projecting_Output_Handler
    {
        async Task it_can_project_to_another_pipeline()
        {
            var pipeline = Pipeline.Build<string, string>(cfg => 
                cfg.Project(() => new Pipeline1Handler(), inner => inner.Add<Pipeline2Handler>())
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe("HELLO WORLD");
        }

        async Task it_can_project_another_pipeline_with_return_result()
        {
            var pipeline = Pipeline.Build<string, string>(cfg => 
                cfg.Project(() => new Pipeline1OutputHandler(), inner => inner.Add<Pipeline2OutputHandler>())
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe("dlrow olleh");
        }

        async Task it_can_project_using_handler_type()
        {
            var pipeline = Pipeline.Build<string, string>(cfg => 
                cfg.Project<Pipeline2Input, Pipeline1Handler>(inner => inner.Add<Pipeline2Handler>())
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe("HELLO WORLD");
        }

        async Task it_can_project_to_output_handler_using_handler_type()
        {
            var pipeline = Pipeline.Build<string, string>(cfg => 
                cfg.Project<Pipeline2Input, string, Pipeline1OutputHandler>(inner => inner.Add<Pipeline2OutputHandler>())
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe("dlrow olleh");
        }

        class Pipeline2Input 
        {
            public string Message {get;set;}
        }

        class Pipeline1Handler : IProjectingOutputHandler<string, Pipeline2Input, string>
        {
            public async Task<string> HandleAsync(string input, Func<Pipeline2Input, Task> projectedPipeline, Func<string, Task<string>> next)
            {
                var pipelineInput  = new Pipeline2Input
                {
                    Message = input
                };

                await projectedPipeline.Invoke(pipelineInput);
                return pipelineInput.Message;
            }
        }

        class Pipeline1OutputHandler : IProjectingOutputHandler<string, Pipeline2Input, string, string>
        {
            public Task<string> HandleAsync(string input, Func<Pipeline2Input, Task<string>> projectedPipeline, Func<string, Task<string>> next)
            {
                var pipelineInput  = new Pipeline2Input
                {
                    Message = input
                };

                return projectedPipeline.Invoke(pipelineInput);
            }
        }

        class Pipeline2Handler : IHandler<Pipeline2Input>
        {
            public Task HandleAsync(Pipeline2Input input, Func<Pipeline2Input, Task> next)
            {
                input.Message = input.Message.ToUpper();
                return Task.CompletedTask;
            }
        }

        class Pipeline2OutputHandler : IOutputHandler<Pipeline2Input, string>
        {
            public Task<string> HandleAsync(Pipeline2Input input, Func<Pipeline2Input, Task<string>> next)
            {
                string reversed = new string(input.Message.ToCharArray().Reverse().ToArray());
                return Task.FromResult(reversed);
            }
        }
    }
}