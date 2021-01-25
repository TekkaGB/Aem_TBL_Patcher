﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Aem_TBL_Patcher.Patchers.P4G
{
    class EffectPatcher : BasePatcher
    {
        public EffectPatcher(byte[] originalBytes, byte[] moddedBytes) : base() { }

        protected override IPatchGenerator[] Patchers => new IPatchGenerator[] { new BytePatches(0, _moddedBytes.Length) };
    }
}
