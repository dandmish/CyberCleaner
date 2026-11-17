using System;
using System.Globalization;

public static class NumberFormatter
{
    private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qt" };

    public static string Format(long value)
    {
        // До 9999 включительно показываем полное число
        if (value <= 9999)
        {
            return value.ToString();
        }

        double doubleValue = value;
        int suffixIndex = 0;

        while (doubleValue >= 1000.0 && suffixIndex < Suffixes.Length - 1)
        {
            doubleValue /= 1000.0;
            suffixIndex++;
        }

        // Форматируем без лишних нулей после точки (например: 10000 -> 10K, 12500 -> 12.5K, 125400 -> 125K)
        string format = doubleValue >= 100 ? "0" : (doubleValue >= 10 ? "0.#" : "0.##");
        return doubleValue.ToString(format, CultureInfo.InvariantCulture) + Suffixes[suffixIndex];
    }
}