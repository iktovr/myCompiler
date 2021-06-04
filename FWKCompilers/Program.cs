using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;
using System.Linq.Expressions;
using Processor.AttrGrammar;
using Processor.AbstractGrammar;

namespace Translator
{

    class Program
    {

        static void Dialog()
        {
            Console.WriteLine("\n----------------------------------------------");
            Console.WriteLine("DFS ( lab 2 ) ................. Enter 1");
            Console.WriteLine("Convert NDFS to DFS ");
            Console.WriteLine("example ....................... Enter 2.1");
            Console.WriteLine("lab 3   ....................... Enter 2");
            Console.WriteLine("CFGrammar ( lab 4 - 6) ........ Enter 3");
            Console.WriteLine("PDA       ( lab 7,8 )   ....... Enter 4");
            Console.WriteLine("PDA...................Enter 4.1");
            Console.WriteLine("LL - analizator ( lab 9 - 11 )  Enter 5");
            Console.WriteLine("LL - analizator (debug-mode) (lab 9 - 11)  ...... Enter 5.1");
            Console.WriteLine("LR - analizator");
            Console.WriteLine("lab 12 - 16   ................. Enter 6");
            Console.WriteLine("example ....................... Enter 6.1");
            Console.WriteLine("              ................. Enter 7");
            Console.WriteLine("               ................ Enter 8");
            Console.WriteLine("lab 14 - 16            ........ Enter 9");
            Console.WriteLine("AT-Grammar....... Enter 10");
            Console.WriteLine("Chain Translation example ..... Enter 11");
            Console.WriteLine("L-attribute translation ....... Enter 12");
        }

