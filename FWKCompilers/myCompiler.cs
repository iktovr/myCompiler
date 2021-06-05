using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Processor.AbstractGrammar;

namespace Translator
{

    public abstract class Automate
    {
        public List<Symbol> Q = null; ///< множество состояний
        public List<Symbol> Sigma = null; ///< множество алфавит
        public dynamic D = new List<DeltaQSigma>(); ///< множество правил перехода
        public string Q0 = null; ///< начальное состояние
        public List<Symbol> F = null; ///< множество конечных состояний

        public Automate() {}

        public Automate(List<Symbol> Q, List<Symbol> Sigma, List<Symbol> F, string q0)
        {
            this.Q = Q;
            this.Sigma = Sigma;
            this.Q0 = q0;
            this.F = F;
        }

        public void AddRule(string state, string term, string nextState) { this.D.Add(new DeltaQSigma(state, term, new List<Symbol> { new Symbol(nextState) })); }

        //для пустого символа "" currStates добавляются в ArrayList - ReachableStates

        private List<Symbol> EpsClosure(List<Symbol> currStates)
        {
            Debug("Eps-Closure", currStates);
            return EpsClosure(currStates, null);
        }

        /// Все достижимые состояния из множества состояний states
        /// по правилам в которых ,LeftTerm = term
        private List<Symbol> EpsClosure(List<Symbol> currStates, List<Symbol> ReachableStates)
        {
            if (ReachableStates == null)
                ReachableStates = new List<Symbol>();
            List<Symbol> nextStates = null;
            var next = new List<Symbol>();
            int count = currStates.Count;
            // Console.WriteLine("count = " + count.ToString());
            for (int i = 0; i < count; i++)
            {
                // foreach(var i in currStates)
                nextStates = FromStateToStates(currStates[i].ToString(), "");
                // Debug("\nFrom", currStates[i].ToString());
                // Debug("NextStates", nextStates);
                // 1. если nextStates = null и это e-clouser
                if (!ReachableStates.Contains(currStates[i]))
                {
                    ReachableStates.Add(new Symbol(currStates[i].ToString()));
                    // Debug("Added currStates = ", currStates[i].ToString());
                }
                if (nextStates != null)
                {
                    // Debug("step R", currStates[i].ToString());
                    // Debug("Contains", ReachableStates.Contains(currStates[i].ToString()).ToString());
                    // 1. из одного состояния возможен переход в несколько состояний,
                    //но это состояние в множестве должно быть только один раз,
                    //то есть для него выполняется операция объединения
                    foreach (var nxt in nextStates)
                    {
                        // Debug("nxt", nxt);
                        ReachableStates.Add(nxt);
                        next.Add(nxt);
                    }
                    // Debug("RS1", ReachableStates);
                }
            }
            // Debug("RS2", ReachableStates);
            if (nextStates == null)
                return ReachableStates;
            else
                return EpsClosure(next, ReachableStates);
        }
        /// Возвращает множество достижимых состояний по символу term
        /// из currStates за один шаг
        private List<Symbol> move(List<Symbol> currStates, string term)
        {
            var ReachableStates = new List<Symbol>();
            var nextStates = new List<Symbol>();
            foreach (var s in currStates)
            {
                nextStates = FromStateToStates(s.symbol, term);
                if (nextStates != null)
                    foreach (var st in nextStates)
                        if (!ReachableStates.Contains(st))
                            ReachableStates.Add(st);
            }
            return ReachableStates;
        }

        /// Все состояния в которые есть переход из текущего состояния currState
        /// по символу term за один шаг
        private List<Symbol> FromStateToStates(string currState, string term)
        {
            var NextStates = new List<Symbol>(); //{currState};
            bool flag = false;
            foreach (var d in D)
            {
                // debugDeltaRule("AllRules", d);
                if (d.LHSQ == currState && d.LHSS == term)
                {
                    NextStates.Add(new Symbol(d.RHSQ[0].symbol));
                    // debugDeltaRule("FromStateToStates DeltaRules", d);
                    flag = true;
                }
            }
            if (flag)
                return NextStates;
            else
                return null;
        }

        private List<Symbol> config = new List<Symbol>();
        private List<DeltaQSigma> DeltaD = new List<DeltaQSigma>(); ///< правила детерминированного автомата

        private List<Symbol> Dtran(List<Symbol> currState)
        {
            List<Symbol> statesSigma = null;
            List<Symbol> newState = null;

            // for (int i = 0; i < Sigma.Count; i++) {
            foreach (var a in Sigma)
            {
                statesSigma = move(currState, a.symbol);
                Debug("move", statesSigma);

                newState = EpsClosure(statesSigma);
                Debug("Dtran " + a.symbol + " " + a.symbol, newState); // index ?
                if (SetName(newState) != null)
                    DeltaD.Add(new DeltaQSigma(SetName(currState), a.symbol, new List<Symbol> { new Symbol(SetName(newState)) }));
                debugDeltaRule("d", new DeltaQSigma(SetName(currState), a.symbol, new List<Symbol> { new Symbol(SetName(newState)) }));
                if (config.Contains(new Symbol(SetName(newState))))
                    continue;
                config.Add(new Symbol(SetName(newState)));
                Debug("config", config);

                Dtran(newState);
                Console.WriteLine("Building completed");
            }
            return null;
        }

