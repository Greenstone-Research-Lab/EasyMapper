using EasyMapper;

User user = new()
{
    Id = 17,
    FirstName = "Ada",
    LastName = "Lovelace",
    Email = "ada@example.com",
    PasswordHash = "must-not-leave-the-domain",
};

UserDto conventional = user.MapTo<UserDto>();
UserDto controlled = EasyMapper.EasyMapper.Map<User, UserDto>(user, map => map
    .Bind(destination => destination.DisplayName)
    .From(source => source.FirstName + " " + source.LastName)
    .Except(destination => destination.PasswordHash));

Console.WriteLine($"Convention: {conventional.Id} / {conventional.Email}");
Console.WriteLine($"Controlled: {controlled.DisplayName} / password exposed: {controlled.PasswordHash is not null}");

internal sealed class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}

internal sealed class UserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? PasswordHash { get; set; }
}
