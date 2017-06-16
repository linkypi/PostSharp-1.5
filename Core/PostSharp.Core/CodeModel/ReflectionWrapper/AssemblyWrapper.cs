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

using System;
using System.Collections.Generic;

namespace PostSharp.CodeModel.ReflectionWrapper
{
    internal class AssemblyWrapper : IReflectionWrapper<IAssembly>, IAssemblyWrapper
    {
        readonly IAssembly assembly;
        private static readonly Guid ignoreTypeTag = new Guid("{F420F0A3-C817-494D-A6BE-65B01A382D37}");

        public AssemblyWrapper(IAssembly assembly)
        {
            this.assembly = assembly;
        }

        public static void HideType(TypeDefDeclaration typeDef)
        {
            typeDef.SetTag(ignoreTypeTag, "ignore");
        }


        public System.Reflection.Assembly UnderlyingSystemAssembly
        {
            get { return this.assembly.GetSystemAssembly(); }
        }

        object IReflectionWrapper.UnderlyingSystemObject { get { return this.UnderlyingSystemAssembly; } }
        public IAssemblyName DeclaringAssemblyName
        {
            get { return this.assembly; }
        }


        public Type[] GetTypes()
        {
            AssemblyEnvelope assemblyEnvelope =  this.assembly.GetAssemblyEnvelope();

            List<Type> types = new List<Type>(assemblyEnvelope.ManifestModule.Types.Count);

            foreach (TypeDefDeclaration typeDef in assemblyEnvelope.ManifestModule.Types)
            {
                if ( typeDef.GetTag(ignoreTypeTag) == null)
                    types.Add(typeDef.GetReflectionWrapper(null, null));
                
            }

            return types.ToArray();
        }


        public string Name
        {
            get { return this.assembly.Name; }
        }

        public string FullName
        {
            get { return this.assembly.FullName; }
        }

        public Version Version
        {
            get { return this.assembly.Version; }
        }

        public byte[] GetPublicKey()
        {
            return this.assembly.GetPublicKey();
        }

        public byte[] GetPublicKeyToken()
        {
            return this.assembly.GetPublicKeyToken();
        }

        public string Culture
        {
            get { return this.assembly.Culture; }
        }

        public bool IsMscorlib
        {
            get { return this.assembly.IsMscorlib; }
        }

        public bool MatchesReference( IAssemblyName assemblyName )
        {
            return this.assembly.MatchesReference( assemblyName );
        }

        #region IReflectionWrapper<IAssembly> Members

        public IAssembly WrappedObject
        {
            get { return this.assembly; }
        }

        #endregion

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return ReflectionWrapperUtil.GetCustomAttributes(this.assembly.GetAssemblyEnvelope().ManifestModule, attributeType);
        }

        public object[] GetCustomAttributes(bool inherit)
        {
            return ReflectionWrapperUtil.GetCustomAttributes(this.assembly.GetAssemblyEnvelope().ManifestModule);
        }

        public bool IsDefined(Type attributeType, bool inherit)
        {
            return ReflectionWrapperUtil.IsCustomAttributeDefined(this.assembly.GetAssemblyEnvelope().ManifestModule, attributeType);
        }
    }
}
