using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;

namespace MPTranslator
{
    class SLRParser
    {
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

            public bool Equals(PairStrInt pair)
            {
                return (pair.Second == Second) && Equals(pair.First, First);
            }

            public void Debug()
            {
                Console.WriteLine(First + " " + Second.ToString());
            }
        }

        protected class PairComparer : IEqualityComparer<PairStrInt>
        {
            public bool Equals(PairStrInt lhs, PairStrInt rhs)
            {
                return lhs.Equals(rhs);
            }

            public int GetHashCode(PairStrInt pair)
            {
                string s = pair.First + pair.Second.ToString();
                return s.GetHashCode();
            }
        }

        protected class State : IEquatable<State>
        {
            public int rulePos { get; }
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

            public string GetRightChainSymbol(myGrammar grammar)
            {
                Prule curRule = (Prule)grammar.Prules[rulePos];
                if (dotPos == curRule.RightChain.Count)
                {
                    return "";
                }
                else
                {
                    return (string)curRule.RightChain[dotPos];
                }
            }

            public string GetLeftSymbol(myGrammar grammar)
            {
                Prule curRule = (Prule)grammar.Prules[rulePos];
                return (string)curRule.LeftNoTerm;
            }

            public void Debug(myGrammar grammar)
            {
                Prule curRule = (Prule)grammar.Prules[rulePos];
                string curStateRightChain = "";
                for (int i = 0; i < curRule.RightChain.Count; ++i)
                {
                    if (i == dotPos)
                    {
                        curStateRightChain += ".";
                    }
                    string s = (string)curRule.RightChain[i];
                    curStateRightChain += s;
                }
                if (curRule.rightChain.Count == dotPos)
                {
                    curStateRightChain += ".";
                }
                Console.WriteLine(curRule.LeftNoTerm + " -> " + curStateRightChain);
            }
        }

        protected myGrammar SLRGrammar;
        protected Dictionary<PairStrInt, PairStrInt> M;

        public SLRParser(myGrammar grammar)
        {
            SLRGrammar = grammar;
            SLRGrammar.AddRule("S'", new ArrayList() { SLRGrammar.S0 });
            SLRGrammar.V.Add("S'");
            SLRGrammar.T.Add("$");

            SLRGrammar.DebugPrules();

            BuildTable();
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
                            Prule curRule = (Prule)SLRGrammar.Prules[rulePos];
                            for (int j = 0; j < curRule.RightChain.Count; ++j)
                            {
                                st.Pop();
                            }
                            curCondition = new PairStrInt(curRule.LeftNoTerm, st.Peek());
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
                    string B = curState.GetRightChainSymbol(SLRGrammar);
                    for (int i = 0; i < SLRGrammar.Prules.Count; ++i) {
                        Prule curRule = (Prule)SLRGrammar.Prules[i];
                        if (B == curRule.leftNoTerm)
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

        protected List<State> Goto(List<State> I, string X)
        {
            List<State> J = null;
            foreach (State st in I)
            {
                if (st.GetRightChainSymbol(SLRGrammar) == X)
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

        protected List< List< State > > SetOfStates(myGrammar grammar)
        {
            List< List< State > > C0 = new List< List<State> >();
            List<State> I = new List<State>();
            I.Add(new State(grammar.Prules.Count - 1, 0));
            C0.Add(Closure(I));
            List< List< State > > C = new List< List<State> >();
            bool changed = false;
            List<string> alphabet = new List<string>();
            foreach (string t in grammar.T)
            {
                alphabet.Add(t);
            }
            foreach (string v in grammar.V)
            {
                alphabet.Add(v);
            }
            do {
                changed = false;
                C = new List< List< State > > (C0);
                foreach (List<State> c in C)
                {
                    foreach (string X in alphabet)
                    {
                        List<State> nextState = Goto(c, X);
                        if (nextState != null && FindSetOfStates(C0, nextState) == -1)
                        {
                            C0.Add(nextState);
                            changed = true;
                        }
                    }
                }
            } while (changed);

            Console.WriteLine("Debug SetOfStates...");
            for (int i = 0; i < C.Count; ++i)
            {
                List<State> c = C[i];
                Console.WriteLine("Debug I_" + i.ToString());
                foreach (State st in c) {
                    st.Debug(SLRGrammar);
                }
            }

            return C;
        }

        protected void BuildTable()
        {
            List< List< State > > C = SetOfStates(SLRGrammar);
            PairComparer comp = new PairComparer();
            M = new Dictionary<PairStrInt, PairStrInt>(comp);
            for (int i = 0; i < C.Count; ++i)
            {
                List<State> I = C[i];

                foreach (State st in I)
                {
                    string a = st.GetRightChainSymbol(SLRGrammar);
                    string A = st.GetLeftSymbol(SLRGrammar);
                    if (SLRGrammar.T.Contains(a))
                    {
                        int j = FindSetOfStates(C, Goto(I, a));
                        PairStrInt conditionFrom = new PairStrInt(a, i);
                        PairStrInt conditionTo = new PairStrInt("S", j);
                        M.Add(conditionFrom, conditionTo);
                    }
                    if (a == "")
                    {
                        if (A == "S'")
                        {
                            PairStrInt conditionFrom = new PairStrInt("$", i);
                            PairStrInt conditionTo = new PairStrInt("A", -1);
                            M.Add(conditionFrom, conditionTo);
                        }
                        else
                        {
                            // для SLR(1) надо FOLLOW
                            // foreach (string terminal in FOLLOW(A))
                            foreach (string terminal in SLRGrammar.T)
                            {
                                PairStrInt conditionFrom = new PairStrInt(terminal, i);
                                PairStrInt conditionTo = new PairStrInt("R", st.rulePos);
                                M.Add(conditionFrom, conditionTo);
                            }
                        }
                    }
                }

                foreach (string X in SLRGrammar.V)
                {
                    int j = FindSetOfStates(C, Goto(I, X));
                    if (j != -1)
                    {
                        PairStrInt conditionFrom = new PairStrInt(X, i);
                        PairStrInt conditionTo = new PairStrInt("", j);
                        M.Add(conditionFrom, conditionTo);
                    }
                }
            }

            // Console.WriteLine("Debug M...");
            // foreach (PairStrInt from in M.Keys)
            // {
            //     Console.Write("From ");
            //     from.Debug();
            //     Console.Write("To ");
            //     M[from].Debug();
            // }
        }
    }
}
