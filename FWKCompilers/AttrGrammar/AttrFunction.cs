﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Processor.AbstractGrammar;

namespace Processor.AttrGrammar {
    public class AttrFunction {
        public List<Symbol> LH; // left part of the function
        public List<Symbol> RH; // right part of the function

        public AttrFunction(List<Symbol> L,List<Symbol> R) {
            LH = new List<Symbol>(L);
            RH = new List<Symbol>(R);
        }
        public void print() {
            for (int i = 0; i<LH.Count; ++i) {
                Console.Write(LH[i]);
                if (i!=(LH.Count-1)) {
                    Console.Write(", ");
                }
            }
            Console.Write(" <- ");
            for (int i = 0; i<RH.Count; ++i) {
                Console.Write(RH[i]);
            }
        }
    }

}
