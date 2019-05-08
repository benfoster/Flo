using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    public class describe_OutputPipelineBuilder_When : nspec
    {
        async Task it_ignores_the_handler_if_the_predicate_returns_false()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.When(input => input == "hello world",
                    builder => builder.Add((input, next) =>
                    {
                        return Task.FromResult(input.Length);
                    })
                )
            );

            var result = await pipeline.Invoke("hello");
            result.ShouldBe(0);
        }

        async Task it_ignores_the_handler_if_the_async_predicate_returns_false()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.When(input => Task.FromResult(input == "hello world"),
                    builder => builder.Add((input, next) =>
                    {
                        return Task.FromResult(input.Length);
                    })
                )
            );

            var result = await pipeline.Invoke("hello");
            result.ShouldBe(0);
        }

        async Task it_executes_the_handler_if_the_predicate_returns_true()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.When(input => input == "hello world",
                    builder => builder.Add((input, next) =>
                    {
                        return Task.FromResult(input.Length);
                    })
                )
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe(11);
        }

        async Task it_executes_the_handler_if_the_async_predicate_returns_true()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.When(input => Task.FromResult(input == "hello world"),
                    builder => builder.Add((input, next) =>
                    {
                        return Task.FromResult(input.Length);
                    })
                )
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe(11);
        }

        async Task it_continues_to_parent_pipeline_if_child_pipeline_returns_default_value()
        {
            var pipeline = Pipeline.Build<TestContext, TestContext>(cfg =>
                cfg.When(input => true,
                    builder => builder.Add((ctx, next) =>
                    {
                        return Task.FromResult(default(TestContext));
                    })
                )
                .Final(ctx => {
                    ctx.Add("Test", "TestValue");
                    return Task.FromResult(ctx);
                })
            );

            var result = await pipeline.Invoke(new TestContext());
            result.ShouldContainKey("Test");
        }

        async Task it_continues_to_parent_pipeline_if_child_pipeline_returns_default_value_with_async_predicate()
        {
            var pipeline = Pipeline.Build<TestContext, TestContext>(cfg =>
                cfg.When(input => Task.FromResult(true),
                    builder => builder.Add((ctx, next) =>
                    {
                        return Task.FromResult(default(TestContext));
                    })
                )
                .Final(ctx => {
                    ctx.Add("Test", "TestValue");
                    return Task.FromResult(ctx);
                })
            );

            var result = await pipeline.Invoke(new TestContext());
            result.ShouldContainKey("Test");
        }

        async Task it_does_not_continue_to_parent_pipeline_if_child_pipeline_returns_value()
        {
            var pipeline = Pipeline.Build<TestContext, TestContext>(cfg =>
                cfg.When(input => true,
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

        async Task it_does_not_continue_to_parent_pipeline_if_child_pipeline_returns_value_with_async_predicate()
        {
            var pipeline = Pipeline.Build<TestContext, TestContext>(cfg =>
                cfg.When(input => Task.FromResult(true),
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