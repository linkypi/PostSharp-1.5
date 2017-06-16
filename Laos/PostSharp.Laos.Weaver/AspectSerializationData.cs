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

using PostSharp.CodeModel;

namespace PostSharp.Laos.Weaver
{
    internal class AspectSerializationData
    {
        private readonly int index;
        private readonly LocalVariableSymbol arrayVariable;
        
        public AspectSerializationData( int index, LocalVariableSymbol arrayVariable )
        {
            this.index = index;
            this.arrayVariable = arrayVariable;
        }

        public int Index
        {
            get { return this.index; }
        }

        public LocalVariableSymbol ArrayVariable
        {
            get { return this.arrayVariable; }
        }
    }
}