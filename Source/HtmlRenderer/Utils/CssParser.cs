﻿//BSD 2014, WinterCore

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlRenderer.Entities;
using HtmlRenderer.Utils;

using HtmlRenderer.WebDom.Parser;

namespace HtmlRenderer.Parse
{

    /// <summary>
    /// Parser to parse CSS stylesheet source string into CSS objects.
    /// </summary>
    public static class CssParser
    {
        #region Fields and Consts

        /// <summary>
        /// split CSS rule
        /// </summary>
        private static readonly char[] _cssBlockSplitters = new[] { '}', ';' };

        #endregion

        /// <summary>
        /// Parse the given stylesheet source to CSS blocks dictionary.<br/>
        /// The CSS blocks are organized into two level buckets of media type and class name.<br/>
        /// Root media type are found under 'all' bucket.<br/>
        /// If <paramref name="combineWithDefault"/> is true the parsed css blocks are added to the 
        /// default css data (as defined by W3), merged if class name already exists. If false only the data in the given stylesheet is returned.
        /// </summary>
        /// <seealso cref="http://www.w3.org/TR/CSS21/sample.html"/>
        /// <param name="stylesheet">raw css stylesheet to parse</param>
        /// <param name="combineWithDefault">true - combine the parsed css data with default css data, false - return only the parsed css data</param>
        /// <returns>the CSS data with parsed CSS objects (never null)</returns>
        public static CssActiveSheet ParseStyleSheet(string stylesheet, bool combineWithDefault)
        {
            var cssData = combineWithDefault ? CssUtils.DefaultCssData.Clone(new object()) : new CssActiveSheet();
            if (!string.IsNullOrEmpty(stylesheet))
            {
                ParseStyleSheet(cssData, stylesheet);
            }
            return cssData;
        }


        public static WebDom.CssDocument ParseStyleSheet2(string cssTextSource)
        {
            var parser = new WebDom.Parser.CssParser();
            parser.ParseCssStyleSheet(cssTextSource.ToCharArray());
            //-----------------------------------
            return parser.OutputCssDocument;
        }


        /// <summary>
        /// Parse the given stylesheet source to CSS blocks dictionary.<br/>
        /// The CSS blocks are organized into two level buckets of media type and class name.<br/>
        /// Root media type are found under 'all' bucket.<br/>
        /// The parsed css blocks are added to the given css data, merged if class name already exists.
        /// </summary>
        /// <param name="cssData">the CSS data to fill with parsed CSS objects</param>
        /// <param name="cssTextSource">raw css stylesheet to parse</param>
        public static void ParseStyleSheet(CssActiveSheet cssData, string cssTextSource)
        {
            if (!String.IsNullOrEmpty(cssTextSource))
            {
                //-----------------------------------
                // cssData.dbugOriginalSources.Add(cssTextSource);
                //-----------------------------------  
                WebDom.CssDocument cssDoc = ParseStyleSheet2(cssTextSource);

                CssActiveSheet cssActiveDoc = new CssActiveSheet();
                cssActiveDoc.LoadCssDoc(cssDoc);

                if (cssData != null)
                {
                    //merge ?
                    cssData.Combine(cssActiveDoc);
                }
                else
                {
                    //cssData.ActiveDoc = cssActiveDoc;
                }
            }
        }
        public static WebDom.CssRuleSet ParseCssBlock2(string className, string blockSource)
        {
            var parser = new WebDom.Parser.CssParser();
            return parser.ParseCssPropertyDeclarationList(blockSource.ToCharArray());

        }
        /// <summary>
        /// Parse a complex font family css property to check if it contains multiple fonts and if the font exists.<br/>
        /// returns the font family name to use or 'inherit' if failed.
        /// </summary>
        /// <param name="value">the font-family value to parse</param>
        /// <returns>parsed font-family value</returns>
        public static string ParseFontFamily(string value)
        {
            return ParseFontFamilyProperty(value);
        }

         



        /// <summary>
        /// Parse a complex font family css property to check if it contains multiple fonts and if the font exists.<br/>
        /// returns the font family name to use or 'inherit' if failed.
        /// </summary>
        /// <param name="propValue">the value of the property to parse</param>
        /// <returns>parsed font-family value</returns>
        static string ParseFontFamilyProperty(string propValue)
        {
            int start = 0;
            while (start > -1 && start < propValue.Length)
            {
                while (char.IsWhiteSpace(propValue[start]) || propValue[start] == ',' || propValue[start] == '\'' || propValue[start] == '"')
                    start++;
                var end = propValue.IndexOf(',', start);
                if (end < 0)
                    end = propValue.Length;
                var adjEnd = end - 1;
                while (char.IsWhiteSpace(propValue[adjEnd]) || propValue[adjEnd] == '\'' || propValue[adjEnd] == '"')
                    adjEnd--;

                var font = propValue.Substring(start, adjEnd - start + 1);

                if (FontsUtils.IsFontExists(font))
                {
                    return font;
                }

                start = end;
            }

            return CssConstants.Inherit;
        }


      
    }
}
