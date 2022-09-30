using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.UI
{

    public interface ICustomLevelUpModal
    {
        LevelUpModalCallback Callback { get; set; }
    }
}