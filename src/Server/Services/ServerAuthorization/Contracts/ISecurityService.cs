namespace BlazorWasmDynamicPermissions.Server.Services.ServerAuthorization.Contracts;

public interface ISecurityService
{
    string GetSha256Hash(string input);
    Guid CreateCryptographicallySecureGuid();
}