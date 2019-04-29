using System.Threading;

namespace Meerkat.Caching
{
    /// <summary>
    /// Provides a lightweight synchronizer.
    /// </summary>
    public interface ISynchronizer
    {
        /// <summary>
        /// Provides a synchronizer for a key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isOwner"></param>
        /// <returns></returns>
        SemaphoreSlim Synchronizer(string key, out bool isOwner);

        /// <summary>
        /// Remove a synchronizer.
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
    }
}