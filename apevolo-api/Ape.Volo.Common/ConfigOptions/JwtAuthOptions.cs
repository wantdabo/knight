﻿using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Common.ConfigOptions;

[OptionsSettings]
public class JwtAuthOptions
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public string SecurityKey { get; set; }
    public int Expires { get; set; }
    public int RefreshTokenExpires { get; set; }
    public string LoginPath { get; set; }
}
