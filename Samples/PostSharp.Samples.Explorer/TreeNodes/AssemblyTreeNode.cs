#region Released to Public Domain by SharpCrafters s.r.o.
/*----------------------------------------------------------------------------*
 *   This file is part of samples of PostSharp.                                *
 *                                                                             *
 *   This sample is free software: you have an unlimited right to              *
 *   redistribute it and/or modify it.                                         *
 *                                                                             *
 *   This sample is distributed in the hope that it will be useful,            *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of            *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.                      *
 *                                                                             *
 *----------------------------------------------------------------------------*/
#endregion


#region Using directives

using System.Windows.Forms;
using PostSharp.CodeModel;

#endregion

namespace PostSharp.Samples.Explorer.TreeNodes
{
    internal sealed class AssemblyTreeNode : DeclarationTreeNode
    {
        public AssemblyTreeNode( AssemblyManifestDeclaration assembly ) : base( assembly, TreeViewImage.Assembly )
        {
            this.Text = assembly.Name;
        }
    }
}