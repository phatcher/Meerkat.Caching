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

        public SemaphoreSlim Synchronizer(string key, out bool isOwner)
        {
            isOwner = false;

            // Adapted as per https://github.com/aspnet/Extensions/issues/708
            if (!locks.TryGetValue(key, out var semaphore))
            {
                SemaphoreSlim createdSemaphore = null;
                // Try to add the value, this is not atomic, so multiple semaphores could be created, but just one will be stored!
                semaphore = locks.GetOrAdd(key, k => createdSemaphore = new SemaphoreSlim(1));
                if (createdSemaphore != semaphore)
                {
                    // This semaphore was not the one that made it into the dictionary, will not be used!
                    createdSemaphore?.Dispose();
                }
                else
                {
                    isOwner = true;
                }
            }

            return semaphore;
        }

        public void Remove(string key)
        {
            locks.TryRemove(key, out _);
        }
    }
}