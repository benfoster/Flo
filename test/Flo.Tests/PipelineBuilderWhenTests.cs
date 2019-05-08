using NSpec;
using Shouldly;
using System.Threading.Tasks;

namespace Flo.Tests
{
    public class describe_PipelineBuilder_When : nspec
    {
        async Task it_ignores_the_handler_if_the_predicate_returns_false()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) =>
                {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => ctx.ContainsKey("Item2"),
                    builder => builder.Add((ctx, next) =>
                    {
                        ctx.Add("Item3", "Item3Value");
                        return next.Invoke(ctx);
                    })
                )
                .Final(ctx =>
                {
                    ctx.Add("Item4", "Item4Value");
                    return Task.FromResult(ctx);
                })
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(2);
            context.ShouldNotContainKey("Item3");
        }

        async Task it_ignores_the_handler_if_the_async_predicate_returns_false()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) =>
                {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => Task.FromResult(ctx.ContainsKey("Item2")),
                    builder => builder.Add((ctx, next) =>
                    {
                        ctx.Add("Item3", "Item3Value");
                        return next.Invoke(ctx);
                    })
                )
                .Final(ctx =>
                {
                    ctx.Add("Item4", "Item4Value");
                    return Task.FromResult(ctx);
                })
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(2);
            context.ShouldNotContainKey("Item3");
        }

        async Task it_executes_the_handler_if_the_predicate_returns_true()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) =>
                {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Add((ctx, next) =>
                {
                    ctx.Add("Item2", "Item2Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => ctx.ContainsKey("Item2"),
                    builder => builder.Final(ctx =>
                    {
                        ctx.Add("Item3", "Item3Value");
                        return Task.FromResult(ctx);
                    })
                )
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(3);
            context.ShouldContainKey("Item3");
        }

        async Task it_executes_the_handler_if_the_async_predicate_returns_true()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) =>
                {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Add((ctx, next) =>
                {
                    ctx.Add("Item2", "Item2Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => Task.FromResult(ctx.ContainsKey("Item2")),
                    builder => builder.Final(ctx =>
                    {
                        ctx.Add("Item3", "Item3Value");
                        return Task.FromResult(ctx);
                    })
                )
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(3);
            context.ShouldContainKey("Item3");
        }

        async Task it_continues_to_parent_pipeline_after_child_pipeline_has_completed()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) =>
                {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => ctx.Count == 1,
                    builder => builder
                        .Add((ctx, next) =>
                        {
                            ctx.Add("Item2", "Item2Value");
                            return next.Invoke(ctx);
                        })
                        .Add((ctx, next) =>
                        {
                            ctx.Add("Item3", "Item3Value");
                            return Task.FromResult(ctx);
                        })
                )
                .Final(ctx =>
                {
                    ctx.Add("Item4", "Item4Value");
                    return Task.FromResult(ctx);
                })
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.ShouldContainKey("Item1");
            context.ShouldContainKey("Item2");
            context.ShouldContainKey("Item3");
            context.ShouldContainKey("Item4");
        }

        async Task it_continues_to_parent_pipeline_after_child_pipeline_has_completed_with_async_predicate()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) =>
                {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => Task.FromResult(ctx.Count == 1),
                    builder => builder
                        .Add((ctx, next) =>
                        {
                            ctx.Add("Item2", "Item2Value");
                            return next.Invoke(ctx);
                        })
                        .Add((ctx, next) =>
                        {
                            ctx.Add("Item3", "Item3Value");
                            return Task.FromResult(ctx);
                        })
                )
                .Final(ctx =>
                {
                    ctx.Add("Item4", "Item4Value");
                    return Task.FromResult(ctx);
                })
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.ShouldContainKey("Item1");
            context.ShouldContainKey("Item2");
            context.ShouldContainKey("Item3");
            context.ShouldContainKey("Item4");
        }
    }
}