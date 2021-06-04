using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Processor.AbstractGrammar;

namespace Translator
{
    /// Обычный символ грамматики
    class SDTSymbol : Symbol, ICloneable
    {
        public Dictionary<string, object> Attributes = null; ///< Атрибуты символа. Доступны также через индексатор

        public SDTSymbol(string value) : base(value) {}

        public SDTSymbol(string value, Dictionary<string, object> attributes) : this(value)
        {
            AddAttributes(attributes);
        }

        public SDTSymbol(SDTSymbol other)
        {
            SDTSymbol symbol = (SDTSymbol)other.Clone();
            Value = symbol.Value;
            Attributes = symbol.Attributes;
        }

        /// Неявное преобразование строки в SDTSymbol
        public static implicit operator SDTSymbol(string str) => new SDTSymbol(str);

        /// Неявное преобразование словаря в SDTSymbol
        /**
         *  Словарь должен иметь запись "NAME", значение которой будет использовано как имя/значение символа
         */
        public static implicit operator SDTSymbol(Dictionary<string, object> dict)
        {
            string name = (string)dict["NAME"];
            dict.Remove("NAME");
            return new SDTSymbol(name, dict);
        }

        /// Неявное преобразование функции в OperationSymbol
        public static implicit operator SDTSymbol(Types.Actions func) => new OperationSymbol(func);

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
            SDTSymbol clone = new SDTSymbol((string)Value.Clone());
            // SDTSymbol clone = new SDTSymbol(Value is null ? null : (string)Value.Clone());
            clone.AddAttributes(Attributes);
            return clone;
        }

