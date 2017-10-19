using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_Pipeline : nspec
    {
        async Task it_can_execute_single_handler()
        {
            var pipeline = Pipeline.Build<Dictionary<string, object>, Task>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("SomeKey", "SomeValue");
                    return Task.CompletedTask;
                })
            );

            var context = new Dictionary<string, object>();
            await pipeline.Invoke(context);

            context["SomeKey"].ShouldBe("SomeValue");
        }

        async Task it_can_execute_multiple_handlers()
        {
            var pipeline = Pipeline.Build<Dictionary<string, object>, Task>(cfg =>
                cfg.Add((ctx, next) => {
                    ctx.Add("Item1", "Item1Value");
                    return next.Invoke(ctx);
                })
                .Add((ctx, next) => {
                    ctx.Add("Item2", "Item2Value");
                    return Task.CompletedTask;
                })
            );

            var context = new Dictionary<string, object>();
            await pipeline.Invoke(context);

            context["Item1"].ShouldBe("Item1Value");
            context["Item2"].ShouldBe("Item2Value");
        }

        void it_can_execute_pipeline_with_return_result()
        {
            var pipeline = Pipeline.Build<string, char[]>(cfg =>
                cfg.Add((input, next) => {
                    input = "Hello";
                    return next.Invoke(input);
                })
                .Add((input, next) => {
                    input += " world!";
                    return input.Reverse().ToArray();
                })
            );

            var chars = pipeline.Invoke("");
            new string(chars).ShouldBe("!dlrow olleH");
        }

        void it_returns_default_value_when_final_handler_is_not_set()
        {
            var pipeline = Pipeline.Build<int, int>(cfg =>
                cfg.Add((input, next) => {
                    input = input * 100;
                    return next.Invoke(input);
                })
            );    

            var result = pipeline.Invoke(5);
            result.ShouldBe(0); // default(int)
        }

        void it_can_override_default_final_handler()
        {
            var pipeline = Pipeline.Build<int, int>(cfg =>
                cfg.Add((input, next) => {
                    input = input * 100;
                    return next.Invoke(input);
                })
                .Add((input, next) => {
                    input = input * 2;
                    return next.Invoke(input);
                })
                .WithFinalHandler(output => output)
            );    

            var result = pipeline.Invoke(5);
            result.ShouldBe(1000);
        }
    }
}