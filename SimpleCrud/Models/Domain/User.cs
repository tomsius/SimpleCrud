using Microsoft.EntityFrameworkCore;

namespace SimpleCrud.Models.Domain;

[PrimaryKey(nameof(Username))]
public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
}
