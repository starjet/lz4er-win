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

            if (args[1].Equals("-pack"))
            {
                string file_to_compress = args[2];
                string original = args[3];

                FileStream fd_original = new FileStream(original, FileMode.Open);
                FileStream fd_to_compress = new FileStream(file_to_compress, FileMode.Open);

                byte[] original_headers = new byte[4];
                byte[] original_headers_2 = new byte[4];
                byte[] data_to_compress = new byte[fd_to_compress.Length];
                byte[] buffer = new byte[fd_to_compress.Length];
                byte[] compressed_data;

                for (int i = 0; i < original_headers.Length; i++)
                {
                    original_headers[i] = (byte)fd_original.ReadByte();
                }

                fd_original.Seek(8, SeekOrigin.Current);
                
                for (int i = 0; i < original_headers_2.Length; i++)
                {
                    original_headers_2[i] = (byte)fd_original.ReadByte();
                }

                for (int i = 0; i < data_to_compress.Length; i++)
                {
                    data_to_compress[i] = (byte)fd_to_compress.ReadByte();
                }

                fd_original.Close();
                fd_to_compress.Close();

                unsafe
                {
                    fixed (byte* d_t_c = data_to_compress)
                    {
                        fixed (byte* buf = buffer)
                        {
                            int compressed_size = Lz4.LZ4_compressHC(d_t_c, buf, data_to_compress.Length);
                            compressed_data = new byte[compressed_size];
                        }
                    }
                }

                for (int i = 0; i < compressed_data.Length; i++)
                {
                    compressed_data[i] = buffer[i];
                }

                string path_out = original + ".new";
                FileStream fd_out = new FileStream(path_out, FileMode.OpenOrCreate);
                fd_out.Write(original_headers, 0, original_headers.Length);
                fd_out.Write(BitConverter.GetBytes(data_to_compress.Length), 0, BitConverter.GetBytes(data_to_compress.Length).Length);
                fd_out.Write(BitConverter.GetBytes(compressed_data.Length), 0, BitConverter.GetBytes(compressed_data.Length).Length);
                fd_out.Write(original_headers_2, 0, original_headers_2.Length);
                fd_out.Write(compressed_data, 0, compressed_data.Length);
                fd_out.Close();

                Environment.Exit(0);
            }

            string path = args[1];
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
