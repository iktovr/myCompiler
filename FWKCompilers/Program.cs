using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data;
using System.Text;
using System.Linq.Expressions;
using Processor.AttrGrammar;
using Processor.AbstractGrammar;

namespace Translator {

  class Program {

    static void Dialog() {
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
      Console.WriteLine("              ................. Enter 9");
      Console.WriteLine("AT-Grammar....... Enter 10");
    }

    struct Tablekey {
      public int I;
      public char J;
      public Tablekey(int i,char j) { I=i; J=j; }
    }

    static ArrayList Grammar = new ArrayList();  //  правила грамматики
    static string Terminals;                     //  список терминалов
    static string NonTerminals;                  //  список нетерминалов

    static void Execute() {
      Console.WriteLine("\nИсходная ");
      Info();
      RemoveEpsilonRules();
      Console.WriteLine("\nПосле удаления е-продукций");

      Grammar.Add("П S");     //дополнить грамматику правилом П -> S       
      NonTerminals+="П";
      Terminals+="$";

      Console.WriteLine("\nПравила: \n ");
      for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext();)
        Console.WriteLine(rule.Current);

      Console.WriteLine("Терминалы : "+Terminals);
      Console.WriteLine("Нетерминалы: "+NonTerminals);
      Console.WriteLine("-----");
      Console.ReadLine();

      // генерация LR(1) таблицы

      ComputeFirstSets(); // вычислить множества FIRST

      Console.WriteLine("Вычислены множества FIRST для символов грамматики и строк \n ");

      string Symbols = NonTerminals;
      for (int i = 0; i<Symbols.Length; i++) { //для каждого символа грамматики X
        char X = Symbols[i];
        Console.WriteLine("First( "+X+" ): "+First(X));
      }
      Console.WriteLine();
      for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext();) {
        string str = ((string)rule.Current).Substring(2);
        Console.WriteLine("First( "+str+" ): "+First(str));
      }

      Console.ReadLine();
      ArrayList[] CArray = CreateCArray(); // создать последовательность С
      Console.WriteLine("Cоздана последовательность С: \n ");
      for (int i = 0; i<CArray.Length; i++) { Console.WriteLine("I"+i+DebugArrayList(CArray[i])); }

      Hashtable ACTION = CreateActionTable(CArray); // создать ACTION таблицу
      if (ACTION==null) { Console.WriteLine("Грамматика не является LR(1)"); Console.ReadLine(); return; }
      Hashtable GOTO = CreateGotoTable(CArray); // создать GOTO таблицу
                                                // распечатать содержимое ACTION и GOTO таблиц:
      Console.WriteLine("\nСоздана ACTION таблица \n ");

      for (IDictionaryEnumerator c = ACTION.GetEnumerator(); c.MoveNext();)
        Console.WriteLine("ACTION["+((Tablekey)c.Key).I+", "+((Tablekey)c.Key).J+"] = "+c.Value);
      Console.WriteLine("\nСоздана GOTO таблица \n ");

      for (IDictionaryEnumerator c = GOTO.GetEnumerator(); c.MoveNext();)
        Console.WriteLine("GOTO["+((Tablekey)c.Key).I+", "+((Tablekey)c.Key).J+"] = "+c.Value);

      Console.ReadLine();

