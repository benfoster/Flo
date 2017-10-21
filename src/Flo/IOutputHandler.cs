using System;
using System.Threading.Tasks;

namespace Flo
{
    public interface IOutputHandler<TInput, TOutput>
    {
        Task<TOutput> HandleAsync(TInput input, Func<TInput, Task<TOutput>> next);
    }
}