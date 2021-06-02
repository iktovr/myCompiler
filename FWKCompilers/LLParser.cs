using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using Processor.AbstractGrammar;

namespace Translator {

    class LLParser {
        private Grammar G;
        private Stack<string> Stack;
        private DataTable Table;
        public string OutputConfigure = "";
        Hashtable FirstSet = new Hashtable();
        Hashtable FollowSet = new Hashtable();

        public LLParser(Grammar grammar) //конструктор
        {
            this.G=grammar;
            Table=new DataTable("ControlTable");
            Stack=new Stack<string>(); //создаем стек(магазин) для символов
                                                                 // Создадим таблицу синтаксического анализа для этой грамматики

            // Определим структуру таблицы
            Table.Columns.Add(new DataColumn("VT",typeof(String)));
            Console.WriteLine("Создадим таблицу. Сначала создадим по столбцу для каждого из этих терминалов: ");
            foreach (var termSymbol in G.T) {
                Console.Write(termSymbol.symbol);
                Console.Write(", ");
                Table.Columns.Add(new DataColumn(termSymbol.symbol,typeof(Production)));
            }
            Console.WriteLine("\nТакже создаем строку для Эпсилон");
            Table.Columns.Add(new DataColumn("EoI",typeof(Production))); // Epsilon
            ComputeFirstSets(grammar); // Вычисляем множество First
            ComputeFollowSets(grammar); // Вычисляем множество Follow
            for (int i = 0; i<grammar.V.Count; i++) // Рассмотрим последовательно все нетерминалы
            {
                DataRow workRow = Table.NewRow(); //Новая строка
                workRow["VT"]=(string)grammar.V[i].symbol;

                Console.Write("Рассмотрим нетерминал ");
                Console.Write((grammar.V[i].symbol));
                Console.Write("\n");

                var rules = getRules((string)grammar.V[i].symbol);
                // Получим все правила, соответствующие текущему нетерминалу

                foreach (var rule in rules) {

                    var currFirstSet = First(rule.RHS.Cast<string>().ToList());
                    foreach (var firstSymbol in currFirstSet) {
                        if (firstSymbol!="") {
                            // Добавить в таблицу
                            Console.Write("   Первый символ правила ");
                            Console.Write(rule.LHS);
                            Console.Write(" -> ");
                            for (int j = 0; j<rule.RHS.Count; j++) {
                                Console.Write(rule.RHS[j]);
                            }
                            Console.Write(" - ");
                            Console.WriteLine(firstSymbol);

                            workRow[firstSymbol]=rule;
                            Console.Write("   Это правило заносим в таблицу на пересечении строки нетерминала ");
                            Console.Write(rule.LHS);
                            Console.Write(" и столбца терминала ");
                            Console.WriteLine(firstSymbol);
                            Console.Write("\n");
                        } else {
                            List<string> currFollowSet = Follow(rule.LHS.symbol);
                            foreach (var currFollowSymb in currFollowSet) {
                                string currFollowSymbFix = (currFollowSymb=="") ? "EoI" : currFollowSymb;
                                workRow[currFollowSymbFix]=rule;
                            }
                        }
                    }
                }
                Table.Rows.Add(workRow);
            }
        }

        public bool Parse(string input) {
            Stack.Push("EoS"); // символ окончания входной последовательности
            Stack.Push(G.S0.symbol);
            int i = 0;
            string currInputSymbol = input[i].ToString();
            string currStackSymbol;
            do {
                currStackSymbol=Stack.Peek();
                if (G.T.Contains(new Symbol(currStackSymbol))) // в вершине стека находится терминал
                {
                    if (currInputSymbol==currStackSymbol) // распознанный символ равен вершине стека
                    {
                        // Извлечь из стека верхний элемент и распознать символ входной последовательности (ВЫБРОС)
                        Stack.Pop();
                        if (i!=input.Length) {
                            i++;
                        }
                        currInputSymbol=(i==input.Length) ? "EoI" : input[i].ToString();
                    } else {
                        // ERROR
                        return false;
                    }
                } else // если в вершине стека нетерминал
                    {
                    DataRow custRows = Table.Select("VT = '"+currStackSymbol.Replace(@"'",@"''")+"'",null)[0];
                    if (!custRows.IsNull(currInputSymbol)) // в клетке[вершина стека, распознанный символ] таблицы разбора существует правило
                    {
                        //  извлечь из стека элемент и занести в стек все терминалы и нетерминалы найденного в таблице правила в стек в порядке обратном порядку их следования в правиле
                        Stack.Pop();
                        foreach (var chainSymbol in ((Production)custRows[currInputSymbol]).RHS.Cast<string>().Reverse().ToList()) {
                            if (chainSymbol!="") {
                                Stack.Push(chainSymbol);
                            }
                        }
                        OutputConfigure+=(((Production)custRows[currInputSymbol]).Id);
                    } else {
                        // ERROR
                        return false;
                    }
                }
            } while (Stack.Peek()!="EoS"); // вершина стека не равна концу входной последовательности

            if (i!=input.Length) // распознанный символ не равен концу входной последовательности
            {
                // ERROR
                return false;
            }

            return true;
        }

