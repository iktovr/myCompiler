# myCompiler aka Фреймворк

Агрегация курсовых проектов нескольких человек. За основу взята версия от начала 2021 года.

## Основные изменения

* Унифицированы отступы, комментарии приведены к стилю Doxygen.
* Канонический LR анализатор вынесен в отдельный файл ([LRParses.cs](https://github.com/iktovr/myCompiler/blob/main/MPTranslator/LRParser.cs)) и собран в класс.
* В класс myGrammar добавлены методы: конструктор копирования, ~~функции для вычисления и доступа к множествам FIRST, FOLLOW~~.
* Добавлена синтаксически управляемая трансляция: вспомогателные классы, 1 вид транслятора ([SDTranslation.cs](https://github.com/iktovr/myCompiler/blob/main/MPTranslator/SDTranslation.cs)).
* Добавлены наброски для лексических анализаторов: абстрактный класс, две простейшие реализации ([Lexer.cs](https://github.com/iktovr/myCompiler/blob/main/MPTranslator/Lexer.cs)).
* ...

## Документация

Попытка в документацию при помощи Doxygen: [myCompiler](https://iktovr.github.io/myCompiler/).

Старые части кода документированы плохо. Новые получше.
