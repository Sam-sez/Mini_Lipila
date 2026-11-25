namespace MiniLipila.Services;

public enum KycStatus
{
    Found,
    NotFound,
    Unavailable
}

public sealed record KycResult(KycStatus Status, string Name = "");