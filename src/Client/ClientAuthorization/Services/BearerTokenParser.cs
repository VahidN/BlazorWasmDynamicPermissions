using System.Security.Claims;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Services;

/// <summary>
///     A JWT parser
/// </summary>
public static class BearerTokenParser
{
    /// <summary>
    ///     Extracts the user claims of a JWT
    /// </summary>
    public static BearerTokenInfo ParseClaimsFromJwt(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            throw new ArgumentNullException(nameof(jwt));
        }

        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        var jsonBytes = Base64UrlDecode(payload);

        var keyValues = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        if (keyValues == null)
        {
            throw new InvalidOperationException($"`{jwt}` is not an expected JSON data.");
        }

        foreach (var keyValue in keyValues)
        {
            if (keyValue.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
                foreach (var itemValue in element.EnumerateArray())
                {
                    var value = itemValue.ToString();
                    if (value is null)
                    {
                        continue;
                    }

                    claims.Add(new Claim(keyValue.Key, value));
                }
            }
            else
            {
                var value = keyValue.Value.ToString();
                if (value is null)
                {
                    continue;
                }

                claims.Add(new Claim(keyValue.Key, value));
            }
        }

        var roles = GetRoles(claims);
        var expirationDateUtc = GetDateUtc(claims, "exp");
        var isExpired = GetIsExpired(expirationDateUtc);
        return new BearerTokenInfo
        {
            Claims = claims,
            Roles = roles,
            ExpirationDateUtc = expirationDateUtc,
            IsExpired = isExpired,
            Token = jwt
        };
    }

    private static List<string> GetRoles(IList<Claim> claims)
    {
        return claims.Where(claim => string.Equals(claim.Type, ClaimTypes.Role, StringComparison.Ordinal))
            .Select(claim => claim.Value).ToList();
    }

    private static byte[] Base64UrlDecode(string base64)
    {
        // From:
        // https://github.com/dvsekhvalnov/jose-jwt/blob/master/jose-jwt/util/Base64Url.cs#L16
        // https://github.com/auth0/jwt-decode/blob/master/lib/base64_url_decode.js#L15
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/0665af62cc58a28ebe184dd97f4d018f84e1d83d/src/Microsoft.IdentityModel.Tokens/Base64UrlEncoder.cs#L175

        base64 = base64.Replace('-', '+'); // 62nd char of encoding
        base64 = base64.Replace('_', '/'); // 63rd char of encoding
        switch (base64.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2:
                base64 += "==";
                break; // Two pad chars
            case 3:
                base64 += "=";
                break; // One pad char
            default: throw new ArgumentOutOfRangeException(nameof(base64), "Illegal base64url string!");
        }

        return Convert.FromBase64String(base64); // Standard base64 decoder
    }

    private static bool GetIsExpired(DateTime? expirationDateUtc)
    {
        return !expirationDateUtc.HasValue || expirationDateUtc.Value <= DateTime.UtcNow;
    }

    private static DateTime? GetDateUtc(IList<Claim> claims, string type)
    {
        var exp = claims.SingleOrDefault(claim => string.Equals(claim.Type, type, StringComparison.Ordinal));
        if (exp == null)
        {
            return null;
        }

        var expValue = GetTimeValue(exp.Value);
        if (expValue == null)
        {
            return null;
        }

        var dateTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return dateTimeEpoch.AddSeconds(expValue.Value);
    }

    private static long? GetTimeValue(string claimValue)
    {
        if (long.TryParse(claimValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultLong))
        {
            return resultLong;
        }

        if (float.TryParse(claimValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultFloat))
        {
            return (long)resultFloat;
        }

        if (double.TryParse(claimValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var resultDouble))
        {
            return (long)resultDouble;
        }

        return null;
    }
}