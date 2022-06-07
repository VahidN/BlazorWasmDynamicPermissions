using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorWasmDynamicPermissions.Client.ClientAuthorization.Components;

/// <summary>
///     A custom NavBar Dropdown Menu component
/// </summary>
public partial class NavBarDropdownMenu : IDisposable
{
    private const string DisplayNone = "display:none;";
    private const string DisplayBlock = "display:block;";
    private bool _isDisposed;
    private string? MenuStyle { set; get; }
    private bool IsInside { set; get; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    ///     Additional user attributes
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; } =
        new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    ///     The InputSelect items list
    /// </summary>
    [Parameter]
    public IReadOnlyList<DropdownMenuItem> Items { get; set; } = new List<DropdownMenuItem>();

    /// <summary>
    ///     The main title of the menu.
    /// </summary>
    [Parameter]
    public string? MainTitle { set; get; }

    private string UniqueId { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    ///     Unsubscribe from the event when our component is disposed
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void ToggleShowMenu()
    {
        MenuStyle = MenuStyle is null || string.Equals(MenuStyle, DisplayNone, StringComparison.OrdinalIgnoreCase)
            ? DisplayBlock
            : DisplayNone;
    }

    private void MouseIn()
    {
        IsInside = true;
    }

    private void MouseOut()
    {
        IsInside = false;
    }

    private void FocusOut()
    {
        if (!IsInside)
        {
            HideMenu();
        }
    }

    private void HideMenu()
    {
        MenuStyle = DisplayNone;
    }

    /// <summary>
    ///     Unsubscribe from the event when our component is disposed
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            try
            {
                if (disposing)
                {
                    NavigationManager.LocationChanged -= LocationChanged;
                }
            }
            finally
            {
                _isDisposed = true;
            }
        }
    }

    private void LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        HideMenu();
        StateHasChanged();
    }

    /// <summary>
    ///     Method invoked when the component is ready to start
    /// </summary>
    protected override void OnInitialized()
    {
        // Subscribe to the event
        NavigationManager.LocationChanged += LocationChanged;
        base.OnInitialized();
    }
}