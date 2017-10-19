using System;
using System.Collections.Generic;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    class describe_Pipeline_Project : nspec
    {
        void it_can_project_to_another_pipeline()
        {
            var pipeline = Pipeline.Build<string, string>(cfg =>
                cfg.Add((input, next) => {
                    input = input.ToUpper();
                    return next.Invoke(input);
                })
                .Project<char[], char[]>((input, projectedPipeline, next) 
                    => {
                        var converted = projectedPipeline.Invoke(input.ToCharArray());
                        return new string(converted);
                    },
                    builder => builder.Terminate(input => {
                        var converted = new List<char>();
                        for (int i = 0; i < input.Length; i++)
                        {
                            if (input[i] != 'l' && input[i] != 'L')
                                converted.Add(input[i]);
                        }
                        return converted.ToArray();
                    })
                )
                .WithFinalHandler(s => s)
            );

            var result = pipeline.Invoke("hello world");
            result.ShouldBe("HEO WORD");
        }
    }
}