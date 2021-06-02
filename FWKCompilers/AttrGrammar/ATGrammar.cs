using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Processor.AbstractGrammar;
using Translator;

namespace Processor.AttrGrammar {
    public class ATGrammar: Grammar   {

        public List<OPSymbol> OP = null; ///< Список операционных символов
        public List<AttrProduction> Rules = new List<AttrProduction>(); ///< Список правил порождения
        public ATGrammar() { }
        /// Конструктор
        public ATGrammar(List<Symbol> Ns,List<Symbol> Ts,List<OPSymbol> OPs /* типо правила  ,*/,Symbol SO) {
            this.OP=OPs;
            this.V=Ns;
            this.T=Ts;
            this.S0=SO;
        }

        /// Добавление правила
        public void Addrule(Symbol LeftNoTerm, /*левый нетерминал и его атрибуты*/
                                                List<Symbol> Right, /*правая часть правил  в виде символов*/
                                                List<AttrFunction> F /*правила атрибутов*/ ) {
            this.Rules.Add(new AttrProduction(LeftNoTerm,Right,F));
        }

        /// Печать грамматики
        public void Print() {
            Console.Write("\nAT-Grammar G = (V, T, OP, P, S)");
            Console.Write("\nV = { ");//нетерминальные символы
            for (int i = 0; i<V.Count; ++i) {
                V[i].print();
                if (i!=V.Count-1)
                    Console.Write(", ");
            }
            Console.Write(" },");
            Console.Write("\nT = { ");//терминальные
            for (int i = 0; i<T.Count; ++i) {
                T[i].print();
                if (i!=T.Count-1)
                    Console.Write(", ");
            }
            Console.Write(" },");

            var opf = new List<OPSymbol>(); //счётчик операционных символов, у которых есть правила атрибутов
            Console.Write("\nOP = { ");//операционные
            foreach (var op in OP) {
                op.print();
                if (op.function!=null)
                    opf.Add(op);
                Console.Write(", ");
            }
            Console.Write(" },");

            Console.Write("\nS = ");
            S0.print();

            //печать правил атрибутов операционных символов
            if (opf.Count!=0) {
                Console.Write("\nOperation Symbols Rules:\n");
                foreach (var op in opf) {
                    op.print();
                    Console.Write("\n");
                }
            }
            //печать правил грамматики
            if (Rules.Count!=0) {
                Console.Write("\nGrammar Rules:\n");
                for (int i = 0; i<Rules.Count; ++i) {
                    Console.Write("\n");
                    Rules[i].print();
                    Console.Write("\n");
                }
            }
        }

        private bool IsOper(string s) {
            return s=="+"||s=="-"||s=="*"||s=="/";
        }
        public void transform() {
            Console.WriteLine("\nPress Enter to start\n");
            Console.ReadLine();
            for (int i = 0; i<Rules.Count; ++i) {
                for (int j = 0; j<Rules[i].F.Count; ++j) {//обработка jго атрибутного правила iго правила грамматики
                    string NewOpS = "";
                    var atrs   = new List<Symbol>();
                    var atrs_l = new List<Symbol>();
                    for (int k = 0; k<Rules[i].F[j].RH.Count; ++k) { //проверка наличия функции в правой чатси правила
                        if (IsOper(Rules[i].F[j].RH[k].symbol)) {
                            NewOpS+=Rules[i].F[j].RH[k]; //создание имени для нового оперционного символа
                        } else {
                            atrs.Add(new Symbol(Rules[i].F[j].RH[k]+"'")); //создание дублирующих символов для правил A <- a, но в формате a' <- a.
                            atrs_l.Add(Rules[i].F[j].RH[k]);  //список атрибутов, входящих в функцию 

                        }
                    }
                    if ((NewOpS.Count())==0) // проверка, что нет функций в правй части правила
                        continue;
                    NewOpS+=i.ToString()+j.ToString(); //создание более уникального имени операционного символа
                    atrs.Add(new Symbol(atrs[0]+"_ans")); // добавление атрибута для результата функции


                    this.OP.Add(new OPSymbol("{"+NewOpS+"}",atrs,new List<Symbol>() { new Symbol(atrs[0]+"_ans") },
                        Rules[i].F[j].RH));// добавление операционного символа с атрибутами и атрибутным правилом
                    for (int k = 0; k<atrs.Count-1; ++k) { //добавление копирующих правил a' <- a
                        Rules[i].F.Add(new AttrFunction(new List<Symbol>() { atrs[k] },new List<Symbol>() { atrs_l[k] }));
                    }
                    Rules[i].F.Add(new AttrFunction(new List<Symbol>(Rules[i].F[j].LH),new List<Symbol>() { new Symbol(atrs[0]+"_ans") }));
                    //добавление правила z1, ... , zm <- p, где p - результат функции операционного символа
                    Rules[i].F.RemoveAt(j); //удаление правила с функцией в правой части
                    j-=1;
                    for (int k = Rules[i].RHS.Count-1; k>=0; --k) {
                        //поиск самой левой позииции для вставки операционного символа,
                        //начиная с самой правой позиции
                        int k1;
                        if (Rules[i].RHS[k].Attr == null)//проверка того, что есть атрибуты у к-го символа правой
                                                                                     //части правила грамматики
                            continue;
                        for (k1=0; k1<Rules[i].RHS[k].Attr.Count; ++k1) {//проверка, что у к-го символа нет атрибута, который есть у операционного символа,
                                                                                                                         //если он есть, то дальше мы не двигаемся и вставляем операционный символ перед ним, инче идём дальше до конца
                            if (atrs_l.Contains(Rules[i].RHS[k].Attr[k1]))
                                break;
                        }
                        if (k1<Rules[i].RHS[k].Attr.Count) { //нашли такой символ, справа от которого вставляем операционный
                            Rules[i].RHS.Insert(k+1,new Symbol("{"+NewOpS+"}",atrs));
                            break;
                        }
                        if (k==0) { //дошли до конца правила и не нашли символа с хотя бы одним атрибутом, совпадающим с атрибутами операционного символа. Такого быть не должно, т.к. это означает, что атрибутные правила содержат атрибуты, отсутствующие у правила грамматики
                            Rules[i].RHS.Insert(k,new Symbol("{"+NewOpS+"}",atrs));
                            break;
                        }
                    }
                }
                //поиск лишних атрибутов в правилах типа 
                //a1, ... , am <- k
                //b1, ..., k, ..., bn <- g
                // и замена на b1, ..., a1, ... , am, ..., bn <- g с удалением правила a1, ... , am <- k
                for (int r = 0; r<Rules[i].F.Count; ++r) {
                    bool deleted = false;
                    for (int l = r+1; l<Rules[i].F.Count; ++l) {
                        if (Rules[i].F[l].LH.Contains(Rules[i].F[r].RH[0])) {
                            Rules[i].F[l].LH.Remove(Rules[i].F[r].RH[0]);
                            deleted=true;
                            foreach (var s in (Rules[i].F[r].LH))
                                Rules[i].F[l].LH.Add(s);
                        }
                    }
                    if (deleted) {
                        Rules[i].F.RemoveAt(r);
                        r-=1;
                    }
                }
                Console.WriteLine("\nChange for "+(i+1).ToString()+"th rule\n");
                Rules[i].print();
                Console.ReadLine();
            }
        }

    } // and AGrammar

}
