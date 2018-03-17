# Flo

[![AppVeyor Build status](https://ci.appveyor.com/api/projects/status/bgqvqvydhh14gk2e?svg=true)](https://ci.appveyor.com/project/benfoster/flo) [![Travis Build Status](https://travis-ci.org/benfoster/Flo.svg?branch=master)](https://travis-ci.org/benfoster/Flo) [![NuGet](https://img.shields.io/nuget/v/Flo.svg)](https://www.nuget.org/packages/Flo)

Flo is a lightweight library for composing chain of responsibility pipelines. The inspiration for this project came from the need to break down a large codebase of tightly coupled behavior into smaller units for better extensibility and testability.

Flo works in a similar way to middleware in ASP.NET Core with a focus on immutability and support for handlers that output a different type i.e. `Func<Tin, Func<TOut>`.

## Where can I get it?

Flo is available on [NuGet](https://www.nuget.org/packages/Flo) and targets NetStandard 1.3.

## Getting started

To create a pipeline similar to ASP.NET Core's middleware, first define the type you wish to pass between your chain of handlers:

```c#
public class HttpContext
{
    public HttpRequest Request {get;set;}
    public HttpResponse Response {get;set;}
}
```

Use `Pipeline.Build<T>` to create a pipeline passing in a configuration with the handlers you wish to add. Handlers can be delegates or strongly typed (see below). The `Build` method returns a `Func<T, Task<T>` or `Func<TIn, Func<Task<TOut>>` that can safely be reused in your application.

```c#
var pipeline = Pipeline.Build<HttpContext>(cfg =>
    cfg.Add((ctx, next) => {
        Log.Logger.Info("Starting request to {path}", ctx.Request.Path);
        return next.Invoke(context);
    })
    .Add((ctx, next) => {
        if (!Validator.Validate(ctx.Request)) 
		{
			await ctx.Response.WriteAsync("invalid");
			return;
		}
		
        return next.Invoke(context);
    })
);

var context = new HttpContext { Path = "/foo" };
context = await pipeline.Invoke(context);
```

### Strongly typed handlers

Flo supports strongly typed handlers. To create a strongly typed handler, implement `IHandler<T>` or `IHandler<TIn, TOut>` depending on the type of pipeline you wish to build:

```c#
public class MerchantValidator : IHandler<RequestPayment, ValidationResult>
{
    public async Task<ValidationResult> HandleAsync(
        RequestPayment command, 
        Func<RequestPayment, Task<ValidationResult>> next)
    {
        if (command.MerchantId <= 0)
            return new ValidationResult
            {
                ErrorCode = "merchant_invalid"
            };
        
        return await next.Invoke(command);
    }
}
```

Strongly typed handlers are registered in the same way as delegate handlers:

```c#
public static Func<RequestPayment, Task<ValidationResult>> Build()
{
	return Pipeline.Build<RequestPayment, ValidationResult>(cfg =>
		cfg.Add<MerchantValidator>()
		.Final(s => Task.FromResult(new ValidationResult { IsValid = true }))
	);
}
```

## Final Handlers 

There are some differences between how the final handler is _handled_ with `Pipeline<T>` and `Pipeline<TIn, TOut>`.

For `Pipeline<T>` you do not need to do anything to mark the final handler as Flo will automatically feed the output your final handler into the input of the previous one, on the way back out. You can therefore safely call `next.Invoke` on your final handler without issue, for example:

```
var pipeline = Pipeline.Build<TestContext>(cfg =>
	cfg.Add((ctx, next) => {
		ctx.Add("Item1", "Item1Value");
		return next.Invoke(ctx);
	})
	.Add((ctx, next) => {
		ctx.Add("Item2", "Item2Value");
		return next.Invoke(ctx); // no issue
	})
);
```

With `Pipeline<TIn, TOut>` Flo can't automatically feed the final handler result into the previous handler so you should _not_ call `next.Invoke` on the final handler, for example:

```c#
var pipeline = Pipeline.Build<string, int>(cfg =>
	cfg.Add((input, next) => {
		input += "hello";
		return next.Invoke(input);
	})
	.Add((input, next) => {
		input += " world";
		return next.Invoke(input);
	})
	.Add((input, next) => {
		return Task.FromResult(input.Length); // don't call next
	})
);
```

If you do, `default(TOut)` will be returned.

