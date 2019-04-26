using System.Threading;
using System.Threading.Tasks;

namespace Meerkat.Test.Sample
{
    public interface ICustomerService
    {
        Task<Customer> GetAsync(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken));

    }
}
