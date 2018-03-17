using System.Threading.Tasks;

namespace FloSample
{
    public interface IMerchantValidator
    {
        Task<bool> IsValid(int merchantId);
    }
}