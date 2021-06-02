using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Processor.AbstractGrammar;

namespace Processor.AttrGrammar {
  public class OPSymbol: Symbol {    
    public AttrFunction function = null;
    public OPSymbol(string s,List<Symbol> a,List<Symbol> L, List<Symbol> R): 
      base(s,a) {
      function = new AttrFunction(L,R); 
    }

    public OPSymbol(string s,List<Symbol> a) : base(s,a) { }

    public override void print() {      
      Console.Write(this.symbol+ "\n");    
    }
  }

}
