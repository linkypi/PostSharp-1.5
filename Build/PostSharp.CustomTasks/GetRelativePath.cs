using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace PostSharp.CustomTasks
{
    public sealed class GetRelativePath : Task
    {

        private ITaskItem[] paths;

        [Required]
        public ITaskItem[] Paths
        {
            get { return paths; }
            set { paths = value; }
        }

        private ITaskItem[] pathsWithMetadata;

        [Output]
        public ITaskItem[] PathsWithMetadata
        {
            get { return pathsWithMetadata; }
            set { pathsWithMetadata = value; }
        }


        private string referenceDirectory;

        [Required]
        public string ReferenceDirectory
        {
            get { return referenceDirectory; }
            set { referenceDirectory = value; }
        }

        public override bool Execute()
        {
            // Create an URI for the reference directory.
            if (!referenceDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                this.referenceDirectory = this.referenceDirectory + Path.DirectorySeparatorChar;
            }
            Uri referenceUri = new Uri(Path.GetFullPath(this.referenceDirectory));

            this.pathsWithMetadata = new ITaskItem[this.paths.Length];


            // Look on each input full directory.
            for (int i = 0; i < this.paths.Length; i++)
            {

                string fullDirectory = this.paths[i].ItemSpec;

                /*
                 * // Normalize.
                if (!fullDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    fullDirectory = fullDirectory + Path.DirectorySeparatorChar;
                }
                 */

                Uri fullUri = new Uri(Path.GetFullPath(fullDirectory));
                Uri inverseUri = fullUri.MakeRelativeUri(referenceUri);
                Uri relativeUri = referenceUri.MakeRelativeUri(fullUri);
                string backwardPath = inverseUri.OriginalString.Replace('/', Path.DirectorySeparatorChar);
                string forwardPath = relativeUri.OriginalString.Replace('/', Path.DirectorySeparatorChar);
                this.pathsWithMetadata[i] = new TaskItem(fullDirectory);
                this.pathsWithMetadata[i].SetMetadata("BackwardPath", backwardPath);
                this.pathsWithMetadata[i].SetMetadata("ForwardPath", forwardPath);
            }



            return true;


        }



    }
}