        public bool Parse1(string input)//Подробный вариант функции
        {
            Console.WriteLine("Приступаю к чтению цепочки символов...");
            Console.Write("      (");//Просто выводит текущее состояние на экран!
            Console.Write(input);
            Console.Write(", ");
            Console.Write("S");
            Console.Write(", ");
            Console.Write(OutputConfigure);
            Console.WriteLine(" )\n");

            Stack.Push("EoS"); // символ окончания входной последовательности
            Stack.Push(G.S0.symbol);
            int i = 0;
            string currInputSymbol = input[i].ToString();
            string currStackSymbol;
            do {
                currStackSymbol=Stack.Peek();
                if (G.T.Contains(new Symbol(currStackSymbol))) // в вершине стека находится терминал
                {
                    Console.Write("   В вершине стека находится терминал ");
                    Console.WriteLine(currStackSymbol);
                    if (currInputSymbol==currStackSymbol) // распознанный символ равен вершине стека
                    {
                        Console.WriteLine("      И данный терминал равен вершине стека...");
                        // Извлечь из стека верхний элемент и распознать символ входной последовательности (ВЫБРОС)
                        Console.WriteLine("      Извлекаю из стека верхний элемент, распознаю символ входной последовательности...");
                        Console.WriteLine("      ВЫБРОС!");
                        Stack.Pop();
                        if (i!=input.Length) {
                            i++;
                        }
                        currInputSymbol=(i==input.Length) ? "EoI" : input[i].ToString();

                        Console.Write("      (");

                        for (int k = i; k<input.Length; k++) {
                            Console.Write(input[k]);
                        }
                        Console.Write(", ");
                        for (int k = 0; k<Stack.Count; k++) {
                            string[] tmp = new string[Stack.Count];//Цикл просто выводит стек на экран!
                            Stack.CopyTo(tmp,0);
                            Console.Write(tmp[k]);
                        }
                        Console.Write(", ");
                        Console.Write(OutputConfigure);
                        Console.WriteLine(")\n");
                    } else {
                        // ERROR
                        Console.WriteLine("      ОШИБКА! Данный терминал не равен вершине стека!");
                        Console.Write(currInputSymbol);
                        Console.Write(" != ");
                        Console.WriteLine(currStackSymbol);
                        Console.Write("\n");
                        return false;
                    }
                } else // если в вершине стека нетерминал
                    {
                    Console.Write("   В вершине стека нетерминал ");
                    Console.WriteLine(currStackSymbol);
                    DataRow custRows = Table.Select("VT = '"+currStackSymbol.Replace(@"'",@"''")+"'",null)[0];
                    if (!custRows.IsNull(currInputSymbol)) // в клетке[вершина стека, распознанный символ] таблицы разбора существует правило
                    {
                        Console.Write("      В таблице разбора, в клетке [");
                        Console.Write(currStackSymbol);
                        Console.Write(",");
                        Console.Write(currInputSymbol);
                        Console.WriteLine("] существует правило...");
                        //  извлечь из стека элемент и занести в стек все терминалы и нетерминалы найденного в таблице правила в стек в порядке обратном порядку их следования в правиле
                        Console.WriteLine("      Извлекаю из стека элемент и заношу все терминалы и нетерминалы\n      найденного в таблице правила в стек в порядке обратном порядку их следования в правиле.");
                        Console.Write("      Вот так: ");
                        Stack.Pop();
                        foreach (var chainSymbol in ((Production)custRows[currInputSymbol]).RHS.Cast<string>().Reverse().ToList()) {
                            if (chainSymbol!="") {
                                Console.Write(chainSymbol);
                                Stack.Push(chainSymbol);
                            }
                        }
                        Console.WriteLine();
                        OutputConfigure+=(((Production)custRows[currInputSymbol]).Id);
                        Console.Write("      Использовано правило под номером ");
                        Console.WriteLine(((Production)custRows[currInputSymbol]).Id);
                        Console.Write("      (");

                        //int i = 0;
                        //string currInputSymbol = input[i].ToString();

                        for (int k = i; k<input.Length; k++) {
                            Console.Write(input[k]);
                        }
                        Console.Write(", ");
                        for (int k = 0; k<Stack.Count; k++) {
                            string[] tmp = new string[Stack.Count];//Цикл просто выводит стек на экран!
                            Stack.CopyTo(tmp,0);
                            Console.Write(tmp[k]);
                        }
                        Console.Write(", ");
                        Console.Write(OutputConfigure);
                        Console.WriteLine(")\n");
                    } else {
                        // ERROR
                        Console.Write("      ОШИБКА! Не существует правила в клетке [");
                        Console.Write(currStackSymbol);
                        Console.Write(",");
                        Console.Write(currInputSymbol);
                        Console.WriteLine("]!\n");
                        return false;
                    }
                }
            } while (Stack.Peek()!="EoS"); // вершина стека не равна концу входной последовательности

            if (i!=input.Length) // распознанный символ не равен концу входной последовательности
            {
                // ERROR
                return false;
            }

            return true;
        }

