using System;
using System.Collections.Generic;
using System.Collections;
using Translator;

namespace Processor.AbstractGrammar
{
    /// Используется в удалении левой рекурсии для правила (см стр 25)
    struct V_struct
    {
        public string V; ///< Нетерминал левой рекурсим
        public List<Symbol> alpha; ///< Цепочка альфа, вида V -> V alpha рекурси
        public List<Symbol> betta; ///< Цепочка бетта, вида V -> betta, где бетта не начинается с V
    }
    public abstract class AGrammar
    {
        public Symbol S0 = null; ///< Начальный символ
        public List<Symbol> T = null; ///< Множество терминалов
        public List<Symbol> V = null; ///< Множество нетерминалов
        public List<Production> P = null; ///< Множество правил продукций (порождений)

        public AGrammar() {}

        public AGrammar(List<Symbol> T, List<Symbol> V, string S0)
        {
            this.T = T;
            this.V = V;
            this.S0 = new Symbol(S0);
            this.P = new List<Production>();
        }

        abstract public void Parse();

        public void AddRule(string LeftNoTerm, List<Symbol> RHS) { this.P.Add(new Production(new Symbol(LeftNoTerm), RHS)); }

        public FSAutomate Transform()
        {
            var Q = this.V;
            Q.Add(new Symbol("qf"));
            var q0 = this.S0;
            var F = new List<Symbol>();
            //Конструируем множество заключительных состояний
            foreach (var p in this.P)
            {
                //Если начальный символ переходит в конечную цепочку,
                //то в множество F добавляется начальный символ S0 и состояние qf
                // F = {S0, qf}
                if (p.LHS.symbol.Contains("S0") && p.RHS.Contains(new Symbol("e")))
                {
                    F = new List<Symbol> { p.LHS, new Symbol("qf") };
                    break;
                }
                //Иначе F = {qf} множество F(конечных состояний) будет состоять из одного состояния qf
                else if (p.LHS.symbol.Contains("S0"))
                {
                    F = new List<Symbol> { new Symbol("qf") };
                    break;
                }
            }

            //Конструируем конечный автомат
            FSAutomate KA = new FSAutomate(Q, this.T, F, q0.symbol);
            bool flag = true;

            foreach (var p in this.P)
            {
                //Если существует правило порождения,
                //в котором из начального символа существует переход в пустую цепочку,
                //то создаем правило (S0, "e", "qf")
                if (flag && p.LHS.symbol.Contains("S0") && p.RHS.Contains(new Symbol("e")))
                {
                    KA.AddRule(p.LHS.symbol, "e", "qf");
                    flag = false;
                }
                //Проходим по всем входным символам
                foreach (var t in this.T)
                {
                    //Если справа есть символ и этот символ терминал,
                    //то добавляем правило (Нетерминал -> (Терминал,  "qf"))
                    if (p.RHS.Contains(t) && NoTermReturn(p.RHS) == null)
                        KA.AddRule(p.LHS.symbol, t.symbol, "qf");
                    //Если справа есть символ и этот символ нетерминал,
                    //то добавляем правило (Нетерминал -> (Терминал, Нетерминал))
                    else if (p.RHS.Contains(t) && NoTermReturn(p.RHS) != null)
                        KA.AddRule(p.LHS.symbol, t.symbol, NoTerminal(p.RHS));
                }
            }
            return KA;
        }

        /// Определение множествa производящих нетерминальных символов
        private List<Symbol> producingSymb()
        {
            var Vp = new List<Symbol>();
            foreach (var p in this.P)
            {
                bool flag = true;
                foreach (var t in this.T)
                    if (p.RHS.Contains(t))
                        flag = false;
                if (!flag && !Vp.Contains(p.LHS))
                    Vp.Add(p.LHS);
            }
            return Vp;
        }

        /// Определение множества достижимых символов за 1 шаг
        private List<Symbol> ReachableByOneStep(string state)
        {
            var Reachable = new List<Symbol>() { new Symbol(state) };
            var tmp = new List<Symbol>();
            int flag = 0;
            foreach (var p in this.P)
            {
                if (p.LHS.ToString() == state)
                    for (int i = 0; i < p.RHS.Count; i++)
                        for (int j = 0; j < Reachable.Count; j++)
                            if (p.RHS[i].ToString() != Reachable[j].ToString())
                            {
                                tmp.Add(p.RHS[i]); // Debug(tmp);Console.WriteLine("");
                                break;
                            }
            }
            foreach (var s in tmp)
            {
                flag = 0;
                for (int i = 0; i < Reachable.Count; i++)
                    if (Reachable[i].symbol == s.symbol)
                        flag = 1;
                if (flag == 0)
                    Reachable.Add(s);
            }
            return Reachable;
        }

