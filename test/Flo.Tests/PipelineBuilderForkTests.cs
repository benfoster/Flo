    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NSpec;
    using Shouldly;

    namespace Flo.Tests
    {
        public class describe_PipelineBuilder_Fork : nspec
        {
            async Task it_ignores_the_handler_if_the_predicate_returns_false()
            {
                var pipeline = Pipeline.Build<TestContext>(cfg =>
                    cfg.Add((ctx, next) =>
                    {
                        ctx.Add("Item1", "Item1Value");
                        return next.Invoke(ctx);
                    })
                    .Fork(ctx => ctx.ContainsKey("Item2"),
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
                context.ShouldContainKey("Item4");
            }

            async Task it_executes_the_handler_and_does_not_continue_to_parent_if_the_predicate_returns_true()
            {
                var pipeline = Pipeline.Build<TestContext>(cfg =>
                    cfg.Add((ctx, next) =>
                    {
                        ctx.Add("Item1", "Item1Value");
                        return next.Invoke(ctx);
                    })
                    .Fork(ctx => ctx.ContainsKey("Item1"),
                        builder => builder.Final(ctx =>
                        {
                            ctx.Add("Item2", "Item2Value");
                            return Task.FromResult(ctx);
                        })
                    )
                    .Add((ctx, next) =>
                    {
                        ctx.Add("Item3", "Item3Value");
                        return next.Invoke(ctx);
                    })
                );

                var context = new TestContext();
                await pipeline.Invoke(context);

                context.Count.ShouldBe(2);
                context.ShouldContainKey("Item1");
                context.ShouldContainKey("Item2");
                context.ShouldNotContainKey("Item3");
            }
        }
    }