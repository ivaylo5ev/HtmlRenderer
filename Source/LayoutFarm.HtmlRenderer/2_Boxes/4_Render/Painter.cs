﻿//BSD 2014, WinterDev

using System;
using System.Collections.Generic;
using LayoutFarm.Drawing;

using HtmlRenderer.Css;
using HtmlRenderer.Boxes;

namespace HtmlRenderer
{
    //----------------------------------------------------------------------------
    public class Painter : BoxVisitor
    {
        Stack<RectangleF> clipStacks = new Stack<RectangleF>();

        PointF[] borderPoints = new PointF[4]; 
        PointF htmlContainerScrollOffset; 
        HtmlIsland visualRootBox;
        IGraphics ig;

        RectangleF latestClip = new RectangleF(0, 0, CssBoxConstConfig.BOX_MAX_RIGHT, CssBoxConstConfig.BOX_MAX_BOTTOM);

        float physicalViewportWidth;
        float physicalViewportHeight;
        float physicalViewportX;
        float physicalViewportY;

        bool aviodGeometyAntialias;
        public Painter(HtmlIsland container, IGraphics ig)
        {
            this.visualRootBox = container;
            this.htmlContainerScrollOffset = container.ScrollOffset;
            this.aviodGeometyAntialias = container.AvoidGeometryAntialias;
            this.ig = ig;
        }

        public GraphicPlatform Platform
        {
            get { return this.ig.Platform; }
        }
        internal void SetPhysicalViewportBound(float x, float y, float width, float height)
        {
            this.physicalViewportX = x;
            this.physicalViewportY = y;
            this.physicalViewportWidth = width;
            this.physicalViewportHeight = height;
        }

        internal IGraphics Gfx
        {
            get
            {
                return this.ig;
            }
        }
        internal bool AvoidGeometryAntialias
        {
            get { return this.aviodGeometyAntialias; }
        }
        //-----------------------------------------------------

        internal float LocalViewportTop
        {
            get { return this.physicalViewportY - ig.CanvasOriginY; }
        }
        internal float LocalViewportBottom
        {
            get { return (this.physicalViewportY + this.physicalViewportHeight) - ig.CanvasOriginY; }
        }

        public PointF Offset
        {
            get { return this.htmlContainerScrollOffset; }
        }

        //=========================================================
        /// <summary>
        /// push clip area relative to (0,0) of current CssBox
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        internal bool PushLocalClipArea(float w, float h)
        {

            //store lastest clip 
            clipStacks.Push(this.latestClip);
            ////make new clip global 
            RectangleF intersectResult = RectangleF.Intersect(
                latestClip,
                new RectangleF(0, 0, w, h));
            this.latestClip = intersectResult;

            //ig.DrawRectangle(Pens.Red, intersectResult.X, intersectResult.Y, intersectResult.Width, intersectResult.Height);
            ig.SetClip(intersectResult);
            return !intersectResult.IsEmpty;
        }
        internal void PopLocalClipArea()
        {
            if (clipStacks.Count > 0)
            {
                RectangleF prevClip = this.latestClip = clipStacks.Pop();
                //ig.DrawRectangle(Pens.Green, prevClip.X, prevClip.Y, prevClip.Width, prevClip.Height);
                ig.SetClip(prevClip);
            }
        }
        /// <summary>
        /// async request for image
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="requestFrom"></param>
        internal void RequestImageAsync(ImageBinder binder, CssImageRun imgRun, object requestFrom)
        {
            HtmlIsland.RaiseRequestImage(
                this.visualRootBox,
                binder,
                requestFrom,
                false);
            //--------------------------------------------------
            if (binder.State == ImageBinderState.Loaded)
            {
                Image img = binder.Image;
                if (img != null)
                {
                    //set real image info
                    imgRun.ImageRectangle = new Rectangle(
                        (int)imgRun.Left, (int)imgRun.Top,
                        img.Width, img.Height);
                }
            }
        }
        //internal void RequestImage(ImageBinder binder, CssBox requestFrom, ReadyStateChangedHandler handler)
        //{
        //    HtmlRenderer.HtmlContainer.RaiseRequestImage(
        //           this.container,
        //           binder,
        //           requestFrom,
        //           false);
        //}
        //=========================================================

        internal void PaintBorders(CssBox box, RectangleF stripArea, bool isFirstLine, bool isLastLine)
        {
            HtmlRenderer.Boxes.BorderPaintHelper.DrawBoxBorders(this, box, stripArea, isFirstLine, isLastLine);
        }
        internal void PaintBorders(CssBox box, RectangleF rect)
        {
            Color topColor = box.BorderTopColor;
            Color leftColor = box.BorderLeftColor;
            Color rightColor = box.BorderRightColor;
            Color bottomColor = box.BorderBottomColor;

            var g = this.Gfx;

            // var b1 = RenderUtils.GetSolidBrush(topColor);
            BorderPaintHelper.DrawBorder(CssSide.Top, borderPoints, g, box, topColor, rect);

            // var b2 = RenderUtils.GetSolidBrush(leftColor);
            BorderPaintHelper.DrawBorder(CssSide.Left, borderPoints, g, box, leftColor, rect);

            // var b3 = RenderUtils.GetSolidBrush(rightColor);
            BorderPaintHelper.DrawBorder(CssSide.Right, borderPoints, g, box, rightColor, rect);

            //var b4 = RenderUtils.GetSolidBrush(bottomColor);
            BorderPaintHelper.DrawBorder(CssSide.Bottom, borderPoints, g, box, bottomColor, rect);

        }
        internal void PaintBorder(CssBox box, CssSide border, Color solidColor, RectangleF rect)
        {

            using (var b = this.Platform.CreateSolidBrush(solidColor))
            {
                PointF[] borderPoints = new PointF[4];
                BorderPaintHelper.DrawBorder(border, borderPoints, this.Gfx, box, b, rect);
            }
        }
        //-------------------------------------
        //painting context for canvas , svg
        Color currentContextFillColor = Color.Black;
        Color currentContextPenColor = Color.Transparent;
        float currentContextPenWidth = 1;
        public bool UseCurrentContext
        {
            get;
            set;
        }
        public Color CurrentContextFillColor
        {
            get { return this.currentContextFillColor; }
            set { this.currentContextFillColor = value; }
        }
        public Color CurrentContextPenColor
        {
            get { return this.currentContextPenColor; }
            set { this.currentContextPenColor = value; }
        }
        public float CurrentContextPenWidth
        {
            get { return this.currentContextPenWidth; }
            set { this.currentContextPenWidth = value; }
        }

        //-------------------------------------
#if DEBUG
        public void dbugDrawDiagonalBox(Pen pen, float x1, float y1, float x2, float y2)
        {
            var g = this.Gfx;
            g.DrawRectangle(pen, x1, y1, x2 - x1, y2 - y1);
            g.DrawLine(pen, x1, y1, x2, y2);
            g.DrawLine(pen, x1, y2, x2, y1);
        }
        public void dbugDrawDiagonalBox(Pen pen, RectangleF rect)
        {
            var g = this.Gfx;
            this.dbugDrawDiagonalBox(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);

        }
#endif

    }



}