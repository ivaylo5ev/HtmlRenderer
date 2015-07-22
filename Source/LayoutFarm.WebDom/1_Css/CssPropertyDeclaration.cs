﻿//BSD  2015,2014 ,WinterDev 
using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Css;

namespace LayoutFarm.WebDom
{

    public enum CssValueOpName
    {
        Unknown,
        Divide,
    }
    public class CssPropertyDeclaration
    {
        bool isAutoGen;
        bool markedAsInherit;

        CssCodeValueExpression firstValue;
        List<CssCodeValueExpression> moreValues;

#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif
        public CssPropertyDeclaration(string unknownName)
        {
            //convert from name to wellknown property name; 
            this.UnknownRawName = unknownName;
        }
        public CssPropertyDeclaration(WellknownCssPropertyName wellNamePropertyName)
        {
            //convert from name to wellknown property name; 
            this.WellknownPropertyName = wellNamePropertyName;
        }
        public CssPropertyDeclaration(WellknownCssPropertyName wellNamePropertyName, CssCodeValueExpression value)
        {
            //from another 
            this.WellknownPropertyName = wellNamePropertyName;
            this.firstValue = value;
            this.markedAsInherit = value.IsInherit;
            //auto gen from another prop
            this.isAutoGen = true;
        }
        public bool IsExpand { get; set; }
        public string UnknownRawName { get; private set; }

        public void AddValue(CssCodeValueExpression value)
        {
            if (firstValue == null)
            {
                this.markedAsInherit = value.IsInherit;
                this.firstValue = value;
            }
            else
            {  
                if (moreValues == null)
                {
                    moreValues = new List<CssCodeValueExpression>();
                }
                moreValues.Add(value);
                markedAsInherit = false;
            }
        }
        public void ReplaceValue(int index, CssCodeValueExpression value)
        {
            if (index == 0)
            {
                this.firstValue = value;
            }
            else
            {
                moreValues[index - 1] = value;
            }
        }

        public WellknownCssPropertyName WellknownPropertyName
        {
            get;
            private set;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.WellknownPropertyName.ToString());
            sb.Append(':');
            CollectValues(sb);
            return sb.ToString();
        }
        void CollectValues(StringBuilder stBuilder)
        {
            if (firstValue != null)
            {
                stBuilder.Append(firstValue.ToString());
            }
            if (moreValues != null)
            {
                int j = moreValues.Count;
                for (int i = 0; i < j; ++i)
                {
                    CssCodeValueExpression propV = moreValues[i];
                    stBuilder.Append(propV.ToString());
                    if (i < j - 1)
                    {
                        stBuilder.Append(' ');
                    }
                }
            }
        }

        public bool MarkedAsInherit
        {
            get
            {
                return this.markedAsInherit;
            }
        }
        public int ValueCount
        {
            get
            {
                if (moreValues == null)
                {
                    if (firstValue == null)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return moreValues.Count + 1;
                }
            }
        }


