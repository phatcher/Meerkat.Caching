#if NETCOREAPP
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using Meerkat.Caching;

using Microsoft.Extensions.Caching.Distributed;

namespace Meerkat.Test.Sample
{
    public class CustomerServiceDistributedCache : ICustomerService
    {
        private readonly ICustomerService service;
        private readonly IDistributedCache cache;
        private readonly TimeSpan duration;

        public CustomerServiceDistributedCache(ICustomerService service, IDistributedCache cache, TimeSpan duration)
        {
            this.service = service;
            this.duration = duration;
            this.cache = cache;
        }

        public async Task<Customer> GetAsync(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = "CustomerService.Get:{id}:{name}";

            var result = await cache.GetAsync(key, cancellationToken);
            if (result != null)
            {
                return Deserialize(result);
            }

            var customer = await service.GetAsync(id, name, cancellationToken);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            };

            await cache.SetAsync(key, Serialize(customer), options, cancellationToken);

            return customer;
        }

        public async Task<Customer> GetAsync2(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = "CustomerService.Get:{id}:{name}";

            var customer = await cache.GetAsync<Customer>(key, cancellationToken);
            if (customer != null)
            {
                return customer;
            }

            customer = await service.GetAsync(id, name, cancellationToken);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            };

            await cache.SetAsync(key, customer, options, cancellationToken);

            return customer;
        }

        public async Task<Customer> GetAsync3(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = "CustomerService.Get:{id}:{name}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            };

            var customer = await cache.GetOrCreateAsync(key, () => service.GetAsync(id, name, cancellationToken), options, cancellationToken);

            return customer;
        }

        private byte[] Serialize(Customer entity)
        {
            if (entity == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, entity);
                return stream.ToArray();
            }
        }

        private Customer Deserialize(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(data))
            {
                return formatter.Deserialize(stream) as Customer;
            }
        }
    }
}
#endif