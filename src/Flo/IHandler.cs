using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flo
{
    public interface IHandler<TIn, TOut>
    {
        Task<TOut> HandleAsync(TIn input, Func<TIn, Task<TOut>> next);
    }
    
    public interface IHandler<T> : IHandler<T, T>
    {
    }
}