using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Xunit;

namespace Akavache.Samples.Test
{
    public class Class1
    {
        public class ConnectivityException : Exception
        {

        }

        [Fact]
        public void DontStoreTheValueReturnedFromTheGetAndFetchFunction()
        {
            BlobCache.InMemory
                .GetAndFetchLatest<object>("key",
                () => Observable.Throw<ConnectivityException>(new ConnectivityException()))
                .Catch((ConnectivityException conn) => Observable.Never<object>())
                .Subscribe();
        }
    }
}
