using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace MPTranslator
{
    /// Обычный символ грамматики
    class Symbol
    {
        public string Value;

        public Symbol(string value)
        {
            Value = value;
        }

        /// Неявное преобразование строки в Symbol
        public static implicit operator Symbol(string str) => new Symbol(str);

        /// Равенсто. Требуется для Dictionary и HashSet
        public override bool Equals(object other)
        {   
            return (other is Symbol) && (Value == ((Symbol)other).Value);
        }

        /// Хеш-функция. Требуется для Dictionary и HashSet
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// Оператор взят из документации
        public static bool operator ==(Symbol a, Symbol b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Value == b.Value;
        }

        public static bool operator !=(Symbol a, Symbol b)
        {
            return !(a == b);
        }

        public static Symbol Epsilon = new Symbol(""); ///< Пустой символ
        public static Symbol Sentinel = new Symbol("$$"); ///< Cимвол конца строки / Символ дна стека
    }

    /// Правило синтаксически управляемой схемы трансляции
    class SDTRule : Rule
    {
        public new Symbol LeftNoTerm;
        public List<Symbol> RightChain;

        public SDTRule(Symbol leftNoTerm, List<Symbol> rightChain)
        {
            LeftNoTerm = leftNoTerm;
            RightChain = rightChain;
        }
    }

    /// Синтаксически управляемая схема трансляции
    class SDTScheme : Grammar
    {
        new public Symbol S0;
        new public List<Symbol> T;
        new public List<Symbol> V;
        new public List<SDTRule> Prules;

        private Dictionary<Symbol, HashSet<Symbol>> FirstSet;
        private Dictionary<Symbol, HashSet<Symbol>> FollowSet;

        public SDTScheme(List<Symbol> t, List<Symbol> v, Symbol s0)
        {
            T = t;
            V = v;
            S0 = s0;
            Prules = new List<SDTRule>();
            FirstSet = new Dictionary<Symbol, HashSet<Symbol>>();
            FollowSet = new Dictionary<Symbol, HashSet<Symbol>>();
        }

        public SDTScheme(List<Symbol> t, List<Symbol> v, Symbol s0, List<SDTRule> prules) : this(t, v, s0)
        {
            Prules = prules;
            ComputeFirstFollow();
        }

        public void ComputeFirstFollow()
        {
            ComputeFirstSets();
            ComputeFollowSets();
        }

        public void AddRule(Symbol leftNoTerm, List<Symbol> rightChain)
        {
            Prules.Add(new SDTRule(leftNoTerm, rightChain));
        }

        public override string Execute() { return null; }

        private void ComputeFirstSets()
        {
            foreach (Symbol term in T)
                FirstSet[term] = new HashSet<Symbol>() { term }; // FIRST[c] = {c}
            FirstSet[Symbol.Epsilon] = new HashSet<Symbol>() { Symbol.Epsilon };
            foreach (Symbol noTerm in V)
                FirstSet[noTerm] = new HashSet<Symbol>(); // First[X] = empty list
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (SDTRule rule in Prules)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    Symbol X = rule.LeftNoTerm;
                    foreach (Symbol Y in rule.RightChain)
                    {
                        foreach (Symbol curFirstSymb in FirstSet[Y])
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
                foreach (SDTRule rule in Prules)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    for (int indexOfSymbol = 0; indexOfSymbol < rule.RightChain.Count; ++indexOfSymbol)
                    {
                        Symbol curSymbol = rule.RightChain[indexOfSymbol];
                        if (T.Contains(curSymbol) || curSymbol == Symbol.Epsilon)
                        {
                            continue;
                        }
                        if (indexOfSymbol == rule.RightChain.Count - 1)
                        {
                            foreach (Symbol curFollowSymbol in FollowSet[rule.LeftNoTerm])
                            {
                                if (FollowSet[curSymbol].Add(curFollowSymbol))
                                {
                                    changes = true;
                                }
                            }
                        }
                        else
                        {
                            HashSet<Symbol> curFirst = First(rule.RightChain[indexOfSymbol + 1]);
                            bool epsFound = false;
                            foreach (Symbol curFirstSymbol in curFirst)
                            {
                                if (curFirstSymbol != Symbol.Epsilon)
                                {
                                    if (FollowSet[rule.RightChain[indexOfSymbol]].Add(curFirstSymbol))
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
                                foreach (Symbol curFollowSymbol in FollowSet[rule.LeftNoTerm])
                                {
                                    if (FollowSet[rule.RightChain[indexOfSymbol]].Add(curFollowSymbol))
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
            return FollowSet[X];
        }
    }

    /// Реализация L-атрибутного СУТ в процессе LL анализа
    class LLTranslator
    {
        protected SDTScheme G;
        protected Stack<Symbol> Stack;
        protected Dictionary<Symbol, Dictionary<Symbol, SDTRule>> Table;

        public LLTranslator(SDTScheme grammar)
        {
            G = grammar;
            G.ComputeFirstFollow();
            Table = new Dictionary<Symbol, Dictionary<Symbol, SDTRule>>();
            Stack = new Stack<Symbol>(); // создаем стек(магазин) для символов
            // Создадим таблицу синтаксического анализа для этой грамматики

            // Определим структуру таблицы
            foreach (Symbol noTermSymbol in G.V)
            {
                Table[noTermSymbol] = new Dictionary<Symbol, SDTRule>();
            }
            foreach (Symbol noTerm in grammar.V) // Рассмотрим последовательно все нетерминалы
            {

                foreach (SDTRule rule in G.Prules)
                {
                    if (rule.LeftNoTerm != noTerm) continue;
                    foreach (Symbol firstSymbol in G.First(rule.RightChain))
                    {
                        if (firstSymbol != Symbol.Epsilon)
                        {
                            // Добавить в таблицу
                            Table[noTerm][firstSymbol] = rule;
                        }
                        else
                        {
                            foreach (Symbol followSymbol in G.Follow(rule.LeftNoTerm))
                            {
                                Table[noTerm][followSymbol] = rule;
                            }
                        }
                    }
                }
            }
            // DebugMTable();
        }

        public bool Parse(List<Symbol> input)
        {
            Stack.Push(Symbol.Sentinel); // символ окончания входной последовательности
            Stack.Push(G.S0);
            input.Add(Symbol.Sentinel);
            int i = 0;
            Symbol curInputSymbol = input[i];
            Symbol curStackSymbol;
            do
            {
                curStackSymbol = Stack.Peek();
                if (G.T.Contains(curStackSymbol)) // в вершине стека находится терминал
                {
                    if (curInputSymbol == curStackSymbol) // распознанный символ равен вершине стека
                    {
                        // Извлечь из стека верхний элемент и распознать символ входной последовательности (ВЫБРОС)
                        Stack.Pop();
                        if (i != input.Count)
                        {
                            i++;
                        }
                        curInputSymbol = input[i];
                    }
                    else
                    {
                        // ERROR
                        return false;
                    }
                }
                else // если в вершине стека нетерминал
                {
                    SDTRule rule;    
                    if (Table[curStackSymbol].TryGetValue(curInputSymbol, out rule)) // в клетке[вершина стека, распознанный символ] таблицы разбора существует правило
                    {
                        // извлечь из стека элемент и занести в стек все терминалы и нетерминалы найденного в таблице правила в стек в порядке обратном порядку их следования в правиле
                        Stack.Pop();
                        foreach (Symbol chainSymbol in Enumerable.Reverse(rule.RightChain))
                        {
                            if (chainSymbol != Symbol.Epsilon)
                            {
                                Stack.Push(chainSymbol);
                            }
                        }
                    }
                    else
                    {
                        // ERROR
                        return false;
                    }
                }
            } while (Stack.Peek() != Symbol.Sentinel); // вершина стека не равна концу входной последовательности

            if (curInputSymbol != Symbol.Sentinel) // распознанный символ не равен концу входной последовательности
            {
                // ERROR
                return false;
            }
            
            return true;
        }

        public void DebugMTable()
        {
            Console.Write("  | ");
            foreach(Symbol s in G.T)
            {
                Console.Write(s.Value);
                Console.Write(" | ");
            }
            Console.Write(Symbol.Sentinel.Value);
            Console.Write(" | ");
            Console.WriteLine("");
            foreach(Symbol s in G.V)
            {
                Console.Write(s.Value);
                Console.Write(" | ");
                SDTRule rule;
                foreach(Symbol s2 in G.T)
                {
                    if (Table[s].TryGetValue(s2, out rule))
                    {
                        Console.Write(rule.LeftNoTerm.Value);
                        Console.Write(" -> ");
                        foreach (Symbol t in (rule.RightChain))
                        {
                            Console.Write(t.Value);
                        }
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    Console.Write(" | ");
                }
                if (Table[s].TryGetValue(Symbol.Sentinel, out rule))
                {
                    Console.Write(rule.LeftNoTerm.Value);
                    Console.Write(" -> ");
                    foreach (Symbol t in (rule.RightChain))
                    {
                        Console.Write(t.Value);
                    }
                }
                else
                {
                    Console.Write(" ");
                }
                Console.Write(" | ");
                Console.WriteLine("");
            }
        }
    }
}
