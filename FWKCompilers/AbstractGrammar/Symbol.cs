using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor.AbstractGrammar
{
    public class Symbol
    {
        public string Value; ///< Строковое значение/имя символа
        public List<Symbol> Attr = null; ///< Множество атрибутов символа

        public Symbol() {}

        public Symbol(string s, List<Symbol> a)
        {
            Value = s;
            Attr = new List<Symbol>(a);
        }

        public Symbol(string value)
        {
            this.Value = value;
            this.Attr = null;
        }

        /// Неявное преобразование строки в Symbol
        public static implicit operator Symbol(string str) => new Symbol(str);

        /// Равенсто. Требуется для Dictionary и HashSet
        public override bool Equals(object other)
        {   
            return (other is Symbol) && (Value == ((Symbol)other).Value);
        }

        /// Хеш-функция. Требуется для Dictionary и HashSet
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator == (Symbol a, Symbol b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Value == b.Value;
        }

        public static bool operator != (Symbol symbol1,Symbol symbol2) {
            return !(symbol1 == symbol2);
        }

        public virtual void print()
        {
            Console.Write(this.Value);
            if (Attr == null)
                return;
            foreach (var a in Attr)
                Console.Write("_" + a.Value + " ");
        }

        public override string ToString() => this != Epsilon ? Value : "e";

        public static readonly Symbol Epsilon = new Symbol(""); ///< Пустой символ
        public static readonly Symbol Sentinel = new Symbol("$$"); ///< Cимвол конца строки / Символ дна стека
    }
}
