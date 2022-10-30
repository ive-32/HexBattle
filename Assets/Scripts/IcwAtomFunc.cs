using System;
using UnityEngine;

    class IcwAtomFunc
    {
        public static bool IsNull(object o, string caller) 
        { 
            if (o == null) Debug.LogError($"Some object is null on Awake in {caller}"); 
            return o == null; 
        }
    }
