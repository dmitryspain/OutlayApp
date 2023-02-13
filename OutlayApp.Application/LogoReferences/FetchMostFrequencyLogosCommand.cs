using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.LogoReferences;

public sealed record FetchMostFrequencyIconsCommand(IEnumerable<string> FrequencyTransactions) : ICommand;