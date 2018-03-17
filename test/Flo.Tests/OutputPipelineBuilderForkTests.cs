using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    public class describe_OutputPipelineBuilder_Fork : nspec
    {
        async Task it_ignores_the_handler_if_the_predicate_returns_false()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.Fork(input => input == "hello world",
                    builder => builder.Add((input, next) =>
                    {
                        return Task.FromResult(input.Length);
                    })
                )
            );

            var result = await pipeline.Invoke("hello");
            result.ShouldBe(0);
        }

        async Task it_executes_the_handler_and_does_not_continue_to_parent_if_the_predicate_returns_true()
        {
            var pipeline = Pipeline.Build<TestContext, TestContext>(cfg =>
                cfg.Fork(input => true,
                    builder => builder.Add((ctx, next) =>
                    {
                        return Task.FromResult(ctx);
                    })
                )
                .Final(ctx => {
                    ctx.Add("Test", "TestValue");
                    return Task.FromResult(ctx);
                })
            );

            var result = await pipeline.Invoke(new TestContext());
            result.ShouldNotContainKey("Test");
        }
    }
}