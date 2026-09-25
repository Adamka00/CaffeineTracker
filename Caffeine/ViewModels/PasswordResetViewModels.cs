using System.ComponentModel.DataAnnotations;

namespace Caffeine.ViewModels;

public sealed class ForgotPasswordForm
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public bool Submitted { get; set; }
}

public sealed class ResetPasswordForm
{
    [Required, StringLength(100)]
    public string Token { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 10)]
    public string Password { get; set; } = "";

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = "";

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public bool InvalidToken { get; set; }

    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public bool Completed { get; set; }
}