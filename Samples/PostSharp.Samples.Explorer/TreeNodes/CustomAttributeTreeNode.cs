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

using System.Text;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;

#endregion

namespace PostSharp.Samples.Explorer.TreeNodes
{
    internal class CustomAttributeTreeNode : DeclarationTreeNode
    {
        public CustomAttributeTreeNode( CustomAttributeDeclaration customAttribute )
            : base( customAttribute, TreeViewImage.CustomAttribute )
        {
            StringBuilder name = new StringBuilder( 256 );
            name.Append( CustomAttributeHelper.Render( customAttribute ) );

            this.Text = name.ToString();
        }
    }
}