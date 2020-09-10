#if NETSTANDARD
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Polly;
using Polly.Caching;

namespace Meerkat.Caching
{
    public static class CachingExtensions
    {
        static CachingExtensions()
        {
            KeySeparator = ":";
        }

        /// <summary>
        /// Gets the separator for cache key segments (default colon)
        /// </summary>
        public static string KeySeparator { get; set; }

        /// <summary>
        /// Serialize an object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Deserialize an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Synchronise access to a policy invocation to get/create a value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy">Policy to use</param>
        /// <param name="factory">Factory to create the entity if not in the cache</param>
        /// <param name="key">Cache/synchronization key to use</param>
        /// <param name="synchronizer">Synchronize to use</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Value from the cache/factory and the value is stored in the cache.</returns>
        public static Task<T> Cache<T>(this CachePolicy policy, string key, Func<Task<T>> factory, ISynchronizer synchronizer, CancellationToken cancellationToken)
        {
            return policy.Cache(key, factory, synchronizer, key, cancellationToken);
        }

        /// <summary>
        /// Synchronise access to a policy invocation to get/create a value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy">Policy to use</param>
        /// <param name="factory">Factory to create the entity if not in the cache</param>
        /// <param name="key">Cache key to use</param>
        /// <param name="synchronizer">Synchronize to use</param>
        /// <param name="syncKey">Synchronization key to use</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Value from the cache/factory and the value is stored in the cache.</returns>
        public static Task<T> Cache<T>(this CachePolicy policy, string key, Func<Task<T>> factory, ISynchronizer synchronizer, string syncKey, CancellationToken cancellationToken)
        {
            return synchronizer.Synchronize(syncKey, () => policy.Cache(key, factory), cancellationToken);
        }

        /// <summary>
        /// Synchronize a function invocation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="synchronizer"></param>
        /// <param name="key">Key to use</param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static T Synchronize<T>(this ISynchronizer synchronizer, string key, Func<T> func, CancellationToken cancellationToken = default(CancellationToken))
        {
            var semaphore = synchronizer.Synchronizer(key, out var isOwner);

            semaphore.Wait(cancellationToken);
            try
            {
                return func();
            }
            finally
            {
                if (isOwner)
                {
                    synchronizer.Remove(key);
                }
                semaphore.Release();
            }
        }

        /// <summary>
        /// Synchronize an asynchronous function invocation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="synchronizer"></param>
        /// <param name="key">Key to use</param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> Synchronize<T>(this ISynchronizer synchronizer, string key, Func<Task<T>> func, CancellationToken cancellationToken)
        {
            var semaphore = synchronizer.Synchronizer(key, out var isOwner);

            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await func();
            }
            finally
            {
                if (isOwner)
                {
                    synchronizer.Remove(key);
                }
                semaphore.Release();
            }
        }

        /// <summary>
        /// Implements a cache for a function, invoking it according to the policy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="policy"></param>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static Task<T> Cache<T>(this CachePolicy policy, string key, Func<Task<T>> factory)
        {
#if NETSTANDARD2_0
            return policy.ExecuteAsync(context => factory.Invoke(), new Context(key));
#else
            return policy.Execute(context => factory.Invoke(), new Context(key));
#endif
        }

        /// <summary>
        /// Computes a cache key from a caller, memberName and parameter values
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="caller"></param>
        /// <param name="memberName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string CacheKey(this IPrincipal principal, object caller, [CallerMemberName] string memberName = "", params object[] values)
        {
            return principal.CacheKey(caller.GetType().Name, memberName, values);
        }

        /// <summary>
        /// Computes a cache key from a service, memberName and parameter values
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="service"></param>
        /// <param name="memberName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string CacheKey(this IPrincipal principal, string service, [CallerMemberName] string memberName = "", params object[] values)
        {
            var contextName = $"{service}.{memberName}";

            return principal.CacheKey(contextName, values);
        }

        /// <summary>
        /// Computes a cache key from a context and parameter values
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="contextName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string CacheKey(this IPrincipal principal, string contextName, params object[] values)
        {
            var cp = principal as ClaimsPrincipal;

            var foo = new List<object>(values);
            foo.Insert(0, cp.ToCacheKey());

            return contextName.CacheKey(foo.ToArray());
        }

        /// <summary>
        /// Compute a cache key from a caller, method and parameter values
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="memberName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string CacheKey(this object caller, [CallerMemberName] string memberName = "", params object[] values)
        {
            return caller.GetType().Name.CacheKey(memberName, values);
        }

        /// <summary>
        /// Compute a cache key from a service name, method and parameter values.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="memberName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string CacheKey(this string service, [CallerMemberName] string memberName = "", params object[] values)
        {
            var contextName = $"{service}.{memberName}";

            return contextName.CacheKey(values);
        }

        /// <summary>
        /// Compute a cache key from a context name and values
        /// </summary>
        /// <param name="contextName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string CacheKey(this string contextName, params object[] values)
        {
            var sb = new StringBuilder(values.Length + 2);

            foreach (var x in values)
            {
                sb.AddElement(x);
            }

            // Put the context on the end to differentiate from other methods etc.
            sb.AddElement(contextName);

            return sb.ToString();
        }

        /// <summary>
        /// Compute a cache key from a claims principal
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static string ToCacheKey(this ClaimsPrincipal principal)
        {
            // HACK: Probably need something better than this
            return string.IsNullOrEmpty(principal?.Identity?.Name) ? "Unknown" : principal.Identity.Name;
        }

        private static void AddElement(this StringBuilder builder, object value)
        {
            builder.Append(value);
            builder.Append(KeySeparator);
        }
    }
}
#endif