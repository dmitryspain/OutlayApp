using OutlayApp.Application.Abstractions.Messaging;

namespace OutlayApp.Application.Backfill;

/// <summary>Imports one statement request's worth of history, ending at <paramref name="To"/> (unix seconds).</summary>
/// <param name="Floor">do not go further back than this (unix seconds)</param>
public sealed record ImportStatementWindowCommand(Guid CardId, long To, long Floor) : ICommand<ImportWindowResult>;

/// <param name="NextTo">where the next window ends</param>
public sealed record ImportWindowResult(int Added, long NextTo, bool Finished);
