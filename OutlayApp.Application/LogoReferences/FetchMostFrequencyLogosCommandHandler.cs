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
    private readonly IUnitOfWork _unitOfWork;

    public FetchMostFrequencyIconsCommandHandler(ICompanyLogoFinder logoFinder,
        ILogoReferenceRepository logoReferenceRepository,
        IInvalidReferenceRepository invalidReferenceRepository, 
        IUnitOfWork unitOfWork)
    {
        _logoFinder = logoFinder;
        _logoReferenceRepository = logoReferenceRepository;
        _invalidReferenceRepository = invalidReferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FetchMostFrequencyIconsCommand request, CancellationToken cancellationToken)
    {
        foreach (var transaction in request.FrequencyTransactions)
        {
            if (await _logoReferenceRepository.ContainsAsync(transaction, cancellationToken))
                continue;

            if (await _invalidReferenceRepository.ContainsAsync(transaction, cancellationToken))
                continue;

            var logo = await _logoFinder.GetCompanyLogo(transaction, cancellationToken);
            if (!string.IsNullOrEmpty(logo))
            {
                var reference = LogoReference.Create(transaction, logo, DateTime.Now);
                await _logoReferenceRepository.AddAsync(reference, cancellationToken);
            }
            else
            {
                var invalidReference = InvalidReference.Create(transaction, DateTime.Now);
                await _invalidReferenceRepository.AddAsync(invalidReference, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}