using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor.AbstractGrammar
{
    public class Symbol : IEquatable<Symbol>
    {
        public string symbol { set; get; } //
        public List<Symbol> Attr = null; ///< Множество атрибутов символа

        public Symbol() {}

        public Symbol(string s, List<Symbol> a)
        {
            symbol = s;
            Attr = new List<Symbol>(a);
        }

        public Symbol(string symbol)
        {
            this.symbol = symbol;
            this.Attr = null;
        }

        public bool Equals(Symbol other)
        {
            if (other == null)
                return false;
            return (this.symbol.Equals(other.symbol));
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
            return symbol.GetHashCode();
        }

        /*
                public static bool operator == (Symbol symbol1,Symbol symbol2) {
                    return symbol1.Equals(symbol2);
                }
                public override int GetHashCode() { return this.symbol.GetHashCode(); }

                public static bool operator !=(Symbol symbol1,Symbol symbol2) {
                    return !symbol1.Equals(symbol2);
                }   
        */
        public override string ToString() {
            return this.symbol;
        }
        

        public virtual void print()
        {
            Console.Write(this.symbol);
            if (Attr == null)
                return;
            foreach (var a in Attr)
                Console.Write("_" + a.symbol + " ");
        }
    }
}
