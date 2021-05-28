using System;
using System.Collections.Generic;
using System.Collections;

namespace MPTranslator
{   
    /// Абстрактный класс лексического анализатора
    abstract class Lexer
    {
        /// Парсинг строки в список токенов
        abstract public List<Symbol> Parse(string _);
    }

    /// Простой лексический анализатор
    class SimpleLexer : Lexer
    {
        /// Токен создается для каждого символа строки
        override public List<Symbol> Parse(string str)
        {
            List<Symbol> result = new List<Symbol>();
            foreach (char c in str)
            {
                result.Add(new Symbol(c.ToString()));
            }
            return result;
        }
    }
}