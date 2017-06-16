#region Copyright (c) 2004-2010 by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of compile-time components of PostSharp.                *
 *                                                                             *
 *   This library is free software: you can redistribute it and/or modify      *
 *   it under the terms of the version 3 of the GNU General Public License     *
 *   as published by the Free Software Foundation.                             *
 *                                                                             *
 *   This library is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             *
 *   GNU General Public License for more details.                              *
 *                                                                             *
 *   You should have received a copy of the GNU General Public License         *
 *   along with this library.  If not, see <http://www.gnu.org/licenses/>.     *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion

#region Using directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.ModuleWriter;
using PostSharp.Utilities;

#endregion

namespace PostSharp.Extensibility.Tasks
{
    /// <summary>
    /// <see cref="Task"/> that compiles a module into MSIL
    /// or into binary form.
    /// </summary>
    public sealed class CompileTask : Task
    {
        #region Member fields

        /// <summary>
        /// Directory in which IL and resource files should
        /// be returned, or <b>null</b> to store these
        /// files in a temporary directory.
        /// </summary>
        private string intermediateDirectory;

        /// <summary>
        /// Target location of the binary module should be
        /// written, or <b>null</b> if the module should not
        /// be written.
        /// </summary>
        private string targetFile;


        /// <summary>
        /// Determines whether the intermediate directory should
        /// be cleaned after compilation.
        /// </summary>
        private BoolWithDefault cleanIntermediateDirectory;

        /// <summary>
        /// Determines how debugging will be supported for
        /// the target module.
        /// </summary>
        private DebugOption debugOption = DebugOption.Auto;

        private ILWriterCompatibility compatibility;

        private bool signAssembly;

        private string privateKeyLocation;

        private bool forbidSignAssembly;

        private string encoding = "Unicode";

        #endregion

        /// <summary>
        /// Initializes a new <see cref="CompileTask"/>.
        /// </summary>
        public CompileTask()
        {
        }


        /// <inheritdoc />
        protected override bool Validate()
        {
            bool valid = true;

            // Cross-validation
            if ( this.targetFile == null && this.intermediateDirectory == null )
            {
                CoreMessageSource.Instance.Write( SeverityType.Error, "PS0018", new object[] {this.TaskType.Name} );
                valid = false;
            }

            if ( this.targetFile == null && this.cleanIntermediateDirectory == BoolWithDefault.False )
            {
                CoreMessageSource.Instance.Write( SeverityType.Error, "PS0020", new object[] {this.TaskType.Name} );
                valid = false;
            }

            if ( this.signAssembly && string.IsNullOrEmpty( this.privateKeyLocation ) )
            {
                valid = false;
                CoreMessageSource.Instance.Write( SeverityType.Error, "PS0075", null );
            }

            return valid;
        }


