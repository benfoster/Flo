using System;
using System.Threading.Tasks;

namespace Flo
{
    public interface IPipelineHandler<TInput, TOutput>
    {
        Task<TOutput> HandleAsync(TInput input, Func<TInput, Task<TOutput>> next);
    }
}