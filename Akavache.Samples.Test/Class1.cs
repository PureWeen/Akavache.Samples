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
        public void MethodTest()
        {
            BlobCache.InMemory
                .GetAndFetchLatest<object>("key",
                () => Observable.Throw<ConnectivityException>(new ConnectivityException()))
                .Catch((ConnectivityException conn) =>
                {
                    //don't return anything if connectivity is broken
                    return Observable.Never<object>();                   

                })
                .Subscribe();
        }
    }
}
