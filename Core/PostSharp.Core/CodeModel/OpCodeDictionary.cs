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

using System.Diagnostics.CodeAnalysis;

#endregion

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Efficient implementation of a dictionary taking an <see cref="OpCodeNumber"/>
    /// as the key type.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    [SuppressMessage( "Microsoft.Naming", "CA1711",
        Justification = "Even if it does not implement IDictionary, it is conceptually a dictionary," )]
    [SuppressMessage( "Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase" )]
    public sealed class OpCodeDictionary<T>
    {
        /// <summary>
        /// Values associated to short opcodes.
        /// </summary>
        private readonly T[] shortValues;

        /// <summary>
        /// Values associated to long opcodes.
        /// </summary>
        private readonly T[] longValues;

        /// <summary>
        /// Initializes a new <see cref="PostSharp.CodeModel.OpCodeDictionary{T}"/>.
        /// </summary>
        public OpCodeDictionary()
        {
            this.shortValues = new T[(int) OpCodeNumber._CountShort];
            this.longValues = new T[(int) OpCodeNumber._CountLarge];
        }


        /// <summary>
        /// Gets a value associated with an <see cref="OpCodeNumber"/>.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        /// <returns>The value associated to <paramref name="opCode"/>, or the
        /// default value of <typeparamref name="T"/> if no value is associated
        /// with <paramref name="opCode"/>.</returns>
        public T GetValue( OpCodeNumber opCode )
        {
            int index = (int) opCode & 0xFF;

            if ( (int) opCode == index )
            {
                ExceptionHelper.Core.AssertValidArgument( index < (int) OpCodeNumber._CountShort,
                                                          "opCode", "InvalidOpCodeNumber", (int) opCode );
                return this.shortValues[index];
            }
            else
            {
                ExceptionHelper.Core.AssertValidArgument( index < (int) OpCodeNumber._CountLarge,
                                                          "opCode", "InvalidOpCodeNumber", (int) opCode );

                return this.longValues[index];
            }
        }

        /// <summary>
        /// Sets a value associated with an <see cref="OpCodeNumber"/>.
        /// </summary>
        /// <param name="opCode">An <see cref="OpCodeNumber"/>.</param>
        /// <param name="value">The value associated with <paramref name="opCode"/>.</param>
        public void SetValue( OpCodeNumber opCode, T value )
        {
            int index = (int) opCode & 0xFF;

            if ( (int) opCode == index )
            {
                this.shortValues[index] = value;
            }
            else
            {
                this.longValues[index] = value;
            }
        }
    }
}