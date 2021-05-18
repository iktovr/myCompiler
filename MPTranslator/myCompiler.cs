using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace MPTranslator
{
    // Используется в удалении левой рекурсии для правила (см стр 25)
    struct V_struct
    {
        public string V; // Нетерминал по которому происходит левая рекусия
        public ArrayList alpha; // Цепочка альфа, вида V -> V alpha
        public ArrayList betta; // Цепочка бетта, вида V -> betta, где бетта не начинается с V
    }
    
    abstract class Rule
    { // LeftNoTerm operator Right 
        public string LeftNoTerm = null;

        public Rule(string LeftNoTerm)
        {
            this.LeftNoTerm = LeftNoTerm;
        }

        public Rule() { }

        public string leftNoTerm
        {
            get { return LeftNoTerm; }
            set { LeftNoTerm = value; }
        }
    } // end Rule

    abstract class Automate
    {
        public ArrayList Q = null; // множество состояний
        public ArrayList Sigma = null; // алфавит
        public ArrayList DeltaList = null; //список правил перехода
        public string Q0 = null; //начальное состояние
        public ArrayList F = null; //конечное состояние

        public Automate() { }

        public Automate(ArrayList Q, ArrayList Sigma, ArrayList F, string q0)
        {
            this.Q = Q;
            this.Sigma = Sigma;
            this.Q0 = q0;
            this.F = F;
            this.DeltaList = new ArrayList();
        }

        public void AddRule(string state, string term, string nextState)
        {
            this.DeltaList.Add(new DeltaQSigma(state, term, nextState));
        }

        //для пустого символа "" currStates добавляются в ArrayList - ReachableStates

        private ArrayList EpsClosure(ArrayList currStates)
        {
            Debug("Eps-Closure", currStates);
            return EpsClosure(currStates, null);
        }

        // Все достижимые состояния из множества состояний states 
        // по правилам в которых ,LeftTerm = term
        private ArrayList EpsClosure(ArrayList currStates, ArrayList ReachableStates)
        {
            if (ReachableStates == null) ReachableStates = new ArrayList();
            ArrayList nextStates = null;
            ArrayList next = new ArrayList();
            int count = currStates.Count;
            //Console.WriteLine("count = " + count.ToString());
            for (int i = 0; i < count; i++)
            {
                nextStates = FromStateToStates(currStates[i].ToString(), "");
                //Debug("\nFrom", currStates[i].ToString());
                //Debug("NextStates", nextStates);
                // 1. если nextStates = null и это e-clouser  
                if (!ReachableStates.Contains(currStates[i].ToString()))
                {
                    ReachableStates.Add(currStates[i].ToString());
                    //Debug("Added currStates = ", currStates[i].ToString());
                }
                if (nextStates != null)
                {
                    //Debug("step R", currStates[i].ToString());
                    //Debug("Contains", ReachableStates.Contains(currStates[i].ToString()).ToString());
                    //1. из одного состояния возможен переход в несколько состояний,
                    //но это состояние в множестве должно быть только один раз,
                    //то есть для него выполняется операция объединения
                    foreach (string nxt in nextStates)
                    {
                        //Debug("nxt", nxt);
                        ReachableStates.Add(nxt);
                        next.Add(nxt);
                    }
                    //Debug("RS1", ReachableStates);
                }
            }
            //Debug("RS2", ReachableStates);
            if (nextStates == null) return ReachableStates;
            else return EpsClosure(next, ReachableStates);
        }
        //возвращает множество достижимых состояний по символу term 
        //из currStates за один шаг
        private ArrayList move(ArrayList currStates, string term)
        {
            ArrayList ReachableStates = new ArrayList();
            ArrayList nextStates = new ArrayList();
            foreach (string s in currStates)
            {
                nextStates = FromStateToStates(s, term);
                if (nextStates != null)
                    foreach (string st in nextStates)
                        if (!ReachableStates.Contains(st))
                            ReachableStates.Add(st);
            }
            return ReachableStates;
        }

        // Все состояния в которые есть переход из текущего состояния currState 
        // по символу term за один шаг
        private ArrayList FromStateToStates(string currState, string term)
        {
            ArrayList NextStates = new ArrayList();//{currState};
            bool flag = false;
            foreach (DeltaQSigma d in DeltaList)
            {
                //debugDeltaRule("AllRules", d);
                if (d.LeftNoTerm == currState && d.leftTerm == term)
                {
                    NextStates.Add(d.RightNoTerm);
                    //debugDeltaRule("FromStateToStates DeltaRules", d);
                    flag = true;
                }
            }
            if (flag) return NextStates;
            else return null;
        }

        private ArrayList config = new ArrayList();
        private ArrayList DeltaD = new ArrayList();//правила детерминированного автомата

        private ArrayList Dtran(ArrayList currState)
        {
            ArrayList statesSigma = null;
            ArrayList newState = null;

            for (int i = 0; i < Sigma.Count; i++)
            {
                statesSigma = move(currState, Sigma[i].ToString());

                Debug("move", statesSigma);

                newState = EpsClosure(statesSigma);
                Debug("Dtran " + i.ToString() + " " + Sigma[i].ToString(), newState);
                if (SetName(newState) != null)
                    DeltaD.Add(new DeltaQSigma(SetName(currState), Sigma[i].ToString(), SetName(newState)));
                debugDeltaRule("d", new DeltaQSigma(SetName(currState), Sigma[i].ToString(), SetName(newState)));
                if (config.Contains(SetName(newState)))
                    continue;
                config.Add(SetName(newState));
                Debug("config", config);

                Dtran(newState);
                Console.WriteLine("Building completed");
            }
            return null;
        }

        //построить Delta-правила ДКА
        public void BuildDeltaDKAutomate(myAutomate ndka)
        {
            this.Sigma = ndka.Sigma;
            this.DeltaList = ndka.DeltaList;
            ArrayList currState = EpsClosure(new ArrayList() { ndka.Q0 });

            //Debug("step 1", currState);

            config.Add(SetName(currState));
            //Debug("name",SetName(currState));
            Dtran(currState);
            this.Q = config;
            this.Q0 = this.Q[0].ToString();
            this.DeltaList = DeltaD;
            this.F = getF(config, ndka.F);

          /*  this.Q = makeNames(config);
              this.Q0 = this.Q[0].ToString();
              this.DeltaList = NameRules(DeltaD);
              this.F = makeNames(getF(config, ndka.F));*/
        }

        private ArrayList getF(ArrayList config, ArrayList F)
        {
            ArrayList newF = new ArrayList();
            foreach (string f in F)
            {
                foreach (string name in this.config)
                {
                    if (name != null && name.Contains(f))
                    {
                        //Debug("substr",name);
                        //Debug("f", f);
                        newF.Add(name);
                    }
                }
            }
            return newF;
        }

        //состояние StateTo достижимо по дельта-правилам из состояния currState
        private bool ReachableStates(string currState, string StateTo)
        {
            string nextstate = currState;
            bool b = true;
            if (currState == StateTo) return false;
            while (b)
            {
                b = false;
                foreach (DeltaQSigma d in this.DeltaList)
                {
                    if (nextstate == d.leftNoTerm)
                    {
                        if (nextstate == StateTo) return true;
                        nextstate = d.RightNoTerm;
                        b = true;
                        break;
                    }
                }
            }
            return false;
        } //end ReachableStates

        private Hashtable names = new Hashtable();

        private ArrayList makeNames(ArrayList config)
        {
            ArrayList NewNames = new ArrayList();
            for (int i = 0; i < config.Count; i++)
            {
                NewNames.Add(i.ToString());
            }
            return NewNames;
        }

        private ArrayList NameRules(ArrayList DeltaRules)
        {
            ArrayList newRules = new ArrayList();
            string newLeftNoTerm = null;

            string newRight = null;

            foreach (DeltaQSigma d in DeltaRules)
            {
                for (int i = 0; i < this.config.Count; i++)
                {
                    if (d.LeftNoTerm == this.config[i].ToString())
                        newLeftNoTerm = this.Q[i].ToString();
                }
                for (int i = 0; i < this.Q.Count; i++)
                {
                    if (d.rightNoTerm == this.config[i].ToString().ToString())
                        newRight = this.Q[i].ToString();
                }
                newRules.Add(new DeltaQSigma(newLeftNoTerm, d.LeftTerm, newRight));
            }
            return newRules;
        }

        private string SetName(ArrayList list)
        {
            string line = null;
            if (list == null) { return ""; }
            for (int i = 0; i < list.Count; i++)
                line += list[i].ToString();
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

        ///***  Debug ***///        
        public void Debug(string step, string line)
        {
            Console.Write(step + ": ");
            Console.WriteLine(line);
        }

        public void Debug(string step, ArrayList list)
        {
            Console.Write(step + ": ");
            if (list == null) { Console.WriteLine("null"); return; }
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null)
                    Console.Write(list[i].ToString() + " ");
            Console.Write("\n");
        }

        public void Debug(ArrayList list)
        {
            Console.Write("{ ");
            if (list == null) { Console.WriteLine("null"); return; }
            for (int i = 0; i < list.Count; i++)
                Console.Write(list[i].ToString() + " ");
            Console.Write(" }\n");
        }

        public void debugDeltaRule(string step, DeltaQSigma d)
        {
            Console.WriteLine(step + ": (" + d.leftNoTerm + " , " + d.leftTerm + " ) -> " + d.RightNoTerm);
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
            foreach (DeltaQSigma d in this.DeltaList)
                debugDeltaRule("", d);
        }
    } //end Automate

    abstract class Grammar
    {
        public string S0 = null; //начальный символ
        public ArrayList T = null; //список терминалов
        public ArrayList V = null; //список нетерминалов
        public ArrayList Prules = null; //список правил порождения

        public Grammar() { }

        public Grammar(ArrayList T, ArrayList V, string S0)
        {
            this.T = T;
            this.V = V;
            this.S0 = S0;
        }

        abstract public string Execute(); // abstract

        public void AddRule(string LeftNoTerm, ArrayList right)
        {
            this.Prules.Add(new Prule(LeftNoTerm, right));
        }

        //Lina Veltman course work
        public myAutomate Transform()
        {
            ArrayList Q = this.V;
            Q.Add("qf");
            string q0 = this.S0;
            ArrayList F = new ArrayList();
            //Конструируем множество заключительных состояний
            foreach (Prule p in this.Prules)
            {
                //Если начальный символ переходит в конечную цепочку,
                //то в множество F добавляется начальный символ S0 и состояние qf
                // F = {S0, qf}
                if (p.LeftNoTerm.Contains("S0") && p.RightChain.Contains("e"))
                {
                    F = new ArrayList { p.LeftNoTerm, "qf" };
                    break;
                }
                //Иначе F = {qf} множество F(конечных состояний) будет состоять из одного состояния qf
                else if (p.LeftNoTerm.Contains("S0"))
                {
                    F = new ArrayList { "qf" };
                    break;
                }
            }
            
            //Конструируем конечный автомат
            myAutomate KA = new myAutomate(Q, this.T, F, q0);
            bool flag = true;

            foreach (Prule p in this.Prules)
            {
                //Если существует правило порождения,
                //в котором из начального символа существует переход в пустую цепочку,
                //то создаем правило (S0, "e", "qf")
                if (flag && p.LeftNoTerm.Contains("S0") && p.RightChain.Contains("e"))
                {
                    KA.AddRule(p.LeftNoTerm, "e", "qf");
                    flag = false;
                }
                //Проходим по всем входным символам
                foreach (string t in this.T)
                {
                    //Если справа есть символ и этот символ терминал,
                    //то добавляем правило (Нетерминал -> (Терминал,  "qf"))
                    if (p.RightChain.Contains(t) && NoTermReturn(p.RightChain) == null)
                        KA.AddRule(p.LeftNoTerm, t, "qf");
                    //Если справа есть символ и этот символ нетерминал,
                    //то добавляем правило (Нетерминал -> (Терминал, Нетерминал))
                    else if (p.RightChain.Contains(t) && NoTermReturn(p.RightChain) != null)
                        KA.AddRule(p.LeftNoTerm, t, NoTerminal(p.RightChain));
                }
            }
            return KA;
        }

        //определение множествa производящих нетерминальных символов
        private ArrayList producingSymb()
        {
            ArrayList Vp = new ArrayList();
            foreach (Prule p in this.Prules)
            {
                bool flag = true;
                foreach (string t in this.T)
                    if (p.RightChain.Contains(t))
                        flag = false;
                if (!flag && !Vp.Contains(p.leftNoTerm)) Vp.Add(p.leftNoTerm);
            }
            return Vp;
        }

        //определение множества достижимых символов за 1 шаг
        private ArrayList ReachableByOneStep(string state)
        {
            ArrayList Reachable = new ArrayList() { state };
            ArrayList tmp = new ArrayList();
            int flag = 0;
            foreach (Prule p in this.Prules)
            {
                if (p.LeftNoTerm == state)
                    for (int i = 0; i < p.rightChain.Count; i++)
                        for (int j = 0; j < Reachable.Count; j++)
                            if (p.rightChain[i].ToString() != Reachable[j].ToString())
                            {
                                tmp.Add(p.rightChain[i].ToString());// Debug(tmp);Console.WriteLine("");
                                break;
                            }
            }
            foreach (string s in tmp)
            {
                flag = 0;
                for (int i = 0; i < Reachable.Count; i++)
                    if (Reachable[i].ToString() == s)
                        flag = 1;
                if (flag == 0) Reachable.Add(s);
            }
            return Reachable;
        }

        //определение множества достижимых символов
        private ArrayList Reachable(string StartState)
        {
            ArrayList Vr = new ArrayList() { this.S0 };
            ArrayList nextStates = ReachableByOneStep(StartState);
            Debug("NEXT", nextStates);
            ArrayList NoTermByStep = NoTermReturn(nextStates);
            Debug("NoTermByStep", NoTermByStep);
            Vr = Unify(Vr, NoTermByStep);
            foreach (string NoTerm in NoTermByStep)
            {
                Vr = Unify(Vr, ReachableByOneStep(NoTerm));
            }
            return Vr;
        }

        //удаление бесполезных символов
        public myGrammar unUsefulDelete()
        {
            Console.WriteLine("\t\tDeleting unuseful symbols");
            Console.WriteLine("Executing: ");
            ArrayList Vp = new ArrayList();
            ArrayList Vr = new ArrayList();
            Vr.Add(this.S0);
            ArrayList Pp = new ArrayList();
            ArrayList P1 = new ArrayList(this.Prules);
            bool flag = false, noadd = false;
            // Создааем множество порождающих символов и одновременно непроизводящие правила
            do
            {
                flag = false;
                foreach (Prule p in P1)
                {
                    noadd = false;
                    //DebugPrule(p);
                    if (p.RightChain == null || p.RightChain.Contains(""))
                    {
                        Pp.Add(p);
                        if (!Vp.Contains(p.LeftNoTerm))
                        {
                            Vp.Add(p.LeftNoTerm);
                        }
                        P1.Remove(p);
                        flag = true;
                        break;
                    }
                    else
                    {
                        foreach (string t in p.RightChain)
                        {
                            if (!this.T.Contains(t) && !Vp.Contains(t))
                            {
                                //Console.WriteLine(t);
                                noadd = true;
                                break;
                            }
                        }
                        if (!noadd)
                        {
                            Pp.Add(p);
                            if (!Vp.Contains(p.LeftNoTerm))
                            {
                                Vp.Add(p.LeftNoTerm);
                            }
                            P1.Remove(p);
                            flag = true;
                            break;
                        }
                    }
                }
            } while (flag);
            
            Debug("Vp", Vp);
            P1.Clear();
            if (!Vp.Contains(this.S0))
            {
                return new myGrammar(new ArrayList(), new ArrayList(), new ArrayList(), this.S0);
            }
            ArrayList T1 = new ArrayList();
            //Создаем множество достижимых символов
            do
            {
                flag = false;
                foreach (Prule p in Pp)
                {
                    if (Vr.Contains(p.leftNoTerm))
                    {
                        foreach (string t in p.RightChain)
                        {
                            if (!Vr.Contains(t))
                            {
                                Vr.Add(t);
                                //noadd = true;
                            }
                        }
                        P1.Add(p);
                        Pp.Remove(p);
                        flag = true;
                        break;
                    }
                }
            } while (flag);
            
            Debug("Vr", Vr);
            Vp.Clear();
            // Обновляем множества терминалов и нетерминалов
            foreach (string t in Vr)
            {
                if (this.T.Contains(t))
                {
                    T1.Add(t);
                }
                else if (this.V.Contains(t))
                {
                    Vp.Add(t);
                }
            }
            Debug("T1", T1);
            Debug("V1", Vp);
            Console.WriteLine("\tUnuseful symbols have been deleted");
            return new myGrammar(T1, Vp, P1, this.S0);
        }


        //public myGrammar unUsefulDelete()
        //{
        //    Console.WriteLine("\t\tDeleting unuseful symbols");
        //    Console.WriteLine("Executing: ");
        //    //построить множество производящих символов
        //    ArrayList Vp = producingSymb(); Debug("Vp", Vp);
        //    ArrayList P1 = new ArrayList();
        //    ArrayList TorVp = Unify(Vp, this.T); Debug("TorVp", TorVp);
        //    Console.WriteLine("\nP1 building:\n");
        //    foreach (Prule p in this.Prules)
        //    {
        //        DebugPrule(p);
        //        ArrayList SymbInRule = SymbInRules(p);//Debug("SinR", SymbInRule); 
        //        bool flag = true;
        //        foreach (string s in SymbInRule)
        //            if (!TorVp.Contains(s) || TermReturn(p.RightChain) == null)
        //                flag = false;
        //        if (flag)
        //        {
        //            P1.Add(p); Console.WriteLine("Added");
        //        }
        //    }
        //    //построить множество достижимых символов
        //    ArrayList Vr = Reachable(this.S0); Debug("Vr", Vr);
        //    ArrayList T1 = intersection(this.T, Vr); Debug("T1", Vr);
        //    ArrayList V1 = intersection(Vr, this.V); Debug("V1", Vr);
        //    ArrayList P2 = new ArrayList();
        //    Console.WriteLine("\nP2 building:\n");
        //    foreach (Prule p in P1)
        //    {
        //        DebugPrule(p);
        //        ArrayList SymbInRule = SymbInRules(p);
        //        bool flag = true;
        //        foreach (string s in SymbInRule)
        //            if (!Vr.Contains(s))
        //                flag = false;
        //        if (flag)
        //        {
        //            P2.Add(p); Console.WriteLine("Added");
        //        }
        //    }
        //    Console.WriteLine("Unuseful symbols have been deleted");
        //    return new myGrammar(T1, V1, P2, this.S0);
        //}

        //построение множества укорачивающих нетерминалов
        private ArrayList ShortNoTerm()
        {
            ArrayList Ve = new ArrayList();
            foreach (Prule p in this.Prules)
            {
                if (p.rightChain.Contains(""))
                    Ve.Add(p.leftNoTerm);
            }
            int i = 0;///!!!
           // if (Ve.Count == 0) return null;

/*                                 
          foreach (string v in Ve) { 
           //  if ( FromWhat(vg()) != null) {
                Ve = Unify(Ve, FromWhat(v.ToString()));
           //  } 
           }          
 */           
            if (Ve.Count != 0)
                //Console.WriteLine("  {0}",Ve.Count);                
                while ( (FromWhat(Ve[i].ToString()) != null) && (Ve.Count < i ) ) 
                {
                    Ve = Unify(Ve, FromWhat(Ve[0].ToString()));
                    i++;                    
                }
            Debug("Ve",Ve);
  
            return Ve;
        }

        //удаление эпсилон правил
        public myGrammar EpsDelete()
        {
            Console.WriteLine("\tDelete e-rules:");
            Console.WriteLine("Executing:");
            ArrayList Erule = new ArrayList();
            ArrayList Ps = new ArrayList(this.Prules);
            //ArrayList NoTerm = new ArrayList();
            Console.WriteLine("e-rules:");
            //находим множество е-правил
            foreach (Prule p in this.Prules)
            {
                if (p.RightChain.Contains(""))
                {
                    DebugPrule(p);
                    Erule.Add(p);
                    Ps.Remove(p);
                }
            }
            //определяем множество неукорачивающихся символов
            ArrayList NoTerms = new ArrayList();

            foreach (Prule p in Erule)
            {
                if (!NoTerms.Contains(p.LeftNoTerm))
                {
                    NoTerms.Add(p.LeftNoTerm);
                }
            }
            bool flag = false, noadd = false;
            do
            {
                flag = false;
                foreach (Prule p in Ps)
                {
                    noadd = false;
                    //DebugPrule(p);
                    foreach (string t in p.RightChain)
                    {
                        if (!NoTerms.Contains(t))
                        {
                            noadd = true;
                            break;
                        }
                    }
                    if (!noadd)
                    {
                        if (!NoTerms.Contains(p.LeftNoTerm))
                        {
                            NoTerms.Add(p.LeftNoTerm);
                        }
                        flag = true;
                        Ps.Remove(p);
                        break;
                    }
                }
            } while (flag);
            Debug("NoShortNoTerms", NoTerms);
            Ps.Clear();
            //string s;
            //Удаляем е-правила и создаем новые в соответствии с алгоритмом
            foreach (Prule p in this.Prules)
            {
                if (Erule.Contains(p))
                {
                    continue;
                }
                Ps.Add(p);
                for (int i = 0; i < p.RightChain.Count; ++i)
                {
                    string t = p.RightChain[i].ToString();
                    if (NoTerms.Contains(t))
                    {
                        //s = t;
                        ArrayList NR = new ArrayList(p.RightChain);
                        NR.RemoveAt(i);
                        Ps.Add(new Prule(p.LeftNoTerm, NR));
                    }
                }
            }
            //проверяем есть ли порождение е из нач символа
            if (NoTerms.Contains(this.S0))
            {
                ArrayList V1 = new ArrayList(this.V);
                V1.Add("S1");
                Ps.Add(new Prule("S1", new ArrayList() { this.S0 }));
                Ps.Add(new Prule("S1", new ArrayList() { "" }));
                Debug("V1", V1);
                Console.WriteLine("\te-rules have been deleted!");
                return new myGrammar(this.T, V1, Ps, "S1");
            }
            else
            {
                Debug("V1:", this.V);
                Console.WriteLine("\te-rules have benn deleted!");
                return new myGrammar(this.T, this.V, Ps, this.S0);
            }
        }

        //        public myGrammar EpsDelete()
        //        {
        //            Console.WriteLine("\t\tDeleting epsylon rules");
        //            Console.WriteLine("Executing: ");
        //            ArrayList Ve = ShortNoTerm(); Debug("Ve", Ve);
        //            ArrayList P1 = new ArrayList();
        //            ArrayList V1 = this.V;
        //            foreach (Prule p in Prules)
        //            {
        //                if (!ContainEps(p))
        //                {
        //                    //DebugPrule(p);
        //                    P1.Add(p);
        //                    Prule p1 = new Prule(p.leftNoTerm, TermReturn(p.rightChain));
        ////                    DebugPrule(p1);
        //                    if (p.rightChain.Count != 1)
        //                    {
        //                        Console.WriteLine("No contain");
        //                        P1.Add(p1);
        //                    }
        //                    else Console.WriteLine("Contain");
        //                }
        //            }
        //            if (Ve.Contains(this.S0))
        //            {
        //                V1.Add("S1");
        //                P1.Add(new Prule("S1", new ArrayList() { this.S0 }));
        //                P1.Add(new Prule("S1", new ArrayList() { "" }));
        //                return new myGrammar(this.T, V1, P1, "S1");
        //            }
        //            else
        //                return new myGrammar(this.T, V1, P1, this.S0);
        //        }

        //удаление цепных правил

        public myGrammar ChainRuleDelete()
        {
            Console.WriteLine("\tChainRule Deleting:");
            Console.WriteLine("Executing: ");
            //  Поиск цепных пар
            List<List<string>> chain_pair_list = new List<List<string>>();
            ArrayList chain_rules = new ArrayList();

            foreach (string v in this.V)
            {
                List<string> chain_pair = new List<string>();
                chain_pair.Add(v);
                chain_pair.Add(v);
                chain_pair_list.Add(chain_pair);
            }
            Console.WriteLine("ChainRules:");
            foreach (Prule p in this.Prules)
            {
                if (TermReturn(p.RightChain) == null && NoTermReturn(p.RightChain) != null && NoTermReturn(p.RightChain).Count == 1)
                {
                    chain_rules.Add(p);
                    DebugPrule(p);
                    for (int i = 0; i < chain_pair_list.Count; ++i)
                    {
                        List<string> chain_pair = new List<string>(chain_pair_list[i]);
                        if (chain_pair[1] == p.LeftNoTerm)
                        {
                            List<string> chain_pair1 = new List<string>();
                            chain_pair1.Add(chain_pair[0]);
                            chain_pair1.Add(NoTermReturn(p.RightChain)[0].ToString());
                            chain_pair_list.Add(chain_pair1);
                        }
                    }
                }
            }

            Console.WriteLine("Deleting...");

            //  Работа оснавная

            ArrayList P = new ArrayList();

            foreach (List<string> chain_pair in chain_pair_list)
            {
                foreach (Prule p in this.Prules)
                {
                    if (p.LeftNoTerm == chain_pair[1] && !(TermReturn(p.RightChain) == null && NoTermReturn(p.RightChain) != null && NoTermReturn(p.RightChain).Count == 1))
                    {
                        Prule P_1 = new Prule(chain_pair[0], p.RightChain);
                        if (!P.Contains(P_1))
                        {
                            P.Add(P_1);
                        }
                    }
                }
            }

            /* foreach (Prule chr in chain_rules) 
            {
                P.Remove(chr);
            }
            */
            Console.WriteLine("\tChainrules have been deleted;");
            return new myGrammar(this.T, this.V, P, this.S0);
        }


        //public myGrammar ChainRuleDelete()
        //{
        //    Console.WriteLine("\t\tDeleting chain rules");
        //    Console.WriteLine("Executing: ");
        //    ArrayList NoChainRules = new ArrayList();
        //    ArrayList ChainRules = new ArrayList();
        //    foreach (Prule p in this.Prules)
        //    {
        //        if (TermReturn(p.rightChain) != null)
        //            NoChainRules.Add(p);
        //        else
        //            ChainRules.Add(p);
        //    }

        //    Console.Write("Chine Rules: ");
        //    if (ChainRules.Count == 0) Console.WriteLine("null");
        //    else
        //    {
        //        Console.WriteLine("");
        //        foreach (Prule p in ChainRules) DebugPrule(p);

        //        foreach (Prule chrule in ChainRules)
        //            foreach (Prule p in NoChainRules)
        //            {
        //                if (p.RightChain.Contains(chrule.leftNoTerm))
        //                    for (int i = 0; i < p.RightChain.Count; i++)
        //                    {
        //                        if (p.RightChain[i].ToString() == chrule.leftNoTerm)
        //                            p.RightChain[i] = chrule.RightChain[0].ToString();
        //                    }
        //            }
        //    }
        //    ArrayList P = new ArrayList();
        //    foreach (Prule p in NoChainRules)
        //        if (!P.Contains(p)) P.Add(p);
        //    return new myGrammar(this.T, this.V, P, this.S0);
        //}

        //удаление левой рекурсии

        public myGrammar LeftRecursDelete()
        {
            Console.WriteLine("\tLeft Recursion delete:");
            Console.WriteLine("Executing: ");
            /*  ArrayList ules = new ArrayList();
             foreach(Prules p in this.Prules){
                 if(p.LeftNoTerm == p.RightChain[0].ToString){
                     ules.Add(p);
                 }
             } */
            ArrayList P = new ArrayList();
            ArrayList V1 = new ArrayList(this.V);
            ArrayList Vr = new ArrayList();
            //ищем рекурсивные правила
            Console.WriteLine("Rules with Recursion:");
            foreach (Prule p in this.Prules)
            {
                if (p.LeftNoTerm == p.RightChain[0].ToString())
                {
                    DebugPrule(p);
                    if (!Vr.Contains(p.LeftNoTerm))
                    {
                        Vr.Add(p.LeftNoTerm);
                    }
                }
            }
            foreach (Prule p in this.Prules)
            {
                if (!Vr.Contains(p.LeftNoTerm))
                {
                    P.Add(p);
                }
            }
            //преобразуем их в новые без левой рекурсии
            ArrayList v_struct_ar = new ArrayList();

            foreach (string v in Vr)
            {
                V_struct v_struct;
                v_struct.alpha = new ArrayList();
                v_struct.betta = new ArrayList();
                v_struct.V = v;
                foreach (Prule r in this.Prules)
                {
                    if (v == r.LeftNoTerm)
                    {
                        if (r.RightChain[0].ToString() == v)
                        {
                            ArrayList alpha_help = new ArrayList();
                            for (int i = 1; i < r.RightChain.Count; i++)
                            {
                                alpha_help.Add(r.RightChain[i]);
                            }
                            if (alpha_help.Count > 0)
                                v_struct.alpha.Add(alpha_help);
                        }
                        else
                        {
                            if (r.RightChain.Count > 0)
                                v_struct.betta.Add(r.RightChain);
                        }
                    }
                }
                v_struct_ar.Add(v_struct);
            }

            foreach (V_struct v_struct in v_struct_ar)
            {
                string new_v = v_struct.V + "'";
                V1.Add(new_v);

                foreach (ArrayList betta_help in v_struct.betta)
                {
                    P.Add(new Prule(v_struct.V, betta_help));
                    ArrayList betta_pravila = new ArrayList();
                    for (int i = 0; i < betta_help.Count; i++)
                    {
                        betta_pravila.Add(betta_help[i]);
                    }
                    betta_pravila.Add(new_v);
                    P.Add(new Prule(v_struct.V, betta_pravila));
                }

                foreach (ArrayList alpha_help in v_struct.alpha)
                {
                    P.Add(new Prule(new_v, alpha_help));
                    ArrayList alpha_pravila = new ArrayList();
                    for (int i = 0; i < alpha_help.Count; i++)
                    {
                        alpha_pravila.Add(alpha_help[i]);
                    }
                    alpha_pravila.Add(new_v);
                    P.Add(new Prule(new_v, alpha_pravila));
                }
            }
            Debug("V1", V1);
            Console.WriteLine("\tLeft Recursion have been deleted!");

            return new myGrammar(this.T, V1, P, this.S0);
        }

            //public myGrammar LeftRecursDelete()
            //{
            //    Console.WriteLine("\t\tDeleting left Recurs");
            //    Console.WriteLine("Executing: ");
            //    ArrayList LeftRecurs = new ArrayList();
            //    ArrayList P1 = new ArrayList();
            //    int i = 0;
            //    foreach (Prule p in this.Prules)
            //    {
            //        if (p.rightChain.Contains(p.leftNoTerm) && p.RightChain[0].ToString() == p.leftNoTerm)
            //        {
            //            DebugPrule(p);
            //            LeftRecurs.Add(p);
            //        }
            //        else P1.Add(p);
            //    }
            //    foreach (Prule p in LeftRecurs)
            //    {
            //        ArrayList right = new ArrayList();
            //        string alfa = "";
            //        for (int j = 1; j < p.RightChain.Count; j++)
            //        {
            //            alfa += p.RightChain[j].ToString();
            //        }
            //        Debug("alfa", alfa);
            //        if (alfa == null) { }
            //        else
            //        {
            //            P1.Add(new Prule(p.leftNoTerm, new ArrayList() { "S" + i.ToString() }));
            //            this.V.Add("S" + i.ToString());
            //            for (int k = 0; k < alfa.Length; k++)
            //            {
            //                right.Add(alfa.Substring(k, 1));
            //            }
            //            right.Add("S" + i.ToString());
            //            P1.Add(new Prule("S" + i.ToString(), right));
            //        }
            //        i++;
            //    }
            //    return new myGrammar(this.T, this.V, P1, this.S0);
            //}



            // **   Debug   **
            public void DebugPrules()
        {
            Console.WriteLine("Prules:");
            foreach (Prule p in this.Prules)
            {
                string right = "";
                for (int i = 0; i < p.rightChain.Count; i++)
                    right += p.rightChain[i].ToString();
                Console.WriteLine(p.leftNoTerm + " -> " + right);
            }
        }

        public void DebugPrule(Prule p)
        {
            string right = "";
            for (int i = 0; i < p.rightChain.Count; i++)
                right += p.rightChain[i].ToString();
            Console.WriteLine(p.leftNoTerm + " -> " + right + " ");
        }

        public void Debug(string step, ArrayList list)
        {
            Console.Write(step + " : ");
            if (list == null) Console.WriteLine("null");
            else
                for (int i = 0; i < list.Count; i++)
                    Console.Write(list[i].ToString() + " ");
            Console.WriteLine("");
        }

        public void Debug(string step, string line)
        {
            Console.Write(step + " : ");
            Console.WriteLine(line);
        }

        //откуда можем прийти в состояние
        private ArrayList FromWhat(string state)
        {
            ArrayList from = new ArrayList();
            bool flag = true;
            foreach (Prule p in this.Prules)
            {
                if (p.RightChain.Contains(state))
                {
                    from.Add(p.leftNoTerm);
                    flag = false;
                }
            }
            if (flag) return null;
            else return from;
        }

        //объединение множеств A or B
        private ArrayList Unify(ArrayList A, ArrayList B)
        {
            ArrayList unify = A;
            foreach (string s in B)
                if (!A.Contains(s))
                    unify.Add(s);
            return unify;
        }

        //пересечение множеств A & B
        private ArrayList intersection(ArrayList A, ArrayList B)
        {
            ArrayList intersection = new ArrayList();
            foreach (string s in A)
                if (B.Contains(s))
                    intersection.Add(s);
            return intersection;
        }

        //Нетерминальные символы из массива
        //private ArrayList NoTermReturn(ArrayList array)
        //{
        //    ArrayList NoTerm = new ArrayList();
        //    foreach (string s in array)
        //        if (this.V.Contains(s))
        //            NoTerm.Add(s);
        //    return NoTerm;
        //}

        private ArrayList NoTermReturn(ArrayList array)
        {
            ArrayList NoTerm = new ArrayList();
            bool flag = true;//added
            foreach (string s in array)
                if (this.V.Contains(s))
                {
                    flag = false;//added
                    NoTerm.Add(s);
                }
            if (flag) return null;//added
            else return NoTerm;
        }
        //added for KA by Lina Veltman
        private string NoTerminal(ArrayList array)
        {
            string NoTermin = "";
            foreach (string s in array)
            {
                if (this.V.Contains(s))
                    NoTermin = s;
            }
            return NoTermin;
        }


        //терминальные символы из массива
        private ArrayList TermReturn(ArrayList A)
        {
            ArrayList Term = new ArrayList();
            bool flag = true;
            foreach (string t in this.T)
                if (A.Contains(t))
                {
                    flag = false;
                    Term.Add(t);
                }
            if (flag) return null;
            else return Term;
        }

        //все символы в правиле
        private ArrayList SymbInRules(Prule p)
        {
            ArrayList SymbInRules = new ArrayList() { p.LeftNoTerm };
            for (int i = 0; i < p.rightChain.Count; i++)
                SymbInRules.Add(p.rightChain[i].ToString());
            return SymbInRules;
        }

        //проверка пустоты правой цепочки
        private bool ContainEps(Prule p)
        {
            if (p.rightChain.Contains("")) return true;
            return false;
        }
    }//end abstract class Grammar
}