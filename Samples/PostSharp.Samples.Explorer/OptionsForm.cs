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

using System;
using System.Windows.Forms;

namespace PostSharp.Samples.Explorer
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            this.checkBoxUseAssemblyNameOnly.Checked = AssemblyResolver.Current.UseAssemblyNameOnly;
        }

        private void buttonOk_Click( object sender, EventArgs e )
        {
            AssemblyResolver.Current.UseAssemblyNameOnly = this.checkBoxUseAssemblyNameOnly.Checked;
        }
    }
}