        static void Main()
        {
            while (true)
            {
                Dialog();
                switch (Console.ReadLine())
                {
                    case "0": // NPDA  automata
                        var npda = new PDA(
                                new List<Symbol>() { new Symbol("q"),new Symbol("qf") },
                                new List<Symbol>() { new Symbol("v"),new Symbol("+"),
                                                                         new Symbol("*"),new Symbol("("),
                                                                         new Symbol(")") },
                                new List<Symbol>() { new Symbol("") },
                                "q0",
                                "S",
                                new List<Symbol>() { new Symbol("qf") });

                        npda.addDeltaRule("q","v","v",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("e") });
                        npda.addDeltaRule("q","+","+",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("e") });
                        npda.addDeltaRule("q","*","*",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("e") });
                        npda.addDeltaRule("q","(","(",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("e") });
                        npda.addDeltaRule("q",")",")",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("e") });

                        npda.addDeltaRule("q","e","S",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("S"),new Symbol("+"),new Symbol("F") });
                        npda.addDeltaRule("q","e","S",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("F") });
                        npda.addDeltaRule("q","e","F",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("F"),new Symbol("*"),new Symbol("L") });
                        npda.addDeltaRule("q","e","F",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("L") });
                        npda.addDeltaRule("q","e","L",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("v") });
                        npda.addDeltaRule("q","e","L",new List<Symbol>() { new Symbol("q") },new List<Symbol>() { new Symbol("("),new Symbol("S"),new Symbol(")") });

                        Console.Write("Debug PDA ");
                        npda.debugDelta();
                        Console.WriteLine("\nEnter the line :");
                        string str = "v + v"; // Console.ReadLine();
                        str += 'e';

                        bool b = npda.Execute_(str, 0, str.Length);
                        if (b)
                        {
                            Console.WriteLine("Yes");
                        }
                        else
                            Console.WriteLine("NO");
                        // mp.Execute(str, 0, str.Length);
                        /*if (mp.ans != "")
                            Console.WriteLine(mp.ans);
                        else
                            Console.WriteLine("NO");
                            */
                        break;

                    case "1":
                        var ka = new FSAutomate(new List<Symbol>() { new Symbol("S0"), new Symbol("A"), new Symbol("B"), new Symbol("C"), new Symbol("D"),new Symbol( "E"), new Symbol("F"), new Symbol("G"),
                                                                                                                 new Symbol("H"),  new Symbol("I"),new Symbol("J"),new Symbol("K"),new Symbol("L"),new Symbol("M"),new Symbol( "N"),new Symbol("qf") },
                        new List<Symbol>() { new Symbol("0"),new Symbol("1"),new Symbol("-"),new Symbol("+"),new Symbol("") },
                        new List<Symbol>() { new Symbol("qf") },
                        "S0");
                        ka.AddRule("S0","1","A");
                        ka.AddRule("A","0","B");
                        ka.AddRule("B","1","C");
                        ka.AddRule("C","0","D");
                        ka.AddRule("D","-","E");
                        ka.AddRule("E","1","F");
                        ka.AddRule("F","+","G");
                        ka.AddRule("G","0","qf");
                        ka.AddRule("G","1","qf");

                        Console.WriteLine("Enter line to execute :");
                        ka.Execute(Console.ReadLine());
                        break;

                    case "1.2":
                        var Gram = new Grammar(new List<Symbol>() { new Symbol("0"),new Symbol("1") },
                        new List<Symbol>() { new Symbol("S0"),new Symbol("A"),new Symbol("B") },
                        "S0");
                        Gram.AddRule("S0", new List<Symbol>() { new Symbol("0") });
                        Gram.AddRule("S0", new List<Symbol>() { new Symbol("0"), new Symbol("A") });
                        Gram.AddRule("A", new List<Symbol>() { new Symbol("1"), new Symbol("B") });
                        Gram.AddRule("B", new List<Symbol>() { new Symbol("0") });
                        Gram.AddRule("B", new List<Symbol>() { new Symbol("0"), new Symbol("A") });

                        // From Automaton Grammar to State Machine(KA)
                        FSAutomate KA = Gram.Transform();
                        KA.DebugAuto();
                        break;

                    case "2.1":
                        var example = new FSAutomate(new List<Symbol>() { new Symbol("S0"), new Symbol("1"), new Symbol("2"),new Symbol("3"),new Symbol("4"), new Symbol("5"),
                                                                                                                                                             new Symbol("6"), new Symbol("7"), new Symbol("8"), new Symbol("9"), new Symbol("qf") },
                                                                                                new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                                                                                                new List<Symbol>() { new Symbol("qf") },
                                                                                                "S0");
                        example.AddRule("S0", "", "1");
                        example.AddRule("S0", "", "7");
                        example.AddRule("1", "", "2");
                        example.AddRule("1", "", "4");
                        example.AddRule("2", "a", "3");
                        example.AddRule("4", "b", "5");
                        example.AddRule("3", "", "6");
                        example.AddRule("5", "", "6");
                        example.AddRule("6", "", "1");
                        example.AddRule("6", "", "7");
                        example.AddRule("7", "a", "8");
                        example.AddRule("8", "b", "9");
                        example.AddRule("9", "b", "qf");

                        FSAutomate dkaEX = new FSAutomate();
                        dkaEX.BuildDeltaDKAutomate(example);
                        dkaEX.DebugAuto();
                        Console.WriteLine("Enter line to execute :");
                        dkaEX.Execute(Console.ReadLine());
                        break;

                    case "2":
                         var ndfsa = new FSAutomate(new List<Symbol>() { new Symbol("S0"),  new Symbol("1"),  new Symbol("2"), new Symbol("3"),  new Symbol("4"),new Symbol("5"),new Symbol("6"),new Symbol("7"),new Symbol("8"),new Symbol("9"),new Symbol("10"),
                                                                                                                                                     new Symbol("11"), new Symbol("12"), new Symbol("13"), new Symbol("14"), new Symbol("15"), new Symbol("16"), new Symbol("17"), new Symbol("18"), new Symbol("19"), new Symbol("20"), new Symbol("qf") },
                                                                                         new List<Symbol>() { new Symbol("1"),new Symbol("0"),new Symbol("+"),new Symbol("2") },
                                                                                         new List<Symbol>() { new Symbol("qf") },
                                                                                         "S0");
                        ndfsa.AddRule("S0","1","1");          //W1
                        ndfsa.AddRule("1","0","2");
                        ndfsa.AddRule("2","+","3");

                        ndfsa.AddRule("3","","4");            //W2
                        ndfsa.AddRule("4","","5");
                        ndfsa.AddRule("4","","7");
                        ndfsa.AddRule("4","","9");
                        ndfsa.AddRule("5","1","6");
                        ndfsa.AddRule("7","2","8");
                        ndfsa.AddRule("6","","9");
                        ndfsa.AddRule("8","","9");
                        ndfsa.AddRule("9","","4");
                        ndfsa.AddRule("9","","10");

                        ndfsa.AddRule("10","1","11");          //W3
                        ndfsa.AddRule("11","0","12");
                        ndfsa.AddRule("12","","13");
                        ndfsa.AddRule("13","","9");
                        ndfsa.AddRule("13","","14");

                        ndfsa.AddRule("14","","15");           //W4
                        ndfsa.AddRule("14","","17");
                        ndfsa.AddRule("15","0","16");
                        ndfsa.AddRule("17","1","18");
                        ndfsa.AddRule("16","","19");
                        ndfsa.AddRule("18","","19");
                        ndfsa.AddRule("19","","14");
                        ndfsa.AddRule("19","","20");
                        ndfsa.AddRule("20","","15");
                        ndfsa.AddRule("14","","qf");
                        ndfsa.AddRule("20","","qf");

                        var dka = new FSAutomate();
                        dka.BuildDeltaDKAutomate(ndfsa);
                        dka.DebugAuto();
                        Console.WriteLine("Enter line to execute :");
                        dka.Execute(Console.ReadLine());
                        break;

                    case "3":
                        var regGr = new Grammar(new List<Symbol>() { new Symbol("b"),new Symbol("c") },
                                                                     new List<Symbol>() { new Symbol("S"),new Symbol("A"),new Symbol("B"),new Symbol("C") },
                                                                     "S");

                        regGr.AddRule("S", new List<Symbol>() { new Symbol("c"), new Symbol("A"), new Symbol("B") });
                        regGr.AddRule("S", new List<Symbol>() { new Symbol("b") });
                        regGr.AddRule("B", new List<Symbol>() { new Symbol("c"), new Symbol("B") });
                        regGr.AddRule("B", new List<Symbol>() { new Symbol("b") });
                        regGr.AddRule("A", new List<Symbol>() { new Symbol("Ab") });
                        regGr.AddRule("A", new List<Symbol>() { new Symbol("B") });
                        regGr.AddRule("A", new List<Symbol>() { new Symbol("") });
                        Console.WriteLine("Grammar:");
                        regGr.Debug("T", regGr.T);
                        regGr.Debug("T", regGr.V);
                        regGr.DebugPrules();

                        Grammar G1 = regGr.EpsDelete();
                        G1.DebugPrules();

                        Grammar G2 = G1.unUsefulDelete();
                        G2.DebugPrules();

                        Grammar G3 = G2.ChainRuleDelete();
                        G3.DebugPrules();

                        Grammar G4 = G3.LeftRecursDelete();
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
                        var CFGrammar = new Grammar(new List<Symbol>() { new Symbol("b"),new Symbol("c") },
                                                                         new List<Symbol>() { new Symbol("S"),new Symbol("A"),new Symbol("B"),new Symbol("D") },
                                                                         "S");

                        CFGrammar.AddRule("S", new List<Symbol>() { new Symbol("b") });
                        CFGrammar.AddRule("S", new List<Symbol>() { new Symbol("c"), new Symbol("A"), new Symbol("B") });
                        CFGrammar.AddRule("S", new List<Symbol>() { new Symbol("c"), new Symbol("B") });

                        CFGrammar.AddRule("A", new List<Symbol>() { new Symbol("b"), new Symbol("D") });
                        CFGrammar.AddRule("A", new List<Symbol>() { new Symbol("b") });
                        CFGrammar.AddRule("A", new List<Symbol>() { new Symbol("c"), new Symbol("B"), new Symbol("D") });
                        CFGrammar.AddRule("A", new List<Symbol>() { new Symbol("c"), new Symbol("B") });

                        CFGrammar.AddRule("D", new List<Symbol>() { new Symbol("b") });
                        CFGrammar.AddRule("D", new List<Symbol>() { new Symbol("b"), new Symbol("D") });

                        CFGrammar.AddRule("B", new List<Symbol>() { new Symbol("b") });
                        CFGrammar.AddRule("B", new List<Symbol>() { new Symbol("cB") });

                        Console.Write("Debug KC-Grammar ");
                        CFGrammar.DebugPrules();

                        PDA MP = new PDA(CFGrammar);
                        Console.Write("Debug Mp ");
                        MP.debugDelta();

                        Console.WriteLine("\nEnter the line :");
                        Console.WriteLine(MP.Execute(Console.ReadLine()).ToString());
                        break;

                    case "4.1":
                        Console.WriteLine("Выберите КС-грамматику: ");

                        var CFGr = new Grammar(new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                                                                    new List<Symbol>() { new Symbol("S"),new Symbol("A"),new Symbol("B") },
                                                                    "S");

                        CFGr.AddRule("S", new List<Symbol>() { new Symbol("a"), new Symbol("A"), new Symbol("b") });
                        CFGr.AddRule("A", new List<Symbol>() { new Symbol("a"), new Symbol("B"), new Symbol("b") });
                        CFGr.AddRule("B", new List<Symbol>() { new Symbol("a"), new Symbol("b") });
                        Console.Write("Debug KC-Grammar ");
                        CFGr.DebugPrules();

                        Grammar kcGr2 = new Grammar(new List<Symbol>() { new Symbol("i"),new Symbol("=") },
                                                                                        new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L")},
                                                                                        "S");

                        kcGr2.AddRule("S", new List<Symbol>() { new Symbol("F"), new Symbol("="), new Symbol("L") });
                        kcGr2.AddRule("F", new List<Symbol>() { new Symbol("i") });
                        kcGr2.AddRule("L", new List<Symbol>() { new Symbol("F") });
                        Console.Write("Debug KC-Grammar ");
                        kcGr2.DebugPrules();

                        string ans = "y";
                        while (ans == "y")
                        {
                            Console.WriteLine("Введите 1, 2 или 3");
                            switch (Console.ReadLine())
                            {
                                case "1":
                                    var pda = new PDA(new List<Symbol>() { new Symbol("q0"),new Symbol("q1"),new Symbol("q2"),new Symbol("qf") },
                                                                           new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                                                                           new List<Symbol>() { new Symbol("z0"),new Symbol("a"),new Symbol("b"),new Symbol("S"),new Symbol("A"),new Symbol("B") },
                                                                           "q0",
                                                                           "S",
                                                                           new List<Symbol>() { new Symbol("qf") });

                                    pda.addDeltaRule("q0", "", "S", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("a"), new Symbol("A"), new Symbol("b") });
                                    pda.addDeltaRule("q1", "", "A", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("a"), new Symbol("B"), new Symbol("b") });
                                    pda.addDeltaRule("q1", "", "B", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("a"), new Symbol("b") });
                                    pda.addDeltaRule("q1", "a", "a", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("") });
                                    pda.addDeltaRule("q1", "b", "b", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("") });
                                    Console.Write("Debug Mp ");
                                    pda.debugDelta();
                                    Console.WriteLine("\nВведите строку :");
                                    Console.WriteLine(pda.Execute(Console.ReadLine()).ToString());
                                    break;

                                case "2":
                                 var pda1 = new PDA(new List<Symbol>() { new Symbol("q0"),new Symbol("q1"),new Symbol("q2"),new Symbol("qf") },
                                                                         new List<Symbol>() { new Symbol("i"),new Symbol("="),new Symbol("") },
                                                                         new List<Symbol>() { new Symbol("i"),new Symbol("="),new Symbol(""),new Symbol("S"),new Symbol("F"),new Symbol("L") },
                                                                         "q0",
                                                                         "S",
                                                                         new List<Symbol>() { new Symbol("qf") });
                                    pda1.addDeltaRule("q0", "", "S", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("F"), new Symbol("="), new Symbol("L") });
                                    pda1.addDeltaRule("q1", "", "F", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("i") });
                                    pda1.addDeltaRule("q1", "", "L", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("i") });
                                    pda1.addDeltaRule("q1", "i", "i", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("") });
                                    pda1.addDeltaRule("q1", "=", "=", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("") });
                                    Console.Write("Debug Mp ");
                                    pda1.debugDelta();
                                    Console.WriteLine("\nВведите строку :");
                                    Console.WriteLine(pda1.Execute(Console.ReadLine()).ToString());
                                    break;

                                case "3":
                                    break;
                            } // end switch 1 or 2
                            Console.WriteLine("Продолжить (y - yes, n - no)");
                            ans = Console.ReadLine();
                        } // end while
                        break;

                    case "5": // LL Разбор
                        var LL = new Grammar(new List<Symbol>() { new Symbol("i"),new Symbol("("),new Symbol(")"),new Symbol("+"),new Symbol("*") },
                                                                  new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                                                                  "S");

                        LL.AddRule("S", new List<Symbol>() { new Symbol("("), new Symbol("F"), new Symbol("+"), new Symbol("L"), new Symbol(")") });
                        LL.AddRule("F", new List<Symbol>() { new Symbol("*"), new Symbol("L") });
                        LL.AddRule("F", new List<Symbol>() { new Symbol("i") });
                        LL.AddRule("L", new List<Symbol>() { new Symbol("F") });

                        var parser = new LLParser(LL);
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
                        var LL1 = new Grammar(new List<Symbol>() { new Symbol("i"),new Symbol("("),new Symbol(")"),new Symbol(":"),new Symbol("*"),new Symbol("") },
                                                                   new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                                                                   "S");

                        LL1.AddRule("S", new List<Symbol>() { new Symbol("("), new Symbol("F"), new Symbol(":"), new Symbol("L"), new Symbol(")") });
                        LL1.AddRule("S", new List<Symbol>() { new Symbol("L"), new Symbol("*") });
                        LL1.AddRule("S", new List<Symbol>() { new Symbol("i") });
                        LL1.AddRule("L", new List<Symbol>() { new Symbol("L"), new Symbol("*") });
                        LL1.AddRule("L", new List<Symbol>() { new Symbol("i") });
                        LL1.AddRule("F", new List<Symbol>() { new Symbol("L"), new Symbol("*") });
                        LL1.AddRule("F", new List<Symbol>() { new Symbol("i") });

                        var parser1 = new LLParser(LL1);
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

                    case "6": // LR(k)
                        break;
                    case "6.1": // LR(k)
                        break;

                    case "7": //МП - автоматы
                        var pda2 = new PDA(new List<Symbol>() { new Symbol("q0"),new Symbol("q1"),new Symbol("q2"),new Symbol("qf") },
                                                                new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                                                                new List<Symbol>() { new Symbol("z0"),new Symbol("a") },
                                                                "q0",
                                                                "S",
                                                             new List<Symbol>() { new Symbol("qf") });
                              // (q0,i@i,S) |- (q1,i@i,F@L)
                              // S->F@L
                              // F->i L->i

                        pda2.addDeltaRule("q0", "e", "S", new List<Symbol>() { new Symbol("q1") }, new List<Symbol>() { new Symbol("F"), new Symbol("@"), new Symbol("L") });
                        pda2.addDeltaRule("q1", "e", "F", new List<Symbol>() { new Symbol("q2") }, new List<Symbol>() { new Symbol("i") });
                        pda2.addDeltaRule("q2", "e", "L", new List<Symbol>() { new Symbol("q3") }, new List<Symbol>() { new Symbol("i") });
                        pda2.addDeltaRule("q3", "i", "i", new List<Symbol>() { new Symbol("q4") }, new List<Symbol>() { new Symbol("e") });
                        pda2.addDeltaRule("q4", "@", "@", new List<Symbol>() { new Symbol("q5") }, new List<Symbol>() { new Symbol("e") });
                        Console.Write("Debug Mp ");
                        pda2.debugDelta();
                        Console.WriteLine("\nEnter the line :");
                        Console.WriteLine(pda2.Execute(Console.ReadLine()).ToString());

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

                    case "8":
                        break;
                    case "9":
                        // Kirill Voronov
                        // Grammar SLRGrammar = new Grammar(new List<Symbol>() { new Symbol("i"), new Symbol("j"), new Symbol("+"), new Symbol("-"), new Symbol("("), new Symbol(")") },
                        //                            new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                        //                            "S");
                        //
                        // SLRGrammar.AddRule("S", new List<Symbol>() { new Symbol("("), new Symbol("F"), new Symbol(")"), new Symbol("+"), new Symbol("("), new Symbol("L"), new Symbol(")") });
                        // SLRGrammar.AddRule("F", new List<Symbol>() { new Symbol("-"), new Symbol("L") });
                        // SLRGrammar.AddRule("F", new List<Symbol>() { new Symbol("i") });
                        // SLRGrammar.AddRule("L", new List<Symbol>() { new Symbol("j") });

                        // Matvey Volkov
                        // Grammar SLRGrammar = new Grammar(new List<Symbol>() { new Symbol("i"), new Symbol("j"), new Symbol("*"), new Symbol("-"), new Symbol("("), new Symbol(")") },
                        //                            new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                        //                            "S");
                        //
                        // SLRGrammar.AddRule("S", new List<Symbol>() { new Symbol("("), new Symbol("F"), new Symbol(")"), new Symbol("*"), new Symbol("L") });
                        // SLRGrammar.AddRule("F", new List<Symbol>() { new Symbol("-"), new Symbol("L") });
                        // SLRGrammar.AddRule("F", new List<Symbol>() { new Symbol("i") });
                        // SLRGrammar.AddRule("L", new List<Symbol>() { new Symbol("j") });

                        Grammar SLRGrammar = new Grammar(new List<Symbol>() { new Symbol("i"), new Symbol("j"), new Symbol("&"), new Symbol("^"), new Symbol("("), new Symbol(")") },
                                                   new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                                                   "S");

                        SLRGrammar.AddRule("S", new List<Symbol>() { new Symbol("F"), new Symbol("^"), new Symbol("L") });
                        SLRGrammar.AddRule("S", new List<Symbol>() { new Symbol("("), new Symbol("S"), new Symbol(")") });
                        SLRGrammar.AddRule("F", new List<Symbol>() { new Symbol("&"), new Symbol("L") });
                        SLRGrammar.AddRule("F", new List<Symbol>() { new Symbol("i") });
                        SLRGrammar.AddRule("L", new List<Symbol>() { new Symbol("j") });

                        SLRParser LR0Grammar = new SLRParser(SLRGrammar);

                        LR0Grammar.Parse();
                        break;
                    case "10":
                        // S, Er    *, +, cr     {ANS}r
                        List<Symbol> V = new List<Symbol>() { new Symbol("S"), new Symbol("E", new List<Symbol>() { new Symbol("r") }) };
                        List<Symbol> T = new List<Symbol>() { new Symbol("*"), new Symbol("+"), new Symbol("c", new List<Symbol>() { new Symbol("r") }) };
                        List<OPSymbol> OP = new List<OPSymbol>() { new OPSymbol("{ANS}", new List<Symbol>() { new Symbol("r") }) };

                        var atgr = new ATGrammar(V, T, OP, new Symbol("S"));
                        atgr.Addrule(new Symbol("S"), // LHS
                                                 new List<Symbol>() { // RHS
                                                                new Symbol("E", // S -> Ep {ANS}r r -> p
                                                                new List<Symbol>() { new Symbol("p") }),new Symbol("{ANS}",new List<Symbol>() { new Symbol("r") }) },
                                                                new List<AttrFunction>() { new AttrFunction(new List<Symbol>() {new Symbol("r") },new List<Symbol> { new Symbol("p") })
                                                         }
                                                );

                        atgr.Addrule(new Symbol("E", new List<Symbol>() {new Symbol("p") }), // Ep -> +EpEr p -> q + r
                                new List<Symbol>() { new Symbol("+"),new Symbol("E",new List<Symbol>() { new Symbol("p") }),
                                                                         new Symbol("E",new List<Symbol>() { new Symbol("r") }) },
                                new List<AttrFunction>() { new AttrFunction(new List<Symbol>() {
                                     new Symbol("p") },new List<Symbol> { new Symbol("q"),new Symbol("+"),new Symbol("r") })
                                });

                        atgr.Addrule(new Symbol("E", new List<Symbol>() { new Symbol("p") }),  // Ep -> *EpEr   p -> q * r
                                new List<Symbol>() { new Symbol("*"),new Symbol("E",new List<Symbol>() { new Symbol("p") }),new Symbol("E",new List<Symbol>() { new Symbol("r") }) },
                                new List<AttrFunction>() { new AttrFunction(new List<Symbol>() {
                                     new Symbol("p") },new List<Symbol> { new Symbol("q"),new Symbol("*"),new Symbol("r") })
                                });

                        atgr.Addrule(new Symbol("E", new List<Symbol>() { new Symbol("p") }), // Ep -> Cr   p -> r
                             new List<Symbol>() { new Symbol("C",new List<Symbol>() { new Symbol("r") }) },
                             new List<AttrFunction>() { new AttrFunction(new List<Symbol>() {
                                 new Symbol("p") },new List<Symbol> { new Symbol("r") })
                             });

                        atgr.Print();

                        atgr.transform();

                        Console.WriteLine("\nPress Enter to show result\n");
                        Console.ReadLine();

                        atgr.Print();
                        Console.WriteLine("\nPress Enter to end\n");
                        Console.ReadLine();
                        break;

                    case "11": // Пример "цепочечного" перевода
                        // Грамматика транслирует выражения из инфиксной записи в постфиксную
                        // Выражения состоят из i, +, * и скобок
                        SDT.Scheme chainPostfix = new SDT.Scheme(new List<SDT.Symbol>() { "i", "+", "*", "(", ")" },
                                                                 new List<SDT.Symbol>() { "E", "E'", "T", "T'", "F" },
                                                                 "E");

                        chainPostfix.AddRule("E",  new List<SDT.Symbol>() { "T", "E'" });
                        chainPostfix.AddRule("E'", new List<SDT.Symbol>() { "+", "T", SDT.Actions.Print("+"), "E'" });
                        chainPostfix.AddRule("E'", new List<SDT.Symbol>() { SDT.Symbol.Epsilon });
                        chainPostfix.AddRule("T",  new List<SDT.Symbol>() { "F", "T'" });
                        chainPostfix.AddRule("T'", new List<SDT.Symbol>() { "*", "F", SDT.Actions.Print("*"), "T'" });
                        chainPostfix.AddRule("T'", new List<SDT.Symbol>() { SDT.Symbol.Epsilon });
                        chainPostfix.AddRule("F",  new List<SDT.Symbol>() { "i", SDT.Actions.Print("i") });
                        chainPostfix.AddRule("F",  new List<SDT.Symbol>() { "(", "E", ")" });

                        SDT.LLTranslator chainTranslator = new SDT.LLTranslator(chainPostfix);
                        // Console.WriteLine("Введите строку: ");
                        List<SDT.Symbol> inp_str = new SDT.SimpleLexer().Parse(Console.ReadLine());
                        if (chainTranslator.Parse(inp_str))
                        {
                            Console.WriteLine("\nУспех. Строка соответствует грамматике.");
                        }
                        else
                        {
                            Console.WriteLine("\nНе успех. Строка не соответствует грамматике.");
                        }
                        break;

                    case "12": // L-атрибутивная грамматика
                        // Грамматика вычисляет результат арифметического выражения
                        // Выражения состоят из целых положительных чисел, +, * и скобок
                        SDT.Types.Attrs sAttrs = new SDT.Types.Attrs() { ["value"] = 0 };
                        SDT.Types.Attrs lAttrs = new SDT.Types.Attrs() { ["inh"] = 0, ["syn"] = 0 };
                        SDT.Scheme lAttrSDT = new SDT.Scheme(new List<SDT.Symbol>() { new SDT.Symbol("number", sAttrs), "+", "*", "(", ")" },
                                                             new List<SDT.Symbol>() { "S", new SDT.Symbol("E", sAttrs), new SDT.Symbol("E'", lAttrs),
                                                                                      new SDT.Symbol("T", sAttrs), new SDT.Symbol("T'", lAttrs), new SDT.Symbol("F", sAttrs) },
                                                             "S");

                        lAttrSDT.AddRule("S",  new List<SDT.Symbol>() { "E", new SDT.Types.Actions((S) => Console.Write(S["E"]["value"].ToString())) });

                        lAttrSDT.AddRule("E",  new List<SDT.Symbol>() { "T", new SDT.Types.Actions((S) => S["E'"]["inh"] = S["T"]["value"]), "E'", new SDT.Types.Actions((S) => S["E"]["value"] = S["E'"]["syn"]) });

                        lAttrSDT.AddRule("E'", new List<SDT.Symbol>() { "+", "T", new SDT.Types.Actions((S) => S["E'1"]["inh"] = (int)S["E'"]["inh"] + (int)S["T"]["value"]), "E'", new SDT.Types.Actions((S) => S["E'"]["syn"] = S["E'1"]["syn"]) });

                        lAttrSDT.AddRule("E'", new List<SDT.Symbol>() { SDT.Symbol.Epsilon, new SDT.Types.Actions((S) => S["E'"]["syn"] = S["E'"]["inh"]) });

                        lAttrSDT.AddRule("T",  new List<SDT.Symbol>() { "F", new SDT.Types.Actions((S) => S["T'"]["inh"] = S["F"]["value"]), "T'", new SDT.Types.Actions((S) => S["T"]["value"] = S["T'"]["syn"]) });

                        lAttrSDT.AddRule("T'", new List<SDT.Symbol>() { "*", "F", new SDT.Types.Actions((S) => S["T'1"]["inh"] = (int)S["T'"]["inh"] * (int)S["F"]["value"]), "T'", new SDT.Types.Actions((S) => S["T'"]["syn"] = S["T'1"]["syn"]) });

                        lAttrSDT.AddRule("T'", new List<SDT.Symbol>() { SDT.Symbol.Epsilon, new SDT.Types.Actions((S) => S["T'"]["syn"] = S["T'"]["inh"]) });

                        lAttrSDT.AddRule("F",  new List<SDT.Symbol>() { "number", new SDT.Types.Actions((S) => S["F"]["value"] = S["number"]["value"]) });

                        lAttrSDT.AddRule("F",  new List<SDT.Symbol>() { "(", "E", ")",  new SDT.Types.Actions((S) => S["F"]["value"] = S["E"]["value"]) });

                        SDT.LLTranslator lAttrTranslator = new SDT.LLTranslator(lAttrSDT);
                        if (lAttrTranslator.Parse(new SDT.ArithmLexer().Parse(Console.ReadLine())))
                        {
                            Console.WriteLine("\nУспех. Строка соответствует грамматике.");
                        }
                        else
                        {
                            Console.WriteLine("\nНе успех. Строка не соответствует грамматике.");
                        }
                        break;

                    default:
                        Console.WriteLine("Выход из программы");
                        return;

                } // end switch
            } // end while
        } // end void Main()
    } // end class Program
}
