using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Translator
{
    /// Дерево разбора
    class ParseTree : ICloneable
    {
        public SDTSymbol Symbol;
        public LinkedList<ParseTree> Next;

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
            if (Symbol is OperationSymbol op)
            {
                // TODO
                op.Value.Invoke(null);
            }
            foreach (ParseTree child in Next)
            {
                child.Execute();
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
            Console.WriteLine("{0," + (d * 4).ToString() + "}", Symbol.Value);
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
            f.WriteLine("\t\"{0}{1}\" [label=\"{0}\"]", Symbol, parent_id);
            foreach (ParseTree child in Next)
            {
                int child_id = id;
                child.PrintToFile(f, ref id);
                f.WriteLine("\t\"{0}{2}\" -- \"{1}{3}\";", Symbol, child.Symbol, parent_id, child_id);
            }
        }
    }
}