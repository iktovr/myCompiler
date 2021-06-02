using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;
using Processor.AbstractGrammar;

namespace Translator
{
    // todo CLOSURE и GOTO в грамматику
    // написать подробнее про LR(0) и LR(1)
    // картинку автомата в Word в виде объекта
    class SLRParser
    {
        /// Пара строка-число
        protected class PairStrInt : IEquatable<PairStrInt>
        {
            public string First { get; set; }
            public int Second { get; set; }

            public PairStrInt() {}

            public PairStrInt(char c, int num)
            {
                First = c.ToString();
                Second = num;
            }

            public PairStrInt(string str, int num)
            {
                First = str;
                Second = num;
            }

            /// Проверка двух пар на равенство
            public bool Equals(PairStrInt pair)
            {
                return (pair.Second == Second) && Equals(pair.First, First);
            }

            public void Debug()
            {
                Console.WriteLine(First + " " + Second.ToString());
            }
        }

        /// Компаратор пар. Требуется для корректной работы Dictionary
        protected class PairComparer : IEqualityComparer<PairStrInt>
        {
            /// Метод проверки равенства
            public bool Equals(PairStrInt lhs, PairStrInt rhs)
            {
                return lhs.Equals(rhs);
            }

            /// Метод вычисления хеш-функции
            public int GetHashCode(PairStrInt pair)
            {
                string s = pair.First + pair.Second.ToString();
                return s.GetHashCode();
            }
        }

        /// LR(0)-ситуация - продукция с точкой в некоторой позиции правой части
        protected class State : IEquatable<State>
        {
            /// Номер правила грамматика
            public int rulePos { get; }
            /// Позиция точки в правой части правила
            public int dotPos { get; }

            public State() {}

            public State(int rulePosition, int dotPosition)
            {
                rulePos = rulePosition;
                dotPos = dotPosition;
            }

            public bool Equals(State st)
            {
                return (st.rulePos == rulePos) && (st.dotPos == dotPos);
            }

            /// Получение символа, стоящего после точки
            public Symbol GetRHSymbol(Grammar grammar)
            {
                Production rule = grammar.P[rulePos];
                if (dotPos == rule.RHS.Count)
                {
                    return new Symbol("");
                }
                else
                {
                    return rule.RHS[dotPos];
                }
            }

            /// Получение символа, стоящего в левой части ситуации
            public Symbol GetLHSymbol(Grammar grammar)
            {
                Production rule = grammar.P[rulePos];
                return rule.LHS;
            }

            /// Отладочная печать ситуации
            public void Debug(Grammar grammar)
            {
                Production rule = grammar.P[rulePos];
                string curStateRHS = "";
                for (int i = 0; i < rule.RHS.Count; ++i)
                {
                    if (i == dotPos)
                    {
                        curStateRHS += ".";
                    }
                    curStateRHS += rule.RHS[i].ToString();
                }
                if (rule.RHS.Count == dotPos)
                {
                    curStateRHS += ".";
                }
                Console.WriteLine(rule.LHS.ToString() + " -> " + curStateRHS);
            }
        }

        /// Пополненная контекстно-свободная грамматика
        protected Grammar SLRGrammar;
        /// Канонический набор множеств LR(0)-ситуаций
        protected List< List< State > > C = new List< List< State > >();
        /// LR(0)-автомат переходов грамматики
        protected FSAutomate KA = new FSAutomate();
        /// Управляющая SLR-таблица, представленная в виде словаря
        Dictionary<PairStrInt, PairStrInt> M = null;
        /// Новый начальный нетерминал S'
        Symbol startSymbol = new Symbol("S'");
        /// Пустая цепочка
        Symbol EPS = new Symbol("");

        public SLRParser(Grammar grammar)
        {
            SLRGrammar = grammar;
            // Пополнение грамматики
            SLRGrammar.AddRule(startSymbol.ToString(), new List<Symbol>() { SLRGrammar.S0 });
            SLRGrammar.V.Add(startSymbol);
            SLRGrammar.T.Add(new Symbol("$"));
            SLRGrammar.DebugPrules();

            InitAutomate();
            BuildAutomate();
            BuildTable();
        }

        /// Инициализация автомата и добавление символов в алфавит
        protected void InitAutomate()
        {
            KA.Q = new List<Symbol>() { new Symbol("0") };
            KA.Sigma = new List<Symbol>();
            foreach (Symbol t in SLRGrammar.T)
            {
                KA.Sigma.Add(t);
            }
            foreach (Symbol v in SLRGrammar.V)
            {
                KA.Sigma.Add(v);
            }
            KA.D = new List<DeltaQSigma>();
        }

        /// Построение замыкания множества LR(0)-ситуаций
        protected List<State> Closure(List<State> I)
        {
            List<State> J0 = new List<State>(I);
            List<State> J = null;
            bool changed = false;
            do
            {
                changed = false;
                J = new List<State>(J0);
                foreach (State curState in J)
                {
                    Symbol B = curState.GetRHSymbol(SLRGrammar);
                    for (int i = 0; i < SLRGrammar.P.Count; ++i) {
                        Production rule = SLRGrammar.P[i];
                        if (B.Equals(rule.LHS))
                        {
                            State gammaState = new State(i, 0);
                            if (!J0.Contains(gammaState))
                            {
                                J0.Add(gammaState);
                                changed = true;
                            }
                        }
                    }
                }

            } while (changed);
            return J;
        }

