#if NETCOREAPP
using System.Threading;
using System.Threading.Tasks;

using Meerkat.Caching;

using Polly;
using Polly.Caching;

namespace Meerkat.Test.Sample
{
    public class CustomerServiceCache : ICustomerService
    {
        private readonly ICustomerService service;
        private readonly CachePolicy policy;
        private readonly ISynchronizer synchronizer;

        public CustomerServiceCache(ICustomerService service, CachePolicy policy, ISynchronizer synchronizer)
        {
            this.service = service;
            this.policy = policy;
            this.synchronizer = synchronizer;
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

        public Task<Customer> GetAsync2(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = "CustomerService.Get:{id}:{name}";

            return policy.Cache(key, () => service.GetAsync(id, name, cancellationToken));
        }

        public Task<Customer> GetAsync3(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = this.CacheKey(null, id, name);

            return policy.Cache(key, () => service.GetAsync(id, name, cancellationToken), synchronizer, cancellationToken);
        }
    }
}
#endif