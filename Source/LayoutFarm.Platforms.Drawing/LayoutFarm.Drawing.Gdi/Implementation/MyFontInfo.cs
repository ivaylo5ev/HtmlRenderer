﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;

namespace LayoutFarm.Drawing
{
    class BasicGdi32FontHelper
    {
        System.Drawing.Bitmap bmp;
        IntPtr hdc;
        bool isInit;
        public BasicGdi32FontHelper()
        {

        }
        ~BasicGdi32FontHelper()
        {
            MyWin32.DeleteDC(hdc);
            if (bmp != null)
            {
                bmp.Dispose();
                bmp = null;
            }
        }
        void Init()
        {

            bmp = new System.Drawing.Bitmap(2, 2);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
            hdc = g.GetHdc();
            isInit = true;

        }
        public int[] MeasureCharWidths(IntPtr hFont)
        {
            if (!isInit) Init();


            int[] charWidths;
            MyWin32.SelectObject(hdc, hFont);

            charWidths = new int[256];
            unsafe
            {

                NativeTextWin32.FontABC[] abcSizes = new NativeTextWin32.FontABC[256];
                fixed (NativeTextWin32.FontABC* abc = abcSizes)
                {
                    NativeTextWin32.GetCharABCWidths(hdc, (uint)0, (uint)255, abc);

                }

                for (int i = 0; i < 161; i++)
                {
                    charWidths[i] = abcSizes[i].Sum;

                }
                for (int i = 161; i < 255; i++)
                {
                    charWidths[i] = abcSizes[i].Sum;

                }
            }
            return charWidths;
        }
        public int MeasureStringWidth(IntPtr hFont, char[] buffer)
        {
            if (!isInit) Init();

            MyWin32.SelectObject(this.hdc, hFont);
            NativeTextWin32.WIN32SIZE size;
            NativeTextWin32.GetTextExtentPoint32(hdc, buffer, buffer.Length, out size);
            return size.Width;
        }
        public int MeasureStringWidth(IntPtr hFont, char[] buffer, int length)
        {
            if (!isInit) Init();
            MyWin32.SelectObject(this.hdc, hFont);
            NativeTextWin32.WIN32SIZE size;
            NativeTextWin32.GetTextExtentPoint32(hdc, buffer, length, out size);
            return size.Width;
        }

    }


    class MyFontInfo : FontInfo
    { 
        int[] charWidths;
        IntPtr hFont;
        BasicGdi32FontHelper gdiFontHelper; 
        Font resolvedFont;

        public MyFontInfo(Font f,
            int lineHeight, float ascentPx,
            float descentPx, float baseline,
            BasicGdi32FontHelper gdiFontHelper)
        {
         
            this.LineHeight = lineHeight;
            this.DescentPx = descentPx;
            this.AscentPx = ascentPx;
            this.BaseLine = baseline;
            this.FontHeight = f.Height;

            this.gdiFontHelper = gdiFontHelper;
            System.Drawing.Font innerFont = ((System.Drawing.Font)(f.InnerFont));
            hFont = innerFont.ToHfont();
            charWidths = gdiFontHelper.MeasureCharWidths(hFont);

        }
        public override Font ResolvedFont
        {
            get { return resolvedFont; }
        }
        public override IntPtr HFont
        {
            get
            {
                return hFont;
            }
        }
        public override int GetCharWidth(char c)
        {
            int converted = (int)c;
            if (converted > 160)
            {
                converted -= 3424;
            }
            if (converted < 256 && converted > -1)
            {
                return charWidths[converted];
            }
            else
            {
                return 0;
            }
        }
        public override int GetStringWidth(char[] buffer)
        {
            if (buffer == null)
            {
                return 0;
            }
            else
            {
                return gdiFontHelper.MeasureStringWidth(this.hFont, buffer);

            }
        }
        public override int GetStringWidth(char[] buffer, int length)
        {
            if (buffer == null)
            {
                return 0;
            }
            else
            {
                return this.gdiFontHelper.MeasureStringWidth(this.hFont, buffer, length);

            }
        }
       
    }
   
}