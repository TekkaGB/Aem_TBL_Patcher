﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aem_TBL_Patcher
{
    struct PatchEdit
    {
        public long Offset { get; set; }
        public byte[] BytesEdit { get; set; }
    }

    class Program
    {
        private static string currentDir = String.Empty;

        private static EncountPatcher encPatcher = new EncountPatcher();
        private static SkillPatcher sklPatcher = new SkillPatcher();
        private static UnitPatcher untPatcher = new UnitPatcher();
        private static PersonaPatcher psaPatcher = new PersonaPatcher();

        static void Main(string[] args)
        {
            Console.WriteLine("Aemulus TBL Patcher");
            currentDir = Directory.GetCurrentDirectory();
            CreatePatches();
            Console.WriteLine("Enter any key to exit...");
            Console.ReadLine();
        }

        private static void CreatePatches()
        {
            string originalFolderDir = $@"{currentDir}\original";
            string moddedFolderDir = $@"{currentDir}\modded";
            string patchesFolderDir = $@"{currentDir}\patches";

            try
            {
                Directory.CreateDirectory(originalFolderDir);
                Directory.CreateDirectory(moddedFolderDir);
                if (Directory.Exists(patchesFolderDir))
                {
                    Directory.Delete(patchesFolderDir, true);
                }
                Directory.CreateDirectory(patchesFolderDir);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            string[] modTblFiles = null;

            try
            {
                modTblFiles = Directory.GetFiles(moddedFolderDir, "*.tbl", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (modTblFiles == null)
                return;

            foreach(string tblFile in modTblFiles)
            {
                string originalTblFile = $@"{originalFolderDir}\{Path.GetFileName(tblFile)}";

                if (!File.Exists(originalTblFile))
                {
                    Console.WriteLine($"Error: Missing Original TBL File: {Path.GetFileName(tblFile)}");
                    continue;
                }

                string tblTag = GetTblTag(Path.GetFileNameWithoutExtension(tblFile));
                if (tblTag == null)
                {
                    Console.WriteLine("TBL tag was not found!");
                    return;
                }

                try
                {
                    byte[] originalBytes = File.ReadAllBytes(originalTblFile);
                    byte[] moddedBytes = File.ReadAllBytes(tblFile);

                    if (originalBytes.Length != moddedBytes.Length)
                    {
                        ConsoleError($"{Path.GetFileName(tblFile)} (Original): {originalBytes.Length} bytes\n{Path.GetFileName(tblFile)} (Modded): {moddedBytes.Length} bytes");
                        ConsoleError("Error: File size mismatch!");
                        return;
                    }

                    /*
                    if (tblTag.Equals("ENC"))
                    {
                        Console.WriteLine("Using Encounter Patcher");
                        patches = encPatcher.GetPatches(originalBytes, moddedBytes);
                    }
                    else if (tblTag.Equals("SKL"))
                    {
                        Console.WriteLine("Using Skill Patcher");
                        patches = sklPatcher.GetPatches(originalBytes, moddedBytes);
                    }
                    else if (tblTag.Equals("UNT"))
                    {
                        Console.WriteLine("Using Unit Patcher");
                        patches = untPatcher.GetPatches(originalBytes, moddedBytes);
                    }
                    else if (tblTag.Equals("PSA"))
                    {
                        Console.WriteLine("Using Persona Patcher");
                        patches = psaPatcher.GetPatches(originalBytes, moddedBytes);
                    }
                    else
                    {
                        patches = new List<PatchEdit>();

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
                    */


                    List<PatchEdit> patches = new List<PatchEdit>();

                    IPatcher tblPatcher = GetPatcher(tblTag);
                    if (tblPatcher != null)
                        patches = tblPatcher.GetPatches(originalBytes, moddedBytes);

                    // skip tbl tags with no patches needed
                    if (patches.Count < 1)
                        continue;

                    StringBuilder tblLogBuilder = new StringBuilder();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{tblTag}: Creating patches...");
                    Console.ResetColor();

                    foreach (PatchEdit patch in patches)
                    {
                        tblLogBuilder.AppendLine($"Offset: {patch.Offset.ToString("X")} Length: {patch.BytesEdit.Length}");

                        string outputFile = $@"{currentDir}\patches\{tblTag}_{patch.Offset.ToString("X")}.tblpatch";
                        using (FileStream fs = new FileStream(outputFile, FileMode.Create))
                        {
                            foreach (byte tagByte in Encoding.ASCII.GetBytes(tblTag))
                                fs.WriteByte(tagByte);
                            foreach (byte offsetByte in BitConverter.GetBytes(patch.Offset).Reverse())
                                fs.WriteByte(offsetByte);
                            foreach (byte editByte in patch.BytesEdit)
                                fs.WriteByte(editByte);
                        }
                    }

                    string logFilePath = $@"{currentDir}\log_{tblTag}.txt";
                    File.WriteAllText(logFilePath, tblLogBuilder.ToString());
                    Console.WriteLine($"{tblTag} Log: {logFilePath}");

                    Console.WriteLine($"Total Patches: {patches.Count}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static IPatcher GetPatcher(string tblTag)
        {
            IPatcher patcher = null;

            switch (tblTag)
            {
                case "ENC":
                    Console.WriteLine("Using Encount Patcher");
                    patcher = new EncountPatcher();
                    break;
                case "SKL":
                    Console.WriteLine("Using Skill Patcher");
                    patcher = new SkillPatcher();
                    break;
                case "UNT":
                    Console.WriteLine("Using Unit Patcher");
                    patcher = new UnitPatcher();
                    break;
                case "PSA":
                    Console.WriteLine("Using Persona Patcher");
                    patcher = new PersonaPatcher();
                    break;
                case "MSG":
                    Console.WriteLine("Using Msg Patcher");
                    patcher = new MsgPatcher();
                    break;
                default:
                    break;
            }

            return patcher;
        }

        private static void ConsoleError(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ResetColor();
        }

        private static string GetTblTag(string tblName)
        {
            switch (tblName)
            {
                case "SKILL":
                    return "SKL";
                case "UNIT":
                    return "UNT";
                case "MSG":
                    return "MSG";
                case "PERSONA":
                    return "PSA";
                case "ENCOUNT":
                    return "ENC";
                case "EFFECT":
                    return "EFF";
                case "MODEL":
                    return "MDL";
                case "AICALC":
                    return "AIC";
                default:
                    return null;
            }
        }
    }
}
