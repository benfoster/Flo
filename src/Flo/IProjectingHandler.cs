using System;
using System.Threading.Tasks;

namespace Flo
{
    public interface IProjectingHandler<TInput, TProjectedInput>
    {
        Task HandleAsync(
            TInput input, 
            Func<TProjectedInput, Task> projectedPipeline, 
            Func<TInput, Task> next);
    }

    public interface IProjectingHandler<TInput, TProjectedInput, TProjectedOutput>
    {
        Task HandleAsync(
            TInput input, 
            Func<TProjectedInput, Task<TProjectedOutput>> projectedPipeline, 
            Func<TInput, Task> next);
    }
}