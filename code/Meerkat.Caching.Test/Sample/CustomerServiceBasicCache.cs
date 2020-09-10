#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;

using Polly;
using Polly.Caching;

namespace Meerkat.Test.Sample
{
    public class CustomerServiceBasicCache : ICustomerService
    {
        private readonly ICustomerService service;
        private readonly CachePolicy policy;

        public CustomerServiceBasicCache(ICustomerService service, CachePolicy policy)
        {
            this.service = service;
            this.policy = policy;
        }

        public Task<Customer> GetAsync(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = "CustomerService.Get:{id}:{name}";

#if NETCOREAPP2_1
            return policy.ExecuteAsync(context => service.GetAsync(id, name, cancellationToken), new Context(key));
#else
            return policy.Execute(context => service.GetAsync(id, name, cancellationToken), new Context(key));
#endif
        }
    }
}
#endif