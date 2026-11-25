using Flurl.Http;
using Microsoft.Extensions.Options;
using MiniLipila.Models.Lipila;

namespace MiniLipila.Services;

public class LipilaKycService(
    IConfiguration config,
    IOptions<LipilaSettings> lipila,
    ILogger<LipilaKycService> logger)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public async Task<KycResult> LookupAsync(string phone, CancellationToken ct)
    {
        var apiKey = config["Lipila:ApiKey"]
                     ?? throw new InvalidOperationException("Lipila:ApiKey is not set");

        try
        {
            var kyc = await $"{lipila.Value.BaseUrl}{lipila.Value.KycPath}/{phone}"
                .WithHeader("x-api-key", apiKey)
                .WithTimeout(Timeout)
                .GetJsonAsync<LipilaKycResponse>(cancellationToken: ct);

            var name = $"{kyc.FirstName} {kyc.LastName}".Trim();

            // Lipila gave no name: still a valid number, so save "Unknown"
            return new KycResult(KycStatus.Found, name.Length > 0 ? name : "Unknown");
        }
        catch (FlurlHttpException ex) when (ex.StatusCode is 400 or 404)
        {
            // Lipila answers 400 ("Failed to fetch KYC") for a number it does not know.
            // We log the body so we notice if 400 ever means something else.
            var body = await ex.GetResponseStringAsync();
            logger.LogInformation("KYC says not found (status {Status}): {Body}", ex.StatusCode, body);
            return new KycResult(KycStatus.NotFound);
        }
        catch (FlurlHttpException ex)
        {
            // timeout, network problem, 401, 5xx, unreadable reply: we do not know the answer
            logger.LogWarning(ex, "KYC lookup failed (status {Status})", ex.StatusCode);
            return new KycResult(KycStatus.Unavailable);
        }
    }
}