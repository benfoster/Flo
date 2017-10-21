using System;
using System.Threading.Tasks;

namespace Flo
{
    public interface IHandler<TInput>
    {
        Task HandleAsync(TInput input, Func<TInput, Task> next);
    }
}