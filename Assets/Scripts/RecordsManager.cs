using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Содержит список с игровыми рекордами. Позволяет сохранять новые рекорды и сбрасывать их.
/// </summary>
public static class RecordsManager
{
	// Переменная содержащая путь к .json файлу с рекордами
	private static readonly string RecordsPath = Path.Combine(Application.streamingAssetsPath, "records.json");
	// Список с рекордами в виде записей типа int
	private static readonly List<int> RecordsList;
	/// <summary>
	/// Считывает список рекордов из .json файла в List-коллекцию класса.
	/// </summary>
	static RecordsManager()
	{
		RecordsList = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText(RecordsPath));	
	}
	/// <summary>
	/// Дает доступ к списку с рекордами.
	/// </summary>
	/// <returns>Список с рекордами в виде коллекции <c>List</c>.</returns>
	public static List<int> GetRecordsList()
	{
		return RecordsList;
	}
	/// <summary>
	/// Записывает новый рекорд.
	/// </summary>
	/// <param name="newRecord">Число, которое необходимо записывать в качестве нового рекорда.</param>
	public static void AddTry(int newRecord)
	{
		// Добавляем новую запись
		RecordsList.Add(newRecord);
		// Сортируем список по убыванию
		RecordsList.Sort();
		RecordsList.Reverse();
		// Удаляем лишние элементы (порог - 15 элементов)
		if (RecordsList.Count > 15)
		{
			for (int i = RecordsList.Count; i >= 15; i--)
			{
				RecordsList.RemoveAt(i);
			}
		}
		// Сохраняем список на носитель
		SaveRecords();
	}
	/// <summary>
	/// Сбрасывает список рекордов.
	/// </summary>
	public static void ResetRecords()
	{
		// Очищаем список
		RecordsList.Clear();
		// Перезаписываем пустой список
		SaveRecords();
	}
	/// <summary>
	/// Записывает список рекордов в .json файл.
	/// </summary>
	public static void SaveRecords()
	{
		File.WriteAllText(RecordsPath, JsonConvert.SerializeObject(RecordsList, Formatting.Indented));
	}
}
