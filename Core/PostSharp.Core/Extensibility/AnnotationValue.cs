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
using System.Diagnostics;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Collections;
using PostSharp.Reflection;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Represents a custom attribute value that is not declared inside a module,
    /// i.e. is not a <see cref="PostSharp.CodeModel.MetadataDeclaration"/>.
    /// </summary>
    public class AnnotationValue : IAnnotationValue, IDisposable
    {
        /// <summary>
        /// Reference to the custom attribute constructor.
        /// </summary>
        private IMethod constructor;

        /// <summary>
        /// Collection of constructor arguments.
        /// </summary>
        /// <value>
        /// A <see cref="MemberValuePairCollection"/>, or <b>null</b> if the custom
        /// attributes has not been deserialized.
        /// </value>
        private readonly MemberValuePairCollection constructorArguments = new MemberValuePairCollection();

        /// <summary>
        /// Collection of constructor arguments.
        /// </summary>
        /// <value>
        /// A <see cref="MemberValuePairCollection"/>, or <b>null</b> if the custom
        /// attributes has not been deserialized.
        /// </value>
        private readonly MemberValuePairCollection namedArguments = new MemberValuePairCollection();


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAttributeDeclaration"/> type.
        /// </summary>
        /// <param name="constructor">Custom attribute constructor.</param>
        /// <remarks>
        /// The <paramref name="constructor"/> parameter should be set to a valid
        /// instance constructor. This rule is not enforced programmatically.
        /// </remarks>
        public AnnotationValue( IMethod constructor )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( constructor, "constructor" );

            #endregion

            this.constructor = constructor;
        }

        /// <summary>
        /// Gets the collection of constructor arguments.
        /// </summary>
        /// <value>A collection of values (<see cref="MemberValuePair"/>)
        /// where <see cref="MemberValuePair.MemberName"/> 
        /// is not signiticative.</value>
        public MemberValuePairCollection ConstructorArguments
        {
            get
            {
                this.AssertNotDisposed();
                return this.constructorArguments;
            }
        }

        /// <inheritdoc />
        public IAnnotationValue Translate( ModuleDeclaration module )
        {
            if ( module == this.constructor.Module )
                return this;

            AnnotationValue annotationValue = new AnnotationValue( (IMethod) this.constructor.Translate( module ) );
            foreach ( MemberValuePair pair in this.constructorArguments )
            {
                annotationValue.constructorArguments.Add( pair.Translate( module ) );
            }
            foreach ( MemberValuePair pair in this.namedArguments )
            {
                annotationValue.namedArguments.Add( pair.Translate( module ) );
            }

            return annotationValue;
        }

        /// <summary>
        /// Gets the collection of named arguments.
        /// </summary>
        /// <value>
        /// A collection of member-value associations (<see cref="MemberValuePair"/>).
        /// </value>
        public MemberValuePairCollection NamedArguments
        {
            get
            {
                this.AssertNotDisposed();
                return this.namedArguments;
            }
        }

        /// <summary>
        /// Gets the constructor that
        /// was used to build the custom attribute.
        /// </summary>
        public IMethod Constructor
        {
            get
            {
                this.AssertNotDisposed();
                return this.constructor;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Throws an exception if the current instance has been disposed.
        /// </summary>
        [Conditional( "ASSERT" )]
        protected void AssertNotDisposed()
        {
            if ( this.IsDisposed )
            {
                throw new ObjectDisposedException( this.GetType().Name );
            }
        }

        /// <summary>
        /// Determines whether the current instance has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.constructor == null; }
        }

        /// <summary>
        /// Disposes the resources hold by the current instance.
        /// </summary>
        /// <param name="disposing"><b>disposing</b> if the current method is called because an
        /// explicit call of <see cref="Dispose()"/>, or <b>false</b> if the method
        /// is called by the destructor.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this.constructorArguments.Dispose();
                this.namedArguments.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if ( this.constructor != null )
            {
                this.Dispose( true );
                this.constructor = null;
                GC.SuppressFinalize( this );
            }
        }

        #endregion

        #region Implementation of IObjectConstruction

        string IObjectConstruction.TypeName
        {
            get
            {
                StringBuilder s = new StringBuilder();
                this.Constructor.DeclaringType.WriteReflectionTypeName( s, ReflectionNameOptions.UseAssemblyName );
                return s.ToString();
            }
        }

        int IObjectConstruction.ConstructorArgumentCount
        {
            get { return this.constructorArguments.Count; }
        }

        object IObjectConstruction.GetConstructorArgument( int index )
        {
            return this.constructorArguments[index].Value.GetRuntimeValue();
        }

        string[] IObjectConstruction.GetPropertyNames()
        {
            string[] names = new string[this.namedArguments.Count];
            for ( int i = 0; i < names.Length; i++ )
            {
                names[i] = this.namedArguments[i].MemberName;
            }

            return names;
        }

        object IObjectConstruction.GetPropertyValue( string name )
        {
            return this.namedArguments[name].Value.GetRuntimeValue();
        }

        #endregion
    }

    /// <summary>
    /// Represents a <see cref="AnnotationValue"/> that is applied to one
    /// and only one target element.
    /// </summary>
    public sealed class AnnotationInstance : AnnotationValue, IAnnotationInstance
    {
        /// <summary>
        /// Element on which the custom attribute is applied.
        /// </summary>
        private readonly MetadataDeclaration targetElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationInstance"/> type.
        /// </summary>
        /// <param name="constructor">Custom attribute constructor.</param>
        /// <param name="targetElement">Element on which the custom attribute applies.</param>
        /// <remarks>
        /// The <paramref name="constructor"/> parameter should be set to a valid
        /// instance constructor. This rule is not enforced programmatically.
        /// </remarks>
        public AnnotationInstance( MetadataDeclaration targetElement, IMethod constructor )
            : base( constructor )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( targetElement, "targetElement" );

            #endregion

            this.targetElement = targetElement;
        }

        /// <inheritdoc />
        public MetadataDeclaration TargetElement
        {
            get { return this.targetElement; }
        }

        /// <inheritdoc />
        public IAnnotationValue Value
        {
            get { return this; }
        }
    }

    /// <summary>
    /// Implementation of <see cref="IAnnotationInstance"/> that allows the same custom attribute
    /// value to be applied to many target elements.
    /// </summary>
    public sealed class SharedAnnotationInstance : IAnnotationInstance
    {
        private readonly IAnnotationValue value;
        private readonly MetadataDeclaration targetElement;

        /// <summary>
        /// Initializes a new <see cref="SharedAnnotationInstance"/>.
        /// </summary>
        /// <param name="value">Custom attribute value.</param>
        /// <param name="targetElement">Target element.</param>
        public SharedAnnotationInstance( IAnnotationValue value, MetadataDeclaration targetElement )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( value, "value" );
            ExceptionHelper.AssertArgumentNotNull( targetElement, "targetElement" );

            #endregion

            this.value = value;
            this.targetElement = targetElement;
        }

        /// <inheritdoc />
        public IAnnotationValue Value
        {
            get { return this.value; }
        }

        /// <inheritdoc />
        public MetadataDeclaration TargetElement
        {
            get { return this.targetElement; }
        }
    }
}