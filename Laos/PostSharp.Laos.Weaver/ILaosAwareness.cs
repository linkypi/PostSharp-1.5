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

using System.Runtime.Serialization;
using PostSharp.CodeModel;

namespace PostSharp.Laos.Weaver
{
    /// <summary>
    /// An awareness is some user logic that is invoked before and after aspects are applied
    /// to code. The awareness receives the list of aspects applied to any declaration.
    /// An awareness can perform further transformations of these declarations.
    /// </summary>
    /// <remarks>
    /// <para>A typical use of an awareness is to make PostSharp Laos aware of serialization
    /// libraries, like <b>DataContractSerializer</b> or <b>BinaryFormatter</b>. Indeed, these
    /// formatters do not invoke type constructors, and aspects need to be initialized in an
    /// other way (with methods annotated with <see cref="OnDeserializingAttribute"/>,
    /// specifically).
    /// </para>
    /// <para>The methods of this interface are invoked in the following order:</para>
    /// <list type="number">
    ///     <item><see cref="Initialize"/>, once;</item>
    ///     <item><see cref="ValidateAspects"/>, for each target declaration;</item>
    ///     <item><see cref="BeforeImplementAspects"/>, for each declaration;</item>
    ///     <item><see cref="AfterImplementAspects"/>, for each declaration;</item>
    ///     <item><see cref="AfterImplementAllAspects"/>, once.</item>
    /// </list>
    /// </remarks>
    public interface ILaosAwareness
    {
        /// <summary>
        /// Initializes the current instance. The method is invoked only once before any
        /// other method is invoked.
        /// </summary>
        /// <param name="laosTask"></param>
        void Initialize( LaosTask laosTask );

        /// <summary>
        /// Method invoked during the validation phase.
        /// </summary>
        /// <param name="targetDeclaration">Declaration on which aspects are applied.</param>
        /// <param name="aspectWeavers">All aspects applied to that declaration, in ascending order.</param>
        /// <remarks>
        /// This method can emit errors using <see cref="PostSharp.Extensibility.MessageSource"/>.
        /// </remarks>
        void ValidateAspects(MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers);

        /// <summary>
        /// Method invoked before aspects start to be implemented on a declaration.
        /// </summary>
        /// <param name="targetDeclaration">Declaration on which aspects are applied.</param>
        /// <param name="aspectWeavers">All aspects applied to that declaration, in ascending order.</param>
        void BeforeImplementAspects(MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers);

        /// <summary>
        /// Method invoked after aspects have been implemented on a declaration.
        /// </summary>
        /// <param name="targetDeclaration">Declaration on which aspects are applied.</param>
        /// <param name="aspectWeavers">All aspects applied to that declaration, in ascending order.</param>
        void AfterImplementAspects(MetadataDeclaration targetDeclaration, LaosAspectWeaver[] aspectWeavers);

        /// <summary>
        /// Method invoked after all aspects have been initialized.
        /// </summary>
        void AfterImplementAllAspects();
    }
}
