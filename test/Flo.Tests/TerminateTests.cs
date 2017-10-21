using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_Pipeline_Terminate : nspec
    {
        async Task it_terminates_the_pipeline_without_calling_next()
        {
            var pipeline = Pipeline.Build<Dictionary<string, object>>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Terminate(ctx => {
                    ctx.Add("Item2", "Item2Value");
                    return Task.CompletedTask;
                })
                .Add((ctx, next) => {
                    ctx.Add("Item3", "Item3Value");
                    return next.Invoke(ctx);
                })
            );

            var context = new Dictionary<string, object>();
            await pipeline.Invoke(context);

            context.Count.ShouldBe(2);
            context.ContainsKey("Item3").ShouldBe(false);
        }
    }
}