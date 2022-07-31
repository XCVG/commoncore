using PseudoExtensibleEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[PseudoExtensible]
public enum PxTestBaseEnum
{
    Unspecified, First, Second, Something = 100
}

[PseudoExtend(typeof(PxTestBaseEnum))]
public enum PxTestExtensionEnum
{
    Third = 3, SomethingElse = 128
}
