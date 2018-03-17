using System.Threading.Tasks;

namespace FloSample
{
    public interface ICurrencyValidator
    {
        Task<bool> IsValid(string currency);
    }
}