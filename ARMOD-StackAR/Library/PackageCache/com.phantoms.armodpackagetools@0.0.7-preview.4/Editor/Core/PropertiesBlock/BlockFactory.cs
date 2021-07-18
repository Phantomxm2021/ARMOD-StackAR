using System;
using UnityEngine;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public static class BlockFactory
    {
        public static AbstractBlock CreateBlock(string _blockType, Configures _configures)
        {
            Type tmp_Type = Type.GetType(_blockType);
            // ReSharper disable once PossiblyMistakenUseOfParamsMethod
            object tmp_Instance = Activator.CreateInstance(type: tmp_Type ?? throw new Exception(), args: _configures);
            return tmp_Instance as AbstractBlock;
        }
    }
}