using System.ComponentModel.DataAnnotations;
public class UserRequest
{
    [Required(ErrorMessage = "User name is required")]
    [MinLength(3, ErrorMessage = "User name must be at least 3 characters.")]
    [MaxLength(20, ErrorMessage = "User name cannot exceed 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "User name can only contain letters, numbers, and underscores.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "E-mail is not valid")]
    [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]

    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [Length(10, 100, ErrorMessage = "Password must be between {1} characters and {2} characters")]
    public string Password { get; set; }
}