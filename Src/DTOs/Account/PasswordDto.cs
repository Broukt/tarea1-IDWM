using System.ComponentModel.DataAnnotations;

namespace courses_dotnet_api.Src.DTOs.Account;

public class PasswordDto
{
    public string PasswordKey { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}
