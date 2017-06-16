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

using PostSharp.Extensibility;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using PostSharp.Samples.Librarian.Framework;

namespace PostSharp.Samples.Librarian.Weaver
{
    /// <summary>
    /// Creates the weavers defined by the 'PostSharp.Samples.Librarian' plug-in.
    /// </summary>
    public class LibrarianAspectWeaverFactory : Task, ILaosAspectWeaverFactory
    {
        /// <summary>
        /// Called by PostSharp Laos to get the weaver of a given aspect.
        /// If the current plug-in does not know this aspect, it should return <b>null</b>.
        /// </summary>
        /// <param name="aspectSemantic">The aspect requiring a weaver.</param>
        /// <returns>A weaver (<see cref="LaosAspectWeaver"/>), or <b>null</b> if the <paramref name="aspectSemantic"/>
        /// is not recognized by the current factory.</returns>
        public LaosAspectWeaver CreateAspectWeaver(AspectTargetPair aspectSemantic)
        {
            if (aspectSemantic.IsDerivedFrom( typeof(AspectedAssemblyAttribute.ImplementCloneableAspect)))
            {
                return new ImplementCloneableAspectWeaver();
            }
            else if (aspectSemantic.IsDerivedFrom(typeof(TypeValidationAspect)))
            {
                return new ImplementValidableAspectWeaver();
            }
            else
            {
                return null;
            }
        }
    }
}