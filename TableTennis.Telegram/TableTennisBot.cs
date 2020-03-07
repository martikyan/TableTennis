using System;
using System.Threading.Tasks;
using TableTennis.DataAccess.Telegram;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TableTennis.Telegram
{
    public class TableTennisBot
    {
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly TelegramBotClient _botClient;
        private readonly IChatsRepository _chatsRepository;
        private readonly TelegramBotConfiguration _configuration;

        public TableTennisBot(TelegramBotConfiguration configuration, IChatsRepository chatsRepository,
            IAccessTokenRepository accessTokenRepository)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _chatsRepository = chatsRepository ?? throw new ArgumentNullException(nameof(chatsRepository));
            _accessTokenRepository =
                accessTokenRepository ?? throw new ArgumentNullException(nameof(accessTokenRepository));

            _botClient = new TelegramBotClient(_configuration.AccessToken);
            _botClient.OnMessage += MessageHandler;
            _botClient.StartReceiving();
        }

        private async void MessageHandler(object sender, MessageEventArgs e)
        {
            await HandleTokenGenerationMessageRequest(e.Message);
            await HandleAuthenticationMessageRequest(e.Message);
            await HandleStartMessageRequest(e.Message);
        }

        private async Task HandleStartMessageRequest(Message message)
        {
            if (message.Text == "/start")
                await _botClient.SendTextMessageAsync(message.Chat.Id,
                    $"Get an *access token* from *@{_configuration.AdminUsername}* to start using me.",
                    ParseMode.Markdown);
        }

        private async Task HandleTokenGenerationMessageRequest(Message message)
        {
            if (!message.Text.StartsWith("/generate")) return;

            if (!string.Equals(message.Chat.Username, _configuration.AdminUsername)) return;

            var guid = await GenerateAccessTokenAsync();
            await _botClient.SendTextMessageAsync(message.Chat.Id,
                "Sending a new access token. Just send it to me to *authenticate*.", ParseMode.Markdown);
            await _botClient.SendTextMessageAsync(message.Chat.Id, $"`{guid}`", ParseMode.Markdown);
        }

        private async Task HandleAuthenticationMessageRequest(Message message)
        {
            if (message.Text?.Length != 36) return;

            var exist = await _chatsRepository.ExistsAsync(message.Chat.Id);
            if (exist) return;

            var isAuthenticated = await _accessTokenRepository.ExistsAsync(message.Text);
            if (!isAuthenticated) return;

            await _chatsRepository.AddChatAsync(message.Chat.Id);
            await _botClient.SendTextMessageAsync(message.Chat.Id, "*Authenticated*", ParseMode.Markdown);
        }

        private async Task<string> GenerateAccessTokenAsync()
        {
            var guid = Guid.NewGuid().ToString();
            await _accessTokenRepository.AddAsync(guid);

            return guid;
        }
    }
}