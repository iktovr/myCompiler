using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;

namespace MPTranslator
{
    
    class Program
    {

        static void Dialog()
        {
            Console.WriteLine("\n----------------------------------------------");
            Console.WriteLine("DKA ( lab 2 ) ........................ Enter 1");
            Console.WriteLine("Covert NDKA to DKA ");
            Console.WriteLine("example ....................... Enter 2.1");
            Console.WriteLine("lab 3   ....................... Enter 2");
            Console.WriteLine("Grammar ( lab 4 - 6 )  ............... Enter 3");
            Console.WriteLine("MP - auto ( lab 7,8 )  ............... Enter 4");
            Console.WriteLine("MPs...................Enter 4.1");
            Console.WriteLine("LL - analizator ( lab 9 - 11 )  ...... Enter 5");
            Console.WriteLine("LL - analizator (debug-mode) ( lab 9 - 11 )  ...... Enter 5.1");
            Console.WriteLine("LR - analizator");
            Console.WriteLine("lab 12 - 16   ................. Enter 6");
            Console.WriteLine("example ....................... Enter 6.1");
            Console.WriteLine("MP_Automate with delta rules   ................. Enter 7");
            Console.WriteLine("MP_Translator   ................. Enter 8");
            Console.WriteLine("MP_Transl_Translator   ................. Enter 9");
            Console.WriteLine("Chain Translation example ................. Enter 10");
            Console.WriteLine("L-attribute translation grammar ........... Enter 11");
        }

        static string[,] Mtable(myGrammar G)
        { /// построение заданной таблицы      
            string[,] t = new string[G.T.Count + G.V.Count + 2, G.T.Count + 1];
            t[0, 3] = "(F+L),1";
            t[1, 3] = "(L*),2";
            t[1, 0] = "i,3";
            t[2, 3] = "F,4";
            t[2, 0] = "F,4";
            t[3, 0] = "выброс";
            t[4, 1] = "выброс";
            t[5, 2] = "выброс";
            t[6, 3] = "выброс";
            t[7, 4] = "выброс";
            t[8, 5] = "допуск";
            return t;
        }

        //// алгоритм разбора
        static int parsing(string s, int c, myGrammar G)
        { 
            Stack z = new Stack();  // магазин
            string eline = null;
            string zline = null;
            string qline = null;

            z.Push("$");
            z.Push(s);
            string[,] table = Mtable(G);  // таблица
            ArrayList outstr = new ArrayList();  // выходная лента
            int i, j;
            bool d = false;  // допуск

            for (int k = 0; k < s.Length + 1; k++)
            {
                if (c == 0) return 3;  // разбор по шагам
                if (k == s.Length) j = G.T.Count;   // символ пустой строки
                else
                {
                    if (!G.T.Contains(s[k].ToString())) { d = false; break; }  // не символ алфавита
                    j = G.T.IndexOf(s[k].ToString());  // выбор столбца
                }
                if (G.V.Contains(z.Peek())) i = G.V.IndexOf(z.Peek());  // выбор строки
                else if (z.Peek().ToString() == "$") i = G.T.Count + G.V.Count;
                else i = G.T.IndexOf(z.Peek()) + G.V.Count;
                if (k != s.Length + 1)
                {   // вывод на экран
                    eline = "";
                    zline = "";
                    qline = "";
                    for (int m = k; m < s.Length; m++) eline = eline + (s[m].ToString());
                    Stack z1 = new Stack();
                    while (z.Count != 0)
                    {
                        zline = zline + (z.Peek().ToString());
                        z1.Push(z.Pop());
                    }
                    while (z1.Count != 0)
                        z.Push(z1.Pop());
                    foreach (string o in outstr)
                        qline = qline + o;
                }
                if (table[i, j] == "выброс") { z.Pop(); continue; }   // выброс символа
                else if (table[i, j] == "допуск") { d = true; break; }   // допуск строки
                else if (table[i, j] == null) { d = false; break; }  // ошибка
                else
                {   // запись в магазин и на выходную ленту
                    int zp = table[i, j].IndexOf(',');    // разбор ячейки таблицы до запятой
                    z.Pop();
                    for (int l = zp - 1; l >= 0; l--)
                    {
                        z.Push(table[i, j][l].ToString());   // в магазин
                    }
                    outstr.Add(table[i, j][zp + 1].ToString());  // на ленту
                    k--;
                }
                c--;
            }
            if (d) return 1;
            else return 2;
        }

