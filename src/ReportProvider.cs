using Grpc.Core;
using CredChecker;
using LegalEntityChecker;
using ReportProvider;
using static CredChecker.CredChecker;
using static LegalEntityChecker.LegalEntityChecker;
using static ReportProvider.ReportProvider;

namespace HuginAndMunin;

public class ReportProvider(
    LegalEntityCheckerClient _legalEntityInfoClient,
    CredCheckerClient _credCheckerClient) : ReportProviderBase
{
    public override async Task<ReportReponse> Get(ReportRequest request, ServerCallContext context)
    {
        var legalEntityRequest = new LegalEntityInfoRequest() { Tin = request.Tin };
        var legalEntityInfoTask = CallLegalEntityCheckerAsync(_legalEntityInfoClient, legalEntityRequest, context.CancellationToken);

        var creditsStateRequest = new GetDigitalMinistryCreditsStateRequest() { Inn = $"{request.Tin}" };
        var creditsStateInfoTask = CallCredCheckerAsync(_credCheckerClient, creditsStateRequest, context.CancellationToken);

        // Wait for both tasks to complete
        await Task.WhenAll(legalEntityInfoTask, creditsStateInfoTask);

        var legalEntityInfo = legalEntityInfoTask.Result;
        var creditsState = creditsStateInfoTask.Result;

        return new ReportReponse()
        {
            Name = legalEntityInfo.Name,
            Tin = legalEntityInfo.Tin,
            IncorporationDate = legalEntityInfo.IncorporationDate,
            AuthorizedCapital = legalEntityInfo.AuthorizedCapital,
            Address = legalEntityInfo.Address,
            LegalEntityStatus = legalEntityInfo.LegalEntityStatus,
            AccreditationState = creditsState.State,
            SalaryDelays = legalEntityInfo.SalaryDelays
        };
    }

    private static async Task<GetDigitalMinistryCreditsStateResponse> CallCredCheckerAsync(
        CredCheckerClient _credCheckerClient,
        GetDigitalMinistryCreditsStateRequest creditsStateRequest,
        CancellationToken ct) =>
        await _credCheckerClient.GetDigitalMinistryCreditsStateAsync(creditsStateRequest, cancellationToken: ct);

    private static async Task<LegalEntityInfoReponse> CallLegalEntityCheckerAsync(
        LegalEntityCheckerClient _legalEntityInfoClient,
        LegalEntityInfoRequest legalEntityRequest,
        CancellationToken ct) =>
        await _legalEntityInfoClient.GetAsync(legalEntityRequest, cancellationToken: ct);
}
