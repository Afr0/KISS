﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using KISS;

namespace PDPatcher
{
    public class FileManager
    {
        /// <summary>
        /// Takes a backup of all files in the client's manifest.
        /// </summary>
        /// <param name="Manifest">The client's manifest.</param>
        /// <param name="WorkingDir">The client's residing directory.</param>
        public static void Backup(ManifestFile Manifest, string WorkingDir)
        {
            if(!Directory.Exists(WorkingDir + "Backup"))
                Directory.CreateDirectory(WorkingDir);

            foreach (PatchFile PFile in Manifest.PatchFiles)
            {
                FileManager.CreateDirectory(WorkingDir + "Backup\\" + PFile.Address);
                File.Copy(WorkingDir + PFile.Address, WorkingDir + "Backup\\" + PFile.Address);
            }
        }

        /// <summary>
        /// Creates a directory if it does not yet exist.
        /// </summary>
        /// <param name="FilePath">Path of the director(ies) to create.</param>
        public static void CreateDirectory(string FilePath)
        {
            string Dir = FilePath.Replace(Path.GetFileName(FilePath), "");

            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
        }
    }
}
