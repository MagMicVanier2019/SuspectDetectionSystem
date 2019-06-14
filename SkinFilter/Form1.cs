using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Emgu;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;


namespace SkinFilter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Load Picture...";
                ofd.Filter = "Jpeg Image (*.jpg)|*.jpg|Bitmap Image (*.bmp)|*.bmp|Png Image (*.png)|*.png|All Files (*.*)|*.*";
                ofd.RestoreDirectory = true;
                ofd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                    pictureBox1.Load(textBox1.Text);
                }
                GC.Collect();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox5.Checked == false)
            {
                if (numericUpDown1.Value > numericUpDown2.Value)
                {
                    if (numericUpDown4.Value > numericUpDown3.Value)
                    {
                        if (pictureBox1.Image != null)
                        {
                            //Fall in (Within range gets removed)
                            //        20, 160              10, 255
                            Bitmap CalculatedMask = BackProjection.BackProject((Bitmap)pictureBox1.Image, new int[] { (int)numericUpDown2.Value, (int)numericUpDown1.Value }, new int[] { (int)numericUpDown3.Value, (int)numericUpDown4.Value }, checkBox4.Checked);
                            if (checkBox3.Checked)
                            {
                                pictureBox2.Image = CalculatedMask;
                            }
                            else
                            {
                                pictureBox2.Image = BackProjection.SuperPositionedImage(CalculatedMask, (Bitmap)pictureBox1.Image, checkBox6.Checked);
                            }
                            GC.Collect();
                        }
                        else
                        {
                            MessageBox.Show("No Image loaded!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error! The min Saturation is larger then the max Saturation.");
                    }
                }
                else
                {
                    MessageBox.Show("Error! The min Hue is larger then the max hue.");
                }
            }
            else
            {
                //Get Values

                Hsv Max = new Hsv((double)trackBar1.Value, 360, 75);
                Hsv Min = new Hsv((double)trackBar2.Value, 0, 75);
                //MessageBox.Show(Max.Hue.ToString() + ", " + Max.Satuation.ToString() + ", " + Max.Value.ToString(), "Max");
                //MessageBox.Show(Min.Hue.ToString() + ", " + Min.Satuation.ToString() + ", " + Min.Value.ToString(), "Min");
                Bitmap CalculatedMask = BackProjection.BackProject((Bitmap)pictureBox1.Image, new int[] { (int)Min.Hue, (int)Max.Hue }, new int[] { (int)Min.Satuation, (int)Max.Satuation }, true);
                if (checkBox3.Checked)
                {
                    pictureBox2.Image = CalculatedMask;
                }
                else
                {
                    pictureBox2.Image = BackProjection.SuperPositionedImage(CalculatedMask, (Bitmap)pictureBox1.Image, checkBox6.Checked);
                }
                
            }
            GC.Collect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Export Processed Image...";
                    sfd.RestoreDirectory = true;
                    sfd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    sfd.Filter = "Jpeg Image (*.jpg)|*.jpg";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        pictureBox2.Image.Save(sfd.FileName, ImageFormat.Jpeg);
                    }
                }
            }
        }

        private void CreateExampleColor()
        {
            if (checkBox1.Checked)
            {
                button2_Click(null, null);
            }

            //Make Pixel Color Example
            Hsv CurrMax = new Hsv((double)numericUpDown1.Value, (double)numericUpDown4.Value, (255));
            Hsv CurrMin = new Hsv((double)numericUpDown2.Value, (double)numericUpDown3.Value, (255));
            Image<Hsv, Byte> ImgMax = new Image<Hsv, Byte>(1, 1, CurrMax);
            Image<Hsv, Byte> ImgMin = new Image<Hsv, Byte>(1, 1, CurrMin);
            pictureBox3.Image = ImgMax.Bitmap;
            pictureBox4.Image = ImgMin.Bitmap;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            CreateExampleColor();                                             //Auto refresh color
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            CreateExampleColor();                                             //Auto refresh color
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            CreateExampleColor();                                             //Auto refresh color
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

            CreateExampleColor();                                             //Auto refresh color

        }

        private void CalculateValues(Color CurrColor)
        {
            Image<Bgr, Byte> RGBPixel = new Image<Bgr, Byte>(1, 1, new Bgr(CurrColor));          //New RGB Pixel
            Image<Hsv, Byte> HSVPixel = RGBPixel.Convert<Hsv, Byte>();                           //Get HSV from RGB
            Hsv TrueColor = HSVPixel[0,0];                                                       //True color is equal to HSV pixel
            textBox2.Text = TrueColor.Hue.ToString() + ", " + TrueColor.Satuation.ToString() + ", " + TrueColor.Value.ToString();  //display HSV values in textbox
            int HueLeway = (int)numericUpDown5.Value;                                            //Grab Leway
            int SatLeway = (int)numericUpDown6.Value;                                            //Grab Leway
            int[] MaxColor = new int[] { (int)(TrueColor.Hue + (HueLeway / 2)), (int)(TrueColor.Satuation + (SatLeway / 2)), (int)(TrueColor.Value) };    //Make Max
            int[] MinColor = new int[] { (int)(TrueColor.Hue - (HueLeway / 2)), (int)(TrueColor.Satuation - (SatLeway / 2)), (int)(TrueColor.Value) };    //Make Min
            //MessageBox.Show(CurrColor.GetHue().ToString() + ", " + CurrColor.GetSaturation().ToString() + ", " + CurrColor.GetBrightness().ToString(), "Actual Hsv Value");
            //MessageBox.Show(MaxColor[0].ToString() + ", " + MaxColor[1].ToString() + ", " + MaxColor[2].ToString(), "Max Color Values");
            //MessageBox.Show(MinColor[0].ToString() + ", " + MinColor[1].ToString() + ", " + MinColor[2].ToString(), "Min Color Values");
            for (int i = 0; i < MaxColor.Count(); i++)
            {
                if (MaxColor[i] < 0)
                {
                    MaxColor[i] = 0;                        //Prevent negative values
                }
            }
            for (int i = 0; i < MinColor.Count(); i++)
            {
                if (MinColor[i] < 0)
                {
                    MinColor[i] = 0;                          //Prevent negative values
                }
            }
            if (MaxColor[0] == 0)
            {
                MaxColor[0] = 1;                           //Prevent negative and zero values
            }
            if (MinColor[0] == 0)
            {
                MinColor[0] = 1;                           //Prevent negative and zero values
            }
            //MessageBox.Show(MaxColor[0].ToString() + ", " + MaxColor[1].ToString() + ", " + MaxColor[2].ToString(), "New Max Color Values");
            //MessageBox.Show(MinColor[0].ToString() + ", " + MinColor[1].ToString() + ", " + MinColor[2].ToString(), "New Min Color Values");
            Hsv ColorMax = new Hsv(MaxColor[0], MaxColor[1], MaxColor[2]);  //Max HSV
            Hsv ColorMin = new Hsv(MinColor[0], MinColor[1], MinColor[2]);  //Min HSV
            numericUpDown1.Value = (int)ColorMax.Hue;                       //Get Hue Max
            numericUpDown4.Value = (int)ColorMax.Satuation;                 //Get Sat Max
            numericUpDown2.Value = (int)ColorMin.Hue;                       //Get Hue Min
            numericUpDown3.Value = (int)ColorMin.Satuation;                 //Get Sat Min
            textBox2.BackColor = CurrColor;                                 //Set backcolor
            Image<Hsv, Byte> TrueImage = new Image<Hsv, Byte>(1, 1, TrueColor);   //Make color image
            pictureBox5.Image = TrueImage.Bitmap;                                 //Display color
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())              //Open Color dialog
            {
                if (cd.ShowDialog() == DialogResult.OK)             //if dialog is a success
                {
                    Color CurrColor = cd.Color;
                    CalculateValues(CurrColor);                     //Calculate color values
                }
            }
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (textBox2.BackColor != null)                                    //auto scan
            {
                CalculateValues(textBox2.BackColor);
            }
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (textBox2.BackColor != null)                                    //auto scan
            {
                CalculateValues(textBox2.BackColor);
            }
        }

        private void button5_Click(object sender, EventArgs e)     //Debugging button (ignore)
        {
            pictureBox2.Image = BackProjection.BackProject((Bitmap)pictureBox1.Image, new int[] { (int)numericUpDown2.Value, (int)numericUpDown1.Value }, new int[] { (int)numericUpDown3.Value, (int)numericUpDown4.Value }, checkBox4.Checked);
            GC.Collect();
        }

        private void DebugFinder(Color color)
        {
           
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            DebugFinder(textBox2.BackColor);
        }


        private void Form1_Load(object sender, EventArgs ee)
        {
            trackBar1.Enabled = false;                                                  //Initialize Default Settings
            trackBar2.Enabled = false;
            checkBox5.Checked = true;
            try
            {
                List<Hsv> Rainbow = new List<Hsv>();                                    //New list of HSV colors
                Bitmap Truebmp = new Bitmap(180, pictureBox6.Height);                   //New Bitmap using pictureBox6 Height
                for (int p = 0; p < Truebmp.Height; p++)                                //foreach row of Bitmap
                {
                    for (int i = 0; i < 180; i++)                                       //from 0 - 180
                    {
                        Hsv Curr = new Hsv(i, 255, 255);                                //new HSV using i
                        Image<Hsv, Byte> CurrImage = new Image<Hsv, Byte>(1, 1, Curr);  //Convert To Image
                        pictureBox6.Image = CurrImage.Bitmap;                           //Get bitmap of image
                        Color CurrBgr = CurrImage.Bitmap.GetPixel(0, 0);                //Grab color of bitmap (this is all due to windows not supporting HSV)
                        Truebmp.SetPixel(i, p, CurrBgr);                                //set pixel to color
                    }
                }
                pictureBox6.Image = Truebmp;                                            //Set pictureBox to the constructed rainbow
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //MessageBox.Show(trackBar1.Value.ToString(), "Bar 1");
            if (trackBar2.Value > trackBar1.Value)                     //slider balancing
            {
                trackBar2.Value = trackBar1.Value;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //trackBar1.Minimum = trackBar2.Value;
            //MessageBox.Show(trackBar2.Value.ToString(), "Bar 2");
            if (trackBar1.Value < trackBar2.Value)                     //slider balancing
            {
                trackBar1.Value = trackBar2.Value;
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label9.Text = trackBar1.Value.ToString();                  //Display current value
            if (checkBox7.Checked)                                     //Auto check
            {
                button2_Click(null, null);
            }
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            label10.Text = trackBar2.Value.ToString();                //Display current value
            if (checkBox7.Checked)                                    //Auto check
            {
                button2_Click(null, null);
            }
        }

        private void pictureBox6_Paint(object sender, PaintEventArgs e)
        {
        
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked == true)             //Change between advanced and simplified
            {
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
                numericUpDown4.Enabled = false;
                numericUpDown5.Enabled = false;
                numericUpDown6.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                checkBox4.Enabled = false;
                button5.Enabled = false;
                button4.Enabled = false;
                trackBar1.Enabled = true;
                trackBar2.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown5.Enabled = true;
                numericUpDown6.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                checkBox4.Enabled = true;
                button5.Enabled = true;
                button4.Enabled = true;
                trackBar1.Enabled = false;
                trackBar2.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            
        }
    }
}
