﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Aem_TBL_Patcher
{
    class BytePatcher
    {
        public void GenerateBytePatches(List<PatchEdit> patches, byte[] originalBytes, byte[] moddedBytes)
        {
            for (long byteIndex = 0, totalBytes = originalBytes.Length; byteIndex < totalBytes; byteIndex++)
            {
                byte currentOriginalByte = originalBytes[byteIndex];
                byte currentModdedByte = moddedBytes[byteIndex];

                // mismatched bytes indicating edited bytes
                if (currentOriginalByte != currentModdedByte)
                {
                    PatchEdit newPatch = new PatchEdit();
                    newPatch.Offset = byteIndex;

                    // read ahead for the edited bytes
                    for (long byteEditIndex = byteIndex, byteCount = 0; byteEditIndex < totalBytes; byteEditIndex++, byteCount++)
                    {
                        // exit loop once bytes match again
                        if (originalBytes[byteEditIndex] == moddedBytes[byteEditIndex])
                        {
                            newPatch.BytesEdit = new byte[byteCount];
                            Array.Copy(moddedBytes, byteIndex, newPatch.BytesEdit, 0, byteCount);
                            byteIndex = byteEditIndex - 1;
                            break;
                        }
                    }

                    patches.Add(newPatch);
                }
            }
        }
    }
}