        /// Вычисление множества GOTO(I, X)
        protected List<State> Goto(List<State> I, Symbol X)
        {
            List<State> J = null;
            foreach (State st in I)
            {
                if (X.Equals(st.GetRHSymbol(SLRGrammar)))
                {
                    List<State> movedDotState = new List<State>();
                    movedDotState.Add(new State(st.rulePos, st.dotPos + 1));
                    List<State> movedDotClosure = Closure(movedDotState);
                    if (J == null)
                    {
                        J = new List<State>();
                    }
                    foreach (State movedDotSt in movedDotClosure)
                    {
                        if (!J.Contains(movedDotSt))
                        {
                            J.Add(movedDotSt);
                        }
                    }
                }
            }
            return J;
        }

        /// Возвращает истину, если уже есть правило перехода в автомате
        protected bool FindDeltaRuleInKA(DeltaQSigma rule)
        {
            if (rule == null)
            {
                return false;
            }
            foreach (DeltaQSigma d in KA.D)
            {
                if (rule.LHSQ == d.LHSQ && rule.LHSS == d.LHSS)
                {
                    return true;
                }
            }
            return false;
        }

        /// Возвращает индекс замыкания множества LR(0)-ситуаций в указанном
        /// каноническом наборе. Если множество не было найдено, возращает -1
        protected int FindSetOfStates(List< List< State > > C, List<State> I)
        {
            if (I == null)
            {
                return -1;
            }
            for (int i = 0; i < C.Count; ++i)
            {
                List<State> J = C[i];
                if (J.Count == I.Count)
                {
                    bool equals = true;
                    foreach (State st in J)
                    {
                        if (!I.Contains(st))
                        {
                            equals = false;
                            break;
                        }
                    }
                    if (equals)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        /// Построение канонического набора множеств LR(0)-ситуаций и автомата перехода между ситуациями
        protected void BuildAutomate()
        {
            List< List< State > > C0 = new List< List< State > >();
            List<State> I = new List<State>();
            I.Add(new State(SLRGrammar.P.Count - 1, 0));
            C0.Add(Closure(I));
            bool changed = false;
            do {
                changed = false;
                C = new List< List< State > > (C0);
                for (int i = 0; i < C.Count; ++i)
                {
                    List<State> c = C[i];
                    foreach (Symbol X in KA.Sigma)
                    {
                        List<State> nextState = Goto(c, X);
                        if (nextState != null)
                        {
                            int nextStateId = FindSetOfStates(C0, nextState);
                            // Если замыкание не найдено
                            if (nextStateId == -1)
                            {
                                nextStateId = C0.Count;
                                KA.Q.Add(new Symbol(nextStateId.ToString()));
                                C0.Add(nextState);
                                changed = true;
                            }
                            DeltaQSigma nextStateEdge = new DeltaQSigma(i.ToString(), X.ToString(), new List<Symbol> { new Symbol(nextStateId.ToString()) });
                            if (!FindDeltaRuleInKA(nextStateEdge))
                            {
                                KA.D.Add(nextStateEdge);
                            }
                        }

                    }
                }
            } while (changed);

            Console.WriteLine("Debug С");
            for (int i = 0; i < C.Count; ++i)
            {
                List<State> c = C[i];
                Console.WriteLine("Debug I_" + i.ToString());
                foreach (State st in c) {
                    st.Debug(SLRGrammar);
                }
            }
        }

        // ToDo
        // Возвращать таблицу для конечного автомата
        // Сохранить результат вычислений GOTO
        // Автомат на вход

        /// Построение управляющей таблицы КС-грамматики
        protected void BuildTable()
        {
            PairComparer comp = new PairComparer();
            M = new Dictionary<PairStrInt, PairStrInt>(comp);
            for (int i = 0; i < C.Count; ++i)
            {
                List<State> I = C[i];
                foreach (State st in I)
                {
                    // Console.WriteLine("i = " + i.ToString() + ", Cur state is");
                    st.Debug(SLRGrammar);
                    Symbol a = st.GetRHSymbol(SLRGrammar);
                    Symbol A = st.GetLHSymbol(SLRGrammar);
                    // Console.WriteLine("a = " + a.ToString() + ", A = " + A.ToString());
                    if (a.Equals(EPS))
                    {
                        // Console.WriteLine("Empty!");
                        if (A.Equals(startSymbol))
                        {
                            PairStrInt conditionFrom = new PairStrInt("$", i);
                            PairStrInt conditionTo = new PairStrInt("A", -1);
                            M[conditionFrom] = conditionTo;
                        }
                        else
                        {
                            // для SLR(1) надо FOLLOW
                            // foreach (string terminal in FOLLOW(A))
                            foreach (Symbol X in SLRGrammar.T)
                            {
                                PairStrInt conditionFrom = new PairStrInt(X.ToString(), i);
                                PairStrInt conditionTo = new PairStrInt("R", st.rulePos);
                                M[conditionFrom] = conditionTo;
                            }
                        }
                    }

                    foreach (DeltaQSigma edge in KA.D)
                    {
                        if (i.ToString() == edge.LHSQ)
                        {
                            Symbol X = new Symbol(edge.LHSS);
                            int j = int.Parse(edge.RHSQ[0].symbol);
                            if (SLRGrammar.T.Contains(X))
                            {
                                PairStrInt conditionFrom = new PairStrInt(X.ToString(), i);
                                PairStrInt conditionTo = new PairStrInt("S", j);
                                M[conditionFrom] = conditionTo;
                            }
                            if (SLRGrammar.V.Contains(X))
                            {
                                PairStrInt conditionFrom = new PairStrInt(X.ToString(), i);
                                PairStrInt conditionTo = new PairStrInt("", j);
                                M[conditionFrom] = conditionTo;
                            }
                        }
                    }
                }
            }

            // for (int i = 0; i < C.Count; ++i)
            // {
            //     List<State> I = C[i];
            //
            //     foreach (State st in I)
            //     {
            //         Symbol a = st.GetRHSymbol(SLRGrammar);
            //         Symbol A = st.GetLHSymbol(SLRGrammar);
            //         if (SLRGrammar.T.Contains(a))
            //         {
            //             int j = FindSetOfStates(C, Goto(I, a));
            //             PairStrInt conditionFrom = new PairStrInt(a.ToString(), i);
            //             PairStrInt conditionTo = new PairStrInt("S", j);
            //             M.Add(conditionFrom, conditionTo);
            //         }
            //         if (a.symbol == "")
            //         {
            //             if (A.symbol == "S'")
            //             {
            //                 PairStrInt conditionFrom = new PairStrInt("$", i);
            //                 PairStrInt conditionTo = new PairStrInt("A", -1);
            //                 M.Add(conditionFrom, conditionTo);
            //             }
            //             else
            //             {
            //                 // для SLR(1) надо FOLLOW
            //                 // foreach (string terminal in FOLLOW(A))
            //                 foreach (Symbol terminal in SLRGrammar.T)
            //                 {
            //                     PairStrInt conditionFrom = new PairStrInt(terminal.ToString(), i);
            //                     PairStrInt conditionTo = new PairStrInt("R", st.rulePos);
            //                     M.Add(conditionFrom, conditionTo);
            //                 }
            //             }
            //         }
            //     }
            //
            //     foreach (Symbol X in SLRGrammar.V)
            //     {
            //         int j = FindSetOfStates(C, Goto(I, X));
            //         if (j != -1)
            //         {
            //             PairStrInt conditionFrom = new PairStrInt(X.ToString(), i);
            //             PairStrInt conditionTo = new PairStrInt("", j);
            //             M.Add(conditionFrom, conditionTo);
            //         }
            //     }
            // }

            Console.WriteLine("Debug M...");
            foreach (PairStrInt from in M.Keys)
            {
                Console.Write("From ");
                from.Debug();
                Console.Write("To ");
                M[from].Debug();
            }
        }

        public void Execute()
        {
            string answer = "y";
            do
            {
                Console.WriteLine("\n Введите строку: \n");
                string input = Console.In.ReadLine();
                Console.WriteLine("\n Введена строка: " + input + "\n");
                Console.WriteLine("\n Процесс вывода: \n ");

                string w = input + "$";
                Stack<int> st = new Stack<int>();
                st.Push(0);
                int i = 0;
                bool accepted = false;
                bool error = false;
                do
                {
                    char a = w[i];
                    PairStrInt curCondition = new PairStrInt(a, st.Peek());
                    PairStrInt tableCondition = null;
                    if (!M.TryGetValue(curCondition, out tableCondition))
                    {
                        error = true;
                        break;
                    }
                    switch (tableCondition.First)
                    {
                        // Accept - Принятие
                        case "A":
                            accepted = true;
                            break;
                        // Shift - Перенос
                        case "S":
                            st.Push(tableCondition.Second);
                            ++i;
                            break;
                        // Reduction - Свёртка
                        case "R":
                            int rulePos = tableCondition.Second;
                            Production rule = SLRGrammar.P[rulePos];
                            for (int j = 0; j < rule.RHS.Count; ++j)
                            {
                                st.Pop();
                            }
                            curCondition = new PairStrInt(rule.LHS.ToString(), st.Peek());
                            tableCondition = M[curCondition];
                            st.Push(tableCondition.Second);
                            Console.WriteLine("Вывод: " + (rulePos + 1).ToString());
                            break;
                        default:
                            error = true;
                            break;
                    }
                }
                while (!accepted && !error);

                if (accepted)
                {
                    Console.WriteLine("Строка допущена");
                }
                else
                {
                    Console.WriteLine("Строка отвергнута");
                }

                Console.WriteLine("\n Продолжить? (y or n) \n");
                answer = Console.ReadLine();
            }
            while (answer == "y");
        }
    }
}
