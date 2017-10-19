using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    public class describe_Pipeline_When : nspec
    {
        async Task it_ignores_the_handler_if_the_predicate_returns_false()
        {
            var pipeline = Pipeline.Build<Dictionary<string, object>, Task>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => ctx.ContainsKey("Item2"), 
                    async (ctx, innerPipeline, next) => {
                        await innerPipeline.Invoke(ctx);
                        await next.Invoke(ctx);
                    }, 
                    builder => builder.Add((ctx, next) => {
                        ctx.Add("Item3", "Item3Value");
                        return next.Invoke(ctx);
                    })
                )
                .Terminate(ctx => {
                    ctx.Add("Item4", "Item4Value");
                    return Task.CompletedTask;
                })
            );

            var context = new Dictionary<string, object>();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(2);
            context.ContainsKey("Item3").ShouldBe(false);
        }

        async Task it_executes_the_handler_if_the_predicate_returns_true()
        {
            var pipeline = Pipeline.Build<Dictionary<string, object>, Task>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Add((ctx, next) => {
                    ctx.Add("Item2", "Item2Value");
                    return next.Invoke(ctx);
                })
                .When(ctx => ctx.ContainsKey("Item2"), 
                    async (ctx, innerPipeline, next) => {
                        await innerPipeline.Invoke(ctx);
                    }, 
                    builder => builder.Terminate(ctx => {
                        ctx.Add("Item3", "Item3Value");
                        return Task.CompletedTask;
                    })
                )
            );

            var context = new Dictionary<string, object>();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(3);
            context.ContainsKey("Item3").ShouldBe(true);
        }
    }
}