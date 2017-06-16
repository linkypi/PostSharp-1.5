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

using PostSharp.PlatformAbstraction;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Options of the <see cref="TargetPlatformAdapter"/>.<see cref="TargetPlatformAdapter.Assemble"/> method.
    /// </summary>
    public sealed class AssembleOptions
    {
        private string sourceFile;
        private string targetFile;

        /// <summary>
        /// Initializes a new <see cref="AssembleOptions"/>.
        /// </summary>
        /// <param name="sourceFile">Source file.</param>
        /// <param name="targetFile">Target file.</param>
        public AssembleOptions( string sourceFile, string targetFile )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( sourceFile, "sourceFile" );
            ExceptionHelper.AssertArgumentNotNull( targetFile, "targetFile" );

            #endregion

            this.sourceFile = sourceFile;
            this.targetFile = targetFile;
        }

        /// <summary>
        /// Gets ot sets the debug options.
        /// </summary>
        public DebugOption DebugOptions { get; set; }

        /// <summary>
        /// Gets or sets the path of the file containing unmanaged resources in .RES format.
        /// </summary>
        /// <value>
        /// A complete file path, or <b>null</b> if the module does not contain unmanaged resources.
        /// </value>
        public string UnmanagedResourceFile { get; set; }

        /// <summary>
        /// Gets or sets the path of the source file.
        /// </summary>
        public string SourceFile
        {
            get { return sourceFile; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                sourceFile = value;
            }
        }

        /// <summary>
        /// Gets or sets the path of the target file.
        /// </summary>
        public string TargetFile
        {
            get { return targetFile; }
            set
            {
                #region Preconditions

                ExceptionHelper.AssertArgumentNotNull( value, "value" );

                #endregion

                targetFile = value;
            }
        }

        /// <summary>
        /// Determines whether the assembly should be signed using a digital key.
        /// </summary>
        public bool SignAssembly { get; set; }

        /// <summary>
        /// If <see cref="SignAssembly"/> is <b>true</b>, full path of the key
        /// file that should be used to sign the assembly. If the key should not
        /// be taken from a file, but from a key repository, this property
        /// should start with an '<b>@</b>' sign.
        /// </summary>
        public string PrivateKeyLocation { get; set; }
    }


    /// <summary>
    /// Determines how debugging will be supported for
    /// the target module. 
    /// </summary>
    public enum DebugOption
    {
        /// <summary>
        /// Determines automatically (<see cref="Pdb"/> if the
        /// source module has debugging information, otherwise
        /// <see cref="None"/>.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Create a PDB file.
        /// </summary>
        Pdb,

        /// <summary>
        /// No support for debugging.
        /// </summary>
        None
    }
}