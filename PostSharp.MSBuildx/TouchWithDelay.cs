using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.MSBuild
{
    /// <summary>
    /// <b>[MSBuild Task]</b> Touches a file according to the modification time of another file.
    /// </summary>
    public sealed class PostSharp15TouchWithDelay : Task
    {
        private ITaskItem[] files;

        /// <summary>
        /// Gets or sets the files to be touched. Required.
        /// </summary>
        [Required]
        public ITaskItem[] Files
        {
            get { return files; }
            set { files = value; }
        }

        private long delay = 1000;

        /// <summary>
        /// Gets or sets the delay w.r.t. <see cref="ReferenceFile"/>.
        /// </summary>
        /// <value>
        /// The number of milliseconds. Default is 1000.
        /// </value>
        public long Delay
        {
            get { return delay; }
            set { delay = value; }
        }

        private ITaskItem referenceFile;

        /// <summary>
        /// Gets or sets the file giving the reference time.
        /// </summary>
        [Required]
        public ITaskItem ReferenceFile
        {
            get { return referenceFile; }
            set { referenceFile = value; }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            DateTime touchTime = File.GetLastWriteTime( this.referenceFile.ItemSpec ).AddMilliseconds( delay );
            foreach ( ITaskItem file in files )
            {
                this.Log.LogMessage( "Touching {0}.", file.ItemSpec );
                if ( !File.Exists( file.ItemSpec ) )
                {
                    using ( FileStream fileStream = File.OpenWrite( file.ItemSpec ) )
                    {
                        fileStream.Flush();
                    }
                }

                File.SetLastWriteTime( file.ItemSpec, touchTime );
            }

            return true;
        }
    }
}