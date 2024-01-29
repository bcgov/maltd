using BcGov.Jag.AccountManagement.Shared;
using MediatR;

namespace BcGov.Jag.AccountManagement.Server.Features.Users.Lookup;

public class Request : IRequest<DetailedUser>
{
    public Request(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Argument cannot be null or empty", nameof(username));
        }

        Username = username.Trim();
    }

    public string Username { get; }
}