        /// Определение множества достижимых символов
        private List<Symbol> Reachable(string StartState)
        {
            var Vr = new List<Symbol>() { this.S0 };
            var nextStates = ReachableByOneStep(StartState);
            Debug("NEXT", nextStates);
            var NoTermByStep = NoTermReturn(nextStates);
            Debug("NoTermByStep", NoTermByStep);
            Vr = Unify(Vr, NoTermByStep);
            foreach (var NoTerm in NoTermByStep)
            {
                Vr = Unify(Vr, ReachableByOneStep(NoTerm.symbol));
            }
            return Vr;
        }

        /// Удаление бесполезных символов
        public Grammar unUsefulDelete()
        {
            Console.WriteLine("\t\tDeleting unuseful symbols");
            Console.WriteLine("Executing: ");
            var Vp = new List<Symbol>();
            var Vr = new List<Symbol>();
            Vr.Add(this.S0);
            var Pp = new List<Production>();
            var P1 = new List<Production>(this.P);
            bool flag = false, noadd = false;
            // Создааем множество порождающих символов и одновременно непроизводящие правила
            do
            {
                flag = false;
                foreach (var p in P1)
                {
                    noadd = false;
                    // DebugPrule(p);
                    if (p.RHS == null || p.RHS.Contains(new Symbol("")))
                    {
                        Pp.Add(p);
                        if (!Vp.Contains(p.LHS))
                        {
                            Vp.Add(p.LHS);
                        }
                        P1.Remove(p);
                        flag = true;
                        break;
                    }
                    else
                    {
                        foreach (var t in p.RHS)
                        {
                            if (!this.T.Contains(t) && !Vp.Contains(t))
                            {
                                // Console.WriteLine(t);
                                noadd = true;
                                break;
                            }
                        }
                        if (!noadd)
                        {
                            Pp.Add(p);
                            if (!Vp.Contains(p.LHS))
                            {
                                Vp.Add(p.LHS);
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
                return new Grammar(new List<Symbol>(), new List<Symbol>(), this.S0.symbol);
            }
            var T1 = new List<Symbol>();
            //Создаем множество достижимых символов
            do
            {
                flag = false;
                foreach (var p in Pp)
                {
                    if (Vr.Contains(p.LHS))
                    {
                        foreach (var t in p.RHS)
                        {
                            if (!Vr.Contains(t))
                            {
                                Vr.Add(t);
                                // noadd = true;
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
            foreach (var t in Vr)
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
            return new Grammar(T1, Vp, P1, this.S0.symbol);
        }

        private List<Symbol> ShortNoTerm()
        {
            var Ve = new List<Symbol>();
            foreach (var p in this.P)
            {
                if (p.RHS.Contains(new Symbol("")))
                    Ve.Add(p.LHS);
            }
            int i = 0; ///!!!
            if (Ve.Count != 0)
                // Console.WriteLine("  {0}",Ve.Count);
                while ((FromWhat(Ve[i].ToString()) != null) && (Ve.Count < i))
                {
                    Ve = Unify(Ve, FromWhat(Ve[0].symbol));
                    i++;
                }
            Debug("Ve", Ve);

            return Ve;
        }

        /// Удаление эпсилон правил
        public Grammar EpsDelete()
        {
            Console.WriteLine("\tDelete e-rules:");
            Console.WriteLine("Executing:");
            var Erule = new List<Production>();
            var Ps = new List<Production>(this.P);
            // ArrayList NoTerm = new ArrayList();
            Console.WriteLine("e-rules:");
            //находим множество е-правил
            foreach (var p in this.P)
            {
                if (p.RHS.Contains(new Symbol("")))
                {
                    DebugPrule(p);
                    Erule.Add(p);
                    Ps.Remove(p);
                }
            }
            //определяем множество неукорачивающихся символов
            var NoTerms = new List<Symbol>();

            foreach (var p in Erule)
            {
                if (!NoTerms.Contains(p.LHS))
                {
                    NoTerms.Add(p.LHS);
                }
            }
            bool flag = false, noadd = false;
            do
            {
                flag = false;
                foreach (var p in Ps)
                {
                    noadd = false;
                    // DebugPrule(p);
                    foreach (var t in p.RHS)
                    {
                        if (!NoTerms.Contains(t))
                        {
                            noadd = true;
                            break;
                        }
                    }
                    if (!noadd)
                    {
                        if (!NoTerms.Contains(p.LHS))
                        {
                            NoTerms.Add(p.LHS);
                        }
                        flag = true;
                        Ps.Remove(p);
                        break;
                    }
                }
            } while (flag);
            Debug("NoShortNoTerms", NoTerms);
            Ps.Clear();
            //Удаляем е-правила и создаем новые в соответствии с алгоритмом
            foreach (var p in this.P)
            {
                if (Erule.Contains(p))
                    continue;
                Ps.Add(p);
                foreach (var s in p.RHS)
                {
                    if (NoTerms.Contains(s))
                    {
                        var NR = new List<Symbol>(p.RHS);
                        NR.Remove(s);
                        Ps.Add(new Production(p.LHS, NR));
                    }
                }
            }
            //проверяем есть ли порождение е из нач символа
            if (NoTerms.Contains(this.S0))
            {
                var V1 = new List<Symbol>(this.V);
                V1.Add(new Symbol("S1"));
                Ps.Add(new Production(new Symbol("S1"), new List<Symbol>() { this.S0 }));
                Ps.Add(new Production(new Symbol("S1"), new List<Symbol>() { new Symbol("") }));
                Debug("V1", V1);
                Console.WriteLine("\te-rules have been deleted!");
                return new Grammar(this.T, V1, Ps, "S1");
            }
            else
            {
                Debug("V1:", this.V);
                Console.WriteLine("\te-rules have benn deleted!");
                return new Grammar(this.T, this.V, Ps, this.S0.symbol);
            }
        }

        /// Удаление цепных правил
        public Grammar ChainRuleDelete()
        {
            Console.WriteLine("\tChainRule Deleting:");
            Console.WriteLine("Executing: ");
            //  Поиск цепных пар
            var chain_pair_list = new List<List<Symbol>>();
            var chain_rules = new List<Symbol>();

            foreach (var v in this.V)
            {
                var chain_pair = new List<Symbol>();
                chain_pair.Add(v);
                chain_pair.Add(v);
                chain_pair_list.Add(chain_pair);
            }
            Console.WriteLine("ChainRules:");
/*
            foreach (var p in this.P) {
                if (TermReturn(p.RHS)==null && NoTermReturn(p.RHS)!=null&&
                                                NoTermReturn(p.RHS).Count==1) {
                    chain_rules.Add(p);
                    DebugPrule(p);
                    foreach (var s in chain_pair_list) {
                        if (s.Equals(new Symbol(p.LHS))) {
                            var chain_pair1 = new List<Symbol>();
                            chain_pair1.Add(chain_pair_list[0]);
                            chain_pair1.Add(NoTermReturn(p.RHS)[0]);
                            chain_pair_list.Add(chain_pair1);
                        }
                    }
                }
            }

            Console.WriteLine("Deleting...");
            var P = new List<Production>();

            foreach (var c in chain_pair_list) {
                foreach (var p in this.P) {
                    if (p.LHS==c[1].symbol&&!(TermReturn(p.RHS)==null &&
                                NoTermReturn(p.RHS)!=null&&NoTermReturn(p.RHS).Count==1)) {
                        var P_1 = new Production(c[0].symbol,p.RHS);
                        if (!P.Contains(P_1)) {
                            P.Add(P_1);
                        }
                    }
                }
            }
*/
            Console.WriteLine("\tChainrules have been deleted;");
            return new Grammar(this.T, this.V, P, this.S0.symbol);
        }

        /// Удаление левой рекурсии
        public Grammar LeftRecursDelete()
        {
            Console.WriteLine("\tLeft Recursion delete:");
            Console.WriteLine("Executing: ");
            var P = new List<Production>();
            var V1 = new List<Symbol>(this.V);
            var Vr = new List<Symbol>();
            //ищем рекурсивные правила
            Console.WriteLine("Rules with Recursion:");
            foreach (var p in this.P)
            {
                if (p.LHS == p.RHS[0])
                {
                    DebugPrule(p);
                    if (!Vr.Contains(p.LHS))
                    {
                        Vr.Add(p.LHS);
                    }
                }
            }
            foreach (var p in this.P)
            {
                if (!Vr.Contains(p.LHS))
                    P.Add(p);
            }
            //преобразуем их в новые без левой рекурсии
            var v_struct_ar = new List<Symbol>();

            foreach (var v in Vr)
            {
                V_struct v_struct;
                v_struct.alpha = new List<Symbol>();
                v_struct.betta = new List<Symbol>();
                v_struct.V = v.symbol;
                foreach (var r in this.P)
                {
                    if (v.symbol == r.LHS.symbol)
                    {
                        if (r.RHS[0] == v)
                        {
                            var alpha_help = new List<Symbol>();
                            for (int i = 1; i < r.RHS.Count; i++)
                            {
                                alpha_help.Add(r.RHS[i]);
                            }
                            if (alpha_help.Count > 0)
                                v_struct.alpha = alpha_help;
                        }
                        else
                        {
                            if (r.RHS.Count > 0)
                                v_struct.betta = r.RHS;
                        }
                    }
                }
                // v_struct_ar.Add(v_struct);
            }
/*
            foreach (var v in v_struct_ar) {
                var new_v = v.symbol +"'";
                V1.Add(new Symbol(new_v));

                foreach (var b in v_struct.betta) {
                    P.Add(new Production(v_struct.V,b));
                    var betta_pravila = new List<Symbol>();
                    for (int i = 0; i<b.Count; i++) {
                        betta_pravila.Add(b[i]);
                    }
                    betta_pravila.Add(new_v);
                    P.Add(new Production(v_struct.V,betta_pravila));
                }

                foreach (var a in v_struct.alpha) {
                    P.Add(new Production(new_v,a));
                    var alpha_pravila = new List<Symbol>();
                    for (int i = 0; i < a.Count; i++) {
                        alpha_pravila.Add(a[i]);
                    }
                    alpha_pravila.Add(new_v);
                    P.Add(new Production(new_v,alpha_pravila));
                }
            }
            Debug("V1",V1);
            Console.WriteLine("\tLeft Recursion have been deleted!");
*/
            return new Grammar(this.T,V1,P,this.S0.symbol);
        }

        // **   Debug   **
        public void DebugPrules()
        {
            Console.WriteLine("Prules:");
            foreach (var p in this.P)
            {
                string right = "";
                for (int i = 0; i < p.RHS.Count; i++)
                    right += p.RHS[i].ToString();
                Console.WriteLine(p.LHS + " -> " + right);
            }
        }
        public void DebugPrule(Production p)
        {
            var right = "";
            for (int i = 0; i < p.RHS.Count; i++)
                right += p.RHS[i].ToString();
            Console.WriteLine(p.LHS + " -> " + right + " ");
        }

        public void Debug(string step, List<Symbol> list)
        {
            Console.Write(step + " : ");
            if (list == null)
                Console.WriteLine("null");
            else
                foreach (var s in list)
                    Console.Write(s.symbol + " ");
            Console.WriteLine("");
        }

        public void Debug(string step, string line)
        {
            Console.Write(step + " : ");
            Console.WriteLine(line);
        }

        /// Откуда можем прийти в состояние
        private List<Symbol> FromWhat(string state)
        {
            var from = new List<Symbol>();
            bool flag = true;
            foreach (var p in this.P)
            {
                if (p.RHS.Contains(new Symbol(state)))
                {
                    from.Add(p.LHS);
                    flag = false;
                }
            }
            if (flag)
                return null;
            else
                return from;
        }

        // Объединение множеств A or B
        private List<Symbol> Unify(List<Symbol> A, List<Symbol> B)
        {
            var unify = A;
            foreach (var s in B)
                if (!A.Contains(s))
                    unify.Add(s);
            return unify;
        }

        // Пересечение множеств A & B
        private List<Symbol> intersection(List<Symbol> A, List<Symbol> B)
        {
            var intersection = new List<Symbol>();
            foreach (var s in A)
                if (B.Contains(s))
                    intersection.Add(s);
            return intersection;
        }

        // Нетерминальные символы из массива
        private List<Symbol> NoTermReturn(List<Symbol> array)
        {
            var NoTerm = new List<Symbol>();
            bool flag = true; // added
            foreach (var s in array)
                if (this.V.Contains(s))
                {
                    flag = false; // added
                    NoTerm.Add(s);
                }
            if (flag)
                return null; // added
            else
                return NoTerm;
        }

        private string NoTerminal(List<Symbol> array)
        {
            var NoTermin = "";
            foreach (var s in array)
            {
                if (this.V.Contains(s))
                    NoTermin = s.symbol;
            }
            return NoTermin;
        }

        // Терминальные символы из массива
        private List<Symbol> TermReturn(List<Symbol> A)
        {
            var Term = new List<Symbol>();
            bool flag = true;
            foreach (var t in this.T)
                if (A.Contains(t))
                {
                    flag = false;
                    Term.Add(t);
                }
            if (flag)
                return null;
            else
                return Term;
        }

        // Все символы в правиле
        private List<Symbol> SymbInRules(Production p)
        {
            var SymbInRules = new List<Symbol>() { p.LHS };
            for (int i = 0; i < p.RHS.Count; i++)
                SymbInRules.Add(p.RHS[i]);
            return SymbInRules;
        }

        // Проверка пустоты правой цепочки
        private bool ContainEps(Production p)
        {
            if (p.RHS.ToString().Contains(""))
                return true;
            return false;
        }

    } // end abstract Grammar class

}
