using System.Collections.Concurrent;
using System.Threading;

namespace Meerkat.Caching
{
    /// <summary>
    /// Uses a <see cref="ConcurrentDictionary{TKey,TValue}"/> to hold the <see cref="SemaphoreSlim"/>s.
    /// </summary>
    public class ConcurrentSynchronizer : ISynchronizer
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> locks;

        public ConcurrentSynchronizer()
        {
            locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public SemaphoreSlim Synchronizer(string key)
        {
            return locks.GetOrAdd(key, new SemaphoreSlim(1));
        }
    }
}