using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace PostSharp.CustomTasks
{
    public sealed class XCopy : Task
    {
        private string files;

        [Required]
        public string Files
        {
            get { return files; }
            set { files = value; }
        }

        private string baseSourceDirectory;

        [Required]
        public string BaseSourceDirectory
        {
            get { return baseSourceDirectory; }
            set { baseSourceDirectory = value; }
        }

        private string baseTargetDirectory;

        [Required]
        public string BaseTargetDirectory
        {
            get { return baseTargetDirectory; }
            set { baseTargetDirectory = value; }
        }


        public override bool Execute()
        {

            string fullBaseSourceDirectory = Path.GetFullPath(this.baseSourceDirectory).TrimEnd('\\');
            string fullBaseTargetDirectory = Path.GetFullPath(this.baseTargetDirectory).TrimEnd('\\');

            this.Log.LogMessage("XCopy: SourceDirectory={0}, TargetDirectory={1}.",
                fullBaseSourceDirectory, fullBaseTargetDirectory);

            if (!Directory.Exists(fullBaseSourceDirectory))
            {
                this.Log.LogError("Cannot find the directory \"{0}\".", fullBaseSourceDirectory);
                return false;
            }

            foreach (string file in this.files.Split(';'))
            {
                string fullSourcePath = Path.GetFullPath(file);
                if (!File.Exists(fullSourcePath))
                {
                    this.Log.LogError("Cannot find the file \"{0}\".", file);
                    continue;
                }

                if (!fullSourcePath.ToLowerInvariant().StartsWith(fullBaseSourceDirectory.ToLowerInvariant()))
                {
                    this.Log.LogError("The file \"{0}\" is not located under the directory \"{1}\".",
                        file, this.baseSourceDirectory);
                    continue;
                }


                string fullTargetPath = Path.Combine(fullBaseTargetDirectory,
                    fullSourcePath.Substring(fullBaseSourceDirectory.Length + 1));

                // Create the directory
                string fullTargetDirectory = Path.GetDirectoryName(fullTargetPath);
                if (!Directory.Exists(fullTargetDirectory))
                {
                    Directory.CreateDirectory(fullTargetDirectory);
                }

                // Finally copy the file
                this.Log.LogMessage("Copying \"{0}\"...", file);
                this.Log.LogMessage( MessageImportance.Low, "{0} -> {1}",
                    fullSourcePath, fullTargetPath);
                File.Copy(fullSourcePath, fullTargetPath, true);
            }

            return true;
        }
    }
}
