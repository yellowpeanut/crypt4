﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace crypt4
{
    public partial class Form1 : Form
    {
        private string fileContent = String.Empty;
        Arithmetic ari = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fileStream = dialog.OpenFile();
                fileContent = File.ReadAllText(dialog.FileName);
                ari = new Arithmetic(fileContent);
                label4.Text = $"Initial file size: {getFileSize(fileContent, ari.bitsPerSymbol)} bytes";
                richTextBox1.Text = fileContent;
                string text = "";
                foreach (var symb in ari.SymbolsFrequency)
                {
                    text += $"{symb.Key}\t{symb.Value}\n";
                }
                richTextBox3.Text = text;
            }
            else
            {
                MessageBox.Show("Error opening the file", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        //compress
        private void button2_Click(object sender, EventArgs e)
        {
            if (fileContent == String.Empty)
            {
                MessageBox.Show("Initial file is empty!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string compressedFile = ari.Compress(fileContent);
            richTextBox4.Text = compressedFile;
            File.WriteAllText("2.txt", compressedFile);
            label5.Text = $"Compressed size: {getFileSize(compressedFile, 8)} bytes";

            string text = "";
            foreach (var symb in ari.SymbolsFrequency)
            {
                text += $"{symb.Key}\t{symb.Value}\t{ari.SymbolsProbability[symb.Key]}\n";
            }
            richTextBox3.Text = text;
        }

        //decompress
        private void button3_Click(object sender, EventArgs e)
        {
            string compressedFile = File.ReadAllText("2.txt");
            if (compressedFile == String.Empty)
            {
                MessageBox.Show("Compressed file is empty!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ari is null)
                ari = new Arithmetic();
            string decompressedFile = ari.Decompress(compressedFile);
            File.WriteAllText("3.txt", decompressedFile);
            richTextBox2.Text = decompressedFile;
            label6.Text = $"Decompressed size: {getFileSize(decompressedFile, ari.bitsPerSymbol)} bytes";
        }

        private int getFileSize(string file, int bitsPerSymbol)
        {
            return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(file.Length * bitsPerSymbol) / 8.0));
        }
    }

}
