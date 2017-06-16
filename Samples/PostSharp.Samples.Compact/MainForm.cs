using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PostSharp.Samples.Compact
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        
        [RecordAspect("Button 1")]
        private void button1_Click(object sender, EventArgs e)
        {

        }

        [RecordAspect("Button 2")]
        private void button2_Click(object sender, EventArgs e)
        {

        }

        [RecordAspect("Button 3")]
        private void button3_Click(object sender, EventArgs e)
        {

        }

        internal void AddRecordToHistory(string record)
        {
            this.historyListBox.Items.Add(record);
        }
    }
}