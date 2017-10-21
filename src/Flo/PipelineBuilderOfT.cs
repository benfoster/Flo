using System;
using System.Threading.Tasks;

namespace Flo
{
    public class PipelineBuilder<TInput, TOutput> : Builder<TInput, Task<TOutput>>
    {
        private readonly Func<Type, object> _serviceProvider;
        
        public PipelineBuilder(Func<Type, object> serviceProvider = null) 
            : base(input => Task.FromResult(default(TOutput)))
        {
            _serviceProvider = serviceProvider ?? (type => Activator.CreateInstance(type));
        }

        public PipelineBuilder<TInput, TOutput> Add(Func<TInput, Func<TInput, Task<TOutput>>, Task<TOutput>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            AddHandler(handler);
            return this;
        }

        public PipelineBuilder<TInput, TOutput> When(
            Func<TInput, bool> predicate,
            Func<TInput, Func<TInput, Task<TOutput>>, Func<TInput, Task<TOutput>>, Task<TOutput>> handler,
            Action<PipelineBuilder<TInput, TOutput>> configurePipeline)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));
            
            return Add((input, next) =>
            {
                if (predicate.Invoke(input))
                {
                    var builder = new PipelineBuilder<TInput, TOutput>(_serviceProvider)
                        .WithFinalHandler(InnerHandler);

                    configurePipeline(builder);
                    return handler.Invoke(input, builder.Build(), next);
                }

                return next.Invoke(input);
            });
        }

        /// <summary>
        /// Projects to another asynchronous pipeline that takes an 
        /// input of type <typeparamref name="TProjectedInput"/>
        /// </summary>
        /// <param name="Func<TInput"></param>
        /// <param name="handler"></param>
        /// <param name="configurePipeline"></param>
        /// <returns></returns>
        public PipelineBuilder<TInput, TOutput> Project<TProjectedInput>(
            Func<TInput, Func<TProjectedInput, Task>, Func<TInput, Task<TOutput>>, Task<TOutput>> handler,
            Action<PipelineBuilder<TProjectedInput>> configurePipeline)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));
            
            return Add((input, next) =>
            {
                var builder = new PipelineBuilder<TProjectedInput>(_serviceProvider);
                configurePipeline(builder);
                return handler.Invoke(input, builder.Build(), next);
            });
        }

        /// <summary>
        /// Projects to another asynchronous pipeline that takes an input of type <typeparamref name="TProjectedInput"/> 
        /// and returns a task returning <typeparamref name="TProjectedOutput"/>
        /// </summary>
        /// <param name="Func<TInput"></param>
        /// <param name="handler"></param>
        /// <param name="Action<PipelineBuilder<TProjectedInput"></param>
        /// <param name="configurePipeline"></param>
        /// <returns></returns>
        public PipelineBuilder<TInput, TOutput> Project<TProjectedInput, TProjectedOutput>(
            Func<TInput, Func<TProjectedInput, Task<TProjectedOutput>>, Func<TInput, Task<TOutput>>, Task<TOutput>> handler,
            Action<PipelineBuilder<TProjectedInput, TProjectedOutput>> configurePipeline)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));
            
            return Add((input, next) =>
            {
                var builder = new PipelineBuilder<TProjectedInput, TProjectedOutput>(_serviceProvider);
                configurePipeline(builder);
                return handler.Invoke(input, builder.Build(), next);
            });
        }

        public PipelineBuilder<TInput, TOutput> Terminate(Func<TInput, Task<TOutput>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return Add((input, next) => handler.Invoke(input));
        }

        public PipelineBuilder<TInput, TOutput> WithFinalHandler(Func<TInput, Task<TOutput>> innerHandler)
        {
            if (innerHandler == null) throw new ArgumentNullException(nameof(innerHandler));
            InnerHandler = innerHandler;
            return this;
        }

        public PipelineBuilder<TInput, TOutput> Add<THandler>() where THandler : class, IPipelineHandler<TInput, TOutput>
        {
            Func<THandler> handlerFactory = () => _serviceProvider.Invoke(typeof(THandler)) as THandler;
            return Add(handlerFactory);
        }

        public PipelineBuilder<TInput, TOutput> Add<THandler>(Func<THandler> handlerFactory) where THandler : IPipelineHandler<TInput, TOutput>
        {
            if (handlerFactory == null) throw new ArgumentNullException(nameof(handlerFactory));

            return Add((input, next) => 
            {
                var handler = handlerFactory.Invoke();
                
                if (handler != null)
                {
                    return handler.HandleAsync(input, next);
                }

                return next.Invoke(input);
            });
        }
    }
}