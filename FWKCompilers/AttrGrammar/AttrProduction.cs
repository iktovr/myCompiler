using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Processor.AbstractGrammar;

namespace Processor.AttrGrammar {
  public class AttrProduction : Production {
    public List<AttrFunction> F; //atributes functions 

    public AttrProduction(Symbol LHS,List<Symbol> RHS,List<AttrFunction> F) :
      base (LHS,RHS) {
      this.F   = new List<AttrFunction>(F);
    }

    public void print() {
      LHS.print();
      Console.Write(" -> ");
      for (int i = 0; i<RHS.Count; ++i) {
        RHS[i].print();
      }
      Console.Write("\n");
      for (int i = 0; i<F.Count; ++i) {
        F[i].print();
        if (i!=(F.Count-1))
          Console.Write("\n");
      }
    }
  } // end class AttrProduction   
}
