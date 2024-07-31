using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLogic
{
    public class BasicLogic
    {
        public static ITelegramBotClient bot = new TelegramBotClient("7397871713:AAGg-UhBY1g0rHv0k3BDldNDfxVVHpo2T4Q");

        static List<KeyboardButton> buttons1 = new List<KeyboardButton>() { new KeyboardButton("Рассчитать эффекты"),
                                                                            new KeyboardButton("Подписаться на наш канал"),
                                                                            new KeyboardButton("Поделиться ботом"),
                                                                            new KeyboardButton("Связаться с менеджером") };

        static List<KeyboardButton> buttons2 = new List<KeyboardButton>() { new KeyboardButton("Перерасчёт"),
                                                                            new KeyboardButton("Подписаться на наш канал"),
                                                                            new KeyboardButton("Связаться с менеджером"),
                                                                            new KeyboardButton("Назад") };

        static List<KeyboardButton> buttons3 = new List<KeyboardButton>() { new KeyboardButton("Перейти на наш канал"),
                                                                            new KeyboardButton("Назад") };

        static List<KeyboardButton> buttons4 = new List<KeyboardButton>() { new KeyboardButton("Назад") };

        static ReplyKeyboardMarkup keyboard1 = new ReplyKeyboardMarkup(buttons1) { ResizeKeyboard = true };
        static ReplyKeyboardMarkup keyboard2 = new ReplyKeyboardMarkup(buttons2) { ResizeKeyboard = true };
        static ReplyKeyboardMarkup keyboard3 = new ReplyKeyboardMarkup(buttons3) { ResizeKeyboard = true };
        static ReplyKeyboardMarkup keyboard4 = new ReplyKeyboardMarkup(buttons4) { ResizeKeyboard = true };

        static ForceReplyMarkup ForceReplyMarkup = new ForceReplyMarkup();
        private static Dictionary<long, ClassLibrary.User> UserCollection = new Dictionary<long, ClassLibrary.User>();
        private static ClassLibrary.User User;


        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            try
            {
                if (!UserCollection.ContainsKey(chatId))
                {
                    UserCollection.Add(chatId, new ClassLibrary.User(message));
                    await SendWelcomeMessages(botClient, chatId);
                    return; // Добавляем return, чтобы избежать обработки дальнейших сообщений
                }

                var user = UserCollection[chatId];

                if (user.CurrentState == "awaiting_revenue")
                {
                    await SaveRevenueValue(botClient, message);
                    return;
                }

                if (user.CurrentState == "awaiting_store")
                {
                    await SaveStoreValue(botClient, message);
                    return;
                }

                switch (messageText)
                {
                    case ("Рассчитать эффекты"):
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Укажите количество торговых точек Вашей сети",
                            replyMarkup: ForceReplyMarkup,
                            cancellationToken: cancellationToken);
                        user.CurrentState = "awaiting_store";
                        UserCollection[chatId] = user;
                        break;

                    case ("Поделиться ботом"):
                        await ShareBotName(botClient, chatId);
                        break;

                    case ("Перерасчёт"):
                        if (user.StoreValue > 0) // Проверяем, что количество торговых точек было задано
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Укажите среднюю выручку торговой сети в год, например 38 000 000 рублей",
                                replyMarkup: ForceReplyMarkup,
                                cancellationToken: cancellationToken);
                            user.CurrentState = "awaiting_revenue";
                            UserCollection[chatId] = user;
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Сначала укажите количество торговых точек.",
                                replyMarkup: keyboard1,
                                cancellationToken: cancellationToken);
                        }
                        break;

                    case ("Подписаться на наш канал"):
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Ура! Теперь все новости доступны Вам на нашем канале",
                            replyMarkup: new ReplyKeyboardMarkup(new List<KeyboardButton> { new KeyboardButton("Перейти на наш канал"), new KeyboardButton("Назад") })
                            { ResizeKeyboard = true },
                            cancellationToken: cancellationToken);
                        break;

                    case ("Перейти на наш канал"):
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Перейдите на наш канал по ссылке: [GoodsForeCast](https://t.me/goodsforecast_blog)",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken);
                        break;

                    case ("Связаться с менеджером"):
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Здравствуйте, меня зовут [Имя]. Какой у вас вопрос?",
                            replyMarkup: new ReplyKeyboardMarkup(new List<KeyboardButton> { new KeyboardButton("Назад") })
                            { ResizeKeyboard = true });
                        break;

                    case ("Назад"):
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Выберите опцию:",
                            replyMarkup: keyboard1);
                        user.CurrentState = null; // Сбрасываем состояние
                        UserCollection[chatId] = user;
                        break;

                    default:
                        var replyMessage = message.ReplyToMessage;
                        if (replyMessage == null)
                        {
                            // Обработка пустого сообщения
                            await PrintVoidMessage(botClient, message);
                        }
                        else
                        {
                            switch (replyMessage.Text)
                            {
                                case ("Укажите количество торговых точек Вашей сети"):
                                    await SaveStoreValue(botClient, message);
                                    break;
                                case ("Укажите среднюю выручку торговой сети в год, например 38 000 000 рублей"):
                                    await SaveRevenueValue(botClient, message);
                                    break;
                                default:
                                    await PrintVoidMessage(botClient, message);
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                await HandlePollingErrorAsync(botClient, ex, cancellationToken);
            }
        }

        private static async Task ShareBotName(ITelegramBotClient botClient, long chatId)
        {
            // Получаем имя бота
            var botInfo = await botClient.GetMeAsync();
            var botName = botInfo.Username;

            // Отправляем сообщение с именем бота и инструкцией
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Вот имя нашего бота: @{botName}\n\n" +
                      "Копируйте это имя и делитесь им с другими!",
                replyMarkup: keyboard1);
        }

        private static async Task SendWelcomeMessages(ITelegramBotClient botClient, long chatId)
        {
            // Первое сообщение о функциональности бота
            var functionalityMessage = "*Что умеет этот бот?*\n" +
                                       "Поможет рассчитать эффекты от применения автоматизированного инструмента " +
                                       "контроля доступностью товара на полке магазина";

            // Второе приветственное сообщение
            var userName = await GetUserNameAsync(botClient, chatId);
            var welcomeMessage = $"Привет, *{userName}*!\n" +
                                 "Бот поможет рассчитать эффект от применения решения OSA от GoodsForeCast, " +
                                 "а также оперативно заказать демонстрацию нашего продукта в удобное Вам время.";

            // Отправляем первое сообщение
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: functionalityMessage,
                parseMode: ParseMode.Markdown);

            // Отправляем второе сообщение с кнопками
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: welcomeMessage,
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard1);
        }

        private static async Task<string> GetUserNameAsync(ITelegramBotClient botClient, long chatId)
        {
            var chat = await botClient.GetChatAsync(chatId);
            return chat.FirstName ?? "Пользователь";
        }

        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static async Task SaveStoreValue(ITelegramBotClient botClient, Message message)
        {
            if (UserCollection.TryGetValue(message.Chat.Id, out var user))
            {
                var normalizedText = message.Text.Replace(" ", "");

                if (int.TryParse(normalizedText, out var convValue))
                {
                    if (convValue > 0)
                    {
                        user.StoreValue = convValue;
                        user.CurrentState = "awaiting_revenue"; // Устанавливаем состояние ожидания выручки
                        UserCollection[user.Id] = user;

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Укажите среднюю выручку торговой сети в год, например 38 000 000 рублей",
                            replyMarkup: ForceReplyMarkup);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка валидации. Количество торговых точек должно быть положительным целым числом больше 0.");
                        // Не меняем клавиатуру, чтобы ожидать новые данные
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка валидации. Введены некорректные значения. Пожалуйста, введите целое положительное число больше 0.");
                    // Не меняем клавиатуру, чтобы ожидать новые данные
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Пользователь не найден. Начните с указания количества торговых точек.");
                // Не меняем клавиатуру, чтобы ожидать новые данные
            }
        }
        private static async Task SaveRevenueValue(ITelegramBotClient botClient, Message message)
        {
            if (UserCollection.TryGetValue(message.Chat.Id, out var user))
            {
                var normalizedText = message.Text.Replace(',', '.');

                if (double.TryParse(normalizedText, NumberStyles.Any, CultureInfo.InvariantCulture, out var revenueValue))
                {
                    if (revenueValue >= 0)
                    {
                        var effect = revenueValue * 1.4;

                        user.AverageRevenueYear = revenueValue;
                        user.CurrentState = null; // Сбрасываем состояние после успешного ввода
                        UserCollection[user.Id] = user;

                        var formattedRevenueValue = revenueValue.ToString("N2", CultureInfo.InvariantCulture).Replace('.', ',');
                        var formattedEffect = effect.ToString("N2", CultureInfo.InvariantCulture).Replace('.', ',');

                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"Предварительные эффекты:\n+1,4 % к РТО\n+ {formattedEffect} рублей в год",
                            replyMarkup: keyboard2);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка валидации. Выручка должна быть неотрицательным числом.");
                        // Не меняем клавиатуру, чтобы ожидать новые данные
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка валидации. Введены некорректные значения. Пожалуйста, введите корректное число.");
                    // Не меняем клавиатуру, чтобы ожидать новые данные
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Пользователь не найден. Начните с указания количества торговых точек.");
                // Не меняем клавиатуру, чтобы ожидать новые данные
            }
        
        }

        private static async Task PrintVoidMessage(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Пустое сообщение",
                replyMarkup: keyboard1);
        }
    }
}
