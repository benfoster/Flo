using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSpec;
using Shouldly;

namespace Flo.Tests
{
    public class describe_Handler : nspec
    {
        async Task it_creates_and_invoke_handler_using_default_service_provider()
        {
            var pipeline = Pipeline.Build<TestContext>(cfg =>
                 cfg.Add<TestHandler>()
                    .Add<TestHandler>()
            );

            var context = new TestContext();
            await pipeline.Invoke(context);
            context.Count.ShouldBe(2);
        }

        async Task it_initialises_handlers_lazily()
        {
            bool initialised = false;
            
            var pipeline = Pipeline.Build<object>(cfg =>
                 cfg.Add(() => new LazyHandler(() => initialised = true))
            );

            initialised.ShouldBe(false);
            await pipeline.Invoke(null);
            initialised.ShouldBe(true);
        }

        async Task it_supports_handlers_with_result()
        {
            var pipeline = Pipeline.Build<string, int>(cfg =>
                cfg.Add<StringLengthCountHandler>()
            );

            var output = await pipeline.Invoke("hello world");
            output.ShouldBe(11);                
        }

        async Task it_can_use_a_custom_service_provider()
        {
            var pipeline = Pipeline.Build<string, string>(cfg =>
                cfg.Add<OverridingHandler>()
            , type => new OverridingHandler("Override")); // always returns this handler type

            var output = await pipeline.Invoke("hello world");
            output.ShouldBe("Override");        
        }

        class TestHandler : IHandler<TestContext>
        {           
            public Task<TestContext> HandleAsync(TestContext input, Func<TestContext, Task<TestContext>> next, CancellationToken cancellationToken)
            {
                input.Add(Guid.NewGuid().ToString(), Guid.NewGuid());
                return next.Invoke(input);
            }
        }

        class LazyHandler : IHandler<object>
        {
            private readonly Action _callback;

            public LazyHandler(Action callback)
            {
                _callback = callback;
            }
            
            public Task<object> HandleAsync(object input, Func<object, Task<object>> next, CancellationToken cancellationToken)
            {
                _callback.Invoke();
                return next.Invoke(input);
            }
        }

        class StringLengthCountHandler : IHandler<string, int>
        {
            public Task<int> HandleAsync(string input, Func<string, Task<int>> next, CancellationToken cancellationToken)
            {
                return Task.FromResult(input.Length);
            }
        }

        class OverridingHandler : IHandler<string, string>
        {
            private readonly string _output;
            
            public OverridingHandler() : this("Default") {}

            public OverridingHandler(string output)
            {
                _output = output;
            }
            
            public Task<string> HandleAsync(string input, Func<string, Task<string>> next, CancellationToken cancellationToken)
            {
                return Task.FromResult("Override");
            }
        }
    }
}