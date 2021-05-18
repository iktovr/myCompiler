using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPTranslator
{

  class Translator : myMp {
    //инициализация магазинных и операционных символов
    public Translator(ArrayList Q,ArrayList Sigma,ArrayList Gamma,string Q0,string z0,ArrayList F) :
        base(Q,Sigma,Gamma,Q0,z0,F) { }

    public override bool Execute(string str) {
      //сразу нулевое правило брать
      var delta = (DeltaQSigmaGamma)this.DeltaList[0];
      currState=this.Q0;
      ArrayList list = new ArrayList();
      int i = 0;
      int j = 0;
      str=str+"e"; //empty step вставить "" не получается, так как это считается пустым символом, 
                   //который не отображается в строке
      string s;
      //delta.debug();
      for (; ; )
      {
        if (delta==null) {
          return false;
        }

        if (delta.LeftT!="")  // И В ВЕРШИНЕ СТЕКА ТЕРМИНАЛЬНЫЙ СИМВОЛ LeftT!!!! пустой такт
        {
          for (; i<str.Length;) //модель считывающего устройства
          {
            if (Z.Peek().ToString()==str[i].ToString()) {
              this.Z.Pop();
              currState=delta.RightQ.ToString();
              i++;
            } else return false;
            break;
          }
        } else if (delta.LeftT=="")  // И В ВЕРШИНЕ СТЕКА НЕ ТЕРМИНАЛЬНЫЙ СИМВОЛ LeftT!!!!
        {
          //шаг 1 вытолкнуть из стека и занести в стек rightZ 
          this.Z.Pop();
          s=arrToStr(delta.RightZ);
          for (j=s.Length-1; j>=0; j--) this.Z.Push(s[j]);
        }

        Infix_convert((DeltaQSigmaGammaSix)delta,list); // перевод в обратную польскую

        if (this.Z.Count!=0) {
          currState=arrToStr(delta.RightQ);
          delta=findDelta(currState,Z.Peek().ToString());
          //delta.debug();
        } else if (str[i].ToString()=="e") {
          return true;
        } else return false;
        Console.WriteLine();
      } // end for
        //проверка на терминал или нетерминал в вершине стека
        //изменение правила по верхушке стека
    } // end Execute

    public void Infix_convert(DeltaQSigmaGammaSix delta,ArrayList list) {
      //  list.Add(delta.rightSix.Clone()); 
      if (list.Count==0) {    //Для самого первого дельта-правила заносим правило перевода (3 элемента)
        foreach (string d in delta.rightSix) {
          list.Add(d);

          Console.Write(d);
          Console.Write(" ");
        }
        Console.WriteLine();
      } else //Если уже есть правило, то делаем в нем замены
      {
        //Ищем символ в котором происходит изменение (например P->i)
        for (int i = 0; i<list.Count; i++) {

          //если символ найдем меняем его и завершаем обход
          if (delta.LeftZ.Equals(list[i].ToString())) {
            list[i]=Utility.convert(delta.rightSix);
            break;
          }
        }
        //Печатаем выходную строку
        for (int i = 0; i<list.Count; i++) {
          Console.Write(list[i]);
          Console.Write(" ");
        }
        Console.WriteLine();

      }
    }


    } // end class

}
