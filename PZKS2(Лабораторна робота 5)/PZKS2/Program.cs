using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PZKS2
{
    public class ErrorDetail
    {
        public string Message { get; }
        public int Position { get; }
        public int Length { get; }

        public ErrorDetail(string message, int position, int length)
        {
            Message = message;
            Position = position;
            Length = length;
        }
    }

    public class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("Введіть арифметичний вираз:");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                SimplificationCounter.Reset();

                var tokens = Lexer.Tokenize(input);
                var errors = new List<ErrorDetail>();
                for (int i = 0; i < tokens.Count - 1; i++)
                {
                    if (tokens[i].Value == "/" && tokens[i + 1].Value == "0")
                    {
                        errors.Add(new ErrorDetail("Помилка: Виявлено ділення на нуль!", tokens[i].Position, tokens[i].Length));
                    }
                }

                Node astRoot = null;
                try
                {
                    astRoot = ExpressionBuilderParallel.BuildAstFromTokens(tokens, errors);
                }
                catch (Exception ex)
                {
                    errors.Add(new ErrorDetail(
                        ex.Message,
                        tokens.LastOrDefault()?.Position ?? input.Length - 1,
                        tokens.LastOrDefault()?.Length ?? 1));
                }

                if (errors.Any())
                {
                    Console.WriteLine("\n--- Помилки синтаксису/лексики ---");
                    Console.WriteLine(input);

                    var distinctErrors = errors.GroupBy(e => new { e.Message, e.Position }).Select(g => g.First()).ToList();

                    for (int i = 0; i < distinctErrors.Count; i++)
                    {
                        var error = distinctErrors[i];
                        string indicator = new string(' ', error.Position) + new string('^', Math.Max(1, error.Length));

                        Console.WriteLine(indicator);
                        Console.WriteLine($"{i + 1}. {error.Message}");
                    }
                    Console.WriteLine("\n----------------------------------\n");
                    continue;
                }

                if (astRoot == null) continue;
                Console.WriteLine("  МОДЕЛЮВАННЯ КОНВЕЄРНОЇ СИСТЕМИ (ШАРИ=2, ДИНАМІЧНИЙ ТАКТ)");
                int timeParallel = ParallelExecutionModel.CalculateParallelTime(astRoot);
                int timeSequential = ParallelExecutionModel.CalculateSequentialTime(astRoot);
                int maxParallelism = ParallelExecutionModel.FindMaxParallelism(astRoot);
                int P = maxParallelism;
                if (timeParallel == 0) timeParallel = 1;
                if (timeSequential == 0) timeSequential = 1;
                double speedup = (double)timeSequential / timeParallel;
                double efficiency = speedup / P;
                Console.WriteLine($"-> Загальний час послідовного виконання (Ts): {timeSequential} тактів");
                Console.WriteLine($"-> Час виконання в заданій ПКС (Tp): {timeParallel} тактів");
                Console.WriteLine($"-> Мінімальна кількість процесорів (P, максимальна ширина): {P}");
                Console.WriteLine("--- Результати аналізу ---");
                Console.WriteLine($"   Коефіцієнт прискорення (Sp = Ts / Tp): {speedup:F2}");
                Console.WriteLine($"   Ефективність роботи системи (Ep = Sp / P): {efficiency:F2}");
                Console.WriteLine("\nКінцеве Дерево (після оптимізації):");
                TreePrinter.Print(astRoot);
            }
        }
    }
}