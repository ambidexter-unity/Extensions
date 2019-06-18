using UnityEngine;

namespace Extensions
{
	public static class GameObjectExtensions
	{
		/// <summary>
		/// Получить или создать компонент на геймобжекте компонента.
		/// </summary>
		/// <param name="component">Компонент, на геймобжекте которого будет найден или создан новый компонент.</param>
		/// <typeparam name="T">Класс создаваемого компонента.</typeparam>
		/// <returns>Экземпряр созданного компонента, или <code>null</code>, если создание невозможно.</returns>
		public static T GetOrCreateComponent<T>(this Component component) where T : Component
		{
			return component.gameObject != null ? component.gameObject.GetOrCreateComponent<T>() : null;
		}

		/// <summary>
		/// Получить или создать компонент на геймобжекте.
		/// </summary>
		/// <param name="gameObject">Геймобжект, на котором будет найден или создан новый компонент.</param>
		/// <typeparam name="T">Класс создаваемого компонента.</typeparam>
		/// <returns>Экземпляр созданного компонента.</returns>
		public static T GetOrCreateComponent<T>(this GameObject gameObject) where T : Component
		{
			var c = gameObject.GetComponent<T>();
			return c != default(T) ? c : gameObject.AddComponent<T>();
		}
	}
}