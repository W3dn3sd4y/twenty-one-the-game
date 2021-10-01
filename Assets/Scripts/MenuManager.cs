using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Отвечает за работу сцены главного меню.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Main buttons")]
    public Button play;
    public Button openRules;
    public Button records;
    public Button exit;
    [Header("Records buttons")]
    public Button reset;
    public Button closeRecords;
    [Header("Rules buttons")]
    public Button nextSlide;
    public Button previousSlide;
    public Button closeRules;
    [Header("Records")]
    public RectTransform content;
    public RectTransform recordPrefab;
    public RectTransform recordsTable;
    [Header("Rules")]
    public RectTransform rules;
    public RectTransform slide;
    // Переменная хранящая текущее отображаемое изображение окна правил
    private string _currentSlide = "start";
    // Список со всеми существующими изображениями для окна правил, исключая текущее отображаемое
    private readonly List<string> _slides = new List<string>()
    {
        "middle", "win_conditions", "prices"
    };
    
    void Awake()
    {
        // Присваиваем кнопкам меню функции
        play.onClick.AddListener(delegate
        {
            // Загрузка основной сцены игры
            SceneManager.LoadScene("MainScene");
        });
        openRules.onClick.AddListener(delegate
        {
            // Открытие окна правил и его инициализация
            rules.gameObject.SetActive(true);
            InitializeRules();
        });
        records.onClick.AddListener(delegate
        {
            // Открытие окна рекордов и его инициализация
            recordsTable.gameObject.SetActive(true);
            InitializeRecordsTable();
            
        });
        exit.onClick.AddListener(Application.Quit);
    }
    /// <summary>
    /// Открывает окно рекордов и заполняет его таблицу записями рекордов
    /// </summary>
    private void InitializeRecordsTable()
    {
        // Очищаем таблицу от существующих элементов
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        // Заполняем таблицу рекордами
        var count = 1;
        foreach (var record in RecordsManager.GetRecordsList())
        {
            // Создаем экземпляр записи
            var instance = Instantiate(recordPrefab, content, false);
            // Заполняем его поля
            instance.Find("Number").GetComponent<Text>().text = "№" + count.ToString();
            instance.Find("Score").GetComponent<Text>().text = record.ToString();
            count++;
        }
        // Присваиваем кнопкам окна функции
        reset.onClick.AddListener(delegate
        {
            // Сбрасываем рекорды
            RecordsManager.ResetRecords();
            // Повторно инициализируем таблицу
            InitializeRecordsTable();
        });
        closeRecords.onClick.AddListener(delegate
        {
            // Делаем окно неактивным
            recordsTable.gameObject.SetActive(false);
            // Снимаем функции со всех кнопок окна
            reset.onClick.RemoveAllListeners();
            closeRecords.onClick.RemoveAllListeners();
        });
    }
    /// <summary>
    /// Открывает окно правил
    /// </summary>
    private void InitializeRules()
    {
        // Присваиваем кнопкам окна функции, аналогично методу InitializeRecordsTable()
        closeRules.onClick.AddListener(delegate
        {
            rules.gameObject.SetActive(false);
            closeRules.onClick.RemoveAllListeners();
            nextSlide.onClick.RemoveAllListeners();
            previousSlide.onClick.RemoveAllListeners();
        });
        nextSlide.onClick.AddListener(NextSlide);
        previousSlide.onClick.AddListener(PreviousSlide);
    }
    /// <summary>
    /// Вспомогательная функция для перехода с одного изображения правил на следующее
    /// </summary>
    private void NextSlide()
    {
        // Присваиваем объекту изображения спрайт согласно первому элементу списка изображений
        slide.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Tutorial/" +_slides[0]);
        // Добавляем старое изображение в конец списка
        _slides.Add(_currentSlide);
        // Присваиваем переменной текущего изображение первое изображение из списка
        _currentSlide = _slides[0];
        // Удаляем первое изображения (т.к. оно перешло в переменную _currentSlide)
        _slides.RemoveAt(0);
    }
    /// <summary>
    /// Вспомогательная функция для перехода с одного изображения правил на предыдущее
    /// </summary>
    private void PreviousSlide()
    {
        // Присваиваем объекту изображения спрайт согласно последнему элементу списка изображений
        slide.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Tutorial/" +_slides[_slides.Count - 1]);
        // Добавляем старое изображение в начало списка
        _slides.Insert(0, _currentSlide);
        // Присваиваем переменной текущего изображение последнее изображение из списка
        _currentSlide = _slides[_slides.Count - 1];
        // Удаляем последнее изображения (т.к. оно перешло в переменную _currentSlide)
        _slides.RemoveAt(_slides.Count - 1);
    }
}
