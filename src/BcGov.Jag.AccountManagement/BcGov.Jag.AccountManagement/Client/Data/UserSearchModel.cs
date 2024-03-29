﻿using System.ComponentModel.DataAnnotations;

namespace BcGov.Jag.AccountManagement.Client.Data;

public class UserSearchModel
{
    [Required]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9\\_]+$", ErrorMessage = "Username must start with a letter and only contain letters, numbers and underscore")]
    public string Username { get; set; } = string.Empty;
}
