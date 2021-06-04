using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;
using Processor.AbstractGrammar;

namespace Translator
{

    // todo картинку автомата в Word в виде объекта

    // todo наследование LR Grammar
    // class LRGrammar : Grammar

    class SLRParser
    {
        /// Пара символ-число
        protected class PairSymbInt : IEquatable<PairSymbInt>
        {
            public Symbol First { get; set; }
            public int Second { get; set; }

            public PairSymbInt() {}

            public PairSymbInt(char c, int num)
            {
                First = new Symbol(c.ToString());
                Second = num;
            }

            public PairSymbInt(string str, int num)
            {
                First = new Symbol(str);
                Second = num;
            }

            public PairSymbInt(Symbol sym, int num)
            {
                First = sym;
                Second = num;
            }

            /// Проверка двух пар на равенство
            public bool Equals(PairSymbInt pair)
            {
                return (pair.Second == Second) && Equals(pair.First, First);
            }

            public void Debug()
            {
                Console.WriteLine(First + " " + Second.ToString());
            }
        }

        /// Компаратор пар. Требуется для корректной работы Dictionary
        protected class PairComparer : IEqualityComparer<PairSymbInt>
        {
            /// Метод проверки равенства
            public bool Equals(PairSymbInt lhs, PairSymbInt rhs)
            {
                return lhs.Equals(rhs);
            }

