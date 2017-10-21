using System;
using System.Threading.Tasks;

namespace Flo
{
    public interface IProjectingOutputHandler<TInput, TProjectedInput, TOutput>
    {
        Task<TOutput> HandleAsync(
            TInput input, 
            Func<TProjectedInput, Task> projectedPipeline, 
            Func<TInput, Task<TOutput>> next);
    } 
    
    public interface IProjectingOutputHandler<TInput, TProjectedInput, TProjectedOutput, TOutput>
    {
        Task<TOutput> HandleAsync(
            TInput input, 
            Func<TProjectedInput, Task<TProjectedOutput>> projectedPipeline, 
            Func<TInput, Task<TOutput>> next);
    }    
}