using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Interface representing a component that can report light level/color
    /// </summary>
    public interface IReportLight
    {
        Color Light { get; }
    }
}
