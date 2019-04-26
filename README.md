Meerkat Caching
===============

The [Meerkat.Caching](https://www.nuget.org/packages/Meerkat.Caching/) library is a simple cache that abstracts an interface from System.Runtime.Caching's MemoryCache and also provides some helpers when using NET Core.

The rationale for this is so we can have a simple caching implementation for using in, say an ASP.NET MVC application, which can then be subsituted for a more advanced cache such as Redis without changing the calls.

We also address the lack of region support in MemoryCache in our wrapper by providing a strategy pattern for constructing a composite key/region key which is then passed to the underlying MemoryCache instance.

[![NuGet](https://img.shields.io/nuget/v/Meerkat.Caching.svg)](https://www.nuget.org/packages/Meerkat.Caching/)
[![Build status](https://ci.appveyor.com/api/projects/status/7ycnghu7s0umys9e/branch/master?svg=true)](https://ci.appveyor.com/project/PaulHatcher/meerkat-caching/branch/master)


Welcome to contributions from anyone.

You can see the version history [here](RELEASE_NOTES.md).

## Build the project
* Windows: Run *build.cmd*

The tooling should be automatically installed by paket/Fake. The default build will compile and test the project, and also produce a nuget package.

## Library License

The library is available under the [MIT License](http://en.wikipedia.org/wiki/MIT_License), for more information see the [License file][1] in the GitHub repository.

 [1]: https://github.com/phatcher/Meerkat.Caching/blob/master/License.md

## Getting Started

For .NET Framework library is intended to be a drop-in replacement for MemoryCache so we have a static class called MemoryObjectCache that wraps it and provides region support. We also provide a static
factory class, MemoryObjectCacheFactory to ease the change over.

So originally you might have

```
var cache = MemoryCache.Default
```

You now have
```
var cache = MemoryObjectCacheFactory.Default
```

Under the hood, the factory is just creating a MemoryObjectCache which wraps the MemoryCache and also a RegionKeyStrategy which modifies the keys you add/lookup to provide region support e.g.

```
/// <summary>
/// Get a reference to the default <see cref="MemoryObjectCache"/> instance.
/// </summary>
/// <returns></returns>
public static MemoryObjectCache Default()
{
    var strategy = new RegionKeyStrategy();
    var cache = MemoryCache.Default;

    return new MemoryObjectCache(cache, strategy);
}
```
## NET Core

Unlike NET Framework, .NET Core has cache interfaces specifically [IMemoryCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-2.2) for in memory caching and [IDistributedCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2) for distributed caches such as Redis.

We still have work to do to make this useful for a specific service. For example given a service contract as follows...

```
public class Customer 
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public interface ICustomerService
{
    Task<Customer> GetAsync(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken));
}
```
We want to implement a caching layer over this to improve application performance we need to consider a number of factors...

1. What happens on failure?
2. How do we want to build the cache key
3. How to serialize to/from the cache, APIs generally take byte[] may be string
3. What happens to simultaneous calls

Some common patterns appear, but first, let us look at a basic implementation of a cache layer for ICustomerService using IDistributedCache
```
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
```
This works but the serialization code is likely to be needed in lots of services, so we can generalize as follows...
```
public static byte[] ToByteArray(this object value)
{
    if (value == null)
    {
        return null;
    }

    var formatter = new BinaryFormatter();
    using (var stream = new MemoryStream())
    {
        formatter.Serialize(stream, value);
        return stream.ToArray();
    }
}

public static T FromByteArray<T>(this byte[] data)
    where T : class
{
    if (data == null)
    {
        return default(T);
    }

    var formatter = new BinaryFormatter();
    using (var stream = new MemoryStream(data))
    {
        return formatter.Deserialize(stream) as T;
    }
}

public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
    where T : class
{
    await cache.SetAsync(key, value.ToByteArray(), options, cancellationToken);
}

public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default(CancellationToken))
    where T : class
{
    var result = await cache.GetAsync(key, cancellationToken);
    return result?.FromByteArray<T>();
}
```
This allows us to re-write the GetAsync method to this
```
public async Task<Customer> GetAsync(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
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
```
For IMemoryCache there is a GetOrCreateAsync extension method, but a similar method does not exist for IDistributedCache, probably due to the serialization question. As we've already solved this we can
help as follows...
```
public static async Task<T> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> factory, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default(CancellationToken))
    where T : class
{
    var value = await cache.GetAsync<T>(key, cancellationToken);
    if (value != null)
    {
        return value;
    }

    value = await factory();

    await cache.SetAsync(key, value, options, cancellationToken);

    return value;
}
```
and then the GetAsync method goes to
```
public async Task<Customer> GetAsync(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
{
    var key = "CustomerService.Get:{id}:{name}";
    var options = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = duration
    };

    var customer = await cache.GetOrCreateAsync(key, () => service.GetAsync(id, name, cancellationToken), options, cancellationToken);

    return customer;
}
```

At the moment our caching layer is coupled to IDistributedCache and also is not resilient to service failures. For service failures you really should be looking at patterns such 
as [Retry](https://github.com/App-vNext/Polly#retry) and [Circuit Breaker](https://github.com/App-vNext/Polly#circuit-breaker). These implementations are both part of a resilence and fault-handling package called [Polly](https://github.com/App-vNext/Polly#retry).

We can re-write our cache layer using a Polly [CachePolicy](https://github.com/App-vNext/Polly#cache)

```
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

        return policy.ExecuteAsync(context => service.GetAsync(id, name, cancellationToken), new Context(key));
    }
}
```
By doing this, we have hidden the caching implementation i.e. our service cache doesn't need to know how/when entries expire or whether we are using IMemoryCache or IDistributedCache

We can put a bit more syntatic sugar around this 
```
public static Task<T> Cache<T>(this CachePolicy policy, string key, Func<Task<T>> factory)
{
    return policy.ExecuteAsync(context => factory.Invoke(), new Context(key));
}
```
Making the implementation
```
public Task<Customer> GetAsync2(int id, string name = null, CancellationToken cancellationToken = default(CancellationToken))
{
    var key = "CustomerService.Get:{id}:{name}";

    return policy.Cache(key, () => service.GetAsync(id, name, cancellationToken));
}
```
One issue that we've found in production is that the service may be hit multiple times for the same arguments e.g. from different threads. If the underlying service call is expensive this
means wasted effort since the result should be the same, otherwise you shouldn't be caching it.

The solution was to introduce a lightweight synchronization service which delivers SemaphoreSlim instance according to some key, typically the same cache key used before. This ways calls for different arguments do not block each other,
but calls for the same arguments wait for the first call to return.

Here's the extension methods for the CachePolicy and synchronizer...
```
public static Task<T> Cache<T>(this CachePolicy policy, string key, Func<Task<T>> factory, ISynchronizer synchronizer, CancellationToken cancellationToken)
{
    return synchronizer.Synchronize(key, () => policy.Cache(key, factory), cancellationToken);
}

public static async Task<T> Synchronize<T>(this ISynchronizer synchronizer, string key, Func<Task<T>> factory, CancellationToken cancellationToken)
{
    var semaphore = synchronizer.Synchronizer(key);

    await semaphore.WaitAsync(cancellationToken);
    try
    {
        return await factory();
    }
    finally
    {
        semaphore.Release();
    }
}
```