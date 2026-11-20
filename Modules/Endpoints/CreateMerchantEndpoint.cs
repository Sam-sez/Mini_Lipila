using FastEndpoints;
using MiniLipila.Data;
using MiniLipila.Models;

namespace MiniLipila.Modules.Endpoints;

public class CreateMerchantRequest
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<decimal> Wallets { get; set; } = new();
}

public class CreateMerchantResponse
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<WalletResponse> Wallets { get; set; } = new();
}

public class CreateMerchantEndpoint(AppDbContext db) : Endpoint<CreateMerchantRequest, CreateMerchantResponse>
{
    public override void Configure()
    {
        Post("/merchants");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateMerchantRequest req, CancellationToken ct)
    {
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            AccountNumber = req.AccountNumber,
            Email = req.Email,
            Wallets = req.Wallets
                .Select(balance => new Wallet { Id = Guid.NewGuid(), Balance = balance })
                .ToList()
        };

        db.Merchants.Add(merchant);
        await db.SaveChangesAsync(ct);

        var response = new CreateMerchantResponse
        {
            Id = merchant.Id,
            AccountNumber = merchant.AccountNumber,
            Email = merchant.Email,
            Wallets = merchant.Wallets
                .Select(w => new WalletResponse { Id = w.Id, Balance = w.Balance })
                .ToList()
        };

        await Send.ResponseAsync(response, 201, ct);
    }
}