﻿namespace DiscordBot.Core.FoldingBot
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Discord;
    using Discord.Commands;

    using DiscordBot.Core.Attributes;
    using DiscordBot.Core.Interfaces;

    using Microsoft.Extensions.Logging;

    internal class FoldingBotModule : ModuleBase<SocketCommandContext>
    {
        private readonly Emoji hourglass = new Emoji("\u23F3");

        private readonly ILogger<FoldingBotModule> logger;

        private readonly IDiscordBotModuleService service;

        public FoldingBotModule(IDiscordBotModuleService service, ILogger<FoldingBotModule> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [Command("bad bot")]
        [Summary("Tell the bot it's being bad")]
        [Hidden]
        public Task AcknowledgeBadBot()
        {
            return Reply("D:");
        }

        [Command("good bot")]
        [Summary("Tell the bot it's being good")]
        [Hidden]
        public Task AcknowledgeGoodBot()
        {
            return Reply(":D");
        }

        [Command("fah")]
        [Summary("Start folding today or update to the latest software")]
        public Task GetFoldingAtHomeUrl()
        {
            return Reply(service.GetFoldingAtHomeUrl());
        }

        [Command("website")]
        [Summary("Learn more about this project")]
        public Task GetHomeUrl()
        {
            return Reply(service.GetHomeUrl());
        }

        [Command("distribution")]
        [Summary("Get the date of our next distribution")]
        [Development]
        public Task GetNextDistributionDate()
        {
            return Reply(service.GetNextDistributionDate());
        }

        [Command("user", RunMode = RunMode.Async)]
        [Usage("{address}")]
        [Summary("Get your stats for the next distribution based on your address")]
        [Development]
        public async Task GetUserStats(string bitcoinAddress)
        {
            await Reply(async () => await service.GetUserStats(bitcoinAddress));
        }

        [Command("help")]
        [Summary("Show the list of available commands")]
        public async Task Help()
        {
            await Reply(service.Help());
        }

        [Command("lookup", RunMode = RunMode.Async)]
        [Usage("{search criteria}")]
        [Summary("Helps to find yourself, not case sensitive and searches the start and end for a match")]
        [Development]
        public async Task LookupUser(string searchCriteria)
        {
            await Reply(async () => await service.LookupUser(searchCriteria));
        }

        [Command("{default}")]
        [Default]
        [Hidden]
        [Summary("Show the list of available commands")]
        public async Task NoCommand()
        {
            await Reply(service.Help());
        }

        [Command("test async", RunMode = RunMode.Async)]
        [Usage("{timeout in seconds defaults to 60 secs}")]
        [Summary("Test long running async methods")]
        [Development]
        public async Task TestAsync(int timeout = 60)
        {
            logger.LogDebug("Testing async with timeout {timeout}", timeout);
            await Reply(async () =>
            {
                await Task.Delay(timeout * 1000);
                return "Async test finished";
            });
        }

        private async Task Reply(string message, [CallerMemberName] string methodName = "")
        {
            await Reply(() => Task.FromResult(message), methodName);
        }

        private async Task Reply(Func<Task<string>> getMessage, [CallerMemberName] string methodName = "")
        {
            try
            {
                logger.LogInformation("Method Invoked: {methodName}", methodName);

                await Context.Message.AddReactionAsync(hourglass);

                await ReplyAsync(await getMessage.Invoke());

                await Context.Message.RemoveReactionAsync(hourglass, Context.Client.CurrentUser,
                    RequestOptions.Default);

                logger.LogInformation("Method Finished: {methodName}", methodName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an unhandled exception");
            }
        }
    }
}