        public CssCodeValueExpression GetPropertyValue(int index)
        {

            switch (index)
            {
                case 0:
                    {
                        return this.firstValue;
                    }
                default:
                    {
                        if (moreValues != null)
                        {
                            return moreValues[index - 1];
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
            }
        }
    }

    public enum CssValueHint : byte
    {
        Unknown,
        Number,
        HexColor,
        LiteralString,
        Iden,
        Func,
        BinaryExpression,
    }
    public class CssCodeColor : CssCodeValueExpression
    {
        CssColor color;
        public CssCodeColor(CssColor color)
            : base(CssValueHint.HexColor)
        {
            this.color = color;
            SetColorValue(color);
        }
        public CssColor ActualColor
        {
            get { return this.color; }
        }
    }
    public abstract class CssCodeValueExpression
    {
#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif

        public CssCodeValueExpression(CssValueHint hint)
        {
#if DEBUG
            //if (this.dbugId == 111)
            //{

            //}
#endif
            this.Hint = hint;
        }


        CssValueEvaluatedAs evaluatedAs;
        CssColor cachedColor;
        LayoutFarm.Css.CssLength cachedLength;
        int cachedInt;
        protected float number;

        public bool IsInherit
        {
            get;
            internal set;
        }
        public CssValueHint Hint
        {
            get;
            private set;
        }
        //------------------------------------------------------
        public float AsNumber()
        {
            return this.number;
        }

        public void SetIntValue(int intValue, CssValueEvaluatedAs evaluatedAs)
        {
            this.evaluatedAs = evaluatedAs;
            this.cachedInt = intValue;
        }
        public void SetColorValue(CssColor color)
        {
            this.evaluatedAs = CssValueEvaluatedAs.Color;
            this.cachedColor = color;
        }
        public void SetCssLength(CssLength len, WebDom.CssValueEvaluatedAs evalAs)
        {
            this.cachedLength = len;
            this.evaluatedAs = evalAs;
        }

        public CssValueEvaluatedAs EvaluatedAs
        {
            get
            {
                return this.evaluatedAs;
            }
        }

        public CssColor GetCacheColor()
        {
            return this.cachedColor;
        }
        public CssLength GetCacheCssLength()
        {
            return this.cachedLength;
        }
        public virtual string GetTranslatedStringValue()
        {
            return this.ToString();
        }
        public int GetCacheIntValue()
        {
            return this.cachedInt;
        }
    }

    public class CssCodePrimitiveExpression : CssCodeValueExpression
    {
        string unit;
        readonly string _propertyValue;

        public CssCodePrimitiveExpression(string value, CssValueHint hint)
            : base(hint)
        {
            this._propertyValue = value;
            switch (hint)
            {
                case CssValueHint.Iden:
                    {
                        //check value  
                        this.IsInherit = value == "inherit";
                    } break;
                case CssValueHint.Number:
                    {
                        this.number = float.Parse(value);
                    } break;
            }
        }
        public CssCodePrimitiveExpression(float number)
            : base(CssValueHint.Number)
        {
            //number             
            this.number = number;
        }
        public string Unit
        {
            get { return unit; }
            set { this.unit = value; }
        }
        public string Value
        {
            get
            {
                return this._propertyValue;
            }
        }
        public override string ToString()
        {
            switch (this.Hint)
            {
                case CssValueHint.Number:
                    {
                        if (unit != null)
                        {
                            return number.ToString() + unit;
                        }
                        else
                        {
                            return number.ToString();
                        }
                    }
                default:
                    if (unit != null)
                    {
                        return Value + unit;
                    }
                    else
                    {
                        return Value;
                    }

            }

        }
    }



    public class CssCodeFunctionCallExpression : CssCodeValueExpression
    {
        string evaluatedStringValue;
        bool isEval;
        List<CssCodeValueExpression> funcArgs = new List<CssCodeValueExpression>();
        public CssCodeFunctionCallExpression(string funcName)
            : base(CssValueHint.Func)
        {
            this.FunctionName = funcName;
        }
        public string FunctionName
        {
            get;
            private set;
        }
        public void AddFuncArg(CssCodeValueExpression arg)
        {
            this.funcArgs.Add(arg);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.FunctionName);
            sb.Append('(');
            int j = funcArgs.Count;

            for (int i = 0; i < j; ++i)
            {
                sb.Append(funcArgs[i].ToString());
                if (i < j - 1)
                {
                    sb.Append(',');
                }
            }
            sb.Append(')');
            return sb.ToString();
        }


        public override string GetTranslatedStringValue()
        {
            if (isEval)
            {
                return this.evaluatedStringValue;
            }
            else
            {
                isEval = true;
                switch (this.FunctionName)
                {
                    case "rgb":
                        {
                            //each is number 
                            int r_value = (int)funcArgs[0].AsNumber();
                            int g_value = (int)funcArgs[1].AsNumber();
                            int b_value = (int)funcArgs[2].AsNumber();

                            return this.evaluatedStringValue = "#" + r_value.ToString("X") + g_value.ToString("X") + b_value.ToString("X");
                        }
                    case "url":
                        {
                            return this.evaluatedStringValue = this.funcArgs[0].ToString();
                        }
                    default:
                        {
                            return this.evaluatedStringValue = this.ToString();
                        }
                }
            }
        }

    }

    public class CssCodeBinaryExpression : CssCodeValueExpression
    {

        public CssCodeBinaryExpression()
            : base(CssValueHint.BinaryExpression)
        {

        }
        public CssValueOpName OpName
        {
            get;
            set;
        }
        public CssCodeValueExpression Left
        {
            get;
            set;
        }
        public CssCodeValueExpression Right
        {
            get;
            set;
        }
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            if (Left != null)
            {
                stbuilder.Append(Left.ToString());
            }
            else
            {
                throw new NotSupportedException();
            }
            switch (this.OpName)
            {
                case CssValueOpName.Unknown:
                    {
                        throw new NotSupportedException();
                    }
                case CssValueOpName.Divide:
                    {
                        stbuilder.Append('/');
                    } break;
            }
            if (Right != null)
            {
                stbuilder.Append(Right.ToString());
            }
            else
            {
                throw new NotSupportedException();

            }
            return stbuilder.ToString();
        }
        public override string GetTranslatedStringValue()
        {

            throw new NotImplementedException();
        }
    }



    public enum CssValueEvaluatedAs : byte
    {
        UnEvaluate,
        Unknown,
        BorderLength,
        Length,
        TranslatedLength,
        Color,
        TranslatedString,

        BorderStyle,
        BorderCollapse,
        WhiteSpace,
        Visibility,
        VerticalAlign,
        TextAlign,
        Overflow,
        TextDecoration,
        WordBreak,
        Position,
        Direction,
        Display,
        Float,
        EmptyCell,
        FontWeight,
        FontStyle,
        FontVariant,

        ListStylePosition,
        ListStyleType,
        BackgroundRepeat,
        BoxSizing,
    }


}