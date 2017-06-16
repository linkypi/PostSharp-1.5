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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using PostSharp.CodeModel;

namespace PostSharp.Samples.Explorer.TreeNodes
{
    class UnmanagedResourceTreeNode : BaseTreeNode
    {
        public UnmanagedResourceTreeNode(UnmanagedResource resource)
            : base(TreeViewImage.Code, resource)
        {
            string typeName = resource.TypeId == UnmanagedResourceType.None
                                  ?  "\"" + resource.TypeName + "\""
                                  : resource.TypeId.ToString();

            this.Text = typeName + ": " + resource.Name.ToString();
        }

    }
}
