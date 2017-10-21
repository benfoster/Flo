using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flo
{
    public class PipelineBuilder<TInput> : Builder<TInput, Task>
    {
        private readonly Func<Type, object> _serviceProvider;
        
        public PipelineBuilder(Func<Type, object> serviceProvider = null) 
            : base(input => Task.CompletedTask)
        {
            _serviceProvider = serviceProvider ?? (type => Activator.CreateInstance(type));
        }

        public PipelineBuilder<TInput> Add(Func<TInput, Func<TInput, Task>, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            AddHandler(handler);
            return this;
        }

        public PipelineBuilder<TInput> When(
            Func<TInput, bool> predicate,
            Func<TInput, Func<TInput, Task>, Func<TInput, Task>, Task> handler,
            Action<PipelineBuilder<TInput>> configurePipeline)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));

            return Add((input, next) =>
            {
                if (predicate.Invoke(input))
                {
                    var builder = new PipelineBuilder<TInput>(_serviceProvider);
                    configurePipeline(builder);
                    return handler.Invoke(input, builder.Build(), next);
                }

                return next.Invoke(input);
            });
        }

        public PipelineBuilder<TInput> When(
            Func<TInput, bool> predicate,
            Action<PipelineBuilder<TInput>> configurePipeline)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));

            return When(predicate, async (input, innerPipeline, next) =>
            {
                await innerPipeline.Invoke(input);
                await next.Invoke(input);
            },
            configurePipeline);
        }

        /// <summary>
        /// Projects to another asynchronous pipeline that takes an 
        /// input of type <typeparamref name="TProjectedInput"/>
        /// </summary>
        /// <param name="Func<TInput"></param>
        /// <param name="handler"></param>
        /// <param name="configurePipeline"></param>
        /// <returns></returns>
        public PipelineBuilder<TInput> Project<TProjectedInput>(
            Func<TInput, Func<TProjectedInput, Task>, Func<TInput, Task>, Task> handler,
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
        public PipelineBuilder<TInput> Project<TProjectedInput, TProjectedOutput>(
            Func<TInput, Func<TProjectedInput, Task<TProjectedOutput>>, Func<TInput, Task>, Task> handler,
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

        public PipelineBuilder<TInput> Terminate(Func<TInput, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return Add((input, next) => handler.Invoke(input));
        }

        public PipelineBuilder<TInput> Add<THandler>() where THandler : class, IHandler<TInput>
        {
            Func<THandler> handlerFactory = () => _serviceProvider.Invoke(typeof(THandler)) as THandler;
            return Add(handlerFactory);
        }

        public PipelineBuilder<TInput> Add(Func<IHandler<TInput>> handlerFactory)
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

        public PipelineBuilder<TInput> Project<TProjectedInput>(Func<IProjectingHandler<TInput, TProjectedInput>> handlerFactory, Action<PipelineBuilder<TProjectedInput>> configurePipeline) 
        {
            if (handlerFactory == null) throw new ArgumentNullException(nameof(handlerFactory));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));

            return Project((input, pipeline, next) => 
            {
                var handler = handlerFactory.Invoke();

                if (handler != null)
                {
                    return handler.HandleAsync(input, pipeline, next);
                }

                return next.Invoke(input);
            }, configurePipeline);
        }

        public PipelineBuilder<TInput> Project<TProjectedInput, TProjectedOutput>(
            Func<IProjectingHandler<TInput, TProjectedInput, TProjectedOutput>> handlerFactory, 
            Action<PipelineBuilder<TProjectedInput, TProjectedOutput>> configurePipeline) 
        {
            if (handlerFactory == null) throw new ArgumentNullException(nameof(handlerFactory));
            if (configurePipeline == null) throw new ArgumentNullException(nameof(configurePipeline));

            return Project((input, pipeline, next) => 
            {
                var handler = handlerFactory.Invoke();

                if (handler != null)
                {
                    return handler.HandleAsync(input, pipeline, next);
                }

                return next.Invoke(input);
            }, configurePipeline);
        }
    }
}