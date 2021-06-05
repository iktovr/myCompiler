using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Processor.AbstractGrammar;

namespace Translator
{
    public class DeltaQSigmaGamma : DeltaQSigma
    {
        public string LHSZ { get; set; } ///< Верхний символ магазин
        public List<Symbol> RHSZ { get; set; } ///< Множество символов магазина
        /// Отображение
        /// Delta (  q1   ,   a    ,   z   ) = {  {q}   ,   {z1z2...} }
        ///         LHSQ     LHSS      LHSZ       RHSQ       RHSZ
        public DeltaQSigmaGamma(string LHSQ, string LHSS, string LeftZ, List<Symbol> RHSQ, List<Symbol> RHSZ) : base(LHSQ, LHSS, RHSQ)
        {
            this.LHSZ = LHSZ;
            this.RHSQ = this.RHSQ;
            this.RHSZ = RHSZ;
        }

        /* Convert List<Symbol> to array:
         * 1.
        string[] array = list.ToArray(typeof(string)) as string[];
        Console.WriteLine(array.ToString());

         * 2.
        string mystr = string.Join(",", array);
        Console.WriteLine(mystr);
        */
        public void debug()
        {
            //  string rightQstr = string.Join("",this.RightQ.ToArray(typeof(string)) as string[]);
            //  string rightZstr = string.Join("",this.RightZ.ToArray(typeof(string)) as string[]);

            //  Console.WriteLine(" delta(Q {0},T {1},Z {2})          = (Q {3},Z {4})",
            //      this.LeftQ,this.LeftT,this.LeftZ,rightQstr,rightZstr);
        }

    } // end class DeltaQSigmaGamma

    class DeltaQSigmaGammaSix : DeltaQSigmaGamma
    {

        /// Delta (  q1   ,   a    ,   z   ) = {  {q}   ,   {z1z2...} }
        /// Delta (  q1   ,   a    ,   z   ) = {  {q}   ,   {z1z2...}, {b1b2.....} }
        /// RightO b1,b2 выходные операционные символы
        ///         LHSQ    LHSS     LHSZ         RHSQ       RHSZ        RHSNew
        public DeltaQSigmaGammaSix(string LeftQ, string LeftT, string LeftZ, List<Symbol> RightQ, List<Symbol> RightZ, List<Symbol> RightSix) : base(LeftQ, LeftT, LeftZ, RightQ, RightZ) { this.rightSix = RightSix; }

        public List<Symbol> rightSix { get; set; }
    } // end class DeltaQSigmaGammaSix

    /// Push Down Automate МП = {}
    class PDA : Automate
    {
        // Q - множество состояний МП - автоматa
        // Sigma - алфавит входных символов
        // D - правила перехода
        // Q0 - начальное состояние
        // F - множество конечных состояний
        public List<Symbol> Gamma = null; ///< Алфавит магазинных символов
        public Stack Z = null;
        public string currState;

