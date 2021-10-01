using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Отвечает за работу основной игровой сцены и управляет всем ее интерфейсом.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public RectTransform cardPrefab;
    public RectTransform chipPrefab;
    [Header("Containers")]
    public RectTransform dealerHand;
    public RectTransform playerHand;
    public RectTransform chipContainer;
    [Header("Text fields")]
    public Text totalBet;
    public Text playerValue;
    public Text dealerValue;
    public Text cash;
    public Text message;
    [Header("Main buttons")]
    public Button makeBet;
    public Button stand;
    public Button restart;
    public Button quit; 
    public Button chipTen;
    public Button chipFifty;
    public Button chipHundred;
    public Button menuButton;
    public Button clear;
    [Header("Menu")]
    public RectTransform menu;
    public Button resume;
    public Button exit;
    
    // Переменная для определения стартового состояния сцены
    private bool _isGameStart = true;
    // Вспомогательная переменная для текстового поля кнопки "makeBet"
    private Text _makeBetText;

    void Awake()
    {
        // Получаем текстовое поле кнопки "Сделать ставку" для дальнейших манипуляций с ее названием
        _makeBetText = makeBet.transform.Find("Text").GetComponent<Text>();
        // Назначаем игровым кнопкам соответствующие функции
        chipTen.onClick.AddListener(delegate { ChipFunction(ChipType.Red); });
        chipFifty.onClick.AddListener(delegate { ChipFunction(ChipType.Blue); });
        chipHundred.onClick.AddListener(delegate { ChipFunction(ChipType.Green); });
        makeBet.onClick.AddListener(MakeBet);
        stand.onClick.AddListener(DealerMakesMove);
        restart.onClick.AddListener(RestartGame);
        quit.onClick.AddListener(EndTry);
        menuButton.onClick.AddListener(delegate { menu.gameObject.SetActive(true); });
        resume.onClick.AddListener(delegate { menu.gameObject.SetActive(false); });
        exit.onClick.AddListener(delegate { SceneManager.LoadScene("MenuScene"); });
        clear.onClick.AddListener(Clear);
        // Присваем значение полю отвечающему за доступные игроку средства
        cash.text = GameLogic.GetCash().ToString();
        // Создаем новую колоду
        GameLogic.GenerateDeck();
    }
    /// <summary>
    /// Добавляет карту в "руку" игрока.
    /// </summary>
    private void MakeBet()
    {
        // Проверяем сделал ли игрок ставку
        if (GameLogic.GetTotalBet() > 0)
        {
            // Создаем экземпляр карты, как визуальный объект (далее ВО)
            var cardInstance = Instantiate(cardPrefab, playerHand);
            // Получаем экземпляр карты из списка-колоды, как программный объект (далее ПО)
            var cardInfo = GameLogic.TakeCard(GameLogic.HandType.Player);
            // Присваиваем ВО соотвествующее изображение - спрайт, путь к которой определяет соответствующий ПО
            cardInstance.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Cards/" +
                cardInfo.Suit + "_" + cardInfo.Rank);
            // Присваиваем значение полю показывающему стоимость "руки" игрока
            var playerHandValue = GameLogic.GetHandValue(GameLogic.HandType.Player);
            playerValue.text = playerHandValue.ToString();
            // Если карта берется первый раз, то дилер также получает карту
            if (_isGameStart)
            {
                cardInstance = Instantiate(cardPrefab, dealerHand);
                cardInfo = GameLogic.TakeCard(GameLogic.HandType.Dealer);
                cardInstance.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Cards/" +
                    cardInfo.Suit + "_" + cardInfo.Rank);
                // Выводим сцену из стартого состояния
                _isGameStart = false;
                // Присваиваем значение полю показывающему стоимость "руки" дилера
                dealerValue.text = GameLogic.GetHandValue(GameLogic.HandType.Dealer).ToString();
                // Меняем режим интерфейса
                ChangeInterfaceMode(InterfaceMode.Middle);
                // Предлагаем игроку карту через сообщение
                DisplayMessage(MessageType.CardOffer);
            }
            // Проверка на перебор
            if (playerHandValue > 21)
            {
                // Сообщаем о поражении
                DisplayMessage(MessageType.Lose);
                ChangeInterfaceMode(InterfaceMode.End);
            }
            // Проверка на победу игрока
            else if (playerHandValue == 21)
            {
                // Сообщаем о победе
                DisplayMessage(MessageType.Win);
                // Перечисляем выигрыш на счет игрока
                GameLogic.TakeGain();
                cash.text = GameLogic.GetCash().ToString();
                ChangeInterfaceMode(InterfaceMode.End);
            }
        }
    }
    /// <summary>
    /// Добавляет на стол фишку и в зависимости от ее стоимости увеличивает сумму ставки.
    /// </summary>
    /// <param name="chipType">Определяет тип добавляемой фишки.</param>
    private void ChipFunction(ChipType chipType)
    {
        // Переменная для хранения пути к спрайту
        var spritePath = "";
        switch (chipType)
        {
            case ChipType.Red:
                // Проверяем возможность ставки
                if (!GameLogic.IncreaseTotalBet(10))
                {
                    // Сообщаем о невозможности ставки
                    DisplayMessage(MessageType.BetIssue);
                    return;
                }
                // Задаем путь к спрайту
                spritePath = "Sprites/UI/chip_red";
                break;
            case ChipType.Blue:
                if (!GameLogic.IncreaseTotalBet(50))
                {
                    DisplayMessage(MessageType.BetIssue);
                    return;
                }
                spritePath = "Sprites/UI/chip_blue";
                break;
            case ChipType.Green:
                if (!GameLogic.IncreaseTotalBet(100))
                {
                    DisplayMessage(MessageType.BetIssue);
                    return;
                }
                spritePath = "Sprites/UI/chip_green";
                break;
        }
        // Создаем экземпляр фишки
        var chipInstance = Instantiate(chipPrefab, chipContainer);
        // Добавляем ему спрайт
        chipInstance.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);
        // Получаем значения ширины и высоты контейнера
        var containerRect = chipContainer.rect;
        // Задаем случайные координаты внутри контейнера экземпляру фишки
        chipInstance.localPosition = new Vector3(Random.Range(-containerRect.width / 2, containerRect.width / 2), 
            Random.Range(-containerRect.height / 2, containerRect.height / 2), chipInstance.position.z);
        // Обновляем значения текстовых полей ставки и доступных средств
        totalBet.text = GameLogic.GetTotalBet().ToString();
        cash.text = GameLogic.GetCash().ToString();
        // Делаем активной кнопку сброса ставки
        clear.gameObject.SetActive(true);
    }
    /// <summary>
    /// Тип фишки.
    /// </summary>
    enum ChipType
    {
        Red,
        Blue,
        Green
    }
    /// <summary>
    /// Заполняет "руку" дилера картами, после чего определяет победителя.
    /// </summary>
    private void DealerMakesMove()
    {
        // Используем сопрограмму для того, чтобы реализовать задержку во время добавления карт
        StartCoroutine(DealerMove());
    }
    /// <summary>
    /// Сопрограмма для <c>DealerMakesMove().</c>
    /// </summary>
    /// <returns></returns>
    private IEnumerator DealerMove()
    {
        // Проверяем стоимость "руки" дилера согласно правилу
        while (GameLogic.GetHandValue(GameLogic.HandType.Dealer) < 17)
        {
            // Создаем экземпляр карты и заполняем его
            var cardInstance = Instantiate(cardPrefab, dealerHand);
            var cardInfo = GameLogic.TakeCard(GameLogic.HandType.Dealer);
            cardInstance.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Cards/" +
                cardInfo.Suit + "_" + cardInfo.Rank);
            // Обновляем значение текстового поля стоимости "руки" дилера
            dealerValue.text = GameLogic.GetHandValue(GameLogic.HandType.Dealer).ToString();
            // Запускаем задержку на полсекунды
            yield return new WaitForSeconds(0.5f);
        }
        // Получаем значения стоимостей "рук" дилера и игрока
        var dV = GameLogic.GetHandValue(GameLogic.HandType.Dealer);
        var pV = GameLogic.GetHandValue(GameLogic.HandType.Player);
        // Условие победы дилера
        if (dV > pV && dV <= 21)
        {
            DisplayMessage(MessageType.Lose);
        }
        // Условие ничьей
        else if (dV == pV && dV <= 21)
        {
            DisplayMessage(MessageType.Draw);
            GameLogic.ReturnBet();
        }
        // Победа игрока
        else
        {
            DisplayMessage(MessageType.Win);
            GameLogic.TakeGain();
        }
        ChangeInterfaceMode(InterfaceMode.End);
        cash.text = GameLogic.GetCash().ToString();
    }
    /// <summary>
    /// Очищает контейнер с фишками и возвращает ставку игроку.
    /// </summary>
    private void Clear()
    {
        // Очищаем контейнер
        ClearContainer(chipContainer);
        // Возвращаем ставку
        GameLogic.ReturnBet();
        // Делаем кнопку неактивной
        clear.gameObject.SetActive(false);
        // Обновляем текстовые поля
        totalBet.text = GameLogic.GetTotalBet().ToString();
        cash.text = GameLogic.GetCash().ToString();
    }
    /// <summary>
    /// Вспомогательная функция для уничтожения дочерних объектов какого-либо объекта.
    /// </summary>
    /// <param name="container">Объект который необходимо очистить от дочерних объектов.</param>
    private void ClearContainer(RectTransform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
    /// <summary>
    /// Отображает сообщение в отведенном для этого элементе интерфейса.
    /// </summary>
    /// <param name="messageType">Тип сообщения.</param>
    private void DisplayMessage(MessageType messageType)
    {
        switch (messageType)
        {
            case MessageType.Win:
                // Помещаем сообщение в текстовое поле
                message.text = "Поздравляю! Победа ваша";
                break;
            case MessageType.Lose:
                message.text = "Вы проиграли";
                break;
            case MessageType.Draw:
                message.text = "Нонсенс! Ничья";
                break;
            case MessageType.Start:
                message.text = "Делайте вашу ставку и берите карту";
                break;
            case MessageType.BetIssue:
                StartCoroutine(BetIssueMessage());
                break;
            case MessageType.CardOffer:
                message.text = "Еще карту?";
                break;
        }
    }
    /// <summary>
    /// Вспомогательная функция для отображения сообщения об ошибке с задержкой.
    /// </summary>
    private IEnumerator BetIssueMessage()
    {
        // Помещаем сообщение об ошибке в текстовое поле
        message.text = "У вас не хватает средств для такой ставки";
        // Меняем цвет текста
        message.color = Color.red;
        // Запускаем задержку в 1 секунду
        yield return new WaitForSeconds(1);
        // Меняем сообщение на стандартное
        DisplayMessage(MessageType.Start);
        // Возвращаем стандартный цвет текста
        message.color = Color.black;
    }
    /// <summary>
    /// Тип сообщения.
    /// </summary>
    enum MessageType
    {
        Win,
        Lose,
        Draw,
        Start,
        BetIssue,
        CardOffer
    }
    /// <summary>
    /// Меняет набор активных элементов интерфейса и изменяет их текстовые поля.
    /// </summary>
    /// <param name="interfaceMode">Режим интерфейса.</param>
    private void ChangeInterfaceMode(InterfaceMode interfaceMode)
    {
        switch (interfaceMode)
        {
            // Режим начала игры
            // Доступна возможность добавить фишек для ставки
            // Доступна возможность сделать ставку
            case InterfaceMode.Start:
                _makeBetText.text = "Сделать ставку";
                makeBet.gameObject.SetActive(true);
                stand.gameObject.SetActive(false);
                restart.gameObject.SetActive(false);
                quit.gameObject.SetActive(false);
                chipTen.interactable = true;
                chipFifty.interactable = true;
                chipHundred.interactable = true;
                break;
            // Режим середины игры
            // Доступны возможности взять карту и остановиться
            case InterfaceMode.Middle:
                _makeBetText.text = "Взять карту";
                stand.gameObject.SetActive(true);
                chipTen.interactable = false;
                chipFifty.interactable = false;
                chipHundred.interactable = false;
                clear.gameObject.SetActive(false);
                break;
            // Режим конца игры
            // Доступна возможность выйти из игры и при наличии средств продолжить игру
            case InterfaceMode.End:
                makeBet.gameObject.SetActive(false);
                stand.gameObject.SetActive(false);
                quit.gameObject.SetActive(true);
                if (GameLogic.GetCash() > 0)
                {
                    restart.gameObject.SetActive(true);
                }
                break;
        }
    }
    /// <summary>
    /// Типы режимов интерфейса.
    /// </summary>
    enum InterfaceMode
    {
        Start,
        Middle,
        End
    }
    /// <summary>
    /// Сбрасывает состояние сцены до начального.
    /// </summary>
    private void RestartGame()
    {
        // Меняем переменную состояния сцены
        _isGameStart = true;
        // Возвращаем в начальное состояние программные версии колоды и "рук" дилера и игрока
        GameLogic.Reset();
        // Пересоздаем колоду
        GameLogic.GenerateDeck();
        // Очищаем визуальные контейнеры
        ClearContainer(playerHand);
        ClearContainer(dealerHand);
        ClearContainer(chipContainer);
        // Меняем режим интерфейса
        ChangeInterfaceMode(InterfaceMode.Start);
        // Заполняем текстовые поля
        totalBet.text = GameLogic.GetTotalBet().ToString();
        dealerValue.text = GameLogic.GetHandValue(GameLogic.HandType.Dealer).ToString();
        playerValue.text = GameLogic.GetHandValue(GameLogic.HandType.Player).ToString();
        cash.text = GameLogic.GetCash().ToString();
        // Выводим стартовое сообщение
        DisplayMessage(MessageType.Start);
    }
    /// <summary>
    /// Сохраняет счет игрока в таблицу рекордов и загружает главное меню.
    /// </summary>
    private void EndTry()
    {
        // Получаем счет игрока
        var newRecord = GameLogic.GetCash();
        if (newRecord > 0)
        {
            // Записываем счет
            RecordsManager.AddTry(newRecord);
        }
        // Сбрасываем поля статического класса до начального состояния
        GameLogic.TotalReset();
        // Загружаем сцену меню
        SceneManager.LoadScene("MenuScene");
    }
}