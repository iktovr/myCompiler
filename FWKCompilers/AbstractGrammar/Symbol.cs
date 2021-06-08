using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor.AbstractGrammar
{
  public class Symbol {
      public string symbol; ///< Строковое значение/имя символа
      public List<Symbol> Attr = null; ///< Множество атрибутов символа

      public int production { set; get;} = 0; // for grammar occur  
      public int symbolPosition { set; get; } = 0; // for grammar occur

      public Symbol() {}
      public Symbol(string s, int production, int symbolPosition)
      {
            this.symbol = s;
            this.production = production;
            this.symbolPosition = symbolPosition;
      }
      public Symbol(string s, List<Symbol> a)
      {
          this.symbol = s;
          Attr = new List<Symbol>(a);
          this.production = 0;
          this.symbolPosition = 0;
      }

      public Symbol(string value)
      {
          this.symbol = value;
          this.Attr = null;
          this.production = 0;
          this.symbolPosition = 0;
      }

      /// Неявное преобразование строки в Symbol
      public static implicit operator Symbol(string str) => new Symbol(str);
      /// Равенсто. Требуется для Dictionary и HashSet
      public override bool Equals(object other)
      {
          return (other is Symbol) && (this.symbol == ((Symbol)other).symbol) && 
                 (((Symbol) other).production == this.production) &&
                 (((Symbol)other).symbolPosition == this.symbolPosition);
      }
      /// Хеш-функция. Требуется для Dictionary и HashSet
      public override int GetHashCode()
      {
          return (this.symbol + this.production.ToString() + this.symbolPosition.ToString()).GetHashCode();
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
          return a.symbol== b.symbol;
      }
      public static bool operator != (Symbol symbol1,Symbol symbol2) {
          return !(symbol1 == symbol2);
      }
      public virtual void print()
      {
          Console.Write(this.symbol);
          if (Attr == null)
              return;
          foreach (var a in Attr)
              Console.Write("_" + a.symbol + " ");
      }
      public override string ToString() => this != Epsilon ? this.symbol : "e";
      public static readonly Symbol Epsilon = new Symbol(""); ///< Пустой символ
      public static readonly Symbol Sentinel = new Symbol("$"); ///< Cимвол конца строки / Символ дна стека
  }
}