        public List<Production> getRules(string noTermSymbol) {
            List<Production> result = new List<Production>();
            for (int i = 0; i<G.P.Count; ++i) {
                Production currRule = (Production)G.P[i];
                if (currRule.LHS.symbol == noTermSymbol) {
                    result.Add(currRule);
                }
            }
            return result;
        }

        public void ComputeFirstSets(Grammar grammar) {
            for (int i = 0; i<grammar.T.Count; i++)
                FirstSet[grammar.T[i].symbol]=new HashSet<string>() { grammar.T[i].symbol };   // FIRST[c] = {c}*/
            for (int i = 0; i<grammar.V.Count; i++)
                FirstSet[grammar.V[i].symbol]=new HashSet<string>();                     //First[x] = empty list
            bool changes = true;
            while (changes) {
                changes=false;
                for (int i = 0; i<grammar.P.Count; ++i) {
                    // Для каждого правила X-> Y0Y1…Yn
                    var currRule = (Production)(grammar.P[i]);
                    string X = currRule.LHS.symbol;
                    List<string> Y = currRule.RHS.Cast<string>().ToList();
                    for (int k = 0; k<Y.Count; k++) {
                        foreach (string currFirstSymb in (HashSet<string>)FirstSet[Y[k]]) {
                            if (((HashSet<string>)FirstSet[X]).Add(currFirstSymb)) //Добавить а в FirstSets[X]
                            {
                                changes=true;
                            }
                        }
                        if (!((HashSet<string>)FirstSet[Y[k]]).Contains("")) {
                            break;
                        }
                    }
                }
            } //  пока вносятся изменения
        }
        public List<string> First(string X) { return ((HashSet<string>)FirstSet[X]).ToList(); }
        public List<string> First(List<string> Y) {
            List<string> result = new List<string>();
            for (int k = 0; k<Y.Count; k++) {
                foreach (string currFirstSymb in (HashSet<string>)FirstSet[Y[k]]) {
                    result.Add(currFirstSymb);
                }
                if (!((HashSet<string>)FirstSet[Y[k]]).Contains("")) {
                    break;
                }
            }
            return result;
        }

        public void ComputeFollowSets(Grammar grammar) {
            for (int i = 0; i<grammar.V.Count; i++)
                FollowSet[grammar.V[i].symbol]=new HashSet<string>();
            FollowSet[G.S0]=new HashSet<string>() { "" };
            bool changes = true;
            while (changes) {
                changes=false;
                for (int i = 0; i<grammar.P.Count; ++i) {
                    // Для каждого правила X-> Y0Y1…Yn
                    Production currRule = (Production)(grammar.P[i]);
                    List<string> rightChain = currRule.RHS.Cast<string>().ToList();
                    for (int indexOfSymbol = 0; indexOfSymbol<rightChain.Count; ++indexOfSymbol) {
                        string currSymbol = rightChain[indexOfSymbol];
                        if (G.T.Contains(new Symbol(currSymbol))) {
                            continue;
                        }
                        if (indexOfSymbol==rightChain.Count-1) {
                            foreach (string currFollowSymbol in (HashSet<string>)FollowSet[currRule.LHS]) {
                                if (((HashSet<string>)FollowSet[rightChain[indexOfSymbol]]).Add(currFollowSymbol)) {
                                    changes=true;
                                }
                            }
                        } else {
                            List<string> currFirst = First(rightChain[indexOfSymbol+1]);
                            bool epsFound = false;
                            foreach (var currFirstSymbol in currFirst) {
                                if (currFirstSymbol!="") {
                                    if (((HashSet<string>)FollowSet[rightChain[indexOfSymbol]]).Add(currFirstSymbol)) {
                                        changes=true;
                                    }
                                } else {
                                    epsFound=true;
                                }
                            }
                            if (epsFound) {
                                foreach (var currFollowSymbol in (HashSet<string>)FollowSet[currRule.LHS]) {
                                    if (((HashSet<string>)FollowSet[rightChain[indexOfSymbol]]).Add(currFollowSymbol)) {
                                        changes=true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public List<string> Follow(string X) { return ((HashSet<string>)FollowSet[X]).ToList(); }

    }
}