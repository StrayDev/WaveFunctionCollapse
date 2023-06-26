using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugModuleData : MonoBehaviour
{
    [SerializeField] private Module Module;

    public void SetModuleData(Module m) => Module = m; 
}