      //синтакический анализ
      string answer = "y";
      while (answer[0]=='y') {
        string input;
        Console.WriteLine("Введите строку: ");
        input=Console.In.ReadLine()+"$"; //считать входную строку
        Console.WriteLine("\nВведена строка: "+input+"\n");
        Console.WriteLine("\nПроцесс вывода: \n ");
        if (input.Equals("$")) { //случай пустой строки
          Console.WriteLine(AcceptEmptyString ?
               "Строка допущена" :
               "Строка отвергнута");
          Console.ReadLine();
          continue; //return;
        }
        Stack stack = new Stack(); //Стек автомата
        stack.Push("0"); //поместить стартовое
                         //нулевое состояние
        try {
          for (; ; )
          {
            int s = Convert.ToInt32((string)stack.Peek());
            //вершина стека
            char a = input[0]; //входной симол
            string action = (string)ACTION[new Tablekey(s,a)];
            //элемент
            //ACTION -таблицы
            if (action[0]=='s') { //shift
              stack.Push(a.ToString()); //поместить в стек а
              stack.Push(action.Substring(2));
              //поместить в стек s'
              input=input.Substring(1);
              //перейти к следующему символу строки
            } else if (action[0]=='r') { //reduce
                                         //rule[1] = A, rule[2] = alpha
              string[] rule = action.Split(' ');
              //удалить 2 * Length(alpha) элементов стека
              for (int i = 0; i<2*rule[2].Length; i++)
                stack.Pop();
              //вершина стека
              int state = Convert.ToInt32((string)stack.Peek());
              //поместить в стек А и GOTO[state, A]
              stack.Push(rule[1]);
              stack.Push((GOTO[new Tablekey(state,rule[1][0])]).ToString());

              //вывести правило
              Console.WriteLine(rule[1]+"->"+rule[2]); Console.ReadLine();
            } else if (action[0]=='a') //accept
              break;
          }
          Console.WriteLine("Строка допущена"); //Console.ReadLine();
        } catch (Exception) { Console.WriteLine("Строка отвергнута"); } //Console.ReadLine(); 
        Console.ReadLine();
        Console.WriteLine("\n Продолжить? (y or n) \n");
        answer=Console.ReadLine();
      }

    }

    static void ReadGrammar() {
      Terminals="";
      NonTerminals="";
      Grammar.Clear();
      string s;
      Hashtable term = new Hashtable();       //  временная таблица терминалов 
      Hashtable nonterm = new Hashtable();    //  и нетерминалов
      Console.WriteLine("\nВведите продукции: \n ");
      while ((s=Console.In.ReadLine())!="") { //считывание правил
        Grammar.Add(s); //добавитьть правило в грамматику
        for (int i = 0; i<s.Length; i++)
          //  анализ элементов правила
          if (s[i]!=' ') {
            //  если текущий символ - терминал, еще не добавленный в term
            if (s[i]==s.ToLower()[i]&&!term.ContainsKey(s[i]))
              term.Add(s[i],null);
            if (s[i]!=s.ToLower()[i]&&!nonterm.ContainsKey(s[i]))
              nonterm.Add(s[i],null);
          }
      }
      //  переписываем терминалы и нетерминалы в строки Terminals и NonTerminals
      for (IDictionaryEnumerator c = term.GetEnumerator(); c.MoveNext();)
        Terminals+=(char)c.Key;
      for (IDictionaryEnumerator c = nonterm.GetEnumerator(); c.MoveNext();)
        NonTerminals+=(char)c.Key;
    }

    static string DebugArrayList(ArrayList arraylist) {
      string arraylist_str = " { ";
      for (int i = 0; i<arraylist.Count; i++) {
        if (i==0)
          arraylist_str=arraylist_str+arraylist[i].ToString();
        else
          arraylist_str=arraylist_str+"; "+arraylist[i].ToString();
      }
      arraylist_str=arraylist_str+" } ";
      return arraylist_str;
    }

    static void Info() {
      Console.WriteLine("КС - грамматика : "+
                       " \nАлфавит нетерминальных символов: "+NonTerminals+
                       " \nАлфавит терминальных символов: : "+Terminals+
                       " \nПравила : \n"+DebugArrayList(Grammar));
      Console.ReadLine();
    }

    // список найденных комбинаций
    static ArrayList combinations = new ArrayList();
    static void GenerateCombinations(int depth,string s) {
      if (depth==0)
        combinations.Add(s);
      else {
        GenerateCombinations(depth-1,"0"+s);
        GenerateCombinations(depth-1,"1"+s);
      }
    }

    //  создает список правил, в которых вычеркнут один или более символов А в правой части
    static ArrayList GenerateRulesWithout(char A) {
      ArrayList result = new ArrayList();  //  итоговый список
                                           //цикл по правилам
      for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext();) {
        string current = (string)rule.Current;  //  текущее правило,
        string rhs = current.Substring(2);  //  его правая часть,                       
        string[] rhs_split = rhs.Split(A);  //  отдельные сегменты rhs, разделенные А
        int counter;
        if (rhs.IndexOf(A)!=-1) { //если правая часть содержит А
          counter=0;  //подсчитывает количество вхождений А
          for (int i = 0; i<rhs.Length; i++)
            if (rhs[i]==A)
              counter++;
          combinations.Clear();
          GenerateCombinations(counter,"");  //генерация комбинаций
          for (IEnumerator element = combinations.GetEnumerator(); element.MoveNext();)
            if (((string)element.Current).IndexOf('1')!=-1) {
              //  если текущая комбинация содержит хоть один вычеркиваемый символ (т.е. единицу)
              string combination = (string)element.Current;
              string this_rhs = rhs_split[0];
              //  если текущий символ комвинации - единица, 
              //  то вычеркиваем А(просто соединяем сегменты правой части правила),
              //  иначе вставляем дополнительный символ А) 
              //
              for (int i = 0; i<combination.Length; i++)
                this_rhs+=(combination[i]=='0' ? A.ToString() : "")+rhs_split[i+1];
              result.Add(current[0]+" "+this_rhs);
            }
        } // end if
      } // end for
      return result;
    }

    static bool AcceptEmptyString;      // допускать ли пустую строку
    static void RemoveEpsilonRules() {  // удаление е-правил
      AcceptEmptyString=false;      // флаг принадлежности пустой строки языку
      bool EpsilonRulesExist;
      do {
        EpsilonRulesExist=false;
        for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext();)
          if (((string)rule.Current)[2]=='e') {    // нашли эпсилон-правило     
                                                   // принимаем пустую строку, если левая часть правила содержит стартовый символ
            char A = ((string)rule.Current)[0];
            if (A=='S') { AcceptEmptyString=true; }
            Grammar.AddRange(GenerateRulesWithout(A));
            Grammar.Remove(rule.Current);       // удаляем e-правило                        
            EpsilonRulesExist=true;
            break;
          }
      }
      while (EpsilonRulesExist);      //  пока существуют эпсилон-правила
    }

    static Hashtable FirstSets = new Hashtable();       //Набор множеств First
    public static void ComputeFirstSets() {
      for (int i = 0; i<Terminals.Length; i++)
        FirstSets[Terminals[i]]=Terminals[i].ToString();   // FIRST[c] = {c}*/
      for (int i = 0; i<NonTerminals.Length; i++)
        FirstSets[NonTerminals[i]]="";                     //First[x] = ""
      bool changes;
      do {
        changes=false;
        for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext();) {
          // Для каждого правила X-> Y0Y1…Yn
          char X = ((string)rule.Current)[0];
          string Y = ((string)rule.Current).Substring(2);
          for (int k = 0; k<Terminals.Length; k++) {
            char a = Terminals[k]; // для всех терминалов а
                                   // а принадлежит First[Y0]
            if (((string)FirstSets[Y[0]]).IndexOf(a)!=-1)
              if (((string)FirstSets[X]).IndexOf(a)==-1) {
                //Добавить а в FirstSets[X]
                FirstSets[X]=(string)FirstSets[X]+a;
                changes=true;
              }
          }
        }
      }
      while (changes); //  пока вносятся изменения
    }

    // функции доступа ко множествам FIRST
    public static string First(char X) { return (string)FirstSets[X]; }

    public static string First(string X) { return First(X[0]); }

    static ArrayList Closure(ArrayList I) {
      ArrayList result = new ArrayList();
      //Console.WriteLine("Closure_множество ситуаций: " + DebugArrayList(I));
      //добавляем все элементы I в замыкание
      for (IEnumerator item = I.GetEnumerator(); item.MoveNext();)
        result.Add(item.Current);
      bool changes;
      do {
        changes=false;
        //для каждого элемента R
        for (IEnumerator item = result.GetEnumerator(); item.MoveNext();) {
          //A -> alpha.Bbeta,a
          string itvalue = (string)item.Current;
          int Bidx = itvalue.IndexOf('.')+1;
          char B = itvalue[Bidx];                     //  B
          if (NonTerminals.IndexOf(B)==-1)           //  если после точки терминал, то ситуацию не обрабатываем
            continue;
          string beta = itvalue.Substring(Bidx+1);
          beta=beta.Substring(0,beta.Length-2);   //  beta
          char a = itvalue[itvalue.Length-1];      //  a
                                                   //для каждого правила B -> gamma
          for (IEnumerator rule = Grammar.GetEnumerator(); rule.MoveNext();)
            if (((string)rule.Current)[0]==B) { //  B - >gramma
              string gamma = ((string)rule.Current).Substring(2);     //  gamma     
              string first_betaa = First(beta+a);
              // для каждого b из FIRST(betaa)
              for (int i = 0; i<first_betaa.Length; i++) {
                //             Console.WriteLine("i= " + i + "first_betaa[i]= " + first_betaa[i]);
                char b = first_betaa[i];           //  b
                string newitem = B+" ."+gamma+","+b;
                //             Console.WriteLine("сгенерирована ситуация: " + newitem);
                // добавить элемент B -> .gamma,b
                if (!result.Contains(newitem)) {
                  result.Add(newitem);
                  //                 Console.WriteLine("добавлена новая ситуация: " + newitem);
                  changes=true;
                  goto breakloop;
                }
              }
            }  // for по правилам B -> gamma
        } // for по  ситуациям R
      breakloop:;
      }
      while (changes);
      //      Console.WriteLine("Closure_замыкание_ result " + DebugArrayList(result));
      return result;
    }

    // Функция GoTo
    static ArrayList GoTo(ArrayList I,char X) {
      ArrayList J = new ArrayList();
      // для всех ситуаций из I
      for (IEnumerator item = I.GetEnumerator(); item.MoveNext();) {
        string itvalue = (string)item.Current;
        string[] parts = itvalue.Split('.');
        if ((parts[1])[0]!=X)
          continue;
        //если ситуация имеет вид A alpha.Xbeta, a
        J.Add(parts[0]+X+"."+parts[1].Substring(1));
      }
      return Closure(J);
    }

    //Процедура получения последовательности С
    static bool SetsEqual(ArrayList lhs,ArrayList rhs) {
      string[] lhsArr = new string[lhs.Count];
      // преобразование списка
      lhs.CopyTo(lhsArr);             // в массив
      Array.Sort(lhsArr);             // и его сортировка
      string[] rhsArr = new string[rhs.Count];
      // то же для второго множества
      rhs.CopyTo(rhsArr);
      Array.Sort(rhsArr);
      if (lhsArr.Length!=rhsArr.Length) // если размеры не равны множества точно не равны
        return false;
      for (int i = 0; i<rhsArr.Length; i++)
        if (!lhsArr[i].Equals(rhsArr[i])) // если же размеры равны, проверяем по элементам
          return false;
      return true;
    }

    // Функция SetsEqual() используется функцией Contatains, 
    // определяющей, является ли множество g элементом списка С
    static bool Contains(ArrayList C,ArrayList g) {
      for (IEnumerator item = C.GetEnumerator(); item.MoveNext();)
        if (SetsEqual((ArrayList)item.Current,g))
          return true;
      return false;
    }

    static ArrayList[] CreateCArray() {
      string Symbols = Terminals+NonTerminals; // все символы грамматики
      ArrayList C = new ArrayList();
      Console.WriteLine("CreateCArray: ");
      // добавить элемент I0 = Closure ({"П .S,$"})
      C.Add(Closure(new ArrayList(new Object[] { "П .S,$" })));
      Console.WriteLine("I0 : "+DebugArrayList(Closure(new ArrayList(new Object[] { "П .S,$" }))));
      Console.ReadLine();
      int counter = 0;
      bool modified;
      do {
        modified=false;
        for (int i = 0; i<Symbols.Length; i++) { //для каждого символа грамматики X
          char X = Symbols[i];
          Console.WriteLine("Для символа "+X);
          // для каждого элемента последовательности С
          for (IEnumerator item = C.GetEnumerator(); item.MoveNext();) {
            ArrayList g = GoTo((ArrayList)item.Current,X);  // GoTo(Ii, X)
            Console.WriteLine("GoTo( "+DebugArrayList((ArrayList)item.Current)+","+X+"): \n"
                                +DebugArrayList(g));
            Console.ReadLine();
            // если множество g непусто и еще не включено в С
            if (g.Count!=0&&!Contains(C,g)) {
              C.Add(g); counter++;
              Console.WriteLine("добавлено I"+counter+" : "+DebugArrayList(g)); Console.ReadLine();
              modified=true; break;
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

    static bool WriteActionTableValue(Hashtable ACTION,int I,char J,string action) {
      Tablekey Key = new Tablekey(I,J);
      if (ACTION.Contains(Key)&&!ACTION[Key].Equals(action)) {
        Console.WriteLine("не LR(1) грамматика"); Console.ReadLine();
        return false;
      }                                    // не LR(1) вид
      else {
        ACTION[Key]=action;
        return true;
      }
    }

    static Hashtable CreateActionTable(ArrayList[] CArray) {
      Hashtable ACTION = new Hashtable();
      for (int i = 0; i<CArray.Length; i++) { // цикл по элементам C
                                              // Для каждой ситуации из множества CArray[i]
        for (IEnumerator item = CArray[i].GetEnumerator(); item.MoveNext();) {
          string itvalue = (string)item.Current;          // ситуация
          char a = itvalue[itvalue.IndexOf('.')+1];     // символ за точкой
                                                        // Если ситуация имеет вид "A alpha.abeta,b"
          if (Terminals.IndexOf(a)!=-1)                 // если a - терминал
            for (int j = 0; j<CArray.Length; j++)
              if (SetsEqual(GoTo(CArray[i],a),CArray[j])) {
                // существует элемент CArray[j], такой,
                // что GoTo(CArray[i], a) == CArray[j]
                // запись ACTION[i, a] = shift j
                if (WriteActionTableValue(ACTION,i,a,"s "+j)==false)
                  return null;
                // грамматика не LR(1)
                break;
              }
          // Если ситуация имеет вид "A alpha., a"
          if (itvalue[itvalue.IndexOf('.')+1]==',') { // за точкой запятая
            a=itvalue[itvalue.Length-1];  // определить значение a
            string alpha = itvalue.Split('.')[0].Substring(2);  // и alpha                      5!
            if (itvalue[0]!='П') {                    // если левая часть не равна П                        
                                                      // ACTION[i, a] = reduce A -> alpha
              if (WriteActionTableValue(ACTION,i,a,"r "+itvalue[0]+" "+alpha)==false)
                return null;                    // грамматика не LR(1)
            }
          }
          // Если ситуация имеет вид "П S.,$"
          if (itvalue.Equals("П S.,$")) {
            // ACTION[i, '$'] = accept
            if (WriteActionTableValue(ACTION,i,'$',"a")==false)
              return null; // грамматика не LR(1)
          }
        }
      }
      return ACTION;
    }

    static Hashtable CreateGotoTable(ArrayList[] CArray) {
      Hashtable GOTO = new Hashtable();
      for (int c = 0; c<NonTerminals.Length; c++)
        // для каждого нетерминала А
        for (int i = 0; i<CArray.Length; i++) {                // для каждого элемента Ii из С
          ArrayList g = GoTo(CArray[i],NonTerminals[c]);
          // g=GoTo[Ii, A]
          for (int j = 0; j<CArray.Length; j++)
            // если в С есть Ij=g
            if (SetsEqual(g,CArray[j]))
              // GOTO[i, A] =j
              GOTO[new Tablekey(i,NonTerminals[c])]=j;
        }
      return GOTO;
    }
    static void Main() {
      while (true) {
        Dialog();
        switch (Console.ReadLine()) {
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
            string str = "v + v"; //Console.ReadLine();
            str+='e';

            bool b = npda.Execute_(str,0,str.Length);
            if (b) { Console.WriteLine("Yes"); } else Console.WriteLine("NO");
            //mp.Execute(str, 0, str.Length);
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
            //P
            Gram.AddRule("S0",new List<Symbol>() { new Symbol("0") });
            Gram.AddRule("S0",new List<Symbol>() { new Symbol("0"), new Symbol("A") });
            Gram.AddRule("A",new List<Symbol>() { new Symbol("1"),  new Symbol("B") });
            Gram.AddRule("B",new List<Symbol>() { new Symbol("0") });
            Gram.AddRule("B",new List<Symbol>() { new Symbol("0"),new Symbol("A") });

            //From Automaton Grammar to State Machine(KA)
            FSAutomate KA = Gram.Transform();
            KA.DebugAuto();
            break;

          case "2.1":
            var example = new FSAutomate(new List<Symbol>() { new Symbol("S0"), new Symbol("1"), new Symbol("2"),new Symbol("3"),new Symbol("4"), new Symbol("5"),
                                                                               new Symbol("6"), new Symbol("7"), new Symbol("8"), new Symbol("9"), new Symbol("qf") },
                                                new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                                                new List<Symbol>() { new Symbol("qf") },
                                                "S0");
            example.AddRule("S0","","1");
            example.AddRule("S0","","7");
            example.AddRule("1","","2");
            example.AddRule("1","","4");
            example.AddRule("2","a","3");
            example.AddRule("4","b","5");
            example.AddRule("3","","6");
            example.AddRule("5","","6");
            example.AddRule("6","","1");
            example.AddRule("6","","7");
            example.AddRule("7","a","8");
            example.AddRule("8","b","9");
            example.AddRule("9","b","qf");

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

            regGr.AddRule("S",new List<Symbol>() { new Symbol("c"),new Symbol("A"),new Symbol("B") });
            regGr.AddRule("S",new List<Symbol>() { new Symbol("b") });
            regGr.AddRule("B",new List<Symbol>() { new Symbol("c"),new Symbol("B") });
            regGr.AddRule("B",new List<Symbol>() { new Symbol("b") });
            regGr.AddRule("A",new List<Symbol>() { new Symbol("Ab") });
            regGr.AddRule("A",new List<Symbol>() { new Symbol("B") });
            regGr.AddRule("A",new List<Symbol>() { new Symbol("") });
            Console.WriteLine("Grammar:");
            regGr.Debug("T",regGr.T);
            regGr.Debug("T",regGr.V);
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
            G4.Debug("T",G4.T);
            G4.Debug("V",G4.V);
            G4.DebugPrules();
            Console.Write("Start symbol: ");
            Console.WriteLine(G4.S0+"\n");
            break;

          case "4": //МП - автоматы
            var CFGrammar = new Grammar(new List<Symbol>() { new Symbol("b"),new Symbol("c") },
                                          new List<Symbol>() { new Symbol("S"),new Symbol("A"),new Symbol("B"),new Symbol("D") },
                                        "S");

            CFGrammar.AddRule("S",new List<Symbol>() { new Symbol("b") });
            CFGrammar.AddRule("S",new List<Symbol>() { new Symbol("c"),new Symbol("A"),new Symbol("B") });
            CFGrammar.AddRule("S",new List<Symbol>() { new Symbol("c"),new Symbol("B")});

            CFGrammar.AddRule("A",new List<Symbol>() { new Symbol("b"),new Symbol("D") });
            CFGrammar.AddRule("A",new List<Symbol>() { new Symbol("b") });
            CFGrammar.AddRule("A",new List<Symbol>() { new Symbol("c"),new Symbol("B"),new Symbol("D") });
            CFGrammar.AddRule("A",new List<Symbol>() { new Symbol("c"),new Symbol("B") });

            CFGrammar.AddRule("D",new List<Symbol>() { new Symbol("b") });
            CFGrammar.AddRule("D",new List<Symbol>() { new Symbol("b"),new Symbol("D") });

            CFGrammar.AddRule("B",new List<Symbol>() { new Symbol("b") });
            CFGrammar.AddRule("B",new List<Symbol>() { new Symbol("cB") });

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

            CFGr.AddRule("S",new List<Symbol>() { new Symbol("a"),new Symbol("A"),new Symbol("b") });
            CFGr.AddRule("A",new List<Symbol>() { new Symbol("a"),new Symbol("B"),new Symbol("b") });
            CFGr.AddRule("B",new List<Symbol>() { new Symbol("a"),new Symbol("b") });
            Console.Write("Debug KC-Grammar ");
            CFGr.DebugPrules();

            Grammar kcGr2 = new Grammar(new List<Symbol>() { new Symbol("i"),new Symbol("=") },
                                            new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L")},
                                            "S");

            kcGr2.AddRule("S",new List<Symbol>() { new Symbol("F"),new Symbol("="),new Symbol("L") });
            kcGr2.AddRule("F",new List<Symbol>() { new Symbol("i") });
            kcGr2.AddRule("L",new List<Symbol>() { new Symbol("F") });
            Console.Write("Debug KC-Grammar ");
            kcGr2.DebugPrules();

            string ans = "y";
            while (ans=="y") {
              Console.WriteLine("Введите 1, 2 или 3");
              switch (Console.ReadLine()) {
                case "1":
                  var pda = new PDA(new List<Symbol>() { new Symbol("q0"),new Symbol("q1"),new Symbol("q2"),new Symbol("qf") },
                                       new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                                       new List<Symbol>() { new Symbol("z0"),new Symbol("a"),new Symbol("b"),new Symbol("S"),new Symbol("A"),new Symbol("B") },
                                       "q0",
                                       "S",
                                       new List<Symbol>() { new Symbol("qf") });

                  pda.addDeltaRule("q0","","S",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("a"),new Symbol("A"),new Symbol("b") });
                  pda.addDeltaRule("q1","","A",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("a"),new Symbol("B"),new Symbol("b") });
                  pda.addDeltaRule("q1","","B",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("a"),new Symbol("b") });
                  pda.addDeltaRule("q1","a","a",new List<Symbol>(){ new Symbol("q1") },new List<Symbol>() { new Symbol("") });
                  pda.addDeltaRule("q1","b","b",new List<Symbol>(){ new Symbol("q1") },new List<Symbol>() { new Symbol("") });
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
                  pda1.addDeltaRule("q0","","S",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("F"),new Symbol("="),new Symbol("L") });
                  pda1.addDeltaRule("q1","","F",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("i") });
                  pda1.addDeltaRule("q1","","L",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("i") });
                  pda1.addDeltaRule("q1","i","i",new List<Symbol>(){ new Symbol("q1") },new List<Symbol>() { new Symbol("")  });
                  pda1.addDeltaRule("q1","=","=",new List<Symbol>(){ new Symbol("q1") },new List<Symbol>() { new Symbol("")  });
                  Console.Write("Debug Mp ");
                  pda1.debugDelta();
                  Console.WriteLine("\nВведите строку :");
                  Console.WriteLine(pda1.Execute(Console.ReadLine()).ToString());
                  break;

                case "3":
                  break;
              } //end switch 1 or 2
              Console.WriteLine("Продолжить (y - yes, n - no)");
              ans=Console.ReadLine();
            } //end while
            break;

          case "5": // LL Разбор
            var LL = new Grammar(new List<Symbol>() { new Symbol("i"),new Symbol("("),new Symbol(")"),new Symbol("+"),new Symbol("*") },
                                               new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                                               "S");

            LL.AddRule("S",new List<Symbol>() { new Symbol("("),new Symbol("F"),new Symbol("+"),new Symbol("L"),new Symbol(")") });
            LL.AddRule("F",new List<Symbol>() { new Symbol("*"),new Symbol("L") });
            LL.AddRule("F",new List<Symbol>() { new Symbol("i") });
            LL.AddRule("L",new List<Symbol>() { new Symbol("F") });

            var parser = new LLParser(LL);
            Console.WriteLine("Введите строку: ");
            if (parser.Parse(Console.ReadLine())) {
              Console.WriteLine("Успех. Строка соответствует грамматике.");
              Console.WriteLine(parser.OutputConfigure);
            } else {
              Console.WriteLine("Не успех. Строка не соответствует грамматике.");
            }
            break;

          case "5.1": // LL Разбор
            var LL1 = new Grammar(new List<Symbol>() { new Symbol("i"),new Symbol("("),new Symbol(")"),new Symbol(":"),new Symbol("*"),new Symbol("") },
                                       new List<Symbol>() { new Symbol("S"),new Symbol("F"),new Symbol("L") },
                                       "S");

            LL1.AddRule("S",new List<Symbol>() { new Symbol("("),new Symbol("F"),new Symbol(":"),new Symbol("L"),new Symbol(")") });
            LL1.AddRule("S",new List<Symbol>() { new Symbol("L"),new Symbol("*") });
            LL1.AddRule("S",new List<Symbol>() { new Symbol("i") });
            LL1.AddRule("L",new List<Symbol>() { new Symbol("L"),new Symbol("*") });
            LL1.AddRule("L",new List<Symbol>() { new Symbol("i") });
            LL1.AddRule("F",new List<Symbol>() { new Symbol("L"),new Symbol("*") });
            LL1.AddRule("F",new List<Symbol>() { new Symbol("i") });

            var parser1 = new LLParser(LL1);
            Console.WriteLine("Введите строку: ");
            if (parser1.Parse1(Console.ReadLine())) {
              Console.WriteLine("Успех. Строка соответствует грамматике.");
              Console.WriteLine(parser1.OutputConfigure);
            } else {
              Console.WriteLine("Не успех. Строка не соответствует грамматике.");
            }
            break;

          case "6":  // LR(k)
            ReadGrammar();
            Execute();
            break;
          case "6.1": // LR(k)
            Terminals="i+*()";
            NonTerminals="SFL";
            Grammar.Add("S (F+L)");
            Grammar.Add("F *L");
            Grammar.Add("F i");
            Grammar.Add("L F");
            Execute();
            break;

          case "7": //МП - автоматы
                    // (q0,i@i,S) |- (q1,i@i,F@L)
                    // S->F@L 
                    // F->i L->i
            var pda2 = new PDA(new List<Symbol>() { new Symbol("q0"),new Symbol("q1"),new Symbol("q2"),new Symbol("qf") },
                               new List<Symbol>() { new Symbol("a"),new Symbol("b") },
                               new List<Symbol>() { new Symbol("z0"),new Symbol("a") },
                               "q0",
                               "S",
                               new List<Symbol>() { new Symbol("qf") });

            pda2.addDeltaRule("q0","e","S",new List<Symbol>() { new Symbol("q1") },new List<Symbol>() { new Symbol("F"),new Symbol("@"),new Symbol("L") });
            pda2.addDeltaRule("q1","e","F",new List<Symbol>() { new Symbol("q2") },new List<Symbol>() { new Symbol("i") });
            pda2.addDeltaRule("q2","e","L",new List<Symbol>() { new Symbol("q3") },new List<Symbol>() { new Symbol("i") });
            pda2.addDeltaRule("q3","i","i",new List<Symbol>() { new Symbol("q4") },new List<Symbol>() { new Symbol("e") });
            pda2.addDeltaRule("q4","@","@",new List<Symbol>() { new Symbol("q5") },new List<Symbol>() { new Symbol("e") });
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
          case "9": 
            break;
          case "10":
            // S, Er    *, +, cr     {ANS}r                             
            List<Symbol> V = new List<Symbol>() { new Symbol("S"),new Symbol("E", new List<Symbol>() { new Symbol("r") }) };
            List<Symbol> T = new List<Symbol>() { new Symbol("*"),new Symbol("+"),new Symbol("c",new List<Symbol>() { new Symbol("r") }) };
            List<OPSymbol> OP = new List<OPSymbol>() { new OPSymbol("{ANS}",new List<Symbol>() { new Symbol("r") }) };

            var atgr = new ATGrammar(V,T,OP,new Symbol("S"));
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

          default:
            Console.WriteLine("Выход из программы");
            return;

        } //end switch
      } //end while
    } //end void Main()
  } //end class Program
}
