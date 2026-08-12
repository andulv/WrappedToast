namespace WrappedToast;

/// <summary>Why the editor requested persistence of its current buffer.</summary>
public enum WrappedToastSaveOrigin
{
    Manual,
    Autosave,
    Flush,
}

/// <summary>
/// Immutable snapshot handed to the host for persistence. A successful callback acknowledges the
/// exact revision; throwing rejects it and leaves the editor buffer dirty.
/// </summary>
public sealed record WrappedToastSaveRequest(
    string Content,
    long Revision,
    WrappedToastSaveOrigin Origin);
