using System;

namespace Meerkat
{
    internal class LazyCatcher<T>
    {
        private readonly Func<T> factory;
        private Lazy<T> lazy;

        public LazyCatcher(Func<T> factory)
        {
            this.factory = factory;
            lazy = new Lazy<T>(factory);
        }

        public T Value
        {
            get
            {
                try
                {
                    return lazy.Value;
                }
                catch (Exception)
                {
                    lazy = new Lazy<T>(factory);
                    throw;
                }
            } 
        }
    }
}
