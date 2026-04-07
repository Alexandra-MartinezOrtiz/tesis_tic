namespace Reportes.Application.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "TicFis";
    public string Audience { get; set; } = "TicFis";
    public string SigningKey { get; set; } = "";
}
