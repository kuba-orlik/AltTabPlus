using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AltTab_Plus {

       public partial class Form1 : Form {
        Thumbnails thumbnails;
        InterceptAltTab altTab;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            
        }

        private void button1_Click(object sender, EventArgs e) {
            thumbnails = new Thumbnails(this.Handle);
            altTab = new InterceptAltTab(addItem, addItem2);
            for (int i = 0; i < thumbnails.ItemNumber; ++i) {
                listBox1.Items.Add(thumbnails.WindowName(i) + " " + (thumbnails.WindowFlag(i).ToString("x")));
            }
            listBox1.Items.Add(thumbnails.ItemNumber);
            thumbnails.DisplayAllThumbnails(ref image, image.Left, image.Top);
        }

        private void addItem() {
            listBox1.Items.Add("alt+tab");
        }

        private void addItem2() {
            listBox1.Items.Add("shift+alt+tab");
        }

    }
}
