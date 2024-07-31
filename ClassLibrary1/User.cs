using Telegram.Bot.Types;
namespace ClassLibrary
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int StoreValue { get; set; }
        public double AverageRevenueYear { get; set; }
        public string CurrentState { get; set; }

        public User(Message message)
        {
            Id = message.Chat.Id;
            Name = message.Chat.FirstName;
            LastName = message.Chat.LastName;
            UserName = message.Chat.Username;
            CurrentState = null; // И
        }
    }
}
