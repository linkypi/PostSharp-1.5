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
using System.Collections.ObjectModel;
using System.IO;

namespace PostSharp.CodeModel
{

    /// <summary>
    /// Abstract class representing an unmanaged (Windows) resource.
    /// The only current implementation is <see cref="RawUnmanagedResource"/>,
    /// but later versions may provide implementations for specific types
    /// of resources.
    /// </summary>
    public abstract class UnmanagedResource
    {
        private readonly UnmanagedResourceName name;
        private UnmanagedResourceName type;


        internal UnmanagedResource( UnmanagedResourceName name, UnmanagedResourceName type, int codePage, int language,
                                    Version version, int characteristics )
        {
            this.name = name;
            this.CodePage = codePage;
            this.Language = language;
            this.type = type;
            this.Version = version;
            this.Characteristics = characteristics;
        }

        /// <summary>
        /// Gets the name of the current resource.
        /// </summary>
        /// <remarks>
        /// The combination of <see cref="Name"/>, type and <see cref="Language"/>
        /// should be unique.
        /// </remarks>
        public UnmanagedResourceName Name { get { return this.name; } }

        /// <summary>
        /// Gets or sets the code page identifier of the current resource.
        /// </summary>
        public int CodePage { get; set; }

        /// <summary>
        /// Gets or sets the language identifier of the current resource.
        /// </summary>
        public int Language { get; set; }

        /// <summary>
        /// Gets the well-known type of the current resource, or <see cref="UnmanagedResourceType.None"/>
        /// if the type is not well-known, in which case its name is given by the <see cref="TypeName"/>
        /// property.
        /// </summary>
        public UnmanagedResourceType TypeId { get { return (UnmanagedResourceType) this.type.Id; } }

        internal UnmanagedResourceName Type { get { return this.type; } }

        /// <summary>
        /// Gets the name of the type of the current resource, in case that the
        /// <see cref="TypeId"/> property is <see cref="UnmanagedResourceType.None"/>/.
        /// </summary>
        public string TypeName { get { return this.type.Name; } }

        /// <summary>
        /// Gets or sets the version of the current resource. 
        /// </summary>
        /// <remarks>
        /// Note that only the <b>Major</b> and <b>Minor</b> parts of the version
        /// are relevant, and they should be coded on a <see cref="short"/>, not
        /// an <see cref="int"/>.
        /// </remarks>
        public Version Version { get; set; }


        /// <summary>
        /// Gets or sets the characteristics (attributes) of the resource.
        /// </summary>
        public int Characteristics { get; set; }

        /// <summary>
        /// Writes the resource data to a <see cref="BinaryWriter"/> (not including the header).
        /// </summary>
        /// <param name="writer">A <see cref="BinaryWriter"/>.</param>
        internal abstract void Write( BinaryWriter writer );
    }

    namespace Collections
    {
        /// <summary>
        /// Collection of unmanaged resources (<see cref="UnmanagedResource"/>).
        /// </summary>
        public sealed class UnmanagedResourceCollection : Collection<UnmanagedResource>
        {
            /// <summary>
            /// Initializes a new <see cref="UnmanagedResourceCollection"/>.
            /// </summary>
            public UnmanagedResourceCollection()
            {
                
            }
        }
    }

    /// <summary>
    /// Represents the name or the type of an <see cref="UnmanagedResource"/>,
    /// which can consist either in a string, either in an integer.
    /// </summary>
    public struct UnmanagedResourceName
    {
        private int id;
        private string name;

        /// <summary>
        /// Initializes a new <see cref="UnmanagedResourceName"/> with an integer.
        /// </summary>
        /// <param name="id">Integer identifier.</param>
        public UnmanagedResourceName( int id )
        {
            this.id = id;
            this.name = null;
        }

        /// <summary>
        /// Initializes a new <see cref="UnmanagedResourceName"/> with a string.
        /// </summary>
        /// <param name="name">String representation of the name.</param>
        public UnmanagedResourceName( string name )
        {
            this.id = 0;
            this.name = name;
        }

