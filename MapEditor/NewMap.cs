using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MapEditor
{
    public partial class NewMap : Form
    {
        public int width;
        public int height;
        //delegate string fff();
        public NewMap()
        {
            InitializeComponent();
            width = 100;
            height = 100;
            //fff f = delegate(){};
            //f.inm
            //List<String> f;
            //f[
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            width = (int)numericUpDown1.Value;
            height = (int)numericUpDown2.Value;
            //MessageBox.Show("w = " + width + " h = " + height);
            Close();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
