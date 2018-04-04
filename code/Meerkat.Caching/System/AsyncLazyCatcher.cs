using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meerkat
{
    //internal class AsyncLazyCatcher<T>
    //{
    //    private readonly Func<Task<T>> factory;
    //    private Lazy<T> lazy;

    //    public AsyncLazyCatcher(Func<Task<T>> factory)
    //    {
    //        this.factory = factory;
    //        lazy = new Lazy<Func<Task<T>>>(factory);
    //    }

    //    public T Value
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return lazy.Value;
    //            }
    //            catch (Exception)
    //            {
    //                lazy = new Lazy<Func<Task<T>>>(factory);
    //                throw;
    //            }
    //        }
    //    }
    //}
}
