using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace MakeGrayPhoto
{
    public partial class Form1 : Form
    {
        public Form1(string[] args) : this()
        {
            if (args.Length > 0)
            {
                OpenPictures(args);
            }
        }

        public Form1()
        {
            InitializeComponent();
            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
            outputFormatsComboBox1.SelectedIndexChanged += OutputFormatsComboBox1_SelectedIndexChanged;
            richTextBox1.MouseDown += RichTextBox1_MouseDown;
            pictureBox1.MinimumSize = pictureBox1.Size;
            pictureBox1.MaximumSize = new Size(pictureBox1.Width * 2, pictureBox1.Height * 2);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            panel1.AutoScroll = true;
            openFileDialog1.Filter = "Image files|*.bmp;*.jpg;*.jpeg;*.png;*.jfif;*.gif|All files|*.*";
            saveFileDialog1.Filter = "Jpeg Image (*.jpg)|*.jpg|Bmp Image (*.bmp)|*.bmp";
            outputFormatsComboBox1.Items.AddRange(Enum.GetNames(typeof(OutputImageFormat)));
            outputFormatsComboBox1.SelectedIndex = 0;
        }

        private enum OutputImageFormat : byte
        {
            jpg = 1,
            bmp
        }

        private enum Direction : byte
        {
            Next,
            Previous
        }
        private int position = -1;
        private string[] imagesList = null;
        OutputImageFormat currentImageFormat;

        private void RichTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (richTextBox1.Text != "" && e.Button == MouseButtons.Right)
            {
                contextMenuStrip2.Show(MousePosition);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(MousePosition);
            }
        }

        private void PictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Size = (pictureBox1.Size != pictureBox1.MaximumSize) ? pictureBox1.MaximumSize : pictureBox1.MinimumSize;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop, false) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            OpenPictures(files);
        }

        private Bitmap GetScreen()
        {
            Size size = Screen.PrimaryScreen.Bounds.Size;
            Bitmap bm = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bm))
            {
                g.CopyFromScreen(0, 0, 0, 0, size);
            }
            return bm;
        }

        private string ImageToBase64String(Image image)
        {
            if (image != null)
            {
                try
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.Save(stream, ImageFormat.Jpeg);
                        byte[] buffer = stream.ToArray();
                        return Convert.ToBase64String(buffer);
                    }
                }
                catch (Exception) { }
            }
            return string.Empty;
        }
		
		private Image ImageFromString(string data, object tag = null)
        {
            byte[] source = Convert.FromBase64String(data);
            using (MemoryStream stream = new MemoryStream(source))
            {
                return new Bitmap(stream) { Tag = tag };
            }
        }

        private void SaveImage(Image image, OutputImageFormat outputFormat, string filename)
        {
            ImageFormat format = outputFormat == OutputImageFormat.jpg ? ImageFormat.Jpeg : ImageFormat.Bmp;
            image.Save(filename, format);
        }

        private void OpenPictures(params string[] paths)
        {
            richTextBox1.Clear();
            imagesList = paths;
            position = 0;
            LoadPicture(imagesList[position]);
            Text = $"{position + 1} из {imagesList.Length}";
        }

        private void SelectPicture(Direction direction)
        {
            if (imagesList == null || imagesList.Length <= 1)
            {
                return;
            }

            if (direction == Direction.Next && ++position >= imagesList.Length)
            {
                position = 0;
            }
            else if (direction == Direction.Previous && --position < 0)
            {
                position = imagesList.Length - 1;
            }

            LoadPicture(imagesList[position]);
            Text = $"{position + 1} из {imagesList.Length}";
        }

        private void LoadPicture(string path)
        {
            try
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = new Bitmap(path);
            }
            catch (ArgumentException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void RotateImage(RotateFlipType rotateFlipType)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap img = new Bitmap(pictureBox1.Image);
                pictureBox1.Image?.Dispose();
                img.RotateFlip(rotateFlipType);
                pictureBox1.Image = img;
            }
        }

        private void MakeGrayImage()
        {
            if (pictureBox1.Image != null)
            {
                Bitmap img = new Bitmap(pictureBox1.Image);
                pictureBox1.Image?.Dispose();
                img.MakeGray();
                pictureBox1.Image = img;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                OpenPictures(openFileDialog1.FileNames);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            MakeGrayImage();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.RotateNoneFlipX);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.RotateNoneFlipY);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            SelectPicture(Direction.Previous);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            SelectPicture(Direction.Next);
        }

        private void СохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string extension = $".{currentImageFormat}";
                string fileName = Path.Combine(desktopPath, Extension.GetRandomName(4, 20, extension));
                SaveImage(pictureBox1.Image, currentImageFormat, fileName);
            }
        }

        private void СохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                saveFileDialog1.FilterIndex = (int)currentImageFormat;
                saveFileDialog1.FileName = Extension.GetRandomName(4, 20);
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveFileDialog1.FileName;
                    SaveImage(pictureBox1.Image, currentImageFormat, fileName);
                }
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            string result = ImageToBase64String(pictureBox1.Image);
            richTextBox1.AppendText(result);
        }

        private void WriteToFile(string path, string context)
        {
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                int position = 0;
                int lineLength = 76;

                while (position < context.Length)
                {
                    if (position + lineLength >= context.Length)
                    {
                        lineLength = context.Length - position;
                    }

                    sw.WriteLine(context.Substring(position, lineLength));
                    position += lineLength;
                }
            }
        }

        private void СохранитьКакToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = Extension.GetRandomName(4, 20, ".txt");
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = saveFileDialog1.FileName;
                string context = richTextBox1.Text;
                WriteToFile(path, context);
            }
        }

        private void ВставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                pictureBox1.Image?.Dispose();
                pictureBox1.Image = Clipboard.GetImage();
            }
            else
            {
                MessageBox.Show("Buffer don't contain an image.");
            }
        }

        private void КопироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Clipboard.SetImage(pictureBox1.Image);
            }
            else
            {
                MessageBox.Show("Image is not selected.");
            }
        }

        private void ОчиститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            Visible = false;
            System.Threading.Thread.Sleep(200);
            Image img = GetScreen();
            Visible = true;
            pictureBox1.Image?.Dispose();
            pictureBox1.Image = img;
            Text = $"{1} из {1}";
        }

        private void MakeGrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeGrayImage();
        }

        private void OutputFormatsComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Enum.TryParse(outputFormatsComboBox1.Text, out currentImageFormat);
        }
    }
}
