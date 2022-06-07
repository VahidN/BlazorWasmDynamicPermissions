namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Components;

public class DropdownMenuItem
{
    public DropdownMenuItem()
    {
    }

    public DropdownMenuItem(string? title, string? url, string? glyphIcon)
    {
        Title = title;
        Url = url;
        GlyphIcon = glyphIcon;
    }

    public string? Title { set; get; }
    public string? Url { set; get; }
    public string? GlyphIcon { set; get; }
}