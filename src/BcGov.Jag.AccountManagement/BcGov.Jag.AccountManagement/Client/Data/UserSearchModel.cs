using System.ComponentModel.DataAnnotations;

namespace BcGov.Jag.AccountManagement.Client.Data;

public class UserSearchModel
{
    [Required]
    [RegularExpression(@"^[a-z][a-z0-9\\_]+$", ErrorMessage = "Username must start with a letter and only contain letters, numbers and underscore")]
    public string Username { get; set; }

    public UserSearchModel()
    {
        Username = string.Empty;
    }
}
