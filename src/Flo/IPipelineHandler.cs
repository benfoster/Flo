using System;
using System.Threading.Tasks;

namespace Flo
{
    public interface IPipelineHandler<TInput>
    {
        Task HandleAsync(TInput input, Func<TInput, Task> next);
    }
}