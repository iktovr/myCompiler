using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor.AbstractGrammar
{
    public class Symbol : IEquatable<Symbol>
    {
        public string Value { set; get; } //
        public List<Symbol> Attr = null; ///< Множество атрибутов символа

        public Symbol() {}

        public Symbol(string s, List<Symbol> a)
        {
            Value = s;
            Attr = new List<Symbol>(a);
        }

        public Symbol(string Value)
        {
            this.Value = Value;
            this.Attr = null;
        }

        /// Неявное преобразование строки в Symbol
        public static implicit operator Symbol(string str) => new Symbol(str);

        public bool Equals(Symbol other)
        {
            if (other == null)
                return false;
            return (this.Value.Equals(other.Value));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            Symbol objAsSymbol = obj as Symbol;
            if (objAsSymbol == null)
                return false;
            else
                return Equals(objAsSymbol);
        }

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

        public override string ToString() {
            return this.Value;
        }

        public virtual void print()
        {
            Console.Write(this.Value);
            if (Attr == null)
                return;
            foreach (var a in Attr)
                Console.Write("_" + a.Value + " ");
        }
    }
}
