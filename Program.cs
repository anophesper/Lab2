using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("6786546902:AAE6NWwBv3CYST6lPhjO9Aw1bzJT98bZSc0");

        static async Task Main(string[] args)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types
            };

            Bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions
            );

            Console.WriteLine("Bot is up and running.");
            Console.ReadLine();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var message = update.Message;
                var messageParts = message.Text.Split(' ');

                switch (messageParts[0].ToLower())
                {
                    case "/start":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Welcome to Express Post Bot! Use /help to see available commands.",
                            cancellationToken: cancellationToken
                        );
                        break;
                    case "/help":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "/track [tracking number] - Track your parcel",
                            cancellationToken: cancellationToken
                        );
                        break;
                    case "/track":
                        if (messageParts.Length > 1)
                        {
                            string trackingNumber = messageParts[1];
                            string result = await TrackParcelAsync(trackingNumber);
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: result,
                                cancellationToken: cancellationToken
                            );
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Please provide a tracking number.",
                                cancellationToken: cancellationToken
                            );
                        }
                        break;
                    default:
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Unknown command. Use /help to see available commands.",
                            cancellationToken: cancellationToken
                        );
                        break;
                }
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = GetErrorMessage(exception);

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static string GetErrorMessage(Exception exception)
        {
            switch (exception)
            {
                case ApiRequestException apiRequestException:
                    return $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}";
                default:
                    return exception.ToString();
            }
        }

        private static async Task<string> TrackParcelAsync(string trackingNumber)
        {
            Database db = new Database();
            string query = $@"
            SELECT 
                p.BillOfLading, p.Type, p.Weight, p.Status,
                u1.FirstName AS SenderFirstName, u1.LastName AS SenderLastName, u1.PhoneNumber AS SenderPhone,
                u2.FirstName AS RecipientFirstName, u2.LastName AS RecipientLastName, u2.PhoneNumber AS RecipientPhone,
                b1.City AS OriginCity, b1.Address AS OriginAddress,
                b2.City AS DestinationCity, b2.Address AS DestinationAddress,
                b3.City AS CurrentCity, b3.Address AS CurrentAddress,
                prd.DeliveryPrice
            FROM 
                Parcel p
            JOIN 
                ParcelUsers pu ON p.BillOfLading = pu.BillOfLading
            JOIN 
                Users u1 ON pu.SenderUser = u1.ID
            JOIN 
                Users u2 ON pu.RecipientUser = u2.ID
            JOIN 
                Route r ON pu.Route = r.ID
            JOIN 
                Branch b1 ON r.Origin = b1.ID
            JOIN 
                Branch b2 ON r.Destination = b2.ID
            LEFT JOIN 
                ParcelRouteDelivery prd ON p.BillOfLading = prd.BillOfLading
            LEFT JOIN 
                Branch b3 ON prd.CurrentBranch = b3.ID
            WHERE 
                p.BillOfLading = '{trackingNumber}'";

            var result = await db.ExecuteQueryAsync(query);

            if (result.Rows.Count > 0)
            {
                var row = result.Rows[0];
                string senderInfo = $"Sender: {row["SenderFirstName"]} {row["SenderLastName"]} {row["SenderPhone"]}";
                string recipientInfo = $"Recipient: {row["RecipientFirstName"]} {row["RecipientLastName"]} {row["RecipientPhone"]}";
                string parcelInfo = $"Type: {row["Type"]}, Weight: {row["Weight"]} kg";
                string originInfo = $"Origin: {row["OriginCity"]}, {row["OriginAddress"]}";
                string destinationInfo = $"Destination: {row["DestinationCity"]}, {row["DestinationAddress"]}";
                string statusInfo = $"Status: {row["Status"]}";
                string currentBranchInfo = row["CurrentCity"] != DBNull.Value
                    ? $"Current Location: {row["CurrentCity"]}, {row["CurrentAddress"]}"
                    : "Current Location: N/A";

                return $"Tracking Number: {trackingNumber}\n\n{senderInfo}\n{recipientInfo}\n\n{parcelInfo}\n\n{originInfo}\n{destinationInfo}\n\n{statusInfo}\n{currentBranchInfo}";
            }
            else
                return "Tracking number not found.";
        }
    }
}
