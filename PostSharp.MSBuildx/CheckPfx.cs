using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.MSBuild
{
    /// <summary>
    /// <b>[MSBuild Task]</b> Checks the kind of key being used to sign an assembly.
    /// If the key is a PFX, exports the SNK from it, sets the <see cref="PublicKeyFile"/>
    /// to the path of this key and sets the <see cref="SignAfterPostSharp"/> property
    /// to <b>true</b>
    /// </summary>
    public sealed class PostSharp15CheckPfx : Task
    {
        private string keyFile;
        private string publicKeyFile;
        private string postSharpKeyFile;
        private bool signAfterPostSharp;

        /// <summary>
        /// Full path to the key file.
        /// </summary>
        [Required]
        public string KeyFile { get { return keyFile; } set { keyFile = value; } }

        /// <summary>
        /// After task execution, gets the full path to the public key.
        /// </summary>
        [Output]
        public string PublicKeyFile { get { return publicKeyFile; } set { publicKeyFile = value; } }

        /// <summary>
        /// After task execution, gets the full path of the key that should
        /// be used to sign the assembly after PostSharp.
        /// </summary>
        [Output]
        public string PostSharpKeyFile { get { return postSharpKeyFile; } set { postSharpKeyFile = value; } }

        /// <summary>
        /// After task exection, determines whether the assembly should be 
        /// signed after PostSharp execution.
        /// </summary>
        [Output]
        public bool SignAfterPostSharp { get { return signAfterPostSharp; } set { signAfterPostSharp = value; } }

        /// <inheritdoc />
        public override bool Execute()
        {
           if ( !string.IsNullOrEmpty(this.keyFile ))
           {
               if (StringComparer.InvariantCultureIgnoreCase.Equals(Path.GetExtension(this.keyFile), ".pfx"))
               {
                   this.signAfterPostSharp = true;
                   this.publicKeyFile = Path.ChangeExtension( this.keyFile, "snk" );
                   this.postSharpKeyFile = this.publicKeyFile;

                   this.Log.LogMessage( MessageImportance.Low, "The private key is a PFX. Signing using the public key (SNK).");

                   if (!File.Exists(this.publicKeyFile) )
                   {
                       this.Log.LogError( "When you sign an assembly using a PFX private key, you should " +
                           "export the public key into an SNK file. We could not find the file '{0}'." +
                           "You can generate it using the command 'sn -p MyKey.pfx MyKey.snk'.",
                           this.publicKeyFile);
                       return false;
                   }

                   return true;
               }
           } 

            this.publicKeyFile = "";
            this.signAfterPostSharp = false;
            this.postSharpKeyFile = this.keyFile;
            return true;
        }

       
    }
}