        /// <summary>
        /// Gets or sets the intermediate directory 
        /// (where IL and resource files will be written).
        /// </summary>
        /// <value>
        /// A directory path, or <b>null</b> to store
        /// intermediate files in a temporary directory.
        /// </value>
        [Description( "Intermediate directory, where IL and resource files are written, " +
                      "or an empty string, or an empty string if intermediate files should not " +
                      "be preserved after compilation." )]
        [ConfigurableProperty]
        public string IntermediateDirectory
        {
            get { return this.intermediateDirectory; }
            set
            {
                if ( string.IsNullOrEmpty( value ) )
                {
                    this.intermediateDirectory = null;
                }
                else
                {
                    this.intermediateDirectory = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the target location of the binary module.
        /// </summary>
        /// <value>
        /// The location of the target binary module (absolute or relative to the project
        /// file), or <b>null</b> if the IL code should not be assembled.
        /// </value>
        [Description( "Target location of the binary module, or an empty string if " +
                      "the intermediate files should not be compiled." )]
        [ConfigurableProperty( true )]
        public string TargetFile
        {
            get { return this.targetFile; }
            set
            {
                if ( string.IsNullOrEmpty( value ) )
                {
                    this.targetFile = null;
                }
                else
                {
                    this.targetFile = value;
                }
            }
        }

        /// <summary>
        /// Determines how debugging will be supported for
        /// the target module.
        /// </summary>
        [Description( "Determines how debugging will be supported in the target module." )]
        [DefaultValue( DebugOption.Auto )]
        [ConfigurableProperty]
        public DebugOption DebugOption { get { return debugOption; } set { debugOption = value; } }

        /// <summary>
        /// Determines whether the assembly should be signed using a digital key.
        /// If set to <see cref="BoolWithDefault.Default"/>, the task will look
        /// for the custom attributes <see cref="AssemblyKeyFileAttribute"/>
        /// and <see cref="AssemblyKeyNameAttribute"/>.
        /// </summary>
        [ConfigurableProperty]
        public bool SignAssembly { get { return signAssembly; } set { signAssembly = value; } }

        /// <summary>
        /// If <b>true</b>, the assembly will not be signed. This property
        /// overwrites the <see cref="SignAssembly"/> property and the
        ///  custom attributes <see cref="AssemblyKeyFileAttribute"/>
        /// and <see cref="AssemblyKeyNameAttribute"/>.
        /// </summary>
        [ConfigurableProperty]
        public bool ForbidSignAssembly { get { return forbidSignAssembly; } set { forbidSignAssembly = value; } }

        /// <summary>
        /// If <see cref="SignAssembly"/> is <b>true</b>, full path of the key
        /// file that should be used to sign the assembly. If the key should not
        /// be taken from a file, but from a key repository, this property
        /// should start with an '<b>@</b>' sign.
        /// </summary>
        /// <remarks>
        /// The path may be given in the form <c>{Reference Directory}<b>|{Relative Path}</b></c>.
        /// In this case, the relative path given in the right part of the string is resolved according
        /// to the reference directory given in the left part. If a full path is given in the right part,
        /// the reference directory is ignored.
        /// </remarks>
        [ConfigurableProperty]
        public string PrivateKeyLocation { get { return privateKeyLocation; } set { privateKeyLocation = value; } }

        /// <summary>
        /// Determines whether the intermediate directory should
        /// be cleaned after compilation.
        /// </summary>
        [ConfigurableProperty]
        public BoolWithDefault CleanIntermediate { get { return cleanIntermediateDirectory; } set { cleanIntermediateDirectory = value; } }

        /// <summary>
        /// Not really supported.
        /// </summary>
        [ConfigurableProperty]
        public ILWriterCompatibility Compatibility { get { return compatibility; } set { compatibility = value; } }

        /// <summary>
        /// Gets or sets the encoding of the intermediate MSIL file.
        /// It needs to be supported by ILASM.
        /// </summary>
        [ConfigurableProperty]
        public string Encoding { get { return encoding; } set { encoding = value; } }


        /// <summary>
        /// Creates a directory and emits a message in case
        /// of exception.
        /// </summary>
        /// <param name="directory">The full directory path.</param>
        [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
        private void CreateDirectory( string directory )
        {
            try
            {
                Directory.CreateDirectory( directory );
            }
            catch ( SystemException e )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0019", new object[]
                                                                                    {
                                                                                        this.TaskType.Name,
                                                                                        directory, e.Message
                                                                                    } );
            }
        }

        private string UpdateAssemblyKeyFileCustomAttribute()
        {
            ModuleDeclaration module = this.Project.Module;

            // Get the type of both custom attributes.
            ITypeSignature assemblyKeyFileAttributeType =
                module.FindType( "System.Reflection.AssemblyKeyFileAttribute, mscorlib",
                                 BindingOptions.OnlyExisting | BindingOptions.DontThrowException );
            ITypeSignature assemblyKeyNameAttributeType =
                module.FindType( "System.Reflection.AssemblyKeyNameAttribute, mscorlib",
                                 BindingOptions.OnlyExisting | BindingOptions.DontThrowException );
            ITypeSignature assemblyDelaySignAttributeType =
                module.FindType( "System.Reflection.AssemblyDelaySignAttribute, mscorlib",
                                 BindingOptions.OnlyExisting | BindingOptions.DontThrowException );

            // If the types were not found, there may not be any instance of these custom attributes.
            if ( assemblyKeyFileAttributeType == null && assemblyKeyNameAttributeType == null )
            {
                return null;
            }

            string keyLocation = null;

            List<CustomAttributeDeclaration> toBeRemoved = new List<CustomAttributeDeclaration>();
            foreach ( CustomAttributeDeclaration attribute in this.Project.Module.AssemblyManifest.CustomAttributes )
            {
                if ( attribute.Constructor.DeclaringType == assemblyKeyFileAttributeType )
                {
                    string path = (string) attribute.ConstructorArguments[0].Value.GetRuntimeValue();
                    if ( !Path.IsPathRooted( path ) )
                    {
                        path = Path.Combine("..", path);
                        keyLocation = Path.Combine( Path.GetDirectoryName( module.Assembly.Location ), path );
                    }
                    else
                    {
                        keyLocation = path;
                    }

                    Trace.CompileTask.WriteLine( "Location of the private key read from the custom attribute " +
                                                 "AssemblyKeyFileAttribute: {{{0}}}.", keyLocation );
                    toBeRemoved.Add( attribute );
                }
                else if ( attribute.Constructor.DeclaringType == assemblyKeyNameAttributeType ||
                          attribute.Constructor.DeclaringType == assemblyDelaySignAttributeType )
                {
                    toBeRemoved.Add( attribute );
                }
            }

            foreach ( CustomAttributeDeclaration attribute in toBeRemoved )
            {
                this.Project.Module.AssemblyManifest.CustomAttributes.Remove( attribute );
            }

            return keyLocation;
        }


        /// <inheritdoc />
        public override bool Execute()
        {
            // Write some warnings.
            if ( (this.Compatibility & ILWriterCompatibility.Bugs) != 0 )
            {
                CoreMessageSource.Instance.Write( SeverityType.Warning,
                                                  "PS0057", new object[] {this.Compatibility} );
            }

            // Detect the need for strong name signing.
            bool concreteSignAssembly;
            string concreteKeyLocation;


            if ( this.forbidSignAssembly )
            {
                concreteSignAssembly = false;
                concreteKeyLocation = null;
            }
            else
            {
                bool hasPublicKey = this.Project.Module.AssemblyManifest.GetPublicKey() != null;

                if ( hasPublicKey )
                {
                    if ( this.signAssembly )
                    {
                        Trace.CompileTask.WriteLine( "The assembly will be signed, because it was explicitely required." );
                        concreteSignAssembly = true;

                        concreteKeyLocation = this.Project.GetFullPath( this.privateKeyLocation );

                        if ( concreteKeyLocation == null )
                            return false;
                    }
                    else
                    {
                        concreteKeyLocation = this.UpdateAssemblyKeyFileCustomAttribute();
                        concreteSignAssembly = concreteKeyLocation != null;

                        if ( concreteSignAssembly )
                        {
                            Project.ValidatePath( concreteKeyLocation );
                            Trace.CompileTask.WriteLine(
                                "The assembly will be signed, because a strong name custom attribute was found." );
                        }
                        else
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0073", null );
                        }
                    }
                }
                else
                {
                    concreteSignAssembly = false;
                    concreteKeyLocation = null;

                    if ( this.signAssembly )
                    {
                        CoreMessageSource.Instance.Write( SeverityType.Warning, "PS0074", null );
                    }
                }
            }

            // Take track of intermdiate files and directory.
            List<string> intermediateFiles = new List<string>();
            bool removeIntermediateDirectory;

            string myIntermediateDirectory;
            if ( this.intermediateDirectory == null )
            {
                string tempPath = Path.GetTempPath();
                myIntermediateDirectory = Path.Combine( tempPath, "postsharp-" + Guid.NewGuid().ToString() );

                removeIntermediateDirectory = true;
            }
            else
            {
                myIntermediateDirectory = this.Project.GetFullPath( this.intermediateDirectory );
                if ( myIntermediateDirectory == null )
                {
                    // Error evaluating the expression.
                    return false;
                }

                removeIntermediateDirectory = false;
                // Create the intermediate directory if it does not exist.
            }

            Trace.CompileTask.WriteLine( "The intermediate directory is {{{0}}}.", myIntermediateDirectory );

            if ( !Directory.Exists( myIntermediateDirectory ) )
            {
                Trace.CompileTask.WriteLine( "Creating the directory {{{0}}}.", myIntermediateDirectory );
                CreateDirectory( myIntermediateDirectory );
            }

            try
            {
                ModuleDeclaration module = this.Project.Module;
                string moduleFileName = Path.GetFileNameWithoutExtension(module.FileName);

                // Writing IL
                string ilFileName =
                    Path.Combine( myIntermediateDirectory, moduleFileName + ".il" );
                intermediateFiles.Add( ilFileName );

                using ( Stream stream = File.Create( ilFileName ) )
                {
                    using ( StreamWriter streamWriter = new StreamWriter( stream, 
                        string.IsNullOrEmpty( this.encoding) ? this.Project.Platform.GetDefaultMsilEncoding() : 
                        System.Text.Encoding.GetEncoding( this.encoding ), 128*1024 ))
                    {
                        
                        Trace.CompileTask.WriteLine( "Genering MSIL..." );
                        ILWriter ilWriter = new ILWriter(streamWriter);
                        ilWriter.Options.Compatibility = this.compatibility | 
                            this.Project.Platform.GetILWriterCompatibility();

                        /*if ( this.compatibility == ILWriterCompatibility.MsilDasm2 )
                        {
                            ilWriter.Options.VerboseCustomAttributes = true; 
                        }*/

                        try
                        {
                            using ( HighPrecisionTimer timer = new HighPrecisionTimer() )
                            {
                                ilWriter.WriteCommentLine( string.Format(
                                                               CultureInfo.CurrentCulture,
                                                               "Generated by PostSharp {0} at {1} (encoding = {2})",
                                                               ApplicationInfo.Version, DateTime.Now, streamWriter.Encoding ) );
                                module.WriteILDefinition( ilWriter );
                                Trace.Timings.WriteLine( "IL code generated in {0} ms.", timer.CurrentTime );
                            }
                        }
                        finally
                        {
                            ilWriter.Flush();
                        }
                    }
                }

                // Writing resources
                if ( module.AssemblyManifest != null )
                {
                    foreach ( ManifestResourceDeclaration resource in module.AssemblyManifest.Resources )
                    {
                        string resourceFileName = Path.Combine( myIntermediateDirectory, resource.Name );
                        intermediateFiles.Add( resourceFileName );

                        Trace.CompileTask.WriteLine( "Writing the managed resource file {{{0}}}...", resourceFileName );

                        using ( Stream stream = File.Create( resourceFileName ) )
                        {
                            const int bufferLen = 2048;
                            int chunkSize;
                            byte[] buffer = new byte[bufferLen];
                            while ( ( chunkSize = resource.ContentStream.Read( buffer, 0, bufferLen ) ) > 0 )
                            {
                                stream.Write( buffer, 0, chunkSize );
                            }
                            stream.Flush();
                            stream.Close();
                        }
                    }
                }

                // Writing unmanaged resources
                string resFileName;
                if ( this.Project.Module.FindMscorlib().GetPublicKeyToken()[0] == 0xb7 
                    && module.UnmanagedResources.Count > 0)
                {
                    resFileName =
                        Path.Combine( myIntermediateDirectory, moduleFileName + ".res" );

                    Trace.CompileTask.WriteLine( "Writing the unmanaged resource file {{{0}}}...", resFileName );

                    using ( Stream stream = File.Create( resFileName ) )
                    {
                        UnmanagedResourceWriter.WriteResources(module.UnmanagedResources, stream);
                    }

                    intermediateFiles.Add( resFileName );
                }
                else
                {
                    resFileName = null;
                }

                // Compile.
                if ( !string.IsNullOrEmpty( this.targetFile ) )
                {
                    string myTargetFile = this.Project.GetFullPath( this.targetFile );
                    if ( myTargetFile == null )
                    {
                        return false;
                    }
                    this.Project.Properties["CompileTargetFileName"] = myTargetFile;

                    // Try to create the target file. This ensures it is possible and nicely handles errors.
                    CheckWritableFile( myTargetFile );
                    CheckWritableFile( Path.ChangeExtension( myTargetFile, "pdb" ) );

                    AssembleOptions options = new AssembleOptions( ilFileName, myTargetFile )
                                                  {
                                                      DebugOptions = this.debugOption,
                                                      UnmanagedResourceFile = resFileName,
                                                      SignAssembly = concreteSignAssembly,
                                                      PrivateKeyLocation = concreteKeyLocation
                                                  };

                    Trace.CompileTask.WriteLine( "Assembling using {{{0}}}.", this.Project.Platform.GetType().Name );
                    this.Project.Platform.Assemble( module, options );
                }

                return true;
            }
            finally
            {
                if ( this.cleanIntermediateDirectory == BoolWithDefault.True ||
                     ( this.cleanIntermediateDirectory == BoolWithDefault.Default &&
                       this.targetFile != null ) )
                {
                    foreach ( string file in intermediateFiles )
                    {
                        Trace.CompileTask.WriteLine( "Deleting the file {{{0}}}.", file );
                        File.Delete( file );
                    }

                    if ( removeIntermediateDirectory )
                    {
                        Trace.CompileTask.WriteLine( "Deleting the directory {{{0}}}.", myIntermediateDirectory );
                        Directory.Delete( myIntermediateDirectory );
                    }
                }
            }
        }

        [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes" )]
        private static void CheckWritableFile( string fileName )
        {
            // Try to create the target file. This ensures it is possible and nicely handles errors.
            try
            {
                // If the target file already exists, delete it.
                if ( File.Exists( fileName ) )
                {
                    File.Delete( fileName );
                }

                // Now create this test file.
                StreamWriter streamWriter = File.CreateText( fileName );
                streamWriter.Close();
                File.Delete( fileName );
                Trace.CompileTask.WriteLine( "The file {{{0}}} is writable.", fileName );
            }
            catch ( SystemException e )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0066",
                                                  new object[] {fileName, e.Message} );
            }
        }
    }
}