            /// Метод вычисления хеш-функции
            public int GetHashCode(PairSymbInt pair)
            {
                string s = pair.First.ToString() + pair.Second.ToString();
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
        protected Grammar SLRGrammar = null;
        /// Канонический набор множеств LR(0)-ситуаций
        protected List< List< State > > phi = new List< List< State > >();
        /// LR(0)-автомат переходов грамматики
        protected FSAutomate LRA = new FSAutomate();
        /// Управляющая SLR-таблица, представленная в виде словаря
        protected Dictionary<PairSymbInt, PairSymbInt> M = null;
        /// Новый начальный нетерминал S'
        protected Symbol startSymbol = new Symbol("S'");

        public SLRParser(Grammar grammar)
        {
            SLRGrammar = grammar;
            // Пополнение грамматики
            SLRGrammar.AddRule(startSymbol.ToString(), new List<Symbol>() { SLRGrammar.S0 });
            SLRGrammar.V.Add(startSymbol);
            SLRGrammar.T.Add(Symbol.Sentinel);
            SLRGrammar.DebugPrules();

            InitAutomate();
            BuildLRAutomate();
            BuildControlTable();
        }

        /// Инициализация автомата и добавление символов в алфавит
        protected void InitAutomate()
        {
            LRA.Q = new List<Symbol>() { new Symbol("0") };
            LRA.Sigma = new List<Symbol>();
            foreach (Symbol t in SLRGrammar.T)
            {
                LRA.Sigma.Add(t);
            }
            foreach (Symbol v in SLRGrammar.V)
            {
                LRA.Sigma.Add(v);
            }
            LRA.D = new List<DeltaQSigma>();
            LRA.F = new List<Symbol>();
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
        protected bool FindDeltaRuleInLRA(DeltaQSigma rule)
        {
            if (rule == null)
            {
                return false;
            }
            foreach (DeltaQSigma d in LRA.D)
            {
                if (rule.LHSQ == d.LHSQ && rule.LHSS == d.LHSS)
                {
                    return true;
                }
            }
            return false;
        }

        /**
         *  Возвращает индекс замыкания множества LR(0)-ситуаций в указанном
         *  каноническом наборе. Если множество не было найдено, возращает -1
         */
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
        protected void BuildLRAutomate()
        {
            List< List< State > > phi0 = new List< List< State > >();
            List<State> I0 = new List<State>();
            I0.Add(new State(SLRGrammar.P.Count - 1, 0));
            phi0.Add(Closure(I0));
            bool changed = false;
            List<Symbol> nonTerminalStates = new List<Symbol>();
            do {
                changed = false;
                phi = new List< List< State > > (phi0);
                for (int i = 0; i < phi.Count; ++i)
                {
                    List<State> I = phi[i];
                    foreach (Symbol X in LRA.Sigma)
                    {
                        List<State> nextState = Goto(I, X);
                        if (nextState != null)
                        {
                            Symbol curStateSymbol = new Symbol(i.ToString());
                            if (!nonTerminalStates.Contains(curStateSymbol))
                            {
                                nonTerminalStates.Add(curStateSymbol);
                            }
                            int nextStateId = FindSetOfStates(phi0, nextState);
                            // Если замыкание не найдено
                            if (nextStateId == -1)
                            {
                                nextStateId = phi0.Count;
                                LRA.Q.Add(new Symbol(nextStateId.ToString()));
                                phi0.Add(nextState);
                                changed = true;
                            }
                            DeltaQSigma nextStateEdge = new DeltaQSigma(curStateSymbol, X, new List<Symbol> { new Symbol(nextStateId.ToString()) });
                            if (!FindDeltaRuleInLRA(nextStateEdge))
                            {
                                LRA.D.Add(nextStateEdge);
                            }
                        }

                    }
                }
            } while (changed);

            for (int i = 0; i < phi.Count; ++i)
            {
                Symbol curStateSymbol = new Symbol(i.ToString());
                if (!nonTerminalStates.Contains(curStateSymbol))
                {
                    LRA.F.Add(curStateSymbol);
                }
            }

            Console.WriteLine("Debug canonical set of states C");
            for (int i = 0; i < phi.Count; ++i)
            {
                List<State> I = phi[i];
                Console.WriteLine("Debug I_" + i.ToString());
                foreach (State st in I) {
                    st.Debug(SLRGrammar);
                }
            }
        }

        /// Построение управляющей таблицы КС-грамматики
        protected void BuildControlTable()
        {
            PairComparer comp = new PairComparer();
            M = new Dictionary<PairSymbInt, PairSymbInt>(comp);
            for (int i = 0; i < phi.Count; ++i)
            {
                List<State> I = phi[i];
                foreach (DeltaQSigma edge in LRA.D)
                {
                    if (i.ToString() == edge.LHSQ.Value)
                    {
                        Symbol X = edge.LHSS;
                        Symbol I_j = edge.RHSQ[0];
                        int j = int.Parse(I_j.Value);
                        if (SLRGrammar.T.Contains(X))
                        {
                            PairSymbInt conditionFrom = new PairSymbInt(X.ToString(), i);
                            PairSymbInt conditionTo = new PairSymbInt("S", j);
                            M[conditionFrom] = conditionTo;
                        }
                        if (SLRGrammar.V.Contains(X))
                        {
                            PairSymbInt conditionFrom = new PairSymbInt(X.ToString(), i);
                            PairSymbInt conditionTo = new PairSymbInt("", j);
                            M[conditionFrom] = conditionTo;
                        }
                        if (LRA.F.Contains(I_j))
                        {
                            foreach (State st in phi[j])
                            {
                                Symbol a = st.GetRHSymbol(SLRGrammar);
                                Symbol A = st.GetLHSymbol(SLRGrammar);
                                if (a.Equals(Symbol.Epsilon))
                                {
                                    if (A.Equals(startSymbol))
                                    {
                                        PairSymbInt conditionFrom = new PairSymbInt("$", j);
                                        PairSymbInt conditionTo = new PairSymbInt("A", -1);
                                        M[conditionFrom] = conditionTo;
                                    }
                                    else
                                    {
                                        // Для просмотра следующего символа требуется FOLLOW(A)
                                        // foreach (Symbol Y in FOLLOW(A))
                                        foreach (Symbol Y in SLRGrammar.T)
                                        {
                                            PairSymbInt conditionFrom = new PairSymbInt(Y.ToString(), j);
                                            PairSymbInt conditionTo = new PairSymbInt("R", st.rulePos);
                                            M[conditionFrom] = conditionTo;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Debug M...");
            foreach (PairSymbInt from in M.Keys)
            {
                Console.Write("From ");
                from.Debug();
                Console.Write("To ");
                M[from].Debug();
            }
        }

        /// Выполнение LR-анализа
        /**
         *  LR-анализатор состоит из выходного буфера, выхода, стека,
         *  программы-драйвера и таблицы синтаксического анализа,
         *  состоящей из двух частей (ACTION и GOTO). Программа-драйвера
         *  одинакова для всех LR-анализаторов; от одного к другому
         *  меняются таблицы синтаксического анализа. Программа
         *  синтаксического анализа по одному считывает символы из
         *  входного буфера.
         */
        // todo public void override Parse()
        public void Parse()
        {
            string answer = "y";
            do
            {
                Console.WriteLine("\n Введите строку: \n");
                string input = Console.In.ReadLine();
                Console.WriteLine("\n Введена строка: " + input + "\n");

                string w = input + "$";
                Stack<int> st = new Stack<int>();
                st.Push(0);
                int i = 0;
                Stack<int> res = new Stack<int>();
                bool accepted = false;
                bool error = false;
                do
                {
                    char a = w[i];
                    PairSymbInt curCondition = new PairSymbInt(a, st.Peek());
                    PairSymbInt tableCondition = null;
                    if (!M.TryGetValue(curCondition, out tableCondition))
                    {
                        error = true;
                        break;
                    }
                    switch (tableCondition.First.Value)
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
                            curCondition = new PairSymbInt(rule.LHS.ToString(), st.Peek());
                            tableCondition = M[curCondition];
                            st.Push(tableCondition.Second);
                            res.Push(rulePos + 1);
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
                    Console.Write("Вывод:");
                    while (res.Count > 0)
                    {
                        Console.Write(" " + res.Pop().ToString());
                    }
                    Console.WriteLine();
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
