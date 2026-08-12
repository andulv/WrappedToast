namespace WrappedToast.Tests;

/// <summary>
/// Subclass that replaces the JS-backed editor buffer read with a controllable string,
/// so save/autosave behavior can be exercised without JavaScript. Mirrors how production
/// reads the live buffer via <see cref="WrappedToast.WrappedToast.ReadLiveBodyAsync"/>.
/// </summary>
internal sealed class TestableWrappedToast : WrappedToast
{
    /// <summary>Content returned by the overridden editor read.</summary>
    public string LiveBody { get; set; } = "";

    /// <summary>Simulates the editor's user-edit callback without JavaScript.</summary>
    public void SimulateUserEdit() => RecordEdit();

    protected override Task<string> ReadLiveBodyAsync() => Task.FromResult(LiveBody);
}
