using System;
using System.Threading.Tasks;
using TableTennis.DataAccess.Telegram;
using TableTennis.RR;
using TableTennis.RR.Models;
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
        private readonly RealTimeRetriever _realTimeRetriever;

        public TableTennisBot(TelegramBotConfiguration configuration, RealTimeRetriever realTimeRetriever, IChatsRepository chatsRepository,
            IAccessTokenRepository accessTokenRepository)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _realTimeRetriever = realTimeRetriever ?? throw new ArgumentNullException(nameof(realTimeRetriever));
            _chatsRepository = chatsRepository ?? throw new ArgumentNullException(nameof(chatsRepository));
            _accessTokenRepository =
                accessTokenRepository ?? throw new ArgumentNullException(nameof(accessTokenRepository));

            _botClient = new TelegramBotClient(_configuration.AccessToken);
            _botClient.OnMessage += MessageHandler;
            _realTimeRetriever.OnGoodBigScorePercentageFound += GoodBigScorePercentageHandler;
            _botClient.StartReceiving();
        }

        private async void GoodBigScorePercentageHandler(int totalgamescount, int totalbigscorescount, string player1name, string player2name)
        {
            var allAuthedChats = await _chatsRepository.GetAllChatsAsync();
            var message = "🏓 *Table Tennis*\n";
            message += $"{player1name} vs {player2name}\n";
            message += $"Total: {totalgamescount}\n";
            message += $"BigScores: {totalbigscorescount}\n\n";
            message += "`There is a big chance of rounds have odd score.`";
            foreach (var chatId in allAuthedChats)
            {
                await _botClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
            }
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
            await _botClient.SendTextMessageAsync(message.Chat.Id, "*I will keep you updated for good bets.*", ParseMode.Markdown);
        }

        private async Task<string> GenerateAccessTokenAsync()
        {
            var guid = Guid.NewGuid().ToString();
            await _accessTokenRepository.AddAsync(guid);

            return guid;
        }
    }
}