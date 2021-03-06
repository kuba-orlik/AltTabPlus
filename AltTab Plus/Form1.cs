﻿using System;
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
            altTab = new InterceptAltTab(this.Handle, addItem, addItem2);
            for (int i = 0; i < thumbnails.itemNumber; ++i) {
                try {
                    listBox1.Items.Add(thumbnails.windowListItem(i).name + " " + (thumbnails.windowListItem(i).flag.ToString("x")));
                }
                catch (Exception ex) {}
            }
            listBox1.Items.Add(thumbnails.itemNumber);
            thumbnails.displayAllThumbnails(ref image, image.Left, image.Top);
        }

        private void addItem() {
            listBox1.Items.Add("alt+tab");
        }

        private void addItem2() {
            listBox1.Items.Add("shift+alt+tab");
        }

        private void button2_Click(object sender, EventArgs e) {
            KillingModule.CloseWindowGentle(thumbnails.windowListItem(0).hWnd);
            thumbnails.eraseAllThumbnails();
        }

    }
}
