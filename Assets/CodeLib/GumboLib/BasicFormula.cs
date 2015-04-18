using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BasicFormula
{

    public enum BasicContextValue
    {
        FLAGS,
        HP,
        MAXHP,
        HPDMG,
        DIST,
        RADIUS,
        RANDOM,
        GUMBOWORD,
        INVALID,
    }

    public struct Element
    {
        public ElementType elementType;
        public int val;
        public Element(ElementType elementType, int val)
        {
            this.elementType = elementType;
            this.val = val;
        }
        public Element(ElementType elementType)
        {
            this.elementType = elementType;
            this.val = 0;
        }
        public Element(int val)
        {
            this.elementType = ElementType.CONST;
            this.val = val;
        }
        public Element(string typ)
        {
            this.elementType = Formula.ElementStrings[typ];
            this.val = 0;
        }
        public static Element BlankElement = new Element(ElementType.NONE, 0);

        public override string ToString()
        {
            if (val != 0 || elementType == ElementType.CONST) 
            {
                return elementType.ToString()+"="+val;
            }
            else 
            {
                return elementType.ToString();
            }
        }
    }

    public enum ElementType
    {
        NONE,
        CONST,
        LB,
        RB,
        
        ADD,
        SUBTRACT,
        MULTIPLY,
        DIVIDE,
        MODULO,
        MIN,
        MAX,

        EQUALTO,
        NOTEQUALTO,
        GREATERTHAN,
        GREATERTHANEQUALTO,
        LESSTHAN,
        LESSTHANEQUALTO,

        LOGICALAND,
        LOGICALOR,

        BITWISEAND,
        BITWISEOR,
        BITWISEMATCHALL,
        BITWISEMATCHANY,
        BITWISEEXCLUDEANY,

        REG1,
        REG2,
        REG3,
        REG4,
        REG5,
        REG6,
        REG7,
        REG8,
        REG9,
    }


    public struct BindingPair
    {
        public BasicContextValue gvalue;
        public int gword;
        public ElementType elementType;
        public BindingPair(BasicContextValue gvalue, ElementType elementType)
        {
            this.gvalue = gvalue;
            this.gword = 0;
            this.elementType = elementType;
        }

        public BindingPair(int gword, ElementType elementType)
        {
            this.gvalue = BasicContextValue.GUMBOWORD;
            this.gword = gword;
            this.elementType = elementType;
        }

        public bool IsGumboWord { get { return gword != 0; } }
    }

    public class Formula
    {
        
        /// <summary>
        /// Environment for all the variables in the formula.
        /// </summary>
        public List<BindingPair> bindings;
        private int[] registry;

        private List<Element> rpnFormula;
        public bool isFormula;
        public bool isConstFormula;
        public bool hasBindings;
        public int? formulaValue;

        public Formula()
        {
            isFormula = false;
            isConstFormula = false;
            hasBindings = false;
            formulaValue = 0;
        }

        public void AddElementList(List<Element> formula)
        {
            rpnFormula = InfixToRPN(formula, out isConstFormula);
            isFormula = rpnFormula.Exists(x => x.elementType != ElementType.NONE);
            if (!isFormula)
            {
                ClearCache();
            }
            else if (isConstFormula)
            {
                formulaValue = EvaluateRPN();
            }
            else
            {
                ClearCache();
            }
        }

        public int AddBinding(BasicContextValue variable)
        {
            if (!hasBindings)
            {
                bindings = new List<BindingPair>();
                registry = new int[9];
                hasBindings = true;
            }
            int registerPlace = this.bindings.Count;
            this.bindings.Add(new BindingPair(variable, RegisterFromInt(registerPlace)));
            return registerPlace;
        }

        public int AddBinding(int variable)
        {
            if (!hasBindings)
            {
                bindings = new List<BindingPair>();
                registry = new int[9];
                hasBindings = true;
            }
            int registerPlace = this.bindings.Count;
            this.bindings.Add(new BindingPair(variable, RegisterFromInt(registerPlace)));
            return registerPlace;
        }

        public int BindingCount()
        {
            return this.bindings == null ? 0 : this.bindings.Count;
        }

        public int BindingIndex(BasicContextValue variable)
        {
            if (this.bindings != null)
            {
                for(int i=0; i<this.bindings.Count; ++i)
                {
                    if (variable == this.bindings[i].gvalue)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public int BindingIndex(int variable)
        {
            if (this.bindings != null)
            {
                for(int i=0; i<this.bindings.Count; ++i)
                {
                    if (variable == this.bindings[i].gword)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool IsMismatched()
        {
            if (isConstFormula)
            {
                return false;
            }
            int operatorCount = 0;
            int registerCount = 0;
            int elementCount = 0;
            for (int i = 0; i < rpnFormula.Count; ++i)
            {
                ElementType token = rpnFormula[i].elementType;
                if (token == ElementType.LB || token == ElementType.RB)
                {
                    //dontcare
                }
                else if (IsOperatorType(token))
                {
                    operatorCount++;
                }
                else if (IsRegisterType(token))
                {
                    registerCount++;
                }
                else
                {
                    elementCount++;
                }
            }
            int shouldbeZero = 1 + operatorCount - registerCount - elementCount;
            return shouldbeZero != 0;
        }

        public static ElementType RegisterFromInt(int i)
        {
            return (ElementType) (i + (int)ElementType.REG1);
        }

        public static int RegisterToInt(ElementType elementType)
        {
            return (int)elementType - (int)ElementType.REG1;
        }


        public int Calculate()
        {
            if (!isFormula)
            {
                return 0;
            }
            if (!formulaValue.HasValue)
            {
                formulaValue = EvaluateRPN();
            }
            return formulaValue.Value;
        }

        private void ClearCache()
        {
            formulaValue = null;
        }

        /// <summary>
        /// Method use to calculate the value of the formula.
        /// </summary>
        public int EvaluateRPN()
        {
            Stack<int> values = new Stack<int>();

            Element element = Element.BlankElement;
            for (int i = 0; i < rpnFormula.Count; ++i)
            {
                element = rpnFormula[i];
                ElementType token = element.elementType;
                if (IsOperatorType(token))
                {
                    if (values.Count >= 2) 
                    {
                        int val1 = values.Pop();
                        int val2 = values.Pop();
                        values.Push(DoOperation(val2, val1, token));
                    }
                    else 
                    {
                        Debug.LogError("Missing values for "+token);
                        values.Push(0);
                    }
                }
                else if (IsRegisterType(token))
                {
                    values.Push(registry[RegisterToInt(token)]);
                }
                else
                {
                    values.Push(element.val);
                }
            }

            if (values.Count != 1)
                throw new InvalidOperationException("Cannot calculate formula."+values.Count+" of "+rpnFormula.Count+" "+ToLongString());

            return values.Pop();
        }

        private static bool IsOperatorType(ElementType elementType)
        {
            return elementType >= ElementType.ADD && elementType <= ElementType.BITWISEEXCLUDEANY;
        }

        public static bool IsRegisterType(ElementType elementType)
        {
            return elementType >= ElementType.REG1 && elementType <= ElementType.REG9;
        }

        public static bool RequiresBitWise(ElementType elementType)
        {
            return elementType >= ElementType.BITWISEMATCHALL && elementType <= ElementType.BITWISEEXCLUDEANY;
        }

        public void SetRegister(ElementType elementType, int val)
        {
            int registerNum = RegisterToInt(elementType);
            if (registry[registerNum] != val)
            {
                registry[registerNum] = val;
                ClearCache();
            }
        }
       
        public void SetRegister(BasicContextValue gvalue, int val)
        {
            SetRegister(bindings.Find(x => x.gvalue == gvalue).elementType, val);
        }

        public string ToLongString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(isFormula?"frm":"-");
            Element element = Element.BlankElement;
            for (int i = 0; i < rpnFormula.Count; ++i)
            {
                element = rpnFormula[i];
                sb.Append(element);
                sb.Append(" ");
            }
            return sb.ToString();
        }

#region Static Members

        /// <summary>
        /// Constant for left association symbols
        /// </summary>
        private static readonly int LEFT_ASSOC = 0;

        /// <summary>
        /// Constant for right association symbols
        /// </summary>
        private static readonly int RIGHT_ASSOC = 1;

        /// <summary>
        /// Static list of operators in the formula
        /// </summary>
        private static Dictionary<ElementType, int[]> Operators = new Dictionary<ElementType, int[]>();
        private static Dictionary<ElementType, int[]> Registers = new Dictionary<ElementType, int[]>();
        public static Dictionary<string, ElementType> ElementStrings = new Dictionary<string, ElementType>();
#endregion

#region Static Methods

        

        // order of op http://en.wikipedia.org/wiki/Order_of_operations
        // 11 LB RB scope
        // 10 unary !
        // 9 mult div
        // 8 add sub
        // 7 bitwise shift
        // 7 bitwise match?
        // 6 comparisons
        // 5 equal notequal
        // 4 bitwise and
        // 3 bitwise XOR
        // 2 bitwise OR
        // 1 logical and
        // 0 logical or

        /// <summary>
        /// Static constructor.
        /// </summary>
        static Formula()
        {
            Operators.Add(ElementType.ADD, new int[] { 8, LEFT_ASSOC });
            Operators.Add(ElementType.SUBTRACT, new int[] { 8, LEFT_ASSOC });
            Operators.Add(ElementType.MULTIPLY, new int[] { 9, LEFT_ASSOC });
            Operators.Add(ElementType.DIVIDE, new int[] { 9, LEFT_ASSOC });
            Operators.Add(ElementType.MODULO, new int[] { 9, LEFT_ASSOC });
            Operators.Add(ElementType.MIN, new int[] { -1, LEFT_ASSOC });
            Operators.Add(ElementType.MAX, new int[] { -1, LEFT_ASSOC });
            Operators.Add(ElementType.EQUALTO, new int[] { 5, LEFT_ASSOC });
            Operators.Add(ElementType.NOTEQUALTO, new int[] { 5, LEFT_ASSOC });
            Operators.Add(ElementType.GREATERTHAN, new int[] { 6, LEFT_ASSOC });
            Operators.Add(ElementType.GREATERTHANEQUALTO, new int[] { 6, LEFT_ASSOC });
            Operators.Add(ElementType.LESSTHAN, new int[] { 6, LEFT_ASSOC });
            Operators.Add(ElementType.LESSTHANEQUALTO, new int[] { 6, LEFT_ASSOC });

            Operators.Add(ElementType.LOGICALAND, new int[] { 1, LEFT_ASSOC });
            Operators.Add(ElementType.LOGICALOR, new int[] { 0, LEFT_ASSOC });
            Operators.Add(ElementType.BITWISEAND, new int[] { 4, LEFT_ASSOC });
            Operators.Add(ElementType.BITWISEOR, new int[] { 2, LEFT_ASSOC });
            Operators.Add(ElementType.BITWISEMATCHALL, new int[] { 7, LEFT_ASSOC });
            Operators.Add(ElementType.BITWISEMATCHANY, new int[] { 7, LEFT_ASSOC });
            Operators.Add(ElementType.BITWISEEXCLUDEANY, new int[] { 7, LEFT_ASSOC });
            Registers.Add(ElementType.REG1, null);
            Registers.Add(ElementType.REG2, null);
            Registers.Add(ElementType.REG3, null);
            Registers.Add(ElementType.REG4, null);
            Registers.Add(ElementType.REG5, null);
            Registers.Add(ElementType.REG6, null);
            Registers.Add(ElementType.REG7, null);
            Registers.Add(ElementType.REG8, null);
            Registers.Add(ElementType.REG9, null);

            ElementStrings.Add("(",ElementType.LB);
            ElementStrings.Add(")",ElementType.RB);
            ElementStrings.Add("+",ElementType.ADD);
            ElementStrings.Add("-",ElementType.SUBTRACT);
            ElementStrings.Add("*",ElementType.MULTIPLY);
            ElementStrings.Add("/",ElementType.DIVIDE);
            ElementStrings.Add("%",ElementType.MODULO);
            //ElementStrings.Add("",ElementType.MIN);
            //ElementStrings.Add("",ElementType.MAX);
            ElementStrings.Add("==",ElementType.EQUALTO);
            ElementStrings.Add("=",ElementType.EQUALTO);
            ElementStrings.Add("!=",ElementType.NOTEQUALTO);
            ElementStrings.Add(">",ElementType.GREATERTHAN);
            ElementStrings.Add(">=",ElementType.GREATERTHANEQUALTO);
            ElementStrings.Add("<",ElementType.LESSTHAN);
            ElementStrings.Add("<=",ElementType.LESSTHANEQUALTO);
            ElementStrings.Add("AND",ElementType.LOGICALAND);
            ElementStrings.Add("&&",ElementType.LOGICALAND);
            ElementStrings.Add("OR",ElementType.LOGICALOR);
            ElementStrings.Add("||",ElementType.LOGICALOR);
            ElementStrings.Add("&",ElementType.BITWISEAND);
            ElementStrings.Add(",",ElementType.BITWISEOR);
            ElementStrings.Add("|",ElementType.BITWISEOR);
            ElementStrings.Add("$",ElementType.BITWISEMATCHALL);
            ElementStrings.Add("MATCH",ElementType.BITWISEMATCHALL);
            ElementStrings.Add("ALL",ElementType.BITWISEMATCHALL);
            ElementStrings.Add("#",ElementType.BITWISEMATCHANY);
            ElementStrings.Add("ANY",ElementType.BITWISEMATCHANY);
            ElementStrings.Add("~",ElementType.BITWISEEXCLUDEANY);
            ElementStrings.Add("NOT",ElementType.BITWISEEXCLUDEANY);

        }
        public static Formula BlankFormula = new Formula();



        /// <summary>
        /// Static method to check if the type of operation is associative (left or right).
        /// </summary>
        /// <param name="token">The token operator.</param>
        /// <param name="type">The type of association (left or right).</param>
        /// <returns>True if it's associative, else false.</returns>
        private static bool isAssociative(ElementType token, int type)
        {
            if (!IsOperatorType(token))
                throw new ArgumentException("Invalid token: " + token);

            if (Operators[token][1] == type)
                return true;

            return false;
        }

        /// <summary>
        /// Static method to compare operator precendece.
        /// </summary>
        /// <param name="token1">First operator.</param>
        /// <param name="token2">Second operator.</param>
        /// <returns>The value of precedence between the two operators.</returns>
        private static int comparePrecedence(ElementType token1, ElementType token2)
        {
            if (!IsOperatorType(token1) || !IsOperatorType(token2))
                throw new ArgumentException("Invalid token: " + token1 + " " + token2);

            return Operators[token1][0] - Operators[token2][0];
        }

        /// <summary>
        /// Static method to transfor a normal formula into an RPN formula.
        /// </summary>
        /// <param name="input">The normal infix formula.</param>
        /// <returns>The RPN formula.</returns>
        public static List<Element> InfixToRPN(List<Element> input, out bool isConstFormula)
        {
            for (int i = 0; i < input.Count; i++)
            {
                ElementType inputType = input[i].elementType;
                if (IsOperatorType(inputType) || inputType == ElementType.LB || inputType == ElementType.RB)
                {
                    if (i != 0 && input[i - 1].elementType != ElementType.NONE)
                        input.Insert(i, Element.BlankElement);
                    if (i != input.Count - 1 && input[i + 1].elementType != ElementType.NONE)
                        input.Insert(i + 1, Element.BlankElement);
                }
            }

            List<Element> outList = new List<Element>();
            Stack<Element> stack = new Stack<Element>();
            isConstFormula = true;
            for(int i=0; i< input.Count; ++i)
            {
                Element element = input[i];
                ElementType token = element.elementType;
                if (token == ElementType.NONE)
                    continue;

                if (IsOperatorType(token))
                {
                    while (stack.Count != 0 && IsOperatorType(stack.Peek().elementType))
                    {
                        if ((isAssociative(token, LEFT_ASSOC) && comparePrecedence(token, stack.Peek().elementType) <= 0) ||
                            (isAssociative(token, RIGHT_ASSOC) && comparePrecedence(token, stack.Peek().elementType) < 0))
                        {
                            outList.Add(stack.Pop());
                            continue;
                        }
                        break;
                    }
                    stack.Push(element);
                }
                else if (token == ElementType.LB)
                {
                    stack.Push(element);
                }
                else if (token == ElementType.RB)
                {
                    while (stack.Count != 0 && stack.Peek().elementType != ElementType.LB)
                    {
                        outList.Add(stack.Pop());
                    }
                    stack.Pop();
                }
                else
                {
                    if (isConstFormula && IsRegisterType(token))
                    {
                        isConstFormula = false;
                    }
                    outList.Add(element);
                }
            }

            while (stack.Count != 0)
                outList.Add(stack.Pop());

            return outList;
        }

        /*private static float DoFloatOperation(float val1, float val2, Chunk OP)
        {
            switch (OP)
            {
                case Chunk.ADD:
                    return val1 + val2;
                case Chunk.SUBTRACT:
                    return val1 - val2;
                case Chunk.MULTIPLY:
                    return val1 * val2;
                case Chunk.DIVIDE:
                    return val1 / val2;
                case Chunk.MODULO:
                    return (int)val1 % (int)val2;
                case Chunk.EQUALTO:
                    return Mathf.Abs(val1 - val2) < 0.01f;
                case Chunk.NOTEQUALTO:
                    return Mathf.Abs(val1 - val2) > 0.01f;
                case Chunk.GREATERTHAN:
                    return val1 > (val2 + 0.01f);
                case Chunk.GREATERTHANEQUALTO:
                    return val1 > (val2 - 0.01f);
                case Chunk.LESSTHAN:
                    return val1 < (val2 - 0.01f);
                case Chunk.LESSTHANEQUALTO:
                    return val1 < (val2 + 0.01f);
                default:
                    return 0;
            }
        }*/

        private static int DoOperation(int val1, int val2, ElementType op)
        {
            switch (op)
            {
                case ElementType.ADD:
                    return val1 + val2;
                case ElementType.SUBTRACT:
                    return val1 - val2;
                case ElementType.MULTIPLY:
                    return val1 * val2;
                case ElementType.DIVIDE:
                    return val1 / val2;
                case ElementType.MODULO:
                    return val1 % val2;
                case ElementType.MIN:
                    return (val1 < val2) ? val1 : val2;
                case ElementType.MAX:
                    return (val1 > val2) ? val1 : val2;
                case ElementType.EQUALTO:
                    return (val1 == val2) ? 1 : 0;
                case ElementType.NOTEQUALTO:
                    return (val1 != val2) ? 1 : 0;
                case ElementType.GREATERTHAN:
                    return (val1 > val2) ? 1 : 0;
                case ElementType.GREATERTHANEQUALTO:
                    return (val1 >= val2) ? 1 : 0;
                case ElementType.LESSTHAN:
                    return (val1 < val2) ? 1 : 0;
                case ElementType.LESSTHANEQUALTO:
                    return (val1 <= val2) ? 1 : 0;
                case ElementType.LOGICALAND:
                    return (val1 > 0 && val2 > 0) ? 1 : 0;
                case ElementType.LOGICALOR:
                    return (val1 > 0 || val2 > 0) ? 1 : 0;
                case ElementType.BITWISEOR:
                    return val1 | val2;
                case ElementType.BITWISEAND:
                    return val1 & val2;
                case ElementType.BITWISEMATCHANY:
                    return (val1 & val2) > 0 ? 1 : 0;
                case ElementType.BITWISEMATCHALL:
                    return (val1 & val2) == val2 ? 1 : 0;
                case ElementType.BITWISEEXCLUDEANY:
                    return (val1 & val2) == 0 ? 1 : 0;
                default:
                    return 0;
            }
        }




#endregion

    }

}