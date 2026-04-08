using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace TriangleCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите стороны A, B и C (каждая с новой строки):");
            string inputA = Console.ReadLine();
            string inputB = Console.ReadLine();
            string inputC = Console.ReadLine();

            var (type, coords) = CalculateTriangle(inputA, inputB, inputC);

            Console.WriteLine($"Тип: {type}");
            Console.WriteLine($"Координаты: [{string.Join(", ", coords)}]");
        }

        static (string, List<(int, int)>) CalculateTriangle(string sA, string sB, string sC)
        {
            string logPath = "log.txt";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string inputs = $"A={sA}, B={sB}, C={sC}";

            try
            {
                // Проверка на валидность чисел
                if (!float.TryParse(sA, NumberStyles.Any, CultureInfo.InvariantCulture, out float a) ||
                    !float.TryParse(sB, NumberStyles.Any, CultureInfo.InvariantCulture, out float b) ||
                    !float.TryParse(sC, NumberStyles.Any, CultureInfo.InvariantCulture, out float c) ||
                    a <= 0 || b <= 0 || c <= 0)
                {
                    var res = ("", new List<(int, int)> { (-2, -2), (-2, -2), (-2, -2) });
                    Log(logPath, $"{timestamp} | FAIL | {inputs} | Result: {res.Item1}, Coords: {string.Join(" ", res.Item2)} | Error: Invalid numeric input");
                    return res;
                }

                // Проверка на существование треугольника
                if (a + b <= c || a + c <= b || b + c <= a)
                {
                    var res = ("не треугольник", new List<(int, int)> { (-1, -1), (-1, -1), (-1, -1) });
                    Log(logPath, $"{timestamp} | FAIL | {inputs} | Result: {res.Item1}");
                    return res;
                }

                // Определение типа
                string type = "разносторонний";
                if (a == b && b == c) type = "равносторонний";
                else if (a == b || b == c || a == c) type = "равнобедренный";

                // Вычисление координат (вершина A в 0,0; вершина B на оси X)
                // Используем теорему косинусов для нахождения угла
                double cosA = (b * b + c * c - a * a) / (2 * b * c);
                double sinA = Math.Sqrt(1 - cosA * cosA);

                double x1 = 0, y1 = 0;
                double x2 = c, y2 = 0;
                double x3 = b * cosA, y3 = b * sinA;

                // Масштабирование в 100x100
                double maxDim = Math.Max(c, Math.Max(x3, y3));
                double scale = 100.0 / maxDim;

                var coords = new List<(int, int)>
                {
                    ((int)(x1 * scale), (int)(y1 * scale)),
                    ((int)(x2 * scale), (int)(y2 * scale)),
                    ((int)(x3 * scale), (int)(y3 * scale))
                };

                Log(logPath, $"{timestamp} | SUCCESS | {inputs} | Type: {type} | Coords: {string.Join(" ", coords)}");
                return (type, coords);
            }
            catch (Exception ex)
            {
                Log(logPath, $"{timestamp} | ERROR | {inputs} | {ex.Message}\n{ex.StackTrace}");
                return ("", new List<(int, int)> { (-2, -2), (-2, -2), (-2, -2) });
            }
        }

        static void Log(string path, string message)
        {
            Console.WriteLine(message);
            File.AppendAllText(path, message + Environment.NewLine);
        }
    }
}