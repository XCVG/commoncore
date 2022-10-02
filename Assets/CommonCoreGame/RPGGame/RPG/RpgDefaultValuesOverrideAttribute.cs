using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonCore.RpgGame.Rpg
{
    /// <summary>
    /// Declares a class as a full override for RpgDefaultValues
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RpgDefaultValuesOverrideAttribute : Attribute
    {
    }
}
