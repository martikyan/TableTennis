using System;
using System.Threading.Tasks;
using TableTennis.DataAccess.Telegram;
using TableTennis.RR;
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
        private readonly ISharedGamesRepository _sharedGamesRepository;

        public TableTennisBot(
            TelegramBotConfiguration configuration,
            RealTimeRetriever realTimeRetriever,
            IChatsRepository chatsRepository,
            IAccessTokenRepository accessTokenRepository,
            ISharedGamesRepository sharedGamesRepository)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _realTimeRetriever = realTimeRetriever ?? throw new ArgumentNullException(nameof(realTimeRetriever));
            _chatsRepository = chatsRepository ?? throw new ArgumentNullException(nameof(chatsRepository));
            _accessTokenRepository =
                accessTokenRepository ?? throw new ArgumentNullException(nameof(accessTokenRepository));
            _sharedGamesRepository =
                sharedGamesRepository ?? throw new ArgumentNullException(nameof(sharedGamesRepository));

            _botClient = new TelegramBotClient(_configuration.AccessToken);
            _botClient.OnMessage += MessageHandler;
            _realTimeRetriever.OnGoodBigScorePercentageFound += GoodBigScorePercentageHandler;
            _realTimeRetriever.OnUnbalancedOddsFound += UnbalancedOddsHandler;
            _botClient.StartReceiving();
        }

        private async void UnbalancedOddsHandler(double odds1, double odds2, string player1Name, string player2Name)
        {
            var alreadyPublished = await _sharedGamesRepository.ExistsAsync(player1Name, player2Name + "B");
            if (alreadyPublished) return;

            // Todo "+ B" is not a good way to make this unique,
            // also, do we need it?
            await _sharedGamesRepository.AddAsync(player1Name, player2Name + "B");

            var allAuthedChats = await _chatsRepository.GetAllChatsAsync();
            var message = "🏓 *Table Tennis*\n";
            message += $"{player1Name} vs {player2Name}\n";
            message += $"{odds1} x {odds2}\n";
            message += "There is a big difference of the odds, ";
            message += "so players' skills differ a lot.\n";
            message += "`There is a big chance of rounds have odd score.`";


            foreach (var chatId in allAuthedChats)
                await _botClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
        }

        private async void GoodBigScorePercentageHandler(int totalGamesCount, int totalBigScoresCount,
            string player1Name, string player2Name)
        {
            var alreadyPublished = await _sharedGamesRepository.ExistsAsync(player1Name, player2Name);
            if (alreadyPublished) return;
            await _sharedGamesRepository.AddAsync(player1Name, player2Name);

            var allAuthedChats = await _chatsRepository.GetAllChatsAsync();
            var message = "🏓 *Table Tennis*\n";
            message += $"{player1Name} vs {player2Name}\n";
            message += $"Total games analyzed: {totalGamesCount}\n";
            message += $"BigScores percentage: {Math.Round(totalBigScoresCount * 100.0 / totalGamesCount, 2)}%\n";
            message += "`There is a big chance of rounds have odd score.`";

            foreach (var chatId in allAuthedChats)
                await _botClient.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
        }

        private async void MessageHandler(object sender, MessageEventArgs e)
        {
            await HandleBroadcastMessageRequest(e.Message);
            await HandleTokenGenerationMessageRequest(e.Message);
            await HandleAuthenticationMessageRequest(e.Message);
            await HandleStartMessageRequest(e.Message);
        }

        private async Task HandleBroadcastMessageRequest(Message message)
        {
            if (!string.Equals(message.Chat.Username, _configuration.AdminUsername)) return;
            if (string.IsNullOrEmpty(message.Text)) return;
            if (!message.Text.StartsWith("/broadcast ")) return;

            var textMessage = message.Text.Remove(0, 11);
            if (string.IsNullOrEmpty(textMessage)) return;

            var finalText = $"Broadcast message by @{_configuration.AdminUsername}\n{textMessage}";
            foreach (var chatId in await _chatsRepository.GetAllChatsAsync())
                await _botClient.SendTextMessageAsync(chatId, finalText, ParseMode.Markdown);
        }

        private async Task HandleStartMessageRequest(Message message)
        {
            if (message.Text == "/start")
                await _botClient.SendTextMessageAsync(message.Chat.Id,
                    $"Get an *access token* from *@{_configuration.AdminUsername}* to start. Then just send it to me as a simple message.",
                    ParseMode.Markdown);
        }

        private async Task HandleTokenGenerationMessageRequest(Message message)
        {
            if (string.IsNullOrEmpty(message.Text)) return;
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

            var chatExist = await _chatsRepository.ExistsAsync(message.Chat.Id);
            if (chatExist)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "You are already subscribed to the updates.");
                return;
            }

            var atExists = await _accessTokenRepository.ExistsAsync(message.Text);
            if (!atExists)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "This token is invalid.");
                return;
            }

            var isUsed = await _accessTokenRepository.IsUsedAsync(message.Text);
            if (isUsed)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "This token is already used.");
                return;
            }

            await _accessTokenRepository.MakeUsedAsync(message.Text);
            await _chatsRepository.AddChatAsync(message.Chat.Id);
            await _botClient.SendTextMessageAsync(message.Chat.Id, "*Authenticated*", ParseMode.Markdown);
            await _botClient.SendTextMessageAsync(message.Chat.Id, "*I will keep you updated about good bets.*",
                ParseMode.Markdown);
        }

        private async Task<string> GenerateAccessTokenAsync()
        {
            var guid = Guid.NewGuid().ToString();
            await _accessTokenRepository.AddAsync(guid);

            return guid;
        }
    }
}