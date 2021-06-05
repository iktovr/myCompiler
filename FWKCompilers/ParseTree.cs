using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Translator
{
    /// Дерево разбора
    class ParseTree : ICloneable
    {
        public SDTSymbol Symbol; ///< Символ
        public LinkedList<ParseTree> Next; ///< Потомки
        public int Id = -1; ///< Номер правила

        public ParseTree(SDTSymbol symbol)
        {
            Symbol = symbol;
            Next = new LinkedList<ParseTree>();
        }

        /// Добавление дочернего узла
        public void Add(ParseTree node) { Next.AddLast(node); }

        /// Добавление дочернего узла
        public void Add(SDTSymbol symbol) { Next.AddLast(new ParseTree(symbol)); }

        /// Выполнение СУТ в процессе прямого обхода дерева
        public void Execute()
        {
            if (Next.Count == 0)
            {
                return;
            }

            // Подсчет индексов символов
            Dictionary<string, int> count = new Dictionary<string, int>();
            List<int> indexes = new List<int>();
            foreach (ParseTree node in Next)
            {
                if (node.Symbol is OperationSymbol || node.Symbol == SDTSymbol.Epsilon)
                {
                    indexes.Add(0);
                    continue;
                }

                int index;
                count.TryGetValue(node.Symbol.Value, out index);
                count[node.Symbol.Value] = index + 1;
                indexes.Add(index + 1);
            }

            Dictionary<string, SDTSymbol> symbols = new Dictionary<string, SDTSymbol>() { [Symbol.Value] = Symbol };

            int i = 0;
            foreach (ParseTree node in Next)
            {
                if (node.Symbol is OperationSymbol || node.Symbol == SDTSymbol.Epsilon)
                {
                    ++i;
                    continue;
                }

                symbols.Add(node.Symbol.Value + (count[node.Symbol.Value] > 1 || node.Symbol == Symbol ? indexes[i].ToString() : ""), node.Symbol);
                ++i;
            }

            foreach (ParseTree child in Next)
            {
                if (child.Symbol is OperationSymbol op && op != null)
                {
                    op.Value.Invoke(symbols);
                }
                else {
                    child.Execute();
                }
            }
        }

        /// Глубокая копия
        public object Clone()
        {
            ParseTree clone = new ParseTree((SDTSymbol)Symbol.Clone());
            foreach (ParseTree child in Next)
            {
                clone.Add((ParseTree)child.Clone());
            }
            return clone;
        }

        /// Печать дерева в консоль
        public void Print(int d = 0)
        {
            Console.WriteLine("{0," + (d * 4).ToString() + "}", Symbol);
            foreach (ParseTree child in Enumerable.Reverse(Next))
            {
                child.Print(d + 1);
            }
        }

        /// Генерация файла для GraphViz
        public void PrintToFile(string filenName)
        {
            StreamWriter f = new StreamWriter(filenName);
            f.WriteLine("graph ParseTree {");
            int id = 0;
            PrintToFile(f, ref id);
            f.WriteLine("}");
            f.Close();
        }

        /// Генерация файла для GraphViz
        private void PrintToFile(StreamWriter f, ref int id)
        {
            int parent_id = id;
            ++id;
            f.WriteLine("\t\"{0}_{1}\" [label=\"{0}\"]", Symbol, parent_id);
            foreach (ParseTree child in Next)
            {
                int child_id = id;
                child.PrintToFile(f, ref id);
                f.WriteLine("\t\"{0}_{2}\" -- \"{1}_{3}\";", Symbol, child.Symbol, parent_id, child_id);
            }
        }
    }

    /// Получение дерева разбора в ходе выполнения соответствующей СУТ
    /**
     * **Важно:** Символы грамматики не должны содержать атрибут "node"
     */
    class ParseTreeTranslator : LLTranslator
    {
        private SDTScheme OriginalGrammar; ///< Оригинальная грамматика
        private ParseTree Root = null; ///< Корень дерева
        private List<int> Output = new List<int>(); ///< Список номеров правил, применённых при разборе

        private readonly SDTSymbol StartNoTerm = "~S~"; ///< Стартовый символ пополненной грамматики

        /// Генератор операционных символов составления дерева
        private Types.Actions AttachNodes(string parent, List<string> childs, int id) =>
            new Types.Actions((S) => 
                {
                    ((ParseTree)S[parent]["node"]).Id = id;
                    foreach (string child in childs)
                        ((ParseTree)S[parent]["node"]).Add((ParseTree)S[child]["node"]);
                }
            );

        public ParseTreeTranslator(SDTScheme grammar)
        {
            // Составление транслирующей грамматики для построения дерева
            // Все символы клонируются
            List<SDTSymbol> V = new List<SDTSymbol>();
            foreach (SDTSymbol noTerm in grammar.V)
            {
                SDTSymbol newNoTerm = new SDTSymbol(noTerm);
                // Добавляется новый атрибут
                if (newNoTerm.Attributes == null) newNoTerm.Attributes = new Types.Attrs();
                newNoTerm.Attributes["node"] = new ParseTree(noTerm);
                V.Add(newNoTerm);
            }
            List<SDTSymbol> T = new List<SDTSymbol>();
            foreach (SDTSymbol term in grammar.T)
            {
                SDTSymbol newTerm = new SDTSymbol(term);
                // Добавляется новый атрибут
                if (newTerm.Attributes == null) newTerm.Attributes = new Types.Attrs();
                newTerm.Attributes["node"] = new ParseTree(term);
                T.Add(newTerm);
            }
            SDTScheme treeGrammar = new SDTScheme(T, V, grammar.S0.Value);

            // Составление правил новой грамматики
            int ruleNumber = 0;
            foreach (SDTRule rule in grammar.Prules)
            {
                SDTSymbol LeftNoTerm = new SDTSymbol(rule.LeftNoTerm.Value);
                List<SDTSymbol> RightChain = new List<SDTSymbol>();

                // Подсчет индексов для составления операционных символов
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

                // Составление правила
                List<string> childs = new List<string>();
                int symbNumber = 0;
                foreach (SDTSymbol symbol in rule.RightChain)
                {
                    // Операционные символы пропускаются
                    if (symbol is OperationSymbol)
                    {
                        ++symbNumber;
                        continue;
                    }

                    RightChain.Add(new SDTSymbol(symbol.Value));

                    // Эпсилон не учитывается операционным символом
                    if (symbol == SDTSymbol.Epsilon)
                    {
                        ++symbNumber;
                        continue;
                    }

                    childs.Add(symbol.Value + (count[symbol.Value] > 1 || symbol == rule.LeftNoTerm ? indexes[symbNumber].ToString() : ""));
                    ++symbNumber;
                }

                RightChain.Add(AttachNodes(rule.LeftNoTerm.Value, childs, ruleNumber)); // Вставка операционного символа составления дерева
                treeGrammar.AddRule(LeftNoTerm, RightChain);
                ++ruleNumber;
            }

            // Пополнение грамматики для возможности получить результат
            treeGrammar.V.Add(StartNoTerm);
            string oldS0 = treeGrammar.S0.Value;
            treeGrammar.AddRule(StartNoTerm, new List<SDTSymbol>() { treeGrammar.S0, new Types.Actions((S) => Root = (ParseTree)S[oldS0]["node"]) });
            treeGrammar.S0 = new SDTSymbol(StartNoTerm);

            OriginalGrammar = grammar;
            G = treeGrammar;
            G.ComputeFirstFollow();
            Table = new Dictionary<SDTSymbol, Dictionary<SDTSymbol, SDTRule>>();
            Stack = new Stack<SDTSymbol>();
            Construct();
            // DebugMTable();
        }

        public new ParseTree Parse(List<SDTSymbol> input)
        {
            foreach (SDTSymbol symbol in input)
            {
                // Добавляется новый атрибут
                if (symbol.Attributes == null) symbol.Attributes = new Types.Attrs();
                symbol["node"] = new ParseTree(symbol);
            }

            if (!base.Parse(input))
            {
                return null;
            }

            // Доработка дерева
            Stack<ParseTree> nodes = new Stack<ParseTree>();
            nodes.Push(Root);
            while (nodes.Count > 0)
            {
                ParseTree node = nodes.Pop();
                node.Next.AddFirst(new ParseTree(null)); // Фиктивный элемент
                LinkedListNode<ParseTree> iter = node.Next.First;
                foreach (SDTSymbol symbol in OriginalGrammar.Prules[node.Id].RightChain)
                {
                    if (iter.Value.Id >= 0)
                    {
                        nodes.Push(iter.Value);
                    }

                    // Удаляем ненужный атрибут node
                    if (iter.Value.Symbol != null && iter.Value.Symbol.Attributes != null)
                        iter.Value.Symbol.Attributes.Remove("node");
                    
                    // Добавляем оригинальные операционные символы и эпсилон
                    if (symbol is OperationSymbol || symbol == SDTSymbol.Epsilon)
                    {
                        node.Next.AddAfter(iter, new ParseTree(symbol));
                    }
                    iter = iter.Next;
                }

                if (iter.Value.Id >= 0)
                {
                    nodes.Push(iter.Value);
                }
                if (iter.Value.Symbol.Attributes != null) iter.Value.Symbol.Attributes.Remove("node");
                node.Next.RemoveFirst(); // Удаляем фиктивный элемент

            }
            
            return Root;
        }
    }
}