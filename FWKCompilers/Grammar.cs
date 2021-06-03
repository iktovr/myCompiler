using System;
using System.Collections.Generic;
using System.Collections;
using Processor.AbstractGrammar;

namespace Translator
{

    public class Grammar : AGrammar
    {
        public Grammar(List<Symbol> T, List<Symbol> V, string S0) : base(T, V, S0)
        {
            Production.Count = 0; //  production ?
        }
        public Grammar(List<Symbol> T, List<Symbol> V, List<Production> production, string S0) : base(T, V, S0)
        {
            Production.Count = 0;
            this.P = production;
        }

        public Grammar() : base() { Production.Count = 0; }

        /// порождение цепочек символов по правилам вывода
        public override string Execute()
        { //
            string bornedLine = null;
            string currState = "S0";
            foreach (var p in this.P)
            {
                if (p.LHS.symbol == currState)
                {
                    //
                }
            }
            return bornedLine;
        }

        /// проверка на принадлежность к терминалам/нетерминалам
        public bool isNoTerm(string v)
        {
            foreach (var vi in this.V)
                if (v.Equals(vi.symbol))
                    return true;
            return false;
        }

        public bool isTerm(string t)
        {
            foreach (var ti in this.T)
                if (t.Equals(ti.symbol))
                    return true;
            return false;
        }
    } // end Grammar
}