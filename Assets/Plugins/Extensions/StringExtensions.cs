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
	}
}