        /// МП для дельта-правил
        public PDA(List<Symbol> Q, List<Symbol> Sigma, List<Symbol> Gamma, string Q0, string z0, List<Symbol> F) : base(Q, Sigma, F, Q0)
        {
            this.Gamma = Gamma;
            this.D = new List<DeltaQSigmaGamma>();

            this.Z = new Stack();
            //  Q0 = Q[0].ToString(); // начальное состояние
            Z.Push(z0); // начальный символ в магазине
            this.F = F; // пустое множество заключительных состояний
        }
        /// МП для КС-грамматик
        public PDA(Grammar KCgrammar)
        {
            //        : base(new ArrayList() { "q" },KCgrammar.T,new ArrayList() { },"q") {
            this.Q = new List<Symbol>() { new Symbol("q") };
            this.Sigma = KCgrammar.T;
            this.F = new List<Symbol>(); //множество заключительных состояний
            this.Gamma = new List<Symbol>();
            this.Q0 = "q";

            this.Z = new Stack();
            foreach (var v1 in KCgrammar.V) // магазинные символы
                Gamma.Add(v1);
            foreach (var t1 in KCgrammar.T)
                Gamma.Add(t1);
            Q0 = Q[0].ToString(); // начальное состояние
            Z.Push(KCgrammar.S0); // начальный символ в магазине

            DeltaQSigmaGamma delta = null;

            foreach (var v1 in KCgrammar.V)
            { // build DeltaQSigmaGamma
                var q1 = new List<Symbol>();
                var z1 = new List<Symbol>();
                foreach (var p in KCgrammar.P)
                {
                    if (p.LHS.Equals(v1))
                    {
                        var zb = new Stack();
                        var rr = new List<Symbol>(p.RHS);
                        rr.Reverse();
                        foreach (var s in rr)
                            zb.Push(s);
                        //      z1.Add(zb); // ???? -----------
                        q1.Add(new Symbol(Q0));
                    }
                }
                delta = new DeltaQSigmaGamma(Q0, "e", v1.symbol, q1, z1);
                this.D.Add(delta);
            }

            foreach (var t1 in KCgrammar.T)
            {
                Stack e = new Stack();
                e.Push("e");
                delta = new DeltaQSigmaGamma(Q0, t1.symbol, t1.symbol, new List<Symbol>() { new Symbol(Q0) }, new List<Symbol>() { new Symbol("e") });
                D.Add(delta);
            }
        }
        public virtual void addDeltaRule(string LHSQ, string LHSS, string LHSZ, List<Symbol> RHSQ, List<Symbol> RHSZ) { D.Add(new DeltaQSigmaGamma(LHSQ, LHSS, LHSZ, RHSQ, RHSZ)); }
        public virtual void addDeltaRule(string LHSQ, string LHSS, string LHSZ, List<Symbol> RHSQ, List<Symbol> RHSZ, List<Symbol> RightSix) { D.Add(new DeltaQSigmaGammaSix(LHSQ, LHSS, LHSZ, RHSQ, RHSZ, RightSix)); }
        public virtual bool Execute_(string str, int i, int Length)
        {
            //сразу нулевое правило брать
            DeltaQSigmaGamma delta = null;
            delta = (DeltaQSigmaGamma)this.D[0];
            currState = this.Q0;
            //      int i = 0;  // sas!!
            int j = 0;
            str = str + "e"; // empty step вставить "" не получается, так как это считается пустым символом,
                             //который не отображается в строке
            string s;
            delta.debug();
            for (;;)
            {
                if (delta == null)
                {
                    return false;
                }
                if (delta.LHSS.Equals("")) // И В ВЕРШИНЕ СТЕКА ТЕРМИНАЛЬНЫЙ СИМВОЛ LeftT!!!! пустой такт
                {
                    for (; i < str.Length;) //модель считывающего устройства
                    {
                        if (Z.Peek().ToString() == str[i].ToString())
                        {
                            this.Z.Pop();
                            currState = delta.RHSQ.ToString();
                            i++;
                        }
                        else
                            return false;
                        break;
                    }
                }
                else if (delta.LHSS.Equals("")) // И В ВЕРШИНЕ СТЕКА НЕ ТЕРМИНАЛЬНЫЙ СИМВОЛ LeftT!!!!
                {
                    //шаг 1 вытолкнуть из стека и занести в стек rightZ
                    this.Z.Pop();
                    s = arrToStr(delta.RHSZ);
                    for (j = s.Length - 1; j >= 0; j--)
                        this.Z.Push(s[j]);
                }
                if (this.Z.Count != 0)
                {
                    currState = arrToStr(delta.RHSQ);

                    this.debugDeltaRule("1", delta);
                    // Execute_ (str,i, str.Length);

                    delta = findDelta(currState, Z.Peek().ToString());
                    delta.debug();
                }
                else if (str[i].ToString() == "e")
                    return true;
                else
                    return false;

            } // end for
              //проверка на терминал или нетерминал в вершине стека
              //изменение правила по верхушке стека
        } // end Execute_
        public virtual bool Execute(string str)
        {
            //сразу нулевое правило брать
            DeltaQSigmaGamma delta = null;
            delta = (DeltaQSigmaGamma)this.D[0];
            currState = this.Q0;
            int i = 0;
            int j = 0;
            str = str + "e"; // empty step вставить "" не получается, так как это считается пустым символом,
                             //который не отображается в строке
            string s;
            delta.debug();
            for (;;)
            {
                if (delta == null)
                {
                    return false;
                }
                if (delta.LHSS.Equals("")) // И В ВЕРШИНЕ СТЕКА ТЕРМИНАЛЬНЫЙ СИМВОЛ LeftT!!!! пустой такт
                {
                    for (; i < str.Length;) //модель считывающего устройства
                    {
                        if (Z.Peek().ToString() == str[i].ToString())
                        {
                            this.Z.Pop();
                            currState = delta.RHSQ.ToString();
                            i++;
                        }
                        else
                            return false;
                        break;
                    }
                }
                else if (delta.LHSS.Equals("")) // И В ВЕРШИНЕ СТЕКА НЕ ТЕРМИНАЛЬНЫЙ СИМВОЛ LeftT!!!!
                {
                    //шаг 1 вытолкнуть из стека и занести в стек rightZ
                    this.Z.Pop();
                    s = arrToStr(delta.RHSZ);
                    for (j = s.Length - 1; j >= 0; j--)
                        this.Z.Push(s[j]);
                }
                if (this.Z.Count != 0)
                {
                    currState = arrToStr(delta.RHSQ);
                    delta = findDelta(currState, Z.Peek().ToString());
                    delta.debug();
                }
                else if (str[i].ToString() == "e")
                    return true;
                else
                    return false;

            } // end for
              //проверка на терминал или нетерминал в вершине стека
              //изменение правила по верхушке стека
        } // end Execute

        /// Поиск правила по состоянию.
        public DeltaQSigmaGamma findDelta(string Q, string a)
        {
            foreach (var delta in this.D)
            {
                if (delta.LHSQ == Q && delta.LHSZ == a)
                    return delta;
            }
            return null; // not find
        }

