using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonCore.TestModule
{
    /// <summary>
    /// A test module intended to load early
    /// </summary>
    [CCEarlyModule]
    public class EarlyTestModule : CCModule
    {
        public EarlyTestModule()
        {
            Log("Initializing early test module");
        }
    }

    /// <summary>
    /// A test module intended to load late
    /// </summary>
    [CCLateModule]
    public class LateTestModule : CCModule
    {
        public LateTestModule()
        {
            Log("Initializing late test module");
        }
    }
}
