using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.MSBuild
{
    /// <summary>
    /// <b>[MSBuild Task]</b> Signs an assembly using the <c>sn</c> utility.
    /// </summary>
    public sealed class PostSharp15Sign : ToolTask
    {
        private string keyFile;
        private string assembly;

        /// <inheritdoc />
        protected override string GenerateFullPathToTool()
        {
            string path =
                ToolLocationHelper.GetPathToDotNetFrameworkSdkFile( this.ToolName,
                                                                    TargetDotNetFrameworkVersion.Version20 );

            if ( path == null )
            {
                this.Log.LogError( "Cannot locate the utility sn.exe." );
            }

            return path;
        }

        /// <inheritdoc />
        protected override string ToolName
        {
            get { return "sn.exe"; }
        }

        /// <summary>
        /// Full path of the file containing the strong name key.
        /// </summary>
        [Required]
        public string KeyFile
        {
            get { return keyFile; }
            set { keyFile = value; }
        }

        /// <summary>
        /// Full path of the assembly to be signed.
        /// </summary>
        [Required]
        public string Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }

        /// <inheritdoc />
        protected override string GenerateCommandLineCommands()
        {
            return string.Format( "-R \"{0}\" \"{1}\"",
                                  this.assembly, this.keyFile );
        }
    }
}