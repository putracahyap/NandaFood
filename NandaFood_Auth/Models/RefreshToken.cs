using System;
using System.Collections.Generic;

namespace NandaFood_Auth.Models;

public partial class RefreshToken
{
    public string Id { get; set; } = null!;

    public string? Token { get; set; }

    public string? JwtId { get; set; }

    public bool? IsRevoked { get; set; }

    public string? AccountsId { get; set; }

    public DateTime? DateAdded { get; set; }

    public DateTime? DateExpire { get; set; }

    public virtual Account? Accounts { get; set; }
}
