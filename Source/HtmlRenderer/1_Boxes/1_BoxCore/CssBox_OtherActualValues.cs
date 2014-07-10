﻿//BSD 2014, WinterDev
//ArthurHub

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using HtmlRenderer.Parse;


namespace HtmlRenderer.Dom
{
    partial class CssBox
    {
        Font _actualFont;
        float _actualLineHeight;
        float _actualWordSpacing;
        float _actualTextIndent;

        float _actualBorderSpacingHorizontal;
        float _actualBorderSpacingVertical; 

        /// <summary>
        /// Gets the line height
        /// </summary>
        public float ActualLineHeight
        {
            get
            {
                return _actualLineHeight; 
            }
        }

        /// <summary>
        /// Gets the text indentation (on first line only)
        /// </summary>
        public float ActualTextIndent
        {
            get
            {
                return _actualTextIndent; 
            }
        }

        /// <summary>
        /// Gets the actual width of whitespace between words.
        /// </summary>
        public float ActualWordSpacing
        {
            get { return _actualWordSpacing; }
        }


        protected float MeasureWordSpacing(LayoutVisitor lay)
        {   
            if ((this._prop_pass_eval & CssBoxAssignments.WORD_SPACING) == 0)
            {
                this._prop_pass_eval |= CssBoxAssignments.WORD_SPACING; 
                _actualWordSpacing = lay.MeasureWhiteSpace(this); 
                if (!this.WordSpacing.IsNormalWordSpacing)
                {
                    _actualWordSpacing += CssValueParser.ConvertToPx(this.WordSpacing, 1, this);
                }
            }
            return this._actualWordSpacing;
        }
        /// <summary>
        /// Gets the actual horizontal border spacing for tables
        /// </summary>
        public float ActualBorderSpacingHorizontal
        {
            get
            {
                if ((this._prop_pass_eval & CssBoxAssignments.BORDER_SPACING_H) == 0)
                {
                    this._prop_pass_eval |= CssBoxAssignments.BORDER_SPACING_H;
                    _actualBorderSpacingHorizontal = this.BorderSpacingHorizontal.Number;
                }
                return _actualBorderSpacingHorizontal;
            }
        }

        /// <summary>
        /// Gets the actual vertical border spacing for tables
        /// </summary>
        public float ActualBorderSpacingVertical
        {
            get
            {
                if ((this._prop_pass_eval & CssBoxAssignments.BORDER_SPACING_V) == 0)
                {
                    this._prop_pass_eval |= CssBoxAssignments.BORDER_SPACING_V;
                    _actualBorderSpacingVertical = this.BorderSpacingVertical.Number;
                }
                 
                return _actualBorderSpacingVertical;
            }
        } 
    }
}