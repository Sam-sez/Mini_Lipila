using FastEndpoints;
using MiniLipila.Data;
using MiniLipila.Models;

namespace MiniLipila.Modules.Endpoints;

public class CreateMerchantRequest
{
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateMerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<WalletResponse> Wallets { get; set; } = new();
}

public class CreateMerchantEndpoint(AppDbContext db) : Endpoint<CreateMerchantRequest, CreateMerchantResponse>
{
    public override void Configure()
    {
        Post("/merchants");
        Summary(s =>
        {
            s.Description = "`this creates a new merchant with one empty wallet`";
            s.Summary = "Creates a new merchant";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateMerchantRequest req, CancellationToken ct)
    {
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            AccountNumber = req.AccountNumber,
            Email = req.Email,
            Wallets = [new Wallet { Id = Guid.NewGuid(), Balance = 0 }]
        };

        db.Merchants.Add(merchant);
        await db.SaveChangesAsync(ct);

        var response = new CreateMerchantResponse
        {
            Id = merchant.Id,
            Name = merchant.Name,
            AccountNumber = merchant.AccountNumber,
            Email = merchant.Email,
            Wallets = merchant.Wallets
                .Select(w => new WalletResponse { Id = w.Id, Balance = w.Balance })
                .ToList()
        };

        await Send.ResponseAsync(response, 201, ct);
    }
}