using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFluent;

namespace Ariane.Tests
{
    public static class TestHelper
    {
        public static async Task WaitFor(Func<bool> action)
        {
            var retryCount = 0;
            bool result = false;
            while(true)
            {
                result = action();
                if (result)
                {
                    break;
                }
                retryCount++;
                if (retryCount > 10)
                {
                    break;
                }
                await Task.Delay(1 * 1000);
            }
            Check.That(result).IsTrue();
        }
    }
}