        /// <summary>
        /// Initializes a new <see cref="UnmanagedResourceName"/> with an
        /// integer <i>or</i> a string.
        /// </summary>
        /// <param name="idOrName">An <see cref="int"/> or a <see cref="string"/>.</param>
        public UnmanagedResourceName( object idOrName )
        {
            if ( idOrName == null )
            {
                this.id = 0;
                this.name = null;
            }
            else if ( ( this.name = idOrName as string ) != null )
            {
                this.id = 0;
            }
            else
            {
                this.id = (int) idOrName;
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="UnmanagedResourceName"/>
        /// is an identifier (<see cref="int"/>).
        /// </summary>
        public bool IsId { get { return this.name == null; } }

        /// <summary>
        /// Determines whether the current <see cref="UnmanagedResourceName"/>
        /// is a name (<see cref="string"/>).
        /// </summary>
        public bool IsName { get { return this.name != null; } }

        /// <summary>
        /// Gets the string representing the current <see cref="UnmanagedResourceName"/>,
        /// or <b>null</b> if the current object has an identifier (<see cref="Id"/>).
        /// </summary>
        public string Name { get { return this.name; } set { this.name = value; } }

        /// <summary>
        /// Gets the integer identifier representing the current <see cref="UnmanagedResourceName"/>,
        /// or <b>0</b> if the current object has a name (<see cref="Name"/>).
        /// </summary>
        public int Id { get { return this.id; } set { this.id = value; } }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.name != null ? "\"" + this.name + "\"" : this.id.ToString();
        }

        /// <summary>
        /// Converts the current object to a boxed <see cref="int"/> or to a <see cref="string"/>.
        /// </summary>
        /// <returns>A boxed <see cref="int"/> or a <see cref="string"/>.</returns>
        public object ToObject()
        {
            return this.name ?? (object) this.id;
        }
    }

    /// <summary>
    /// Types of unmanaged resources (like <see cref="Accelerator"/>, <see cref="Icon"/>, ...).
    /// </summary>
    public enum UnmanagedResourceType
    {
        /// <summary>
        /// Not a well-known type. It may be a named type 
        /// (see <see cref="UnmanagedResource.TypeName"/>).
        /// </summary>
        None = 0,

        /// <summary>
        /// Accelerator table. 
        /// </summary>
        Accelerator = 9,

        /// <summary>
        /// Animated cursor. 
        /// </summary>
        AnimatedCursor = 21,
        /// <summary>
        /// Animated icon. 
        /// </summary>
        AnimatedIcon = 22,
        /// <summary>
        /// Bitmap resource. 
        /// </summary>
        Bitmap = 2,
        /// <summary>
        /// Hardware-dependent cursor resource. 
        /// </summary>
        Cursor = 1,
        /// <summary>
        /// Dialog box. 
        /// </summary>
        Dialog = 5,
        /// <summary>
        /// Allows a resource editing tool to associate a string with an .rc file. 
        /// Typically, the string is the name of the header file that provides symbolic names. 
        /// The resource compiler parses the string but otherwise ignores the value. 
        /// For example, file MyFile.dlg: 
        /// 1 DLGINCLUDE "MyFile.h"
        /// </summary>
        DialogInclude = 17,
        /// <summary>
        /// Font resource. 
        /// </summary>
        Font = 8,
        /// <summary>
        /// Font directory resource. 
        /// </summary>
        FontDirectory = 7,
        /// <summary>
        /// Hardware-independent cursor resource. 
        /// </summary>
        GroupCursor = 12,
        /// <summary>
        /// Hardware-independent icon resource. 
        /// </summary>
        IconCursor = 14,
        /// <summary>
        /// HTML
        /// </summary>
        Html = 23,
        /// <summary>
        /// Hardware-dependent icon resource. 
        /// </summary>
        Icon = 3,
        /// <summary>
        /// Microsoft Windows XP: Side-by-Side Assembly XML Manifest. 
        /// </summary>
        Manifest = 24,
        /// <summary>
        /// Menu resource. 
        /// </summary>
        Menu = 4,
        /// <summary>
        /// Message-table entry. 
        /// </summary>
        MessageTable = 11,
        /// <summary>
        /// Plug and Play resource. 
        /// </summary>
        PlugPlay = 19,
        /// <summary>
        /// Application-defined resource (raw data). 
        /// </summary>
        RcData = 10,
        /// <summary>
        /// String-table entry. 
        /// </summary>
        String = 6,
        /// <summary>
        /// Version resource. 
        /// </summary>
        Version = 16,
        /// <summary>
        /// VxD.
        /// </summary>
        VxD = 20
    }
}