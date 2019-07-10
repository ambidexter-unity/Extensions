using System;
using UnityEngine;

namespace Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		/// Преобразовать код языка в перечисление SystemLanguage.
		/// </summary>
		/// <param name="raw">Исходный код.</param>
		/// <returns>Значение перечисления, соответствующее коду языка.</returns>
		/// <exception cref="NotSupportedException">Код не распознан.</exception>
		public static SystemLanguage AsLanguage(this string raw)
		{
			switch (raw)
			{
				case "ru_ru": return SystemLanguage.Russian;
				case "en_us": return SystemLanguage.English;
				default:
					throw new NotSupportedException($"Language {raw} is not supported.");
			}
		}

		/// <summary>
		/// Сократить запись числа до указанного количества символов.
		/// </summary>
		/// <param name="raw">Исходная строка, содержащая числовое значение.</param>
		/// <returns>Возвращает преобразованную строку.</returns>
		public static string ReduceNumber(this string raw)
		{
			if (!int.TryParse(raw, out var num)) return raw;

			if (num < 1000) return num.ToString();

			if (num < 1000000)
			{
				if (num < 10000) return $"{num / 1000f:F2}K";
				if (num < 100000) return $"{num / 1000f:F1}K";
				return $"{Mathf.RoundToInt(num / 1000f)}K";
			}

			if (num < 10000000) return $"{num / 1000000f:F2}M";
			if (num < 100000000) return $"{num / 1000000f:F1}M";
			return $"{Mathf.RoundToInt(num / 1000000f)}M";
		}
	}
}