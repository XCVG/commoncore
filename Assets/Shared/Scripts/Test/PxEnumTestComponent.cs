using PseudoExtensibleEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PxEnumTestComponent : MonoBehaviour
{
    public int EnumTest;

    private void Start()
    {
        Debug.Log($"PxEnumTest: value={EnumTest} name={PxEnum.GetName(typeof(PxTestBaseEnum), EnumTest)}");
    }
}
