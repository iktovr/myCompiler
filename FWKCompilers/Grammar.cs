using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Processor.AbstractGrammar;

namespace Translator
{

    public class Grammar : AGrammar
    {
        public Grammar(List<Symbol> T, List<Symbol> V, string S0) : base(T, V, S0)
        {
            Production.Count = 0; //  production ?
            FirstSet = new Dictionary<Symbol, HashSet<Symbol>>();
            FollowSet = new Dictionary<Symbol, HashSet<Symbol>>();
        }
        public Grammar(List<Symbol> T, List<Symbol> V, List<Production> production, string S0) : base(T, V, S0)
        {
            Production.Count = 0;
            this.P = production;
            FirstSet = new Dictionary<Symbol, HashSet<Symbol>>();
            FollowSet = new Dictionary<Symbol, HashSet<Symbol>>();
        }

        public Grammar() : base(){
            Production.Count = 0;
            FirstSet = new Dictionary<Symbol, HashSet<Symbol>>();
            FollowSet = new Dictionary<Symbol, HashSet<Symbol>>();
        }

        public override void Parse() {}

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
        
        /*-----------------------------------
         тут находится FIRST FOLLOW*/
        private Dictionary<Symbol, HashSet<Symbol>> FirstSet;
        private Dictionary<Symbol, HashSet<Symbol>> FollowSet;
        
        public void ComputeFirstFollow()
        {
            ComputeFirstSets();
            ComputeFollowSets();
        }
        
        private void ComputeFirstSets()
        {
            FirstSet.Clear();
            foreach (var term in T)
                FirstSet[term] = new HashSet<Symbol>() { term }; // FIRST[c] = {c}
            FirstSet[Symbol.Epsilon] = new HashSet<Symbol>() { Symbol.Epsilon }; // для единообразия
            foreach (Symbol noTerm in V)
                FirstSet[noTerm] = new HashSet<Symbol>(); // First[X] = empty set
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (var rule in P)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    var X = rule.LHS;
                    foreach (var Y in rule.RHS)
                    {
                        foreach (var curFirstSymb in FirstSet[Y])
                        {
                            if (FirstSet[X].Add(curFirstSymb)) // Добавить а в FirstSets[X]
                            {
                                changes = true;
                            }
                        }
                        if (!FirstSet[Y].Contains(Symbol.Epsilon))
                        {
                            break;
                        }
                    }
                }
            } // пока вносятся изменения
        }
        
        public HashSet<Symbol> First(Symbol X)
        {
            return FirstSet[X];
        }

        public HashSet<Symbol> First(List<Symbol> X)
        {
            HashSet<Symbol> result = new HashSet<Symbol>();
            foreach (Symbol Y in X)
            {
                foreach (Symbol curFirstSymb in FirstSet[Y])
                {
                    result.Add(curFirstSymb);
                }
                if (!FirstSet[Y].Contains(Symbol.Epsilon))
                {
                    break;
                }
            }
            return result;
        }
        
        private void ComputeFollowSets()
        {
            foreach (Symbol noTerm in V)
                FollowSet[noTerm] = new HashSet<Symbol>();
            FollowSet[S0].Add(Symbol.Sentinel);
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (Production rule in P)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    for (int indexOfSymbol = 0; indexOfSymbol < rule.RHS.Count; ++indexOfSymbol)
                    {
                        Symbol curSymbol = rule.RHS[indexOfSymbol];
                        if (T.Contains(curSymbol) || curSymbol == Symbol.Epsilon)
                        {
                            continue;
                        }
                        if (indexOfSymbol == rule.RHS.Count - 1)
                        {
                            foreach (Symbol curFollowSymbol in FollowSet[rule.LHS])
                            {
                                if (FollowSet[curSymbol].Add(curFollowSymbol))
                                {
                                    changes = true;
                                }
                            }
                        }
                        else
                        {
                            HashSet<Symbol> curFirst = First(rule.RHS[indexOfSymbol + 1]);
                            bool epsFound = false;
                            foreach (Symbol curFirstSymbol in curFirst)
                            {
                                if (curFirstSymbol != Symbol.Epsilon)
                                {
                                    if (FollowSet[rule.RHS[indexOfSymbol]].Add(curFirstSymbol))
                                    {
                                        changes = true;
                                    }
                                }
                                else
                                {
                                    epsFound = true;
                                }
                            }
                            if (epsFound)
                            {
                                foreach (Symbol curFollowSymbol in FollowSet[rule.LHS])
                                {
                                    if (FollowSet[rule.RHS[indexOfSymbol]].Add(curFollowSymbol))
                                    {
                                        changes = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public HashSet<Symbol> Follow(Symbol X)
        {
            if(FollowSet.ContainsKey(X))
            {
                return FollowSet[X];
            }

            return new HashSet<Symbol>();
        }
        
        
    } // end Grammar
}
