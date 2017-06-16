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
using System.Reflection;
using PostSharp.CodeModel.Helpers;
using PostSharp.Collections;

namespace PostSharp.CodeModel
{
    /// <summary>
    /// Manages and resolves assembly redirection policies.
    /// </summary>
    /// <remarks>
    /// <para>Assembly redirection is the process of replacing an assembly reference
    /// by another reference, allowing to load another assembly than the one
    /// that is actually referenced.</para>
    /// <para>The resolved and redirected assembly name is here named the <i>canonical</i>
    /// assembly name.</para>
    /// </remarks>
    public sealed class AssemblyRedirectionPolicyManager
    {
        private readonly MultiDictionary<string, AssemblyRedirectionPolicy> availablePolicies =
            new MultiDictionary<string, AssemblyRedirectionPolicy>(16, StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<string, IAssemblyName> redirectionCache =
            new Dictionary<string, IAssemblyName>(64, StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<string, AssemblyRedirectionPolicy> policyCache =
            new Dictionary<string, AssemblyRedirectionPolicy>(64, StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Add a policy to the current policy.
        /// </summary>
        /// <param name="policy">Policy.</param>
        internal void AddPolicy(AssemblyRedirectionPolicy policy)
        {
            Trace.AssemblyRedirectionPolicies.WriteLine("Adding policy: {0}", policy);
            this.availablePolicies.Add(policy.OldName, policy);
        }

        /// <summary>
        /// Applies redirection policies on an assembly name given as a <see cref="string"/>.
        /// </summary>
        /// <param name="assemblyName">Name of the referenced assembly.</param>
        /// <returns>Name of the resolved assembly reference, after application
        /// of redirection policies.</returns>
        public string GetCanonicalAssemblyName(string assemblyName)
        {
            return GetCanonicalAssemblyName(new AssemblyName(assemblyName)).FullName;
        }

        /// <summary>
        /// Applies redirection policies on an assembly name given as an <see cref="AssemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">Name of the referenced assembly.</param>
        /// <returns>Name of the resolved assembly reference, after application
        /// of redirection policies.</returns>
        public AssemblyName GetCanonicalAssemblyName(AssemblyName assemblyName)
        {
            return AssemblyNameHelper.Convert(this.GetCanonicalAssemblyName(AssemblyNameHelper.Convert(assemblyName)));
        }

        /// <summary>
        /// Determines whether a redirection polity is defined for a given assembly name (<see cref="AssemblyName"/>).
        /// </summary>
        /// <param name="assemblyName">An assembly name.</param>
        /// <returns><b>true</b> if some redirection policy is defined for <paramref name="assemblyName"/>,
        /// otherwise <b>false</b>.</returns>
        public bool HasRedirectionPolicy(AssemblyName assemblyName)
        {
            return this.HasRedirectionPolicy(AssemblyNameHelper.Convert(assemblyName));
        }

        /// <summary>
        /// Determines whether a redirection polity is defined for a given assembly name (<see cref="IAssemblyName"/>).
        /// </summary>
        /// <param name="assemblyName">An assembly name.</param>
        /// <returns><b>true</b> if some redirection policy is defined for <paramref name="assemblyName"/>,
        /// otherwise <b>false</b>.</returns>
        public bool HasRedirectionPolicy(IAssemblyName assemblyName)
        {
            return this.FindPolicy(assemblyName) != null;
        }

        private AssemblyRedirectionPolicy FindPolicy(IAssemblyName assemblyName)
        {
             AssemblyRedirectionPolicy cached;

            string fullName = assemblyName.FullName;

            if (this.policyCache.TryGetValue(fullName, out cached))
            {
                return cached;
            }
            else
            {
                foreach (string nameFilter in new[] {assemblyName.Name, "*"})
                {
                    foreach (AssemblyRedirectionPolicy policy in this.availablePolicies[nameFilter])
                    {
                        if ( !policy.Matches( assemblyName ) ) continue;

                        this.policyCache.Add(fullName, policy);
                        return policy;
                    }
                }

                this.policyCache.Add(fullName, null);
                return null;
            }
        }

        /// <summary>
        /// Applies redirection policies on an assembly name given as a <see cref="IAssemblyName"/>.
        /// </summary>
        /// <param name="assemblyName">Name of the referenced assembly.</param>
        /// <returns>Name of the resolved assembly reference, after application
        /// of redirection policies.</returns>
        public IAssemblyName GetCanonicalAssemblyName(IAssemblyName assemblyName)
        {
            IAssemblyName cached;

            string fullName = assemblyName.FullName;

            if (this.redirectionCache.TryGetValue(fullName, out cached))
            {
                return cached;
            }
            else
            {
                Trace.AssemblyRedirectionPolicies.WriteLine("Finding uncached redirection for {0}.", assemblyName);

                AssemblyRedirectionPolicy policy = this.FindPolicy(assemblyName);

                if ( policy != null )
                {
                    IAssemblyName newAssemblyName = policy.Apply(assemblyName);
                    Trace.AssemblyRedirectionPolicies.WriteLine("Redirection found and cached: {0}.",
                                                                newAssemblyName);

                    this.redirectionCache.Add(fullName, newAssemblyName);
                    return newAssemblyName;
                }
                else
                {
                    Trace.AssemblyRedirectionPolicies.WriteLine("No redirection found. We cached the identity mapping.");

                    this.redirectionCache.Add(fullName, assemblyName);
                    return assemblyName;
                }

            }
        }
    }
}