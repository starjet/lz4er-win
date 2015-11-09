using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Lz4Net;

namespace lz4er_win
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            String path = args[1];
            FileStream fd = new FileStream(path, FileMode.Open);

            byte[] unpacked_size = new byte[4];
            byte[] fsiz = new byte[4];

            fd.Seek(4, SeekOrigin.Begin);
            for (int i = 0; i < unpacked_size.Length; i++) 
            {
                unpacked_size[i] = (byte)fd.ReadByte();
            }
            for (int i = 0; i < fsiz.Length; i++)
            {
                fsiz[i] = (byte)fd.ReadByte();
            }
            
            byte[] buf_array = new byte[BitConverter.ToUInt32(fsiz, 0)];
            byte[] out_array = new byte[BitConverter.ToUInt32(unpacked_size, 0)];

            fd.Seek(4, SeekOrigin.Current);
            for (int i = 0; i < buf_array.Length; i++)
            {
                buf_array[i] = (byte)fd.ReadByte();
            }

            unsafe
            {
                fixed (byte* buf = buf_array) 
                {
                    fixed (byte* out_ptr = out_array) 
                    {
                        Lz4.LZ4_uncompress(buf, out_ptr, out_array.Length);
                    }
                }
            }

            fd.Close();

            path = path + ".extracted";
            fd = new FileStream(path, FileMode.OpenOrCreate);
            fd.Write(out_array, 0, out_array.Length);
            fd.Close();

            Environment.Exit(0);
        }
    }
}
