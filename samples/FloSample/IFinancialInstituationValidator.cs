using System.Threading.Tasks;

namespace FloSample
{
    public interface IFinancialInstitutionValidator
    {
        Task<bool> IsValid(int merchantId, string recipientDetails);
    }
}