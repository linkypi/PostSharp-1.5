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

using System.Collections.Generic;
using PostSharp.CodeModel;

#endregion

namespace PostSharp.Samples.Explorer
{
    internal class NamedDeclarationComparer : Comparer<NamedDeclaration>
    {
        public static readonly NamedDeclarationComparer Instance = new NamedDeclarationComparer();

        private NamedDeclarationComparer()
        {
        }

        public override int Compare( NamedDeclaration x, NamedDeclaration y )
        {
            return x.Name.CompareTo( y.Name );
        }
    }
}