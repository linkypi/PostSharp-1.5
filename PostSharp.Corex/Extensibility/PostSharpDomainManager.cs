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
using System.Reflection;
using PostSharp.CodeModel.Helpers;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Entry point of PostSharp with a hypothetical unmanaged bootstrapper,
    /// implementing an <see cref="AppDomainManager"/>.
    /// </summary>
    /// <remarks>
    /// The unmanaged bootstrapper should case this object to <see cref="IPostSharpDomainManager"/>,
    /// which is a COM interface.
    /// </remarks>
    public class PostSharpDomainManager : AppDomainManager, IPostSharpDomainManager
    {
        /// <inheritdoc />
        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializeNewDomain(appDomainInfo);

            this.InitializationFlags = AppDomainManagerInitializationOptions.RegisterWithHost;
        }

        /// <inheritdoc />
        public void Initialize(bool assemblyResolve)
        {
            CustomAssemblyBinder.Initialize(assemblyResolve);
        }

        /// <inheritdoc />
        public int Start(string[] args)
        {
            return (int) CommandLineProgram.Main(args);
        }

        /// <inheritdoc />
        public string FindAssembly(string assemblyName)
        {
            return CustomAssemblyBinder.Instance.Locator.FindAssembly( AssemblyNameHelper.Convert( new AssemblyName( assemblyName ) ) );
        }
    }
}