using System.Text.RegularExpressions;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Domain.CompanyLogoReferences;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.LogoReferences;

public class FetchMostFrequencyIconsCommandHandler : ICommandHandler<FetchMostFrequencyIconsCommand>
{
    private readonly ICompanyLogoFinder _logoFinder;
    private readonly ILogoReferenceRepository _logoReferenceRepository;
    private readonly IInvalidReferenceRepository _invalidReferenceRepository;

    public FetchMostFrequencyIconsCommandHandler(ICompanyLogoFinder logoFinder,
        ILogoReferenceRepository logoReferenceRepository,
        IInvalidReferenceRepository invalidReferenceRepository)
    {
        _logoFinder = logoFinder;
        _logoReferenceRepository = logoReferenceRepository;
        _invalidReferenceRepository = invalidReferenceRepository;
    }

    public async Task<Result> Handle(FetchMostFrequencyIconsCommand request, CancellationToken cancellationToken)
    {
        foreach (var transaction in request.FrequencyTransactions)
        {
            if (await _logoReferenceRepository.ContainsAsync(transaction, cancellationToken))
                continue;

            if (await _invalidReferenceRepository.ContainsAsync(transaction, cancellationToken))
                continue;

            const string pattern = @"^\d{6}\*\*\*\*\d{4}$"; // Adjust this pattern according to your needs
            if (Regex.IsMatch(transaction, pattern))
                continue;
            var logo = await _logoFinder.GetCompanyLogo(transaction, cancellationToken);
            if (logo is null)
                break; // the search itself is unavailable now; try these again next time
            if (logo.Length > 0)
            {
                var reference = LogoReference.Create(transaction, logo, DateTime.UtcNow);
                await _logoReferenceRepository.AddAsync(reference, cancellationToken);
            }
            else
            {
                var invalidReference = InvalidReference.Create(transaction, DateTime.UtcNow);
                await _invalidReferenceRepository.AddAsync(invalidReference, cancellationToken);
            }
        }

        return Result.Success();
    }
}