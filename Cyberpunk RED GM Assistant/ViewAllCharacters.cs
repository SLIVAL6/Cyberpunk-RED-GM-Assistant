﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cyberpunk_RED_GM_Assistant
{
    public partial class ViewAllCharacters : Form
    {
        public ViewAllCharacters()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        // View Character Button
        private void button2_Click(object sender, EventArgs e)
        {
            Form3 CheckCharacterView = new Form3(11);
            CheckCharacterView.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        // Create Character Button
        private void button4_Click(object sender, EventArgs e)
        {
            Form2 CreateCharacterView = new Form2();
            CreateCharacterView.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Simply closes this window
            this.Hide();
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        // All character views
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            FlowLayoutPanel characterPanel = new FlowLayoutPanel();
            characterPanel.Size = new Size(255, 60);
            characterPanel.FlowDirection = FlowDirection.TopDown;

            Label nameLabel = new Label();
            nameLabel.AutoSize = true;
            nameLabel.Font = new Font(nameLabel.Font.Name, 15f);
            nameLabel.Text = $"1. Joe Mama"; // find character from charsInQueue
            characterPanel.Controls.Add(nameLabel);

            Label statsLabel = new Label();
            statsLabel.AutoSize = true;
            statsLabel.Font = new Font(statsLabel.Font.Name, 13f);
            string statsText = $"HP 60 | SP 7 | SP 7"; // get character stats and chuck them in here
            statsLabel.Text = statsText;
            characterPanel.Controls.Add(statsLabel);

            flowLayoutPanel1.Controls.Add(characterPanel);
        }

        /*// needs character id in parameters to generate panel with correct text
        private void AddToQueue()
        {
            FlowLayoutPanel characterPanel = new FlowLayoutPanel();
            characterPanel.Size = new Size(255, 60);
            characterPanel.FlowDirection = FlowDirection.TopDown;

            Label nameLabel = new Label();
            nameLabel.AutoSize = true;
            nameLabel.Font = new Font(nameLabel.Font.Name, 15f);
            nameLabel.Text = $"1. Joe Mama"; // find character from charsInQueue
            characterPanel.Controls.Add(nameLabel);

            Label statsLabel = new Label();
            statsLabel.AutoSize = true;
            statsLabel.Font = new Font(statsLabel.Font.Name, 13f);
            string statsText = $"HP 60 | SP 7 | SP 7"; // get character stats and chuck them in here
            statsLabel.Text = statsText;
            characterPanel.Controls.Add(statsLabel);

            queueFPnl.Controls.Add(characterPanel);
        }*/

    }
}
