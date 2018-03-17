using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_OutputPipelineBuilder : nspec
    {
        async Task it_can_execute_single_handler()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.Add((input, next) => {
                    return Task.FromResult(input.Length);
                })
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe(11);
        }

        async Task it_can_execute_multiple_handlers()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.Add((input, next) => {
                    input += "hello";
                    return next.Invoke(input);
                })
                .Add((input, next) => {
                    input += " world";
                    return next.Invoke(input);
                })
                .Add((input, next) => {
                    return Task.FromResult(input.Length);
                })
            );

            var result = await pipeline.Invoke("");
            result.ShouldBe(11);
        }

        async Task it_returns_default_output_value_when_final_handler_not_specified()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.Add((input, next) => {
                    return next.Invoke(input);
                })
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe(default(int));
        }

        async Task it_ignores_subsequent_handlers_when_final_is_used()
        {
            bool nextExecuted = false;
            
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.Final(input => {
                    return Task.FromResult(input.Length);
                })
                .Add((input, next) => {
                    nextExecuted = true;
                    return Task.FromResult(1);
                })
            );

            await pipeline.Invoke("hello world");
            nextExecuted.ShouldBe(false);
        }
    }
}