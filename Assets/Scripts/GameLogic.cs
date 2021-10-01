using System.Collections.Generic;

/// <summary>
/// Основной класс игровой логики. Создает игровую колоду, передает карты в коллекции-руки и считает их стоимость.
/// </summary>
public static class GameLogic
{
    // Список хранящий все возможные ранги карт
    private static readonly List<string> StringRankList = new List<string>
        {"six", "seven", "eight", "nine", "ten", "jack", "queen", "king", "ace"};
    // Список хранящий все возможные масти карт
    private static readonly List<string> StringSuitList = new List<string> {"c", "d", "h", "s"};
    // Список хранящий все карты колоды, исключая те, что хранятся в "руках"
    private static readonly List<Card> Deck = new List<Card>();
    // Списки хранящие карты находящиеся в "руках" игрока и диллера
    private static readonly List<Card> PlayerHand = new List<Card>();
    private static readonly List<Card> DealerHand = new List<Card>();
    // Переменная хранящая значение суммы текущих ставок (равна удвоенной ставке игрока)
    private static int _totalBet = 0;
    // Переменная хранящая значение доступных денежных средств игрока
    private static int _cash = 100;
    
    /// <summary>
    /// Сбрасывает поля класса до начальных значений, исключая поле _cash.
    /// </summary>
    public static void Reset()
    {
        // Очищаем "руки"
        PlayerHand.Clear();
        DealerHand.Clear();
        // Присваиваем значение перменной текущей ставки
        _totalBet = 0;
    }
    /// <summary>
    /// Сбрасывает поля класса до начальных значений, включая поле _cash.
    /// </summary>
    public static void TotalReset()
    {
        Reset();
        // Присваиваем значение перменной банкролла
        _cash = 100;
    }
    /// <summary>
    /// Создает колоду карт.
    /// </summary>
    public static void GenerateDeck()
    {
        // Очищаем колоду
        Deck.Clear();
        // Для каждой комбинации масть-ранг создаем экземпляр класса Card
        foreach (var suit in StringSuitList)
        {
            foreach (var rank in StringRankList)
            {
                var newCard = new Card(suit, rank);
                Deck.Add(newCard);
            }
        }
        // Тасуем колоду
        ShuffleDeck();
    }
    /// <summary>
    /// Тасует колоду карт (<c>Deck</c>) по методу Фишера-Йетса.
    /// </summary>
    private static void ShuffleDeck()
    {
        // Создаем экземпляр класса Random для работы с ним
        var random = new System.Random();
        // Записываем длину колоды
        var n = Deck.Count;
        while (n > 1)
        {
            // Уменьшаем счетчик
            n--;
            // Выбираем случайное число в диапазоне от 0 до n + 1 (текущая величина счетчика + 1) 
            var k = random.Next(n + 1);
            // Запоминаем k-й элемент
            var card = Deck[k];
            // Записываем под индексом k n-й элемент
            Deck[k] = Deck[n];
            // Записываем под индексом n k-й элемент
            Deck[n] = card;
        }
    }
    /// <summary>
    /// Считает суммарную стоимость карт в "руке". 
    /// </summary>
    /// <param name="handType">Тип руки.</param>
    /// <returns>Стоимость "руки".</returns>
    public static int GetHandValue(HandType handType)
    {
        // Список хранящий карты "руки"
        // Выбирается в зависимости от полученного параметра
        List<Card> hand = new List<Card>();
        switch (handType)
        {
            case HandType.Dealer:
                hand = DealerHand;
                break;
            case HandType.Player:
                hand = PlayerHand;
                break;
        }
        // Переменная хранящая стоимость "руки"
        int totalValue = 0;
        // Переменная хранящая количество тузов в "руке"
        int amountOfAces = 0;
        // Переменная хранящая количество вальтов, дам и королей в "руке"
        int amountOfPics = 0;
        // Считаем стоимость "руки" без тузов
        foreach (Card card in hand)
        {
            // Отсев тузов
            if (card.Rank == "ace")
            {
                amountOfAces++;
            }
            else
            {
                // Сверяем ранг карты с его ценностью
                int cardValue = 0;
                switch (card.Rank)
                {
                    case "six":
                        cardValue = 6;
                        break;
                    case "seven":
                        cardValue = 7;
                        break;
                    case "eight":
                        cardValue = 8;
                        break;
                    case "nine":
                        cardValue = 9;
                        break;
                    case "ten":
                        cardValue = 10;
                        break;
                    case "jack":
                        cardValue = 2;
                        amountOfPics++;
                        break;
                    case "queen":
                        cardValue = 3;
                        amountOfPics++;
                        break;
                    case "king":
                        cardValue = 4;
                        amountOfPics++;
                        break;

                }
                // Добавляем стоимость карты в переменную
                totalValue += cardValue;
            }
        }
        // Считаем стоимость тузов в руке и добавляем ее к общей сумме
        if (amountOfAces > 0)
        {
            // Проверяем на одно из победных условий
            if (amountOfAces == 2 && totalValue == 0)
            {
                totalValue = 21;
            }
            else
            {
                for (int i = 0; i < amountOfAces; i++)
                {
                    // Если сумма стоимостей туза (11) и остальных карт не превышает 21, то берем стоимость туза равной 11
                    if ((totalValue + 11) <= 21)
                    {
                        totalValue += 11;
                    }
                    // Иначе берем стоимость туза равной 1
                    else
                    {
                        totalValue += 1;
                    }
                }
            }
        }
        // Проверяем победное условие "5 картинок"
        if (amountOfPics == 5 && hand.Count == 5)
        {
            totalValue = 21;
        }
        // Возвращаем суммарную стоимость "руки"
        return totalValue;
    }
    /// <summary>
    /// Тип руки.
    /// </summary>
    public enum HandType
    {
        Player,
        Dealer
    }
    /// <summary>
    /// Достает верхнюю карту из колоды и добавляет ее в "руку".
    /// </summary>
    /// <param name="taker">Тип "руки".</param>
    /// <returns>Взятую карту.</returns>
    public static Card TakeCard(HandType taker)
    {
        // Присваиваем переменной значение первого элемента списка-колоды
        Card takenCard = Deck[0];
        // Добавляем значение переменной в список-руку, определяемую входным параметром и удаляем первый элемент списка-колоды
        switch (taker)
        {
            case HandType.Dealer:
                DealerHand.Add(takenCard);
                Deck.RemoveAt(0);
                break;
            case HandType.Player:
                PlayerHand.Add(takenCard);
                Deck.RemoveAt(0);
                break;
        }
        // Возвращаем значение переменной-карты
        return takenCard;
    }
    /// <summary>
    /// Увеличивает значение ставки на указанное число.
    /// </summary>
    /// <param name="delta">Число на которое необходимо увеличить ставку.</param>
    /// <returns><c>True</c> - в случае, если средств на операцию хватает. <c>False</c> - при недостатке средств.</returns>
    public static bool IncreaseTotalBet(int delta)
    {
        // Проверяем достаточно ли средств у игрока
        if (_cash - delta >= 0)
        {
            // Увеличиваем полную ставку на дважды взятую ставку пользователя
            _totalBet += delta * 2;
            // Вычитаем значение ставки из средств игрока
            _cash -= delta;
            // Сообщаем об успешной операции
            return true;
        } 
        // Сообщаем об ошибке
        return false;
    }
    /// <summary>
    /// Возвращает значение переменной полной ставки.
    /// </summary>
    public static int GetTotalBet()
    {
        return _totalBet;
    }
    /// <summary>
    /// Возвращает значение переменной средств игрока.
    /// </summary>
    public static int GetCash()
    {
        return _cash;
    }
    /// <summary>
    /// Возвращает значение ставки игрока в его средства.
    /// </summary>
    public static void ReturnBet()
    {
        _cash += _totalBet / 2;
        // Обнуляем ставку
        _totalBet = 0;
    }
    /// <summary>
    /// Передает выигрыш (двойную ставку) в средства игрока.
    /// </summary>
    public static void TakeGain()
    {
        _cash += _totalBet;
        // Обнуляем ставку
        _totalBet = 0;
    }
    /// <summary>
    /// Структура данных для игральной карты
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Масть карты
        /// </summary>
        public readonly string Suit;
        /// <summary>
        /// Ранг карты
        /// </summary>
        public readonly string Rank;

        public Card(string cardSuit, string cardRank)
        { 
            Suit = cardSuit;
            Rank = cardRank;
        }
    }

}
