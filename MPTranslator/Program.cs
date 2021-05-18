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
        }

        struct Tablekey
        {
            public int I;
            public char J;
            public Tablekey(int i, char j) { I = i; J = j; }
        }

        static ArrayList Grammar = new ArrayList();  ///  правила грамматики
        static string Terminals;                     /// список терминалов
        static string NonTerminals;                  ///  список нетерминалов

        static void Execute()
        {
            Console.WriteLine("\nИсходная ");
            Info();
            RemoveEpsilonRules();
            Console.WriteLine("\nПосле удаления е-продукций");

            Grammar.Add("П S");     //дополнить грамматику правилом П -> S       
            NonTerminals += "П";
            Terminals += "$";

            Console.WriteLine("\nПравила: \n ");
            for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext(); )
                Console.WriteLine(rule.Current);

            Console.WriteLine("Терминалы : " + Terminals);
            Console.WriteLine("Нетерминалы: " + NonTerminals);
            Console.WriteLine("-----");
            //Console.ReadLine();

            // генерация LR(1) таблицы

            ComputeFirstSets(); // вычислить множества FIRST

            Console.WriteLine("Вычислены множества FIRST для символов грамматики и строк \n ");

            string Symbols = NonTerminals;
            for (int i = 0; i < Symbols.Length; i++)
            { //для каждого символа грамматики X
                char X = Symbols[i];
                Console.WriteLine("First( " + X + " ): " + First(X));
            }
            Console.WriteLine();
            for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext(); )
            {
                string str = ((string)rule.Current).Substring(2);
                Console.WriteLine("First( " + str + " ): " + First(str));
            }

            //Console.ReadLine();
            ArrayList[] CArray = CreateCArray(); // создать последовательность С
            Console.WriteLine("Cоздана последовательность С: \n ");
            for (int i = 0; i < CArray.Length; i++) { Console.WriteLine("I" + i + DebugArrayList(CArray[i])); }

            Hashtable ACTION = CreateActionTable(CArray); // создать ACTION таблицу
            if (ACTION == null) { Console.WriteLine("Грамматика не является LR(1)"); Console.ReadLine(); return; }
            Hashtable GOTO = CreateGotoTable(CArray); // создать GOTO таблицу
            // распечатать содержимое ACTION и GOTO таблиц:
            Console.WriteLine("\nСоздана ACTION таблица \n ");

            for (IDictionaryEnumerator c = ACTION.GetEnumerator(); c.MoveNext(); )
                Console.WriteLine("ACTION[" + ((Tablekey)c.Key).I + ", " + ((Tablekey)c.Key).J + "] = " + c.Value);
            Console.WriteLine("\nСоздана GOTO таблица \n ");

            for (IDictionaryEnumerator c = GOTO.GetEnumerator(); c.MoveNext(); )
                Console.WriteLine("GOTO[" + ((Tablekey)c.Key).I + ", " + ((Tablekey)c.Key).J + "] = " + c.Value);

            //Console.ReadLine();

            //синтакический анализ
            string answer = "y";
            while (answer[0] == 'y')
            {
                string input;
                Console.WriteLine("Введите строку: ");
                input = Console.In.ReadLine() + "$"; //считать входную строку
                Console.WriteLine("\nВведена строка: " + input + "\n");
                Console.WriteLine("\nПроцесс вывода: \n ");
                if (input.Equals("$"))
                { //случай пустой строки
                    Console.WriteLine(AcceptEmptyString ?
                         "Строка допущена" :
                         "Строка отвергнута");
                    Console.ReadLine();
                    continue; //return;
                }
                Stack stack = new Stack(); //Стек автомата
                stack.Push("0"); //поместить стартовое
                //нулевое состояние
                try
                {
                    for (; ; )
                    {
                        int s = Convert.ToInt32((string)stack.Peek());
                        //вершина стека
                        char a = input[0]; //входной симол
                        string action = (string)ACTION[new Tablekey(s, a)];
                        //элемент
                        //ACTION -таблицы
                        if (action[0] == 's')
                        { //shift
                            stack.Push(a.ToString()); //поместить в стек а
                            stack.Push(action.Substring(2));
                            //поместить в стек s'
                            input = input.Substring(1);
                            //перейти к следующему символу строки
                        }
                        else if (action[0] == 'r')
                        { //reduce
                            //rule[1] = A, rule[2] = alpha
                            string[] rule = action.Split(' ');
                            //удалить 2 * Length(alpha) элементов стека
                            for (int i = 0; i < 2 * rule[2].Length; i++)
                                stack.Pop();
                            //вершина стека
                            int state = Convert.ToInt32((string)stack.Peek());
                            //поместить в стек А и GOTO[state, A]
                            stack.Push(rule[1]);
                            stack.Push((GOTO[new Tablekey(state, rule[1][0])]).ToString());

                            //вывести правило
                            Console.WriteLine(rule[1] + "->" + rule[2]); //Console.ReadLine();
                        }
                        else if (action[0] == 'a') //accept
                            break;
                    }
                    Console.WriteLine("Строка допущена"); //Console.ReadLine();
                }
                catch (Exception) { Console.WriteLine("Строка отвергнута"); } //Console.ReadLine(); 
                //Console.ReadLine();
                Console.WriteLine("\n Продолжить? (y or n) \n");
                answer = Console.ReadLine();
            }

        }

        static void ReadGrammar()
        {
            Terminals = "";
            NonTerminals = "";
            Grammar.Clear();
            string s;
            Hashtable term = new Hashtable();       //  временная таблица терминалов 
            Hashtable nonterm = new Hashtable();    //  и нетерминалов
            Console.WriteLine("\nВведите продукции: \n ");
            while ((s = Console.In.ReadLine()) != "")
            { //считывание правил
                Grammar.Add(s); //добавитьть правило в грамматику
                for (int i = 0; i < s.Length; i++)
                    //  анализ элементов правила
                    if (s[i] != ' ')
                    {
                        //  если текущий символ - терминал, еще не добавленный в term
                        if (s[i] == s.ToLower()[i] && !term.ContainsKey(s[i]))
                            term.Add(s[i], null);
                        if (s[i] != s.ToLower()[i] && !nonterm.ContainsKey(s[i]))
                            nonterm.Add(s[i], null);
                    }
            }
            //  переписываем терминалы и нетерминалы в строки Terminals и NonTerminals
            for (IDictionaryEnumerator c = term.GetEnumerator(); c.MoveNext(); )
                Terminals += (char)c.Key;
            for (IDictionaryEnumerator c = nonterm.GetEnumerator(); c.MoveNext(); )
                NonTerminals += (char)c.Key;
        }

        static string DebugArrayList(ArrayList arraylist)
        {
            string arraylist_str = " { ";
            for (int i = 0; i < arraylist.Count; i++)
            {
                if (i == 0)
                    arraylist_str = arraylist_str + arraylist[i].ToString();
                else
                    arraylist_str = arraylist_str + "; " + arraylist[i].ToString();
            }
            arraylist_str = arraylist_str + " } ";
            return arraylist_str;
        }

        static void Info()
        {
            Console.WriteLine("КС - грамматика : " +
                             " \nАлфавит нетерминальных символов: " + NonTerminals +
                             " \nАлфавит терминальных символов: : " + Terminals +
                             " \nПравила : \n" + DebugArrayList(Grammar));
            Console.ReadLine();
        }

        /// список найденных комбинаций
        static ArrayList combinations = new ArrayList();
        static void GenerateCombinations(int depth, string s)
        {
            if (depth == 0)
                combinations.Add(s);
            else
            {
                GenerateCombinations(depth - 1, "0" + s);
                GenerateCombinations(depth - 1, "1" + s);
            }
        }

        ///  создает список правил, в которых вычеркнут один или более символов А в правой части
        static ArrayList GenerateRulesWithout(char A)
        {
            ArrayList result = new ArrayList();  //  итоговый список
            //цикл по правилам
            for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext(); )
            {
                string current = (string)rule.Current;  //  текущее правило,
                string rhs = current.Substring(2);  //  его правая часть,                       
                string[] rhs_split = rhs.Split(A);  //  отдельные сегменты rhs, разделенные А
                int counter;
                if (rhs.IndexOf(A) != -1)
                { //если правая часть содержит А
                    counter = 0;  //подсчитывает количество вхождений А
                    for (int i = 0; i < rhs.Length; i++)
                        if (rhs[i] == A)
                            counter++;
                    combinations.Clear();
                    GenerateCombinations(counter, "");  //генерация комбинаций
                    for (IEnumerator element = combinations.GetEnumerator(); element.MoveNext(); )
                        if (((string)element.Current).IndexOf('1') != -1)
                        {
                            //  если текущая комбинация содержит хоть один вычеркиваемый символ (т.е. единицу)
                            string combination = (string)element.Current;
                            string this_rhs = rhs_split[0];
                            //  если текущий символ комвинации - единица, 
                            //  то вычеркиваем А(просто соединяем сегменты правой части правила),
                            //  иначе вставляем дополнительный символ А) 
                            //
                            for (int i = 0; i < combination.Length; i++)
                                this_rhs += (combination[i] == '0' ? A.ToString() : "") + rhs_split[i + 1];
                            result.Add(current[0] + " " + this_rhs);
                        }
                } // end if
            } // end for
            return result;
        }

        static bool AcceptEmptyString;      /// допускать ли пустую строку
        static void RemoveEpsilonRules()
        {  // удаление е-правил
            AcceptEmptyString = false;      // флаг принадлежности пустой строки языку
            bool EpsilonRulesExist;
            do
            {
                EpsilonRulesExist = false;
                for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext(); )
                    if (((string)rule.Current)[2] == 'e')
                    {    // нашли эпсилон-правило     
                        // принимаем пустую строку, если левая часть правила содержит стартовый символ
                        char A = ((string)rule.Current)[0];
                        if (A == 'S') { AcceptEmptyString = true; }
                        Grammar.AddRange(GenerateRulesWithout(A));
                        Grammar.Remove(rule.Current);       // удаляем e-правило                        
                        EpsilonRulesExist = true;
                        break;
                    }
            }
            while (EpsilonRulesExist);      //  пока существуют эпсилон-правила
        }

        static Hashtable FirstSets = new Hashtable();       /// Набор множеств First
        public static void ComputeFirstSets()
        {
            for (int i = 0; i < Terminals.Length; i++)
                FirstSets[Terminals[i]] = Terminals[i].ToString();   // FIRST[c] = {c}*/
            for (int i = 0; i < NonTerminals.Length; i++)
                FirstSets[NonTerminals[i]] = "";                     //First[x] = ""
            bool changes;
            do
            {
                changes = false;
                for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext(); )
                {
                    // Для каждого правила X-> Y0Y1…Yn
                    char X = ((string)rule.Current)[0];
                    string Y = ((string)rule.Current).Substring(2);
                    for (int k = 0; k < Terminals.Length; k++)
                    {
                        char a = Terminals[k]; // для всех терминалов а
                        // а принадлежит First[Y0]
                        if (((string)FirstSets[Y[0]]).IndexOf(a) != -1)
                            if (((string)FirstSets[X]).IndexOf(a) == -1)
                            {
                                //Добавить а в FirstSets[X]
                                FirstSets[X] = (string)FirstSets[X] + a;
                                changes = true;
                            }
                    }
                }
            }
            while (changes); //  пока вносятся изменения
        }

        /// функции доступа ко множествам FIRST
        public static string First(char X) { return (string)FirstSets[X]; }

        public static string First(string X) { return First(X[0]); }

        static ArrayList Closure(ArrayList I)
        {
            ArrayList result = new ArrayList();
            //Console.WriteLine("Closure_множество ситуаций: " + DebugArrayList(I));
            //добавляем все элементы I в замыкание
            for (IEnumerator item = I.GetEnumerator(); item.MoveNext(); )
                result.Add(item.Current);
            bool changes;
            do
            {
                changes = false;
                //для каждого элемента R
                for (IEnumerator item = result.GetEnumerator(); item.MoveNext(); )
                {
                    //A -> alpha.Bbeta,a
                    string itvalue = (string)item.Current;
                    int Bidx = itvalue.IndexOf('.') + 1;
                    char B = itvalue[Bidx];                     //  B
                    if (NonTerminals.IndexOf(B) == -1)           //  если после точки терминал, то ситуацию не обрабатываем
                        continue;
                    string beta = itvalue.Substring(Bidx + 1);
                    beta = beta.Substring(0, beta.Length - 2);   //  beta
                    char a = itvalue[itvalue.Length - 1];      //  a
                    //для каждого правила B -> gamma
                    for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext(); )
                        if (((string)rule.Current)[0] == B)
                        { //  B - >gramma
                            string gamma = ((string)rule.Current).Substring(2);     //  gamma     
                            string first_betaa = First(beta + a);
                            // для каждого b из FIRST(betaa)
                            for (int i = 0; i < first_betaa.Length; i++)
                            {
                                //             Console.WriteLine("i= " + i + "first_betaa[i]= " + first_betaa[i]);
                                char b = first_betaa[i];           //  b
                                string newitem = B + " ." + gamma + "," + b;
                                //             Console.WriteLine("сгенерирована ситуация: " + newitem);
                                // добавить элемент B -> .gamma,b
                                if (!result.Contains(newitem))
                                {
                                    result.Add(newitem);
                                    //                 Console.WriteLine("добавлена новая ситуация: " + newitem);
                                    changes = true;
                                    goto breakloop;
                                }
                            }
                        }  // for по правилам B -> gamma
                } // for по  ситуациям R
            breakloop: ;
            }
            while (changes);
            //      Console.WriteLine("Closure_замыкание_ result " + DebugArrayList(result));
            return result;
        }

        /// Функция GoTo
        static ArrayList GoTo(ArrayList I, char X)
        {
            ArrayList J = new ArrayList();
            // для всех ситуаций из I
            for (IEnumerator item = I.GetEnumerator(); item.MoveNext(); )
            {
                string itvalue = (string)item.Current;
                string[] parts = itvalue.Split('.');
                if ((parts[1])[0] != X)
                    continue;
                //если ситуация имеет вид A alpha.Xbeta, a
                J.Add(parts[0] + X + "." + parts[1].Substring(1));
            }
            return Closure(J);
        }

        /// Процедура получения последовательности С
        static bool SetsEqual(ArrayList lhs, ArrayList rhs)
        {
            string[] lhsArr = new string[lhs.Count];
            // преобразование списка
            lhs.CopyTo(lhsArr);             // в массив
            Array.Sort(lhsArr);             // и его сортировка
            string[] rhsArr = new string[rhs.Count];
            // то же для второго множества
            rhs.CopyTo(rhsArr);
            Array.Sort(rhsArr);
            if (lhsArr.Length != rhsArr.Length) // если размеры не равны множества точно не равны
                return false;
            for (int i = 0; i < rhsArr.Length; i++)
                if (!lhsArr[i].Equals(rhsArr[i])) // если же размеры равны, проверяем по элементам
                    return false;
            return true;
        }

        /// Функция SetsEqual() используется функцией Contatains, 
        /// определяющей, является ли множество g элементом списка С
        static bool Contains(ArrayList C, ArrayList g)
        {
            for (IEnumerator item = C.GetEnumerator(); item.MoveNext(); )
                if (SetsEqual((ArrayList)item.Current, g))
                    return true;
            return false;
        }

        static ArrayList[] CreateCArray()
        {
            string Symbols = Terminals + NonTerminals; // все символы грамматики
            ArrayList C = new ArrayList();
            Console.WriteLine("CreateCArray: ");
            // добавить элемент I0 = Closure ({"П .S,$"})
            C.Add(Closure(new ArrayList(new Object[] { "П .S,$" })));
            Console.WriteLine("I0 : " + DebugArrayList(Closure(new ArrayList(new Object[] { "П .S,$" }))));
            Console.ReadLine();
            int counter = 0;
            bool modified;
            do
            {
                modified = false;
                for (int i = 0; i < Symbols.Length; i++)
                { //для каждого символа грамматики X
                    char X = Symbols[i];
                    Console.WriteLine("Для символа " + X);
                    // для каждого элемента последовательности С
                    for (IEnumerator item = C.GetEnumerator(); item.MoveNext(); )
                    {
                        ArrayList g = GoTo((ArrayList)item.Current, X);  // GoTo(Ii, X)
                        Console.WriteLine("GoTo( " + DebugArrayList((ArrayList)item.Current) + "," + X + "): \n"
                                            + DebugArrayList(g));
                        //Console.ReadLine();
                        // если множество g непусто и еще не включено в С
                        if (g.Count != 0 && !Contains(C, g))
                        {
                            C.Add(g); counter++;
                            Console.WriteLine("добавлено I" + counter + " : " + DebugArrayList(g)); //Console.ReadLine();
                            modified = true; break;
                        }
                    }
                }
            }
            while (modified);       // пока вносятся изменения
            ArrayList[] CArray = new ArrayList[C.Count];
            // преобразование списка  в массив
            C.CopyTo(CArray);
            return CArray;
        }

        static bool WriteActionTableValue(Hashtable ACTION, int I, char J, string action)
        {
            Tablekey Key = new Tablekey(I, J);
            if (ACTION.Contains(Key) && !ACTION[Key].Equals(action))
            {
                Console.WriteLine("не LR(1) грамматика"); Console.ReadLine();
                return false;
            }                                    // не LR(1) вид
            else
            {
                ACTION[Key] = action;
                return true;
            }
        }

        static Hashtable CreateActionTable(ArrayList[] CArray)
        {
            Hashtable ACTION = new Hashtable();
            for (int i = 0; i < CArray.Length; i++)
            { // цикл по элементам C
                // Для каждой ситуации из множества CArray[i]
                for (IEnumerator item = CArray[i].GetEnumerator(); item.MoveNext(); )
                {
                    string itvalue = (string)item.Current;          // ситуация
                    char a = itvalue[itvalue.IndexOf('.') + 1];     // символ за точкой
                    // Если ситуация имеет вид "A alpha.abeta,b"
                    if (Terminals.IndexOf(a) != -1)                 // если a - терминал
                        for (int j = 0; j < CArray.Length; j++)
                            if (SetsEqual(GoTo(CArray[i], a), CArray[j]))
                            {
                                // существует элемент CArray[j], такой,
                                // что GoTo(CArray[i], a) == CArray[j]
                                // запись ACTION[i, a] = shift j
                                if (WriteActionTableValue(ACTION, i, a, "s " + j) == false)
                                    return null;
                                // грамматика не LR(1)
                                break;
                            }
                    // Если ситуация имеет вид "A alpha., a"
                    if (itvalue[itvalue.IndexOf('.') + 1] == ',')
                    { // за точкой запятая
                        a = itvalue[itvalue.Length - 1];  // определить значение a
                        string alpha = itvalue.Split('.')[0].Substring(2);  // и alpha                      5!
                        if (itvalue[0] != 'П')
                        {                    // если левая часть не равна П                        
                            // ACTION[i, a] = reduce A -> alpha
                            if (WriteActionTableValue(ACTION, i, a, "r " + itvalue[0] + " " + alpha) == false)
                                return null;                    // грамматика не LR(1)
                        }
                    }
                    // Если ситуация имеет вид "П S.,$"
                    if (itvalue.Equals("П S.,$"))
                    {
                        // ACTION[i, '$'] = accept
                        if (WriteActionTableValue(ACTION, i, '$', "a") == false)
                            return null; // грамматика не LR(1)
                    }
                }
            }
            return ACTION;
        }

        static Hashtable CreateGotoTable(ArrayList[] CArray)
        {
            Hashtable GOTO = new Hashtable();
            for (int c = 0; c < NonTerminals.Length; c++)
                // для каждого нетерминала А
                for (int i = 0; i < CArray.Length; i++)
                {                // для каждого элемента Ii из С
                    ArrayList g = GoTo(CArray[i], NonTerminals[c]);
                    // g=GoTo[Ii, A]
                    for (int j = 0; j < CArray.Length; j++)
                        // если в С есть Ij=g
                        if (SetsEqual(g, CArray[j]))
                            // GOTO[i, A] =j
                            GOTO[new Tablekey(i, NonTerminals[c])] = j;
                }
            return GOTO;
        }

        static string[,] Mtable(myGrammar G)
        { // построение заданной таблицы      
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

        /// алгоритм разбора
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
                        ReadGrammar();
                        Execute();
                        break;
                    case "6.1":
                        Terminals = "+*i()";
                        NonTerminals = "STP";
                        Grammar.Add("S S+T");
                        Grammar.Add("S T");
                        Grammar.Add("T T*P");
                        Grammar.Add("T P");
                        Grammar.Add("P i");
                        Grammar.Add("P (S)");
                        Execute();
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
                                    
                    default :
                        Console.WriteLine("Выход из программы");
                        return;

                } //end switch
            } //end while
        } //end void Main()
    } //end class Program
}
