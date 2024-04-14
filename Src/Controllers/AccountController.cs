using System.Security.Cryptography;
using System.Text;
using courses_dotnet_api.Src.DTOs.Account;
using courses_dotnet_api.Src.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace courses_dotnet_api.Src.Controllers;

public class AccountController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;

    public AccountController(IUserRepository userRepository, IAccountRepository accountRepository)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<IResult> Register(RegisterDto registerDto)
    {
        if (
            await _userRepository.UserExistsByEmailAsync(registerDto.Email)
            || await _userRepository.UserExistsByRutAsync(registerDto.Rut)
        )
        {
            return TypedResults.BadRequest("User already exists");
        }

        await _accountRepository.AddAccountAsync(registerDto);

        if (!await _accountRepository.SaveChangesAsync())
        {
            return TypedResults.BadRequest("Failed to save user");
        }

        AccountDto? accountDto = await _accountRepository.GetAccountAsync(registerDto.Email);

        return TypedResults.Ok(accountDto);
    }

    [HttpPost("login")]
    public async Task<IResult> Login(LoginDto loginDto)
    {
        AccountDto? accountDto = await _accountRepository.GetAccountAsync(loginDto.Email);

        if (accountDto == null)
        {
            return TypedResults.BadRequest("Credentials are invalid");
        }

        PasswordDto? passwordDto = await _accountRepository.GetPasswordAsync(loginDto.Email);

        if (passwordDto == null)
        {
            return TypedResults.BadRequest("Internal server error, please try again later");
        }

        using var hmac = new HMACSHA512(passwordDto.PasswordSalt);
        byte[] computedHashPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        if (!computedHashPassword.SequenceEqual(passwordDto.PasswordHash))
        {
            return TypedResults.BadRequest("Credentials are invalid");
        }

        return TypedResults.Ok(accountDto);
    }
}