        static void Main()
        {
            while (true)
            {
                Dialog();
                switch (Console.ReadLine())
                {
                    case "0": //МП - автоматы Lungo NON DET MP Automat
                        myMp nodmp = new myMp(
                            new ArrayList() { "q", "qf" },
                            new ArrayList() { "v", "+", "*", "(", ")" },
                            new ArrayList() { "" },
                            "q0",
                            "S",
                            new ArrayList() { "qf" });

                        nodmp.addDeltaRule("q", "v", "v", new ArrayList() { "q" }, new ArrayList() { "e" });
                        nodmp.addDeltaRule("q", "+", "+", new ArrayList() { "q" }, new ArrayList() { "e" });
                        nodmp.addDeltaRule("q", "*", "*", new ArrayList() { "q" }, new ArrayList() { "e" });
                        nodmp.addDeltaRule("q", "(", "(", new ArrayList() { "q" }, new ArrayList() { "e" });
                        nodmp.addDeltaRule("q", ")", ")", new ArrayList() { "q" }, new ArrayList() { "e" });

                        nodmp.addDeltaRule("q", "e", "S", new ArrayList() { "q" }, new ArrayList() { "S", "+", "F" });
                        nodmp.addDeltaRule("q", "e", "S", new ArrayList() { "q" }, new ArrayList() { "F" });
                        nodmp.addDeltaRule("q", "e", "F", new ArrayList() { "q" }, new ArrayList() { "F", "*", "L" });
                        nodmp.addDeltaRule("q", "e", "F", new ArrayList() { "q" }, new ArrayList() { "L" });
                        nodmp.addDeltaRule("q", "e", "L", new ArrayList() { "q" }, new ArrayList() { "v" });
                        nodmp.addDeltaRule("q", "e", "L", new ArrayList() { "q" }, new ArrayList() { "(", "S", ")" });

                        Console.Write("Debug Mp ");
                        nodmp.debugDelta();
                        Console.WriteLine("\nEnter the line :");
                        string str = "v + v"; //Console.ReadLine();
                        str += 'e';

                        bool b = nodmp.Execute_(str,0, str.Length);
                        if (b) { Console.WriteLine("Yes"); }
                        else  Console.WriteLine("NO");
                        //mp.Execute(str, 0, str.Length);
                        /*if (mp.ans != "")
                          Console.WriteLine(mp.ans);
                        else
                          Console.WriteLine("NO");
                          */
                        break;

                    case "1":
                        myAutomate ka = new myAutomate(new ArrayList() { "S0", "A", "B", "C", "Bqf" },
                                                       new ArrayList() { "0", "1", "+" },
                                                       new ArrayList() { "Bqf" },
                                                       "S0");
                        ka.AddRule("S0", "0", "A");
                        ka.AddRule("A", "0", "A");
                        ka.AddRule("A", "1", "A");
                        ka.AddRule("A", "+", "B");
                        ka.AddRule("B", "0", "C");
                        ka.AddRule("C", "1", "Bqf");
                        ka.AddRule("Bqf", "0", "C");

                        Console.WriteLine("Enter line to execute :");
                        ka.Execute(Console.ReadLine());
                        break;

                    case "1.2":
                        myGrammar Gram = new myGrammar(new ArrayList() { "0", "1" },
                                                       new ArrayList() { "S0", "A", "B" },
                                                       "S0");
                        //P
                        Gram.AddRule("S0", new ArrayList() { "0" });
                        Gram.AddRule("S0", new ArrayList() { "0", "A" });
                        Gram.AddRule("A", new ArrayList() { "1", "B" });
                        Gram.AddRule("B", new ArrayList() { "0" });
                        Gram.AddRule("B", new ArrayList() { "0", "A" });

                        //From Automaton Grammar to State Machine(KA)
                        myAutomate KA = Gram.Transform();
                        KA.DebugAuto();
                        break;

                    case "2.1":
                        myAutomate ka_example = new myAutomate(new ArrayList() { "S0", "1", "2", "3", "4", "5",
                                                                               "6", "7", "8", "9", "qf" },
                                                            new ArrayList() { "a", "b" },
                                                            new ArrayList() { "qf" },
                                                            "S0");
                        ka_example.AddRule("S0", "", "1");
                        ka_example.AddRule("S0", "", "7");
                        ka_example.AddRule("1", "", "2");
                        ka_example.AddRule("1", "", "4");
                        ka_example.AddRule("2", "a", "3");
                        ka_example.AddRule("4", "b", "5");
                        ka_example.AddRule("3", "", "6");
                        ka_example.AddRule("5", "", "6");
                        ka_example.AddRule("6", "", "1");
                        ka_example.AddRule("6", "", "7");
                        ka_example.AddRule("7", "a", "8");
                        ka_example.AddRule("8", "b", "9");
                        ka_example.AddRule("9", "b", "qf");

                        myAutomate dkaEX = new myAutomate();
                        dkaEX.BuildDeltaDKAutomate(ka_example);
                        dkaEX.DebugAuto();
                        Console.WriteLine("Enter line to execute :");
                        dkaEX.Execute(Console.ReadLine());
                        break;

                    case "2":
                        myAutomate ndka = new myAutomate(new ArrayList() { "S0", "A", "B", "C", "qf" },
                                                         new ArrayList() { "1", "0", "+" },
                                                         new ArrayList() { "qf" },
                                                         "S0");
                        
                        ndka.AddRule("S0", "0", "A");
                        ndka.AddRule("A", "0", "A");
                        ndka.AddRule("A", "1", "A");
                        ndka.AddRule("A", "+", "B");
                        ndka.AddRule("B", "0", "C");
                        ndka.AddRule("C", "1", "B");
                        ndka.AddRule("C", "1", "qf");


                        myAutomate dka = new myAutomate();
                        dka.BuildDeltaDKAutomate(ndka);
                        dka.DebugAuto();
                        Console.WriteLine("Enter line to execute :");
                        dka.Execute(Console.ReadLine());
                        break;

                    case "3":
                        myGrammar G = new myGrammar(new ArrayList() { "a", "b", "c", "d" },
                                                    new ArrayList() { "S", "A", "B", "C", "F" },
                                                    "S");

                        G.AddRule("S", new ArrayList() { "" });
                        G.AddRule("S", new ArrayList() { "c", "F", "B" });
                        //G.AddRule("A", new ArrayList() { "b" });
                        G.AddRule("A", new ArrayList() { "" });
                        G.AddRule("B", new ArrayList() { "B", "c" });
                        G.AddRule("B", new ArrayList() { "b" });
                        G.AddRule("C", new ArrayList() { "a" });
                        G.AddRule("F", new ArrayList() { "d" });
                        G.AddRule("B", new ArrayList() { "" });
                        G.AddRule("F", new ArrayList() { "A", "B" });
                        Console.WriteLine("Grammar:");
                        G.Debug("T", G.T);
                        G.Debug("T", G.V);
                        G.DebugPrules();

                        myGrammar G1 = G.EpsDelete();
                        G1.DebugPrules();

                        myGrammar G2 = G1.ChainRuleDelete();
                        G2.DebugPrules();

                        myGrammar G3 = G2.unUsefulDelete();
                        G3.DebugPrules();

                        myGrammar G4 = G3.LeftRecursDelete();
                        G4.DebugPrules();
                        // G4 - приведенная грамматика

                        Console.WriteLine("--------------------------------------------");
                        Console.WriteLine("Normal Grammatic:");
                        G4.Debug("T", G4.T);
                        G4.Debug("V", G4.V);
                        G4.DebugPrules();
                        Console.Write("Start symbol: ");
                        Console.WriteLine(G4.S0 + "\n");
                        break;

                    case "4": //МП - автоматы
                        /*myGrammar kcGrammar = new myGrammar(new ArrayList() { "a", "b", "d" },
                                                            new ArrayList() { "S", "C", "S'" },
                                                            "S");

                        kcGrammar.AddRule("S", new ArrayList() { "a" });
                        kcGrammar.AddRule("S", new ArrayList() { "C", "a", "S'" });
                        kcGrammar.AddRule("S", new ArrayList() { "a", "S'" });
                        kcGrammar.AddRule("S", new ArrayList() { "C", "a" });
                        kcGrammar.AddRule("S'", new ArrayList() { "b", "S'" });
                        kcGrammar.AddRule("S'", new ArrayList() { "b" });
                        kcGrammar.AddRule("C", new ArrayList() { "d" });

                        Console.Write("Debug KC-Grammar ");
                        kcGrammar.DebugPrules();

                        myMp MP = new myMp(kcGrammar);
                        Console.Write("Debug Mp ");
                        MP.debugDelta();

                        Console.WriteLine("\nEnter the line :");
                        Console.WriteLine(MP.Execute(Console.ReadLine()).ToString());*/
                        break;

                    case "4.1":
                        Console.WriteLine("Выберите КС-грамматику: ");

                        myGrammar kcGr1 = new myGrammar(new ArrayList() { "a", "b" },
                                                        new ArrayList() { "S", "A", "B" },
                                                        "S");

                        kcGr1.AddRule("S", new ArrayList() { "a", "A", "b" });
                        kcGr1.AddRule("A", new ArrayList() { "a", "B", "b" });
                        kcGr1.AddRule("B", new ArrayList() { "a", "b" });
                        Console.Write("Debug KC-Grammar ");
                        kcGr1.DebugPrules();

                        myGrammar kcGr2 = new myGrammar(new ArrayList() { "i", "=" },
                                                        new ArrayList() { "S", "F", "L" },
                                                        "S");

                        kcGr2.AddRule("S", new ArrayList() { "F", "=", "L" });
                        kcGr2.AddRule("F", new ArrayList() { "i" });
                        kcGr2.AddRule("L", new ArrayList() { "F" });
                        Console.Write("Debug KC-Grammar ");
                        kcGr2.DebugPrules();

                        string ans = "y";
                        while (ans == "y")
                        {
                            Console.WriteLine("Введите 1, 2 или 3");
                            switch (Console.ReadLine())
                            {
                                case "1":
                                    myGrammar kcGrammar = new myGrammar(new ArrayList() { "a", "b", "d" },
                                    new ArrayList() { "S", "C", "D" },
                                    "S");

                                    kcGrammar.AddRule("S", new ArrayList() { "a" });
                                    kcGrammar.AddRule("S", new ArrayList() { "C", "a", "D" });
                                    kcGrammar.AddRule("S", new ArrayList() { "a", "D" });
                                    kcGrammar.AddRule("S", new ArrayList() { "C", "a" });
                                    kcGrammar.AddRule("D", new ArrayList() { "b", "D" });
                                    kcGrammar.AddRule("D", new ArrayList() { "b" });
                                    kcGrammar.AddRule("C", new ArrayList() { "d" });
                                    kcGrammar.DebugPrules();

                                    myMp mpA1 = new myMp(new ArrayList() { "a", "b", "d" },
                                                         new ArrayList() { "S", "C", "D" },
                                                         new ArrayList() { "S", "C", "D", "a", "b", "d" },
                                                         "q",
                                                         "S",
                                                         new ArrayList() { "q" });
                                    mpA1.addDeltaRule("q", "", "S", new ArrayList() { "q" }, new ArrayList() { "C", "a", "D" });
                                    mpA1.addDeltaRule("q", "", "D", new ArrayList() { "q" }, new ArrayList() { "b" });
                                    mpA1.addDeltaRule("q", "", "S", new ArrayList() { "q"}, new ArrayList() { "a" });
                                    mpA1.addDeltaRule("q", "", "S", new ArrayList() {"q"}, new ArrayList() { "a", "D" });
                                    mpA1.addDeltaRule("q", "", "S", new ArrayList() {"q"}, new ArrayList() { "C", "a" });
                                    mpA1.addDeltaRule("q", "", "D", new ArrayList() {"q"}, new ArrayList() { "b", "D" });
                                    mpA1.addDeltaRule("q", "", "C", new ArrayList() {"q"}, new ArrayList() { "d" });
                                    mpA1.addDeltaRule("q", "a", "a", new ArrayList() { "q" }, new ArrayList() { "" });
                                    mpA1.addDeltaRule("q", "b", "b", new ArrayList() { "q" }, new ArrayList() { "" });
                                    mpA1.addDeltaRule("q", "d", "d", new ArrayList() { "q" }, new ArrayList() { "" });

                                    mpA1.debugDelta();
                                    Console.WriteLine("\nВведите строку :");
                                    Console.WriteLine(mpA1.Execute(Console.ReadLine()).ToString());
                                    break;
                                
                                case "2":
                                    myMp mpA2 = new myMp(new ArrayList() { "q0", "q1", "q2", "qf" },
                                                         new ArrayList() { "i", "=", "" },
                                                         new ArrayList() { "i", "=", "", "S", "F", "L" },
                                                         "q0",
                                                         "S",
                                                         new ArrayList() { "qf" });
                                    mpA2.addDeltaRule("q0", "", "S", new ArrayList() { "q1" }, new ArrayList() { "F", "=", "L" });
                                    mpA2.addDeltaRule("q1", "", "F", new ArrayList() { "q1" }, new ArrayList() { "i" });
                                    mpA2.addDeltaRule("q1", "", "L", new ArrayList() { "q1" }, new ArrayList() { "i" });
                                    mpA2.addDeltaRule("q1", "i", "i", new ArrayList() { "q1" }, new ArrayList() { "" });
                                    mpA2.addDeltaRule("q1", "=", "=", new ArrayList() { "q1" }, new ArrayList() { "" });
                                    Console.Write("Debug Mp ");
                                    mpA2.debugDelta();
                                    Console.WriteLine("\nВведите строку :");
                                    Console.WriteLine(mpA2.Execute(Console.ReadLine()).ToString());
                                    break;

                                case "3":
                                    Translator translator = new Translator(new ArrayList() { "q0", "q1", "q2", "qf" },
                                                                           new ArrayList() { "i", "=", "" },
                                                                           new ArrayList() { "i", "=", "", "S", "F", "L" },
                                                                           "q0",
                                                                           "S",
                                                                           new ArrayList() { "qf" });

                                    translator.addDeltaRule("q0", "", "E",  new ArrayList() { "q1" }, new ArrayList() { "E", "+", "T" }, new ArrayList() { "E" , "T", "+"});
                                    translator.addDeltaRule("q1", "", "E",  new ArrayList() { "q1" }, new ArrayList() { "T" }, new ArrayList() { "T" });
                                    translator.addDeltaRule("q1", "", "T",  new ArrayList() { "q1" }, new ArrayList() { "P" }, new ArrayList() { "P" });
                                    translator.addDeltaRule("q1", "", "P",  new ArrayList() { "q1" }, new ArrayList() { "i" }, new ArrayList() { "i" });

                                    translator.addDeltaRule("q1", "i", "i",   new ArrayList() { "q1" }, new ArrayList() { "" }, new ArrayList() { "i" }); // делать проверку 5 и 6  на пустоту
                                    translator.addDeltaRule("q1", "(", "(",   new ArrayList() { "q1" }, new ArrayList() { "" }, new ArrayList() {  "" });  // и при е выводить на постоянную
                                    translator.addDeltaRule("q1", ")", ")",   new ArrayList() { "q1" }, new ArrayList() { "" }, new ArrayList() {  "" });  // основу на строку
                                    translator.addDeltaRule("q1", "+", "+",   new ArrayList() { "q1" }, new ArrayList() { "" }, new ArrayList() { "+" });
                                    translator.addDeltaRule("q1", "*", "*",   new ArrayList() { "q1" }, new ArrayList() { "" }, new ArrayList() { "*" });
                                    translator.addDeltaRule("q1",  "",  "",   new ArrayList() { "qf" }, new ArrayList() { "" }, new ArrayList() {  "" });

                                    Console.Write("Debug Mp ");
                                    translator.debugDelta();
                                    Console.WriteLine("\nВведите строку :");
                                    Console.WriteLine(translator.Execute(Console.ReadLine()).ToString());
                                    //Console.WriteLine(translator.Execute("i+i").ToString());
            
                                    break;

                            } //end switch 1 or 2
                            Console.WriteLine("Продолжить (y - yes, n - no)");
                            ans = Console.ReadLine();
                        } //end while
                        break;

                    case "5": // LL Разбор
                        myGrammar example = new myGrammar(new ArrayList() { "i", "=", "*", "(", ")", "" },
                                                          new ArrayList() { "S", "S'", "F" },
                                                          "S");

                        example.AddRule("S", new ArrayList() { "F", "S'" });
                        example.AddRule("S'", new ArrayList() { "=", "F" });
                        example.AddRule("S'", new ArrayList() { "" });
                        example.AddRule("F", new ArrayList() { "(", "*", "F", ")" });
                        example.AddRule("F", new ArrayList() { "i" });

                        LLParser parser = new LLParser(example);
                        Console.WriteLine("Введите строку: ");
                        if (parser.Parse(Console.ReadLine()))
                        {
                            Console.WriteLine("Успех. Строка соответствует грамматике.");
                            Console.WriteLine(parser.OutputConfigure);
                        }
                        else
                        {
                            Console.WriteLine("Не успех. Строка не соответствует грамматике.");
                        }
                        break;

                    case "5.1": // LL Разбор
                        myGrammar example1 = new myGrammar(new ArrayList() { "i", "(", ")", ":", "*", "" },
                                                           new ArrayList() { "S", "F", "L" },
                                                           "S");
                        
                        example1.AddRule("S", new ArrayList() { "(", "F", ":", "L", ")" });
                        example1.AddRule("S", new ArrayList() { "L", "*" });
                        example1.AddRule("S", new ArrayList() { "i" });
                        example1.AddRule("L", new ArrayList() { "L", "*" });
                        example1.AddRule("L", new ArrayList() { "i" });
                        example1.AddRule("F", new ArrayList() { "L", "*" });
                        example1.AddRule("F", new ArrayList() { "i" });

                        LLParser parser1 = new LLParser(example1);
                        Console.WriteLine("Введите строку: ");
                        if (parser1.Parse1(Console.ReadLine()))
                        {
                            Console.WriteLine("Успех. Строка соответствует грамматике.");
                            Console.WriteLine(parser1.OutputConfigure);
                        }
                        else
                        {
                            Console.WriteLine("Не успех. Строка не соответствует грамматике.");
                        }
                        break;

                    case "6":
                        CanonicalLRParser lrParser = new CanonicalLRParser();
                        lrParser.ReadGrammar();
                        lrParser.Execute();
                        break;
                    case "6.1":
                        myGrammar lrGrammar = new myGrammar(new ArrayList() { "+", "*", "i", "(", ")" },
                                                            new ArrayList() { "S", "T", "P" },
                                                            "S");
                        lrGrammar.AddRule("S", new ArrayList() { "S", "+", "T" });
                        lrGrammar.AddRule("S", new ArrayList() { "T" });
                        lrGrammar.AddRule("T", new ArrayList() { "T", "*", "P" });
                        lrGrammar.AddRule("T", new ArrayList() { "P" });
                        lrGrammar.AddRule("P", new ArrayList() { "i" });
                        lrGrammar.AddRule("P", new ArrayList() { "(", "S", ")" });
                        CanonicalLRParser lrExample = new CanonicalLRParser(lrGrammar);
                        lrExample.Execute();
                        break;

                    case "7": //МП - автоматы
                        // (q0,i@i,S) |- (q1,i@i,F@L)
                        // S->F@L 
                        // F->i L->i
                        myMp Mp = new myMp(new ArrayList() { "q0", "q1", "q2", "qf" },
                                           new ArrayList() { "a", "b" },
                                           new ArrayList() { "z0", "a" },
                                           "q0",
                                           "S",
                                           new ArrayList() { "qf" });

                        Mp.addDeltaRule("q0", "e", "S", new ArrayList() { "q1" }, new ArrayList() { "F", "@", "L" });
                        Mp.addDeltaRule("q1", "e", "F", new ArrayList() { "q2" }, new ArrayList() { "i" });
                        Mp.addDeltaRule("q2", "e", "L", new ArrayList() { "q3" }, new ArrayList() { "i" });
                        Mp.addDeltaRule("q3", "i", "i", new ArrayList() { "q4" }, new ArrayList() { "e" });
                        Mp.addDeltaRule("q4", "@", "@", new ArrayList() { "q5" }, new ArrayList() { "e" });
                        Console.Write("Debug Mp ");
                        Mp.debugDelta();
                        Console.WriteLine("\nEnter the line :");
                        Console.WriteLine(Mp.Execute(Console.ReadLine()).ToString());                       

                        /*
                        Mp.addDeltaRule("q0", "a", "z0", new ArrayList() { "q1" }, new ArrayList() { "a", "z0" });
                        Mp.addDeltaRule("q1", "a", "a", new ArrayList() { "q1" }, new ArrayList() { "a", "a" });
                        Mp.addDeltaRule("q1", "b", "a", new ArrayList() { "q2" }, new ArrayList() { "e" });
                        Mp.addDeltaRule("q2", "b", "a", new ArrayList() { "q2" }, new ArrayList() { "e" });
                        Mp.addDeltaRule("q2", "e", "z0", new ArrayList() { "qf" }, new ArrayList() { "e" });
                        Console.Write("Debug Mp ");
                        Mp.debugDelta();

                        Console.WriteLine("\nEnter the line :");
                        Console.WriteLine(Mp.Execute(Console.ReadLine()).ToString());
                        */ 
                        break;
                    
                    case "8": //МП-преобразователь
                        TranslGrammar gramm = new TranslGrammar(new ArrayList() { "a", "b" },
                                                                new ArrayList() { "S", "A" },
                                                                "S",
                                                                new ArrayList() { "0", "1" });

                        gramm.AddTrules("S", new ArrayList() { "a", "A", "b" }, new ArrayList() { "0", "1", "A" });
                        gramm.AddTrules("A", new ArrayList() { "a", "A", "b" }, new ArrayList() { "0", "1", "A" });
                        gramm.AddTrules("A", new ArrayList() { "e" }, new ArrayList() { "e" });
                        //Translator mytrans = new Translator(gramm);
                        //mytrans.debugDelta();
                        Console.WriteLine("Напишите строку:");
                        //Console.WriteLine("Перевод: " + mytrans.Translation(Console.ReadLine()));
                        break;

                    case "9": //МП - автоматы
                        translMp mp = new translMp(new ArrayList() { "q0", "q1", "q2","q3", "qf" },
                                                   new ArrayList() { "a", "b" }, 
                                                   new ArrayList() { "z0", "a" },
                                                   "q0",
                                                   "z0",
                                                   new ArrayList() { "qf" });

                        mp.addDeltaRule("q0", "a", "z0", new ArrayList() { "q1" }, new ArrayList() { "a", "z0" }, new ArrayList() { "e" });
                        mp.addDeltaRule("q1", "a", "a", new ArrayList() { "q1" }, new ArrayList() { "a", "a" }, new ArrayList() { "e" });
                        mp.addDeltaRule("q1", "b", "a", new ArrayList() { "q2" }, new ArrayList() { "e" }, new ArrayList() { "e" });
                        mp.addDeltaRule("q2", "b", "a", new ArrayList() { "q2" }, new ArrayList() { "e" }, new ArrayList() { "e" });
                        mp.addDeltaRule("q2", "e", "z0", new ArrayList() { "qf" }, new ArrayList() { "e" }, new ArrayList() { "e" });
                        //mp.addDeltaRule("q3", "e", "0", new ArrayList() { "qf" }, new ArrayList() { "e" }, new ArrayList() { "e" });
                        //mp.addDeltaRule("q3", "e", "1", new ArrayList() { "qf" }, new ArrayList() { "e" }, new ArrayList() { "e" });
                        Console.Write("Debug Mp ");
                        mp.debugDelta();
                        Console.WriteLine("\nEnter the line :");
                        Console.WriteLine(mp.Execute(Console.ReadLine()).ToString());
                        break;

                    case "10": // Пример "цепочечного" перевода
                        // Грамматика транслирует выражения из инфиксной записи в постфиксную
                        // Выражения состоят из i, +, * и скобок
                        SDTScheme chainPostfix = new SDTScheme(new List<Symbol>() { "i", "+", "*", "(", ")" },
                                                               new List<Symbol>() { "E", "E'", "T", "T'", "F" },
                                                               "E");

                        chainPostfix.AddRule("E", new List<Symbol>() { "T", "E'" });
                        chainPostfix.AddRule("E'", new List<Symbol>() { "+", "T", Actions.Print("+"), "E'" });
                        chainPostfix.AddRule("E'", new List<Symbol>() { Symbol.Epsilon });
                        chainPostfix.AddRule("T", new List<Symbol>() { "F", "T'" });
                        chainPostfix.AddRule("T'", new List<Symbol>() { "*", "F", Actions.Print("*"), "T'" });
                        chainPostfix.AddRule("T'", new List<Symbol>() { Symbol.Epsilon });
                        chainPostfix.AddRule("F", new List<Symbol>() { "i", Actions.Print("i") });
                        chainPostfix.AddRule("F", new List<Symbol>() { "(", "E", ")" });

                        LLTranslator chainTranslator = new LLTranslator(chainPostfix);
                        // Console.WriteLine("Введите строку: ");
                        List<Symbol> inp_str = new SimpleLexer().Parse(Console.ReadLine());
                        if (chainTranslator.Parse(inp_str))
                        {
                            Console.WriteLine("\nУспех. Строка соответствует грамматике.");
                        }
                        else
                        {
                            Console.WriteLine("\nНе успех. Строка не соответствует грамматике.");
                        }
                        break;

                    case "11": // L-атрибутивная грамматика
                        // Грамматика вычисляет результат арифметического выражения
                        // Выражения состоят из целых положительных чисел, +, * и скобок
                        Types.Attrs sAttrs = new Types.Attrs() { ["value"] = 0 };
                        Types.Attrs lAttrs = new Types.Attrs() { ["inh"] = 0, ["syn"] = 0 };
                        SDTScheme lAttrSDT = new SDTScheme(new List<Symbol>() { new Symbol("number", sAttrs), "+", "*", "(", ")" },
                                                           new List<Symbol>() { "S", new Symbol("E", sAttrs), new Symbol("E'", lAttrs),
                                                                                new Symbol("T", sAttrs), new Symbol("T'", lAttrs), new Symbol("F", sAttrs) },
                                                           "S");

                        lAttrSDT.AddRule("S",  new List<Symbol>() { "E", new Types.Actions((S) => Console.Write(S["E"]["value"].ToString())) });

                        lAttrSDT.AddRule("E",  new List<Symbol>() { "T", new Types.Actions((S) => S["E'"]["inh"] = S["T"]["value"]), "E'", new Types.Actions((S) => S["E"]["value"] = S["E'"]["syn"]) });

                        lAttrSDT.AddRule("E'", new List<Symbol>() { "+", "T", new Types.Actions((S) => S["E'1"]["inh"] = (int)S["E'"]["inh"] + (int)S["T"]["value"]), "E'", new Types.Actions((S) => S["E'"]["syn"] = S["E'1"]["syn"]) });

                        lAttrSDT.AddRule("E'", new List<Symbol>() { Symbol.Epsilon, new Types.Actions((S) => S["E'"]["syn"] = S["E'"]["inh"]) });

                        lAttrSDT.AddRule("T",  new List<Symbol>() { "F", new Types.Actions((S) => S["T'"]["inh"] = S["F"]["value"]), "T'", new Types.Actions((S) => S["T"]["value"] = S["T'"]["syn"]) });

                        lAttrSDT.AddRule("T'", new List<Symbol>() { "*", "F", new Types.Actions((S) => S["T'1"]["inh"] = (int)S["T'"]["inh"] * (int)S["F"]["value"]), "T'", new Types.Actions((S) => S["T'"]["syn"] = S["T'1"]["syn"]) });

                        lAttrSDT.AddRule("T'", new List<Symbol>() { Symbol.Epsilon, new Types.Actions((S) => S["T'"]["syn"] = S["T'"]["inh"]) });

                        lAttrSDT.AddRule("F",  new List<Symbol>() { "number", new Types.Actions((S) => S["F"]["value"] = S["number"]["value"]) });

                        lAttrSDT.AddRule("F",  new List<Symbol>() { "(", "E", ")",  new Types.Actions((S) => S["F"]["value"] = S["E"]["value"]) });

                        LLTranslator lAttrTranslator = new LLTranslator(lAttrSDT);
                        if (lAttrTranslator.Parse(new ArithmLexer().Parse(Console.ReadLine())))
                        {
                            Console.WriteLine("\nУспех. Строка соответствует грамматике.");
                        }
                        else
                        {
                            Console.WriteLine("\nНе успех. Строка не соответствует грамматике.");
                        }
                        break;

                    default :
                        Console.WriteLine("Выход из программы");
                        return;

                } //end switch
            } //end while
        } //end void Main()
    } //end class Program
}
