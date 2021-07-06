using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchDB2.Interface
{
    public interface ICacheProvider
    {
        object Get(string key);

        void Invalidate(string key);

        bool IsSet(string key);

        void Set(string key, object data, int cacheTime = 30);
    }
}
