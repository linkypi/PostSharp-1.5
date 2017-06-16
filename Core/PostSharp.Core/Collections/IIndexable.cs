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

using System;

#endregion

namespace PostSharp.Collections
{
    /*
    /// <summary>
    /// An observer receives signal when an observed property has changed.
    /// </summary>
    /// <typeparam name="T">Type of the observed property.</typeparam>
    public interface IObserver<T> : IDisposable
    {
        /// <summary>
        /// Method called back by an <see cref="IObservable{T}"/> when an observed property has changed.
        /// </summary>
        /// <param name="instance">Instance which define the changed property.</param>
        /// <param name="propertyName">Named of the changed property.</param>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        void OnObservedPropertyChanged( IObservable<T> instance, string propertyName, T oldValue, T newValue );
    }

    /// <summary>
    /// An observable type allows observers (<see cref="IObserver{T}"/>) to observe some of its
    /// properties.
    /// </summary>
    /// <typeparam name="T">Type of the observer property.</typeparam>
    public interface IObservable<T>
    {
        /// <summary>
        /// Adds an observer to a property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="observer">Observer.</param>
        void AddObserver( string propertyName, IObserver<T> observer );

        /// <summary>
        /// Removes an observer from a property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="observer">Observer.</param>
        void RemoveObserver( string propertyName, IObserver<T> observer );

        /// <summary>
        /// Gets the value of a property.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>The property value.</returns>
        T GetObservableProperty( string propertyName );
    }
     */
}