        /// Поиск правила по символу в вершине  магазина
        public DeltaQSigmaGamma findDelta(string Z)
        {
            foreach (var delta in this.D)
            {
                if (delta.LeftZ == Z)
                    return delta;
            }
            return null; // not find
        }

        //*** вспомогательные процедуры ***

        /// Объединение множеств A or B
        public List<Symbol> Unify(List<Symbol> A, List<Symbol> B)
        {
            List<Symbol> unify = A;
            foreach (var s in B)
                if (!A.Contains(s))
                    unify.Add(s);
            return unify;
        }

        // Преобразование элементов массива в строку
        public string arrToStr(List<Symbol> array)
        {
            if (array.Equals(null))
                return null;
            else
            {
                string newLine = "";
                foreach (var s in array)
                    newLine += s;
                return newLine;
            }
        }

        /// Проверка на принадлежность множествам автомата
        public bool isGamma(string v)
        {
            foreach (var vi in this.Gamma)
            {
                if (v.Equals(vi))
                    return true;
            }
            return false;
        }

        /// Проверка на принадлежность множествам автомата
        public bool isSigma(string t)
        {
            foreach (var ti in this.Sigma)
            {
                if (t.Equals(ti))
                    return true;
            }
            return false;
        }

        public string StackToString(Stack Z)
        {
            if (Z.Count == 0)
                return null;
            else
            {
                string newLine = "";
                Stack temp = new Stack();
                for (int i = 0; i < Z.Count; i++)
                {
                    temp.Push(Z.Pop());
                    newLine += Z.Peek();
                }
                for (int i = 0; i < temp.Count; i++)
                    Z.Push(temp.Pop());
                return newLine;
            }
        }

        // **   Debug   **
        /// печать текущего состояния магазина
        public string DebugStack(Stack s)
        {
            string p = "|";
            Stack s1 = new Stack();
            while (s.Count != 0)
            {
                s1.Push(s.Pop());
                p = p + s1.Peek().ToString();
            }
            while (s1.Count != 0)
                s.Push(s1.Pop());
            return p;
        }

        public virtual void debugDelta()
        {
            Console.WriteLine("Deltarules :");
            if (this.D == null)
            {
                Console.WriteLine("null");
                return;
            }

            foreach (var d in this.D)
            {
                d.debug();
                // Console.Write("( " + d.leftQ + " , " + d.leftT + " , " + d.leftZ + " )");
                // Console.Write(" -> \n");
                // Console.WriteLine("[ { " + arrToStr(d.rightQ) + " } , { " + arrToStr(d.rightZ) + " } ]");
            }
        }
    } // end class

    class translMp : PDA //МП = {}
    {
        // Q - множество состояний МП - автоматa
        // Sigma - алфавит входных символов
        // DeltaList - правила перехода
        // Q0 - начальное состояние
        // F - множество конечных состояний
        // ans - выходная строка
        public string ans = "";
        public string gamma0 = null;
        public translMp(List<Symbol> Q, List<Symbol> Sigma, List<Symbol> Gamma, string Q0, string Z0, List<Symbol> F) : base(Q, Sigma, Gamma, Q0, Z0, F)
        {
            this.Gamma = Gamma;
            this.Z = new Stack();
            gamma0 = Gamma[0].ToString();
            // Q0 = Q[0].ToString();  // начальное состояние
            Z.Push(gamma0); // начальный символ в магазине
            this.F = F; // пустое множество заключительных состояний
        }
        public translMp(Grammar KCgrammar) : base(KCgrammar)
        {
            this.Gamma = new List<Symbol>();
            this.Z = new Stack();
            foreach (var v1 in KCgrammar.V) // магазинные символы
                Gamma.Add(v1);
            foreach (var t1 in KCgrammar.T)
                Gamma.Add(t1);
            Q0 = Q[0].ToString(); // начальное состояние
            Z.Push(KCgrammar.S0); // начальный символ в магазине
            F = new List<Symbol>(); // пустое множество заключительных состояний
            DeltaQSigmaGamma delta = null;
            foreach (var v1 in KCgrammar.V)
            { // сопоставление правил с отображениями
                var q1 = new List<Symbol>();
                var z1 = new List<Symbol>();
                foreach (var rule in KCgrammar.P)
                {
                    if (rule.LHS.Equals(v1))
                    {
                        var zb = new Stack();
                        var rr = new List<Symbol>(rule.RHS);
                        rr.Reverse();
                        foreach (var s in rr)
                        {
                            zb.Push(s);
                        }
                        // z1.Add(zb); ??????????????????-------
                        q1.Add(new Symbol(Q0));
                    }
                }
                delta = new DeltaQSigmaGamma(Q0, "e", v1.symbol, q1, z1);
                D.Add(delta);
            }
            foreach (var t1 in KCgrammar.T)
            {
                var e = new Stack();
                e.Push("e");
                delta = new DeltaQSigmaGamma(Q0, t1.symbol, t1.symbol, new List<Symbol>() { new Symbol(Q0) }, new List<Symbol>() { new Symbol("e") });
                D.Add(delta);
            }
        }
        ///????????????????????????????????????

    } // end class translMp
}