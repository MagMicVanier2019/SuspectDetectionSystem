using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace SkinFilter
{
    public static class BackProjection
    {
        public static Bitmap BackProject(Bitmap bmp, int[] HueRange, int[] SaturationRange, bool FilterSat)
        {
            Emgu.CV.Image<Bgr, Byte> Mask = new Image<Bgr, Byte>(bmp);                                     //Image Datatype switch
            Mat Copy = new Mat();                                                                          //Result Mat type
            bool useUMat;                                                                                  //bool for Mat Check
            using (InputOutputArray ia = Copy.GetInputOutputArray())                                       //Determine Mask type
                useUMat = ia.IsUMat;                                                                       //If Mat, use Mat
            using (IImage hsv = useUMat ? (IImage)new UMat() : (IImage)new Mat())                          //Mat Image Copies (Hue)
            using (IImage s = useUMat ? (IImage)new UMat() : (IImage)new Mat())                            //Mat Image Copies (Saturation)
            {
                CvInvoke.CvtColor(Mask, hsv, ColorConversion.Bgr2Hsv);                                     //Convert Image to Hsv
                CvInvoke.ExtractChannel(hsv, Copy, 0);                                                     //Extract Hue channel from Hsv
                CvInvoke.ExtractChannel(hsv, s, 1);                                                        //Extract Saturation channel from Hsv
                                                                                                           //the mask for hue less than 20 or larger than 160
                MCvScalar low = new MCvScalar((double)HueRange[0], (double)SaturationRange[0], 75);
                MCvScalar high = new MCvScalar((double)HueRange[1], (double)SaturationRange[1], 75);
                ScalarArray lower = new ScalarArray(low);
                ScalarArray upper = new ScalarArray(high);
                CvInvoke.InRange(Copy, lower, upper, Copy);                                                //Check Ranges
                if (FilterSat == true)
                {                                                                                          //s is the mask for saturation of at least 10, this is mainly used to filter out white pixels
                    CvInvoke.Threshold(s, s, SaturationRange[0], SaturationRange[1], ThresholdType.Binary);    //saturation check
                    CvInvoke.BitwiseAnd(Copy, s, Copy, null);                                                  //If saturation and hue match requirements, place in mask
                }

            }
            return Copy.Bitmap;
        }

        public static Bitmap SuperPositionedImage(Bitmap Mask, Bitmap StandardImage, bool ColorOnly)
        {
            
            Image<Hsv, Byte> GrayOrigin = new Image<Gray, Byte>(StandardImage).Convert<Hsv,Byte>();   //Gray Copy
            Image<Hsv, Byte> HSVOrigin = new Image<Hsv, Byte>(StandardImage);                         //HSV Copy
            Image<Gray, Byte> MaskImageT = new Image<Gray, Byte>(Mask);                               //Mask
            Image<Hsv, Byte> MaskImage = new Image<Hsv, Byte>(Mask);                                  //HSV Mask
            Image<Hsv, Byte> TrueImage = HSVOrigin.Copy(MaskImageT);                                  //HSV through mask
            if (ColorOnly == false)
            {
                CvInvoke.Multiply(TrueImage, MaskImage, TrueImage);                                   
                CvInvoke.Multiply(1 - MaskImage, GrayOrigin, GrayOrigin);
                CvInvoke.Add(TrueImage, GrayOrigin, TrueImage);
                Image<Hsv, Byte> DeathImage = new Image<Hsv, Byte>(StandardImage);
                CvInvoke.Add(TrueImage, HSVOrigin.Copy(MaskImageT), DeathImage);                     //Adding color and gray copy
                return DeathImage.Bitmap;
            }
            else
            {
                return TrueImage.Bitmap;
            }
        }

    }
}
