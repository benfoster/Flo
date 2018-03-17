using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_PipelineBuilder : nspec
    {       
        async Task it_can_execute_single_handler()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("SomeKey", "SomeValue");
                    return Task.FromResult(ctx);
                })
            );

            var context = new TestContext();
            context = await pipeline.Invoke(context);

            context["SomeKey"].ShouldBe("SomeValue");
        }

        async Task it_can_execute_multiple_handlers()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Add((ctx, next) => {
                    ctx.Add("Item2", "Item2Value");
                    return Task.FromResult(ctx);
                })
            );

            var context = new TestContext();
            context = await pipeline.Invoke(context);

            context["Item1"].ShouldBe("Item1Value");
            context["Item2"].ShouldBe("Item2Value");
        }

        async Task it_returns_final_handler_result()
        {
            // Despite invoking next on final handler, the final result is returned
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Add((ctx, next) => {
                    ctx.Add("Item2", "Item2Value");
                    return next.Invoke(ctx);
                })
            );

            var context = new TestContext();
            context = await pipeline.Invoke(context);

            context.ShouldNotBeNull();
        }

        async Task it_ignores_subsequent_handlers_when_final_is_used()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Final(ctx => {
                    ctx.Add("Item2", "Item2Value");
                    return Task.FromResult(ctx);
                })
                .Add((ctx, next) => {
                    ctx.Add("Item3", "Item3Value");
                    return next.Invoke(ctx);
                })
            );

            var context = new TestContext();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(2);
            context.ContainsKey("Item3").ShouldBe(false);
        }
    }
}