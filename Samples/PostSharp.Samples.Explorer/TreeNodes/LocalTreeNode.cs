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

using PostSharp.CodeModel;

#endregion

namespace PostSharp.Samples.Explorer.TreeNodes
{
    internal class LocalTreeNode : BaseTreeNode
    {
        public LocalTreeNode( LocalVariableDeclaration local ) : base( TreeViewImage.Field, local )
        {
            this.Text = local.Type.ToString();
        }
    }
}