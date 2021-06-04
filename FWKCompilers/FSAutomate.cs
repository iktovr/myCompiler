using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Processor.AbstractGrammar;

namespace Translator
{
    /// Delta: Q x Sigma -> Q
    public class DeltaQSigma
    { //: AbstractProduction {
        public Symbol LHSQ { set; get; } = null; ///< Q
        public Symbol LHSS { set; get; } = null; ///< Sigma
        public List<Symbol> RHSQ { set; get; } = null; ///< Q
        public DeltaQSigma(Symbol LHSQ, Symbol LHSS, List<Symbol> RHSQ)
        {
            this.LHSQ = LHSQ;
            this.LHSS = LHSS;
            this.RHSQ = RHSQ;
        }
    } // end DeltaQSigma

    /// Finite State automata (КА)
    public class FSAutomate : Automate
    {
        public FSAutomate(List<Symbol> Q, List<Symbol> Sigma, List<Symbol> F, string q0) : base(Q, Sigma, F, q0) {}

        public FSAutomate() : base() {}

        public void Execute(string chineSymbol)
        {
            var currState = this.Q0;
            int flag = 0;
            int i = 0;
            for (; i < chineSymbol.Length; i++)
            {
                flag = 0;
                foreach (var d in this.D)
                {
                    if (d.LHSQ == currState && d.LHSS == chineSymbol.Substring(i, 1))
                    {
                        currState = d.RHSQ[0].Value; // Для детерминированного К автомата
                        flag = 1;
                        break;
                    }
                }
                if (flag == 0)
                    break;
            } // end for

            Console.WriteLine("Length: " + chineSymbol.Length);
            Console.WriteLine(" i :" + i.ToString());
            Debug("curr", currState);
            if (this.F.Contains(new Symbol(currState)) && i == chineSymbol.Length)
                Console.WriteLine("chineSymbol belongs to language");
            else
                Console.WriteLine("chineSymbol doesn't belong to language");
        } // end Execute
    } // KAutomate
}
