using System;
using System.Linq;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_Pipeline_Projecting_Handler : nspec
    {
        async Task it_can_project_to_another_pipeline()
        {
            var pipeline = Pipeline.Build<Pipeline1Input>(cfg => 
                cfg.Project(() => new Pipeline1Handler(), inner => inner.Add<Pipeline2Handler>())
            );

            var input = new Pipeline1Input { Message = "hello world" };
            await pipeline.Invoke(input);

            input.Message.ShouldBe("HELLO WORLD");
        }

        async Task it_can_project_to_an_output_handler()
        {
            var pipeline = Pipeline.Build<Pipeline1Input>(cfg => 
                cfg.Project(() => new Pipeline1OutputHandler(), inner => inner.Add<Pipeline2OutputHandler>())
            );

            var input = new Pipeline1Input { Message = "hello world" };
            await pipeline.Invoke(input);

            input.Message.ShouldBe("dlrow olleh");
        }

        class Pipeline1Input
        {
            public string Message {get;set;}
        }

        class Pipeline2Input 
        {
            public string Message {get;set;}
        }

        class Pipeline1Handler : IProjectingHandler<Pipeline1Input, Pipeline2Input>
        {
            public async Task HandleAsync(Pipeline1Input input, Func<Pipeline2Input, Task> projectedPipeline, Func<Pipeline1Input, Task> next)
            {
                var nextInput = new Pipeline2Input 
                {
                    Message = input.Message
                };

                await projectedPipeline.Invoke(nextInput);
                input.Message = nextInput.Message;
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

        class Pipeline1OutputHandler : IProjectingHandler<Pipeline1Input, string, string>
        {
            public async Task HandleAsync(Pipeline1Input input, Func<string, Task<string>> projectedPipeline, Func<Pipeline1Input, Task> next)
            {
                input.Message = await projectedPipeline.Invoke(input.Message);
            }
        }

        class Pipeline2OutputHandler : IOutputHandler<string, string>
        {
            public Task<string> HandleAsync(string input, Func<string, Task<string>> next)
            {
                string reversed = new string(input.ToCharArray().Reverse().ToArray());
                return Task.FromResult(reversed);
            }
        }
    }
}