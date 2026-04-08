using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Serilog; // Нужно установить пакет Serilog.Sinks.File и Serilog.Sinks.Console

namespace TriangleCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Console.WriteLine("Введите стороны A, B и C:");
                string inputA = Console.ReadLine();
                string inputB = Console.ReadLine();
                string inputC = Console.ReadLine();

                var (type, coords) = CalculateTriangle(inputA, inputB, inputC);

                Console.WriteLine($"\nРезультат: {type}");
                Console.WriteLine($"Координаты: {string.Join(", ", coords)}");
            }
            finally
            {
                Log.CloseAndFlush(); // Важно закрыть логгер в конце
            }
        }

        static (string, List<(int, int)>) CalculateTriangle(string sA, string sB, string sC)
        {
            string inputs = $"A={sA}, B={sB}, C={sC}";

            try
            {
                // Проверка на числа
                if (!float.TryParse(sA, NumberStyles.Any, CultureInfo.InvariantCulture, out float a) ||
                    !float.TryParse(sB, NumberStyles.Any, CultureInfo.InvariantCulture, out float b) ||
                    !float.TryParse(sC, NumberStyles.Any, CultureInfo.InvariantCulture, out float c) ||
                    a <= 0 || b <= 0 || c <= 0)
                {
                    var res = ("", new List<(int, int)> { (-2, -2), (-2, -2), (-2, -2) });
                    Log.Error("Неуспешный запрос: {Inputs}. Ошибка: Некорректные числа. Результат: {Res}", inputs, res);
                    return res;
                }

                // Проверка на существование
                if (a + b <= c || a + c <= b || b + c <= a)
                {
                    var res = ("не треугольник", new List<(int, int)> { (-1, -1), (-1, -1), (-1, -1) });
                    Log.Warning("Неуспешный запрос (не треугольник): {Inputs}", inputs);
                    return res;
                }

                // Определение типа
                string type = "разносторонний";
                if (a == b && b == c) type = "равносторонний";
                else if (a == b || b == c || a == c) type = "равнобедренный";

                // Координаты (упрощенный расчет)
                double cosA = (b * b + c * c - a * a) / (2 * b * c);
                double sinA = Math.Sqrt(1 - cosA * cosA);
                double x3 = b * cosA, y3 = b * sinA;

                double maxDim = Math.Max(c, Math.Max(x3, y3));
                double scale = 100.0 / (maxDim == 0 ? 1 : maxDim);

                var coords = new List<(int, int)> {
                    (0, 0),
                    ((int)(c * scale), 0),
                    ((int)(x3 * scale), (int)(y3 * scale))
                };

                Log.Information("Успешный запрос: {Inputs}. Тип: {Type}. Координаты: {Coords}", inputs, type, coords);
                return (type, coords);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка при обработке запроса: {Inputs}", inputs);
                return ("", new List<(int, int)> { (-2, -2), (-2, -2), (-2, -2) });
            }
        }
    }
}