        /// Построить Delta-правила ДКА
        public void BuildDeltaDKAutomate(FSAutomate ndka)
        {
            this.Sigma = ndka.Sigma;
            this.D = ndka.D;
            List<Symbol> currState = EpsClosure(new List<Symbol>() { new Symbol(ndka.Q0) });

            // Debug("step 1", currState);

            config.Add(new Symbol(SetName(currState)));
            // Debug("name",SetName(currState));
            Dtran(currState);
            this.Q = config;
            this.Q0 = this.Q[0].ToString();
            this.D = DeltaD;
            this.F = getF(config, ndka.F);
        }

        private List<Symbol> getF(List<Symbol> config, List<Symbol> F)
        {
            var F_ = new List<Symbol>();
            foreach (var f in F)
            {
                foreach (var name in this.config)
                {
                    if (name != null && name.symbol.Equals(f.symbol))
                    {
                        // Debug("substr",name);
                        // Debug("f", f);
                        F_.Add(name);
                    }
                }
            }
            return F_;
        }

        /// Состояние StateTo достижимо по дельта-правилам из состояния currState
        private bool ReachableStates(string currState, string StateTo)
        {
            string nextstate = currState;
            bool b = true;
            if (currState == StateTo)
                return false;
            while (b)
            {
                b = false;
                foreach (var d in this.D)
                {
                    if (nextstate == d.LHSQ)
                    {
                        if (nextstate == StateTo)
                            return true;
                        nextstate = d.RHSQ[0].symbol; // DFS
                        b = true;
                        break;
                    }
                }
            }
            return false;
        } // end ReachableStates

        private Hashtable names = new Hashtable();

        private List<Symbol> makeNames(List<Symbol> config)
        {
            var Names_ = new List<Symbol>(); // new names
            for (int i = 0; i < config.Count; i++)
            {
                Names_.Add(new Symbol(i.ToString()));
            }
            return Names_;
        }

        private List<DeltaQSigma> NameRules(List<DeltaQSigma> D)
        {
            var D_ = new List<DeltaQSigma>(); // new delta functions
            string LHSQ = null;
            var RHS = new List<Symbol>();

            foreach (var d in D)
            {
                for (int i = 0; i < this.config.Count; i++)
                {
                    if (d.LHSQ == this.config[i].ToString())
                        LHSQ = this.Q[i].symbol;
                }
                for (int i = 0; i < this.Q.Count; i++)
                {
                    if (d.RHSQ[0].symbol == this.config[i].ToString().ToString()) // DFS
                        RHS.Add(new Symbol(this.Q[i].symbol));
                }
                D_.Add(new DeltaQSigma(LHSQ, d.LHSQ, RHS));
            }
            return D_;
        }

        private string SetName(List<Symbol> list)
        {
            string line = null;
            if (list == null)
            {
                return "";
            }
            foreach (var sym in list)
                line += sym.symbol;
            return line;
            /*  Debug("key", line);
                if (names.ContainsKey(line)){
                object value = names[line];
                Console.WriteLine("value : " + names[line].ToString());
                return value.ToString();
                }
                else {
                    names.Add(line, N++);
                    return N.ToString();
                }*/
        }

        //***  Debug ***//
        public void Debug(string step, string line)
        {
            Console.Write(step + ": ");
            Console.WriteLine(line);
        }

        public void Debug(string step, List<Symbol> list)
        {
            Console.Write(step + ": ");
            if (list == null)
            {
                Console.WriteLine("null");
                return;
            }
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null)
                    Console.Write(list[i].ToString() + " ");
            Console.Write("\n");
        }

        public void Debug(List<Symbol> list)
        {
            Console.Write("{ ");
            if (list == null)
            {
                Console.WriteLine("null");
                return;
            }
            for (int i = 0; i < list.Count; i++)
                Console.Write(list[i].ToString() + " ");
            Console.Write(" }\n");
        }

        public void debugDeltaRule(string step,DeltaQSigma d) {
            Console.WriteLine(step+": ("+d.LHSQ+" , "+d.LHSS+" ) -> "+d.RHSQ[0]);
        }
        public void debugDeltaRule(string step, DeltaQSigmaGamma d)
        {
            //      Console.WriteLine(step + ": (" + d.leftNoTerm + " , " + d.leftTerm + " ) -> " + d.RightNoTerm);
        }
        public void DebugAuto()
        {
            Console.WriteLine("\nAutomate config:");
            Debug("Q", this.Q);
            Debug("Sigma", this.Sigma);
            Debug("Q0", this.Q0);
            Debug("F", this.F);
            Console.WriteLine("DeltaList:");
            foreach (var d in this.D)
                debugDeltaRule("", d);
        }
    } // end Automate

}
