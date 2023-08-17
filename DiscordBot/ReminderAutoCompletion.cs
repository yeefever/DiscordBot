using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    public class ReminderAutoCompletion : IAutocompleteProvider
    {
        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            DiscordAutoCompleteChoice[] choices = new DiscordAutoCompleteChoice[2]
            {
                new DiscordAutoCompleteChoice("Temp option", "temp descriptipion"),
                new DiscordAutoCompleteChoice("Temp option1", "temp descriptipion1")
            };
            return choices;
        }
    }
}