using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_Pipeline_Project : nspec
    {
        async Task it_can_project_to_another_pipeline()
        {
            var pipeline = Pipeline.Build<string, string>(cfg =>
                cfg.Add((input, next) => {
                    input = input.ToUpper();
                    return next.Invoke(input);
                })
                .Project<char[], char[]>(async (input, projectedPipeline, next) 
                    => {
                        var converted = await projectedPipeline.Invoke(input.ToCharArray());
                        return new string(converted);
                    },
                    builder => builder.Terminate(input => {
                        var converted = new List<char>();
                        for (int i = 0; i < input.Length; i++)
                        {
                            if (input[i] != 'l' && input[i] != 'L')
                                converted.Add(input[i]);
                        }
                        return Task.FromResult(converted.ToArray());
                    })
                )
                .WithFinalHandler(s => Task.FromResult(s))
            );

            var result = await pipeline.Invoke("hello world");
            result.ShouldBe("HEO WORD");
        }
    }
}