        public static new readonly SDTSymbol Epsilon = new SDTSymbol(""); ///< Пустой символ
        public static new readonly SDTSymbol Sentinel = new SDTSymbol("$$"); ///< Cимвол конца строки / Символ дна стека
    }

    /// Операционный символ (Семантическое действие)
    class OperationSymbol : SDTSymbol
    {
        public new Types.Actions Value; ///< Функция семантического действия

        public OperationSymbol(Types.Actions value) : base(null)
        {
            Value = value;
        }

        public OperationSymbol(OperationSymbol other) : this(((OperationSymbol)other.Clone()).Value) {}

        public override string ToString() => "{}";

        /// Глубокая копия
        public override object Clone() => new OperationSymbol((Types.Actions)Value.Clone());
    }

    /// Запись синтеза. Используется в LL-трансляции.
    class SynthSymbol : SDTSymbol
    {
        public string Name; ///< Имя символа
        public Dictionary<string, SDTSymbol> Copies; ///< Копии других записей синтеза

        public SynthSymbol() : base(null)
        {
            Copies = new Dictionary<string, SDTSymbol>();
        }

        public SynthSymbol(string name, SDTSymbol symbol) : this()
        {
            Copies = new Dictionary<string, SDTSymbol>();
            Name = name;
            Value = symbol.Value;
            Attributes = symbol.Attributes;
        }

        public SynthSymbol(string name, string value, Dictionary<string, object> attributes) : this()
        {
            Copies = new Dictionary<string, SDTSymbol>();
            Name = name;
            Value = value;
            Attributes = attributes;
        }

        /// Добавление другой записи синтеза
        public void Add(SynthSymbol other)
        {
            Copies.Add(other.Name, other);
            foreach (KeyValuePair<string, SDTSymbol> pair in other.Copies)
            {
                Copies.Add(pair.Key, pair.Value);
            }
        }

        public override string ToString() => "s" + Name;
    }

    /// Правило синтаксически управляемой схемы трансляции
    class SDTRule
    {
        public SDTSymbol LeftNoTerm; ///< Левая часть продукции
        public List<SDTSymbol> RightChain; ///< Правая часть продукции

        public SDTRule(SDTSymbol leftNoTerm, List<SDTSymbol> rightChain)
        {
            LeftNoTerm = leftNoTerm;
            RightChain = rightChain;
        }

        /// " -> " разделитель. e вместо эпсилон. {} вместо операционного символа
        public override string ToString()
        {
            string str = LeftNoTerm.Value + " -> ";
            foreach (SDTSymbol symbol in RightChain)
            {
                if (symbol is OperationSymbol)
                {
                    str += "{}";
                }
                else if (symbol == SDTSymbol.Epsilon)
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
    class SDTScheme
    {
        public SDTSymbol S0; ///< Начальный символ
        public List<SDTSymbol> T; ///< Терминальные символы
        public List<SDTSymbol> V; ///< Нетерминальные символы
        public List<SDTRule> Prules; ///< Продукции

        private Dictionary<SDTSymbol, HashSet<SDTSymbol>> FirstSet;
        private Dictionary<SDTSymbol, HashSet<SDTSymbol>> FollowSet;

        public SDTScheme(List<SDTSymbol> t, List<SDTSymbol> v, SDTSymbol s0)
        {
            T = t;
            V = v;
            S0 = s0;
            Prules = new List<SDTRule>();
            FirstSet = new Dictionary<SDTSymbol, HashSet<SDTSymbol>>();
            FollowSet = new Dictionary<SDTSymbol, HashSet<SDTSymbol>>();

            S0.AddAttributes(V.Find(x => x == S0).Attributes);
        }

        public SDTScheme(List<SDTSymbol> t, List<SDTSymbol> v, SDTSymbol s0, List<SDTRule> prules) : this(t, v, s0)
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

        public void AddRule(SDTSymbol leftNoTerm, List<SDTSymbol> rightChain)
        {
            SDTRule rule = new SDTRule(leftNoTerm, rightChain);

            // Клонирование атрибутов для каждого символа
            if (rule.LeftNoTerm.Attributes is null)
            {
                rule.LeftNoTerm.AddAttributes(V.Find(x => x == rule.LeftNoTerm).Attributes);
            }
            foreach (SDTSymbol s in rule.RightChain)
            {
                if (s is OperationSymbol || !(s.Attributes is null) || s == SDTSymbol.Epsilon)
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

        public string Execute() { return null; }

        /// Перенесен с небольшими изменениями из LLParser
        private void ComputeFirstSets()
        {
            FirstSet.Clear();
            foreach (SDTSymbol term in T)
                FirstSet[term] = new HashSet<SDTSymbol>() { term }; // FIRST[c] = {c}
            FirstSet[SDTSymbol.Epsilon] = new HashSet<SDTSymbol>() { SDTSymbol.Epsilon }; // для единообразия
            foreach (SDTSymbol noTerm in V)
                FirstSet[noTerm] = new HashSet<SDTSymbol>(); // First[X] = empty set
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (SDTRule rule in Prules)
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    SDTSymbol X = rule.LeftNoTerm;
                    foreach (SDTSymbol Y in rule.RightChain)
                    {
                        if (Y is OperationSymbol)
                        {
                            continue;
                        }
                        foreach (SDTSymbol curFirstSymb in FirstSet[Y])
                        {
                            if (FirstSet[X].Add(curFirstSymb)) // Добавить а в FirstSets[X]
                            {
                                changes = true;
                            }
                        }
                        if (!FirstSet[Y].Contains(SDTSymbol.Epsilon))
                        {
                            break;
                        }
                    }
                }
            } // пока вносятся изменения
        }

        public HashSet<SDTSymbol> First(SDTSymbol X)
        {
            return FirstSet[X];
        }

        public HashSet<SDTSymbol> First(List<SDTSymbol> X)
        {
            HashSet<SDTSymbol> result = new HashSet<SDTSymbol>();
            foreach (SDTSymbol Y in X)
            {
                if (Y is OperationSymbol)
                {
                    continue;
                }

                foreach (SDTSymbol curFirstSymb in FirstSet[Y])
                {
                    result.Add(curFirstSymb);
                }
                if (!FirstSet[Y].Contains(SDTSymbol.Epsilon))
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
            foreach (SDTSymbol noTerm in V)
                FollowSet[noTerm] = new HashSet<SDTSymbol>();
            FollowSet[S0].Add(SDTSymbol.Sentinel);
            bool changes = true;
            while (changes)
            {
                changes = false;
                foreach (SDTRule rule in Prules)
                {
                    for (int curIndex = 0; curIndex < rule.RightChain.Count; ++curIndex)
                    {
                        SDTSymbol curSymbol = rule.RightChain[curIndex];
                        if (T.Contains(curSymbol) || curSymbol is OperationSymbol || curSymbol == SDTSymbol.Epsilon)
                        {
                            continue;
                        }

                        // Поиск следующего не операционного символа
                        int nextIndex = curIndex + 1;
                        while (nextIndex < rule.RightChain.Count && rule.RightChain[nextIndex] is OperationSymbol)
                        {
                            ++nextIndex;
                        }
                        SDTSymbol nextSymbol;
                        if (nextIndex < rule.RightChain.Count)
                        {
                            nextSymbol = rule.RightChain[nextIndex];
                        }
                        else
                        {
                            nextSymbol = SDTSymbol.Epsilon;
                        }

                        bool epsFound = false;
                        foreach (SDTSymbol symbol in First(nextSymbol))
                        {
                            if (symbol != SDTSymbol.Epsilon)
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
                            foreach (SDTSymbol symbol in FollowSet[rule.LeftNoTerm])
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

        public HashSet<SDTSymbol> Follow(SDTSymbol X)
        {
            return FollowSet[X];
        }
    }

    /// Реализация L-атрибутного СУТ в процессе LL анализа
    class LLTranslator
    {
        protected SDTScheme G; ///< АТ-грамматика
        protected Stack<SDTSymbol> Stack; ///< Стек символов
        protected Dictionary<SDTSymbol, Dictionary<SDTSymbol, SDTRule>> Table; ///< Управляющая таблица. Table[нетерминал][терминал]

        public LLTranslator(SDTScheme grammar)
        {
            G = grammar;
            G.ComputeFirstFollow();
            Table = new Dictionary<SDTSymbol, Dictionary<SDTSymbol, SDTRule>>();
            Stack = new Stack<SDTSymbol>();

            // Построение управляющей таблицы
            foreach (SDTSymbol noTermSymbol in G.V)
            {
                Table[noTermSymbol] = new Dictionary<SDTSymbol, SDTRule>();
            }

            // Для каждого правила A -> alpha
            foreach (SDTRule rule in G.Prules)
            {
                // Для каждого a из First(alpha)
                foreach (SDTSymbol firstSymbol in G.First(rule.RightChain))
                {
                    if (firstSymbol != SDTSymbol.Epsilon)
                    {
                        // Добавлем правило в таблицу на пересечение A и a
                        Table[rule.LeftNoTerm][firstSymbol] = rule;
                    }
                    // Если в First(alpha) входит эпсилон
                    else
                    {
                        // Для каждого b из Follow(A)
                        foreach (SDTSymbol followSymbol in G.Follow(rule.LeftNoTerm))
                        {
                            // Добавлем правило в таблицу на пересечении A и b
                            Table[rule.LeftNoTerm][followSymbol] = rule;
                        }
                    }
                }
            }
            // DebugMTable();
        }

        private static readonly SDTSymbol EndOfRule = "~EOR~"; ///< Символ конца правила

        /// Анализ строки
        public bool Parse(List<SDTSymbol> input)
        {
            input.Add(SDTSymbol.Sentinel); // Символ окончания входной последовательности

            Stack.Clear();
            Stack.Push(SDTSymbol.Sentinel); // Символ дна стека
            Stack.Push(new SynthSymbol(G.S0.Value, new SDTSymbol(G.S0)));
            Stack.Push(G.S0);

            int i = 0;
            SDTSymbol curInputSymbol = input[i];
            SDTSymbol curStackSymbol;
            do
            {
                curStackSymbol = Stack.Peek();
                if (curStackSymbol is OperationSymbol op && op != null) // в вершине стека операционный символ
                {
                    Stack.Pop();
                    // Функции операционного символа передаются записи синтеза до конца правила
                    SynthSymbol symbols = new SynthSymbol();
                    Stack<SDTSymbol> tmp = new Stack<SDTSymbol>();
                    while (Stack.Peek() != EndOfRule)
                    {
                        if (Stack.Peek() is SynthSymbol)
                        {
                            symbols.Add((SynthSymbol)Stack.Peek());
                        }
                        tmp.Push(Stack.Pop());
                    }

                    while (tmp.Count > 0)
                    {
                        Stack.Push(tmp.Pop());
                    }

                    op.Value.Invoke(symbols.Copies);
                }
                else if (curStackSymbol is SynthSymbol synth && synth != null) // в вершине стека запись синтеза
                {
                    Stack.Pop();
                    // Запись синтеза копируется в запись синтеза ниже, в пределах данного правила
                    Stack<SDTSymbol> tmp = new Stack<SDTSymbol>();
                    while (Stack.Count > 0 && !(Stack.Peek() is SynthSymbol))
                    {
                        if (Stack.Peek() == EndOfRule)
                        {
                            break;
                        }
                        tmp.Push(Stack.Pop());
                    }

                    if (Stack.Count > 0 && Stack.Peek() is SynthSymbol newSynth && newSynth != null)
                    {
                        newSynth.Add(synth);
                    }

                    while (tmp.Count > 0)
                    {
                        Stack.Push(tmp.Pop());
                    }
                }
                else if (curStackSymbol == EndOfRule) // в вершине стека символ конца правила
                {
                    Stack.Pop();
                }
                else if (G.T.Contains(curStackSymbol)) // в вершине стека терминал
                {
                    if (curInputSymbol == curStackSymbol) // распознанный символ равен вершине стека
                    {
                        Stack.Pop();
                        // В записи синтеза нетерминала обновляются атрибуты
                        Stack.Peek().Attributes = curInputSymbol.Attributes;
                        ++i;
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
                        Stack.Pop();

                        // Подсчет индексов символов
                        Dictionary<string, int> count = new Dictionary<string, int>();
                        List<int> indexes = new List<int>();
                        foreach (SDTSymbol s in rule.RightChain)
                        {
                            if (s is OperationSymbol || s == SDTSymbol.Epsilon)
                            {
                                indexes.Add(0);
                                continue;
                            }

                            int index;
                            count.TryGetValue(s.Value, out index);
                            count[s.Value] = index + 1;
                            indexes.Add(index + 1);
                        }

                        // Символы правила заносятся в обратном порядке в стек
                        SynthSymbol headSynth = new SynthSymbol(rule.LeftNoTerm.Value, rule.LeftNoTerm.Value, Stack.Peek().Attributes);
                        Stack.Push(EndOfRule); // Конец правила
                        Stack.Push(headSynth); // Локальная запись синтеза для заголовка
                        int j = rule.RightChain.Count - 1;
                        foreach (SDTSymbol rightSymbol in Enumerable.Reverse(rule.RightChain))
                        {
                            if (rightSymbol == SDTSymbol.Epsilon)
                            {
                                continue;
                            }

                            if (rightSymbol is OperationSymbol)
                            {
                                Stack.Push(rightSymbol);
                            }
                            else
                            {
                                SDTSymbol copy = new SDTSymbol(rightSymbol);
                                // Запись синтеза копии
                                Stack.Push(new SynthSymbol(copy.Value + (count[rightSymbol.Value] > 1 || copy == rule.LeftNoTerm ? indexes[j].ToString() : ""), copy));
                                // Копия символа
                                Stack.Push(copy);
                            }
                            --j;
                        }
                    }
                    else
                    {
                        // ERROR
                        return false;
                    }
                }
            } while (Stack.Peek() != SDTSymbol.Sentinel); // вершина стека не равна дну стека

            if (curInputSymbol != SDTSymbol.Sentinel) // распознанный символ не равен концу входной последовательности
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
            foreach (SDTSymbol s in G.V)
            {
                maxLenV = Math.Max(maxLenV, s.Value.Length);
            }
            int maxLenSymb = Math.Max(maxLenV, 2);
            foreach (SDTSymbol s in G.T)
            {
                maxLenSymb = Math.Max(maxLenSymb, s.Value.Length);
            }
            int maxLenRule = 0;
            foreach (SDTRule rule in G.Prules)
            {
                maxLenRule = Math.Max(maxLenRule, rule.RightChain.Count);
            }
            maxLenRule = maxLenV + 4 + maxLenRule * maxLenSymb;

            Console.Write("{0,-" + maxLenV.ToString() + "} | ", " ");
            foreach (SDTSymbol s in G.T)
            {
                Console.Write("{0,-" + maxLenRule.ToString() + "} | ", s.Value);
            }
            Console.WriteLine("{0,-" + maxLenRule.ToString() + "} | ", SDTSymbol.Sentinel.Value);
            foreach (SDTSymbol s in G.V)
            {
                Console.Write("{0,-" + maxLenV.ToString() + "} | ", s.Value);
                SDTRule rule;
                string str;
                foreach (SDTSymbol s2 in G.T)
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
                if (Table[s].TryGetValue(SDTSymbol.Sentinel, out rule))
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

    /// Сокращения для некоторых типов
    namespace Types
    {
        /// Функция семантического действия
        delegate void Actions(Dictionary<string, SDTSymbol> _);

        /// Словарь атрибутов
        class Attrs : Dictionary<string, object> {}
    }

    /// "Таблица" со стандартными семантическими действиями
    static class Actions
    {
        /// Печать чего угодно в консоль
        static public Types.Actions Print(object obj) =>
            (_) => Console.Write(obj.ToString());
    }
}
