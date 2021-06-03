using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processor.AbstractGrammar
{
    public class Production
    {
        public Symbol LHS { set; get; } = null; ///< On the Left Hand Side
        public List<Symbol> RHS { set; get; } ///< On the Right Hand Side
        public static int Count = 0;
        public int Id; ///< Production number

        public Production(Symbol LHS, List<Symbol> RHS)
        {
            Count++;
            Id = Count;
            this.LHS = LHS;
            this.RHS = RHS;
        }
    } // end Production
}
