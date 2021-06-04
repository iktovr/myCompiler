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

        public override void Parse() {}

        /// проверка на принадлежность к терминалам/нетерминалам
        public bool isNoTerm(string v)
        {
            foreach (var vi in this.V)
                if (v.Equals(vi.Value))
                    return true;
            return false;
        }

        public bool isTerm(string t)
        {
            foreach (var ti in this.T)
                if (t.Equals(ti.Value))
                    return true;
            return false;
        }
    } // end Grammar
}
