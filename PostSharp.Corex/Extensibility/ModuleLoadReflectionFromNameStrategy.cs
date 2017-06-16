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
using System.IO;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Binding;
using PostSharp.CodeModel.Helpers;

namespace PostSharp.Extensibility
{
	/// <summary>
    /// Implementation of <see cref="ModuleLoadStrategy"/>
    /// loading the assembly by name, using the <b>Assembly.Load</b> method.
    /// </summary>
    [Serializable]
    public sealed class ModuleLoadReflectionFromNameStrategy : ModuleLoadReflectionStrategy
    {
        private readonly string name;
	    private IAssemblyName assemblyName;

	    /// <summary>
        /// Initializes a new <see cref="ModuleLoadReflectionFromNameStrategy"/>.
        /// </summary>
        /// <param name="name">Assembly name.</param>
        public ModuleLoadReflectionFromNameStrategy(string name)
        {
            ExceptionHelper.AssertArgumentNotEmptyOrNull(name, "name");

            this.name = name;
        }

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <inheritdoc />
        public override Assembly LoadAssembly()
        {
            try
            {
                return AssemblyLoadHelper.LoadAssemblyFromName(this.name, this.Evidence);
            }
            catch (IOException e)
            {
                CoreMessageSource.Instance.Write(SeverityType.Fatal, "PS0078",
                                                 new object[] {this.name, e.Message});
                throw;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.name;
        }

        public override bool Matches(Domain domain, IAssemblyName assemblyName)
        {
            if (this.assemblyName == null)
            {
                this.assemblyName = domain.AssemblyRedirectionPolicies.GetCanonicalAssemblyName(
                    AssemblyNameWrapper.GetWrapper(AssemblyName.GetAssemblyName(this.name)));
            }

            return AssemblyComparer.GetInstance().Equals(this.assemblyName, assemblyName);
        }
    }
}