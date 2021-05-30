using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace MPTranslator
{
    /// Обычный символ грамматики
    class Symbol : ICloneable
    {
        public string Value; ///< Строковое значение/имя символа
        public Dictionary<string, object> Attributes = null; ///< Атрибуты символа. Доступны также через индексатор

        public Symbol(string value)
        {
            Value = value;
        }

        public Symbol(string value, Dictionary<string, object> attributes) : this(value)
        {
            AddAttributes(attributes);
        }

        public Symbol(Symbol other)
        {
            Symbol symbol = (Symbol)other.Clone();
            Value = symbol.Value;
            Attributes = symbol.Attributes;
        }

        /// Неявное преобразование строки в Symbol
        public static implicit operator Symbol(string str) => new Symbol(str);

        /// Неявное преобразование словаря в Symbol
        /**
         *  Словарь должен иметь запись "NAME", значение которой будет использовано как имя/значение символа
         */
        public static implicit operator Symbol(Dictionary<string, object> dict)
        {
            string name = (string)dict["NAME"];
            dict.Remove("NAME");
            return new Symbol(name, dict);
        }

        /// Неявное преобразование функции в OperationSymbol
        public static implicit operator Symbol(Action<Dictionary<string, Symbol>> func) => new OperationSymbol(func);

        /// Доступ к атрибутам
        public object this[string name]
        {
            get { return Attributes[name]; }
            set { Attributes[name] = value; }
        }

        /// Клонирование словаря атрибутов
        public void AddAttributes(Dictionary<string, object> attributes)
        {
            if (attributes is null)
            {
                return;
            }
            Attributes = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> pair in attributes)
            {
                if (pair.Value is ICloneable obj)
                {
                    Attributes.Add(pair.Key, obj.Clone());
                }
                else
                {
                    Attributes.Add(pair.Key, pair.Value);
                }
            }
        }

        /// Глубокая копия
        public virtual object Clone()
        {
            Symbol clone = new Symbol((string)Value.Clone());;
            // Symbol clone = new Symbol(Value is null ? null : (string)Value.Clone());;
            clone.AddAttributes(Attributes);
            return clone;
        }

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

    /// Операционный символ (Семантическое действие)
    class OperationSymbol : Symbol
    {
        public new Action<Dictionary<string, Symbol>> Value; ///< Функция семантического действия

        public OperationSymbol(Action<Dictionary<string, Symbol>> value) : base(null)
        {
            Value = value;
        }

        public OperationSymbol(OperationSymbol other) : this(((OperationSymbol)other.Clone()).Value) {}

        /// Глубокая копия
        public override object Clone() => new OperationSymbol((Action<Dictionary<string, Symbol>>)Value.Clone());
    }

    /// Правило синтаксически управляемой схемы трансляции
    class SDTRule : Rule
    {
        public new Symbol LeftNoTerm; ///< Левая часть продукции
        public List<Symbol> RightChain; ///< Правая часть продукции

        public SDTRule(Symbol leftNoTerm, List<Symbol> rightChain)
        {
            LeftNoTerm = leftNoTerm;
            RightChain = rightChain;
        }

        /// " -> " разделитель. e вместо эпсилон. {} вместо операционного символа
        public override string ToString()
        {
            string str = LeftNoTerm.Value + " -> ";
            foreach (Symbol symbol in RightChain)
            {
                if (symbol is OperationSymbol)
                {
                    str += "{}";
                }
                else if (symbol == Symbol.Epsilon)
                {
                    str += "e";
                }
                else
                {
                    str += symbol.Value;
                }
            }
            return str;
        }
    }

    /// Синтаксически управляемая схема трансляции
    class SDTScheme : Grammar
    {
        public new Symbol S0; ///< Начальный символ
        public new List<Symbol> T; ///< Терминальные символы
        public new List<Symbol> V; ///< Нетерминальные символы
        public new List<SDTRule> Prules; ///< Продукции

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
            
            S0.AddAttributes(V.Find(x => x == S0).Attributes);
        }

        public SDTScheme(List<Symbol> t, List<Symbol> v, Symbol s0, List<SDTRule> prules) : this(t, v, s0)
        {
            foreach (SDTRule rule in prules)
            {
                AddRule(rule.LeftNoTerm, rule.RightChain);
            }
            ComputeFirstFollow();
        }

        public void ComputeFirstFollow()
        {
            ComputeFirstSets();
            ComputeFollowSets();
        }

        public void AddRule(Symbol leftNoTerm, List<Symbol> rightChain)
        {
            SDTRule rule = new SDTRule(leftNoTerm, rightChain);

            // Клонирование атрибутов для каждого символа
            if (rule.LeftNoTerm.Attributes is null)
            {
                rule.LeftNoTerm.AddAttributes(V.Find(x => x == rule.LeftNoTerm).Attributes);
            }
            foreach (Symbol s in rule.RightChain)
            {
                if (s is OperationSymbol || !(s.Attributes is null) || s == Symbol.Epsilon)
                {
                    continue;
                }

                if (V.Contains(s))
                {
                    s.AddAttributes(V.Find(x => x == s).Attributes);
                }
                else
                {
                    s.AddAttributes(T.Find(x => x == s).Attributes);
                }
            }
            Prules.Add(rule);
        }

        public override string Execute() { return null; }

        /// Перенесен с небольшими изменениями из LLParser
        private void ComputeFirstSets()
        {
            FirstSet.Clear();
            foreach (Symbol term in T)
                FirstSet[term] = new HashSet<Symbol>() { term }; // FIRST[c] = {c}
            FirstSet[Symbol.Epsilon] = new HashSet<Symbol>() { Symbol.Epsilon }; // для единообразия
            foreach (Symbol noTerm in V)
                FirstSet[noTerm] = new HashSet<Symbol>(); // First[X] = empty set
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
                        if (Y is OperationSymbol)
                        {
                            continue;
                        }
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

        /// Перенесен с большими изменениями из LLParser
        private void ComputeFollowSets()
        {
            FollowSet.Clear();
            foreach (Symbol noTerm in V)
                FollowSet[noTerm] = new HashSet<Symbol>();
            FollowSet[S0].Add(Symbol.Sentinel);
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (SDTRule rule in Prules)
                {
                    for (int curIndex = 0; curIndex < rule.RightChain.Count; ++curIndex)
                    {
                        Symbol curSymbol = rule.RightChain[curIndex];
                        if (T.Contains(curSymbol) || curSymbol is OperationSymbol || curSymbol == Symbol.Epsilon)
                        {
                            continue;
                        }

                        // Поиск следующего не операционного символа
                        int nextIndex = curIndex + 1;
                        while (nextIndex < rule.RightChain.Count && rule.RightChain[nextIndex] is OperationSymbol)
                        {
                            ++nextIndex;
                        }
                        Symbol nextSymbol;
                        if (nextIndex < rule.RightChain.Count)
                        {
                            nextSymbol = rule.RightChain[nextIndex];
                        }
                        else
                        {
                            nextSymbol = Symbol.Epsilon;
                        }

                        bool epsFound = false;
                        foreach (Symbol symbol in First(nextSymbol))
                        {
                            if (symbol != Symbol.Epsilon)
                            {
                                if (FollowSet[curSymbol].Add(symbol))
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
                            foreach (Symbol symbol in FollowSet[rule.LeftNoTerm])
                            {
                                if (FollowSet[curSymbol].Add(symbol))
                                {
                                    changes = true;
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

    /// Реализация постфиксной СУТ в процессе LR анализа
    // class PostfixTranslator : CanonicalLRParser
    // {

    // }

    /// Реализация L-атрибутного СУТ в процессе LL анализа
    class LLTranslator
    {
        protected SDTScheme G; ///< АТ-грамматика
        protected Stack<Symbol> Stack; ///< Стек символов
        protected Dictionary<Symbol, Dictionary<Symbol, SDTRule>> Table; ///< Управляющая таблица. Table[нетерминал][терминал]

        public LLTranslator(SDTScheme grammar)
        {
            G = grammar;
            G.ComputeFirstFollow();
            Table = new Dictionary<Symbol, Dictionary<Symbol, SDTRule>>();
            Stack = new Stack<Symbol>();
            
            foreach (Symbol noTermSymbol in G.V)
            {
                Table[noTermSymbol] = new Dictionary<Symbol, SDTRule>();
            }

            // Для каждого правила A -> alpha
            foreach (SDTRule rule in G.Prules)
            {
                // Для каждого a из First(alpha)
                foreach (Symbol firstSymbol in G.First(rule.RightChain))
                {
                    if (firstSymbol != Symbol.Epsilon)
                    {   
                        // Добавлем правило в таблицу на пересечение A и a
                        Table[rule.LeftNoTerm][firstSymbol] = rule;
                    }
                    // Если в First(alpha) входит эпсилон
                    else
                    {
                        // Для каждого b из Follow(A)
                        foreach (Symbol followSymbol in G.Follow(rule.LeftNoTerm))
                        {
                            // Добавлем правило в таблицу на пересечении A и b
                            Table[rule.LeftNoTerm][followSymbol] = rule;
                        }
                    }
                }
            }
            // DebugMTable();
        }

        /// Анализ строки
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
                if (curStackSymbol is OperationSymbol op && op != null) // в вершине стека операционный символ
                {
                    op.Value.Invoke(null);
                    Stack.Pop();
                }
                else if (G.T.Contains(curStackSymbol)) // в вершине стека терминал
                {
                    if (curInputSymbol == curStackSymbol) // распознанный символ равен вершине стека
                    {
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
                else // в вершине стека нетерминал
                {
                    SDTRule rule;    
                    if (Table[curStackSymbol].TryGetValue(curInputSymbol, out rule)) // в клетке[вершина стека, распознанный символ] таблицы разбора существует правило
                    {
                        // символы правила заносятся в обратном порядке в стек
                        Stack.Pop();
                        foreach (Symbol rightSymbol in Enumerable.Reverse(rule.RightChain))
                        {
                            if (rightSymbol != Symbol.Epsilon)
                            {
                                // в стек помещается копия символа
                                Stack.Push(new Symbol(rightSymbol));
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

        /// Печать управляющей таблицы
        public void DebugMTable()
        {
            int maxLenV = 0;
            foreach (Symbol s in G.V)
            {
                maxLenV = Math.Max(maxLenV, s.Value.Length);
            }
            int maxLenSymb = Math.Max(maxLenV, 2);
            foreach (Symbol s in G.T)
            {
                maxLenSymb = Math.Max(maxLenSymb, s.Value.Length);
            }
            int maxLenRule = 0;
            foreach (SDTRule rule in G.Prules)
            {
                maxLenRule = Math.Max(maxLenRule, rule.RightChain.Count);
            }
            maxLenRule = maxLenV + 4 + maxLenRule * maxLenSymb;

            Console.Write("{0,-" + maxLenV.ToString() +  "} | ", " ");
            foreach(Symbol s in G.T)
            {
                Console.Write("{0,-" + maxLenRule.ToString() + "} | ", s.Value);
            }
            Console.WriteLine("{0,-" + maxLenRule.ToString() + "} | ", Symbol.Sentinel.Value);
            foreach(Symbol s in G.V)
            {
                Console.Write("{0,-" + maxLenV.ToString() +  "} | ", s.Value);
                SDTRule rule;
                string str;
                foreach(Symbol s2 in G.T)
                {
                    if (Table[s].TryGetValue(s2, out rule))
                    {
                        str = rule.ToString();
                    }
                    else
                    {
                        str = " ";
                    }
                    Console.Write("{0,-" + maxLenRule.ToString() + "} | ", str);
                }
                if (Table[s].TryGetValue(Symbol.Sentinel, out rule))
                {
                    str = rule.ToString();
                }
                else
                {
                    str = " ";
                }
                Console.WriteLine("{0,-" + maxLenRule.ToString() + "} | ", str);
            }
        }
    }

    /// "Таблица" со стандартными семантическими действиями
    static class Actions
    {
        /// Печать чего угодно в консоль
        static public Action<Dictionary<string, Symbol>> Print(object obj) =>
            (_) => Console.Write(obj.ToString());
    }
}
