using Microsoft.AspNetCore.Components;

namespace WrappedToast;

/// <summary>
/// Renders frontmatter as a view-only table or an editable table with add/delete/restore actions.
/// Owns its own edit state internally. Call <see cref="GetEditedRows"/> to pull the result on save.
/// </summary>
public partial class FrontMatterPanel
{
    /// <summary>The parsed frontmatter rows to display.</summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<FrontMatterRow> FrontMatterRows { get; set; } = [];

    /// <summary>Whether the panel is in edit mode.</summary>
    [Parameter] public bool IsEditing { get; set; }

    // ── Internal edit state ────────────────────────────────────────────

    private List<EditableRow> _editRows = [];
    private HashSet<int> _deletedIndices = [];

    /// <summary>
    /// Called when <see cref="IsEditing"/> transitions to <c>true</c>.
    /// Clones <see cref="FrontMatterRows"/> into the internal edit buffer.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (IsEditing && _editRows.Count == 0 && FrontMatterRows.Count > 0)
        {
            EnterEditMode();
        }

        if (!IsEditing && _editRows.Count > 0)
        {
            ExitEditMode();
        }
    }

    /// <summary>
    /// Returns the current edited rows as immutable <see cref="FrontMatterRow"/> records,
    /// with deleted rows excluded. Only valid while <see cref="IsEditing"/> is <c>true</c>.
    /// </summary>
    public IReadOnlyList<FrontMatterRow> GetEditedRows()
    {
        return _editRows
            .Where((_, i) => !_deletedIndices.Contains(i))
            .Select(r => new FrontMatterRow(r.Key, r.Value, r.Level, r.IsSection))
            .ToList();
    }

    private void EnterEditMode()
    {
        _editRows = FrontMatterRows
            .Select(r => new EditableRow(r.Key, r.Value, r.Level, r.IsSection))
            .ToList();
        _deletedIndices = [];
    }

    private void ExitEditMode()
    {
        _editRows = [];
        _deletedIndices = [];
    }

    // ── Internal mutation handlers ─────────────────────────────────────

    private void AddRow()
    {
        _editRows.Add(new EditableRow("", "", 0, false));
    }

    private void DeleteRow(int index)
    {
        if (index >= 0 && index < _editRows.Count)
        {
            _deletedIndices.Add(index);
        }
    }

    private void RestoreRow(int index)
    {
        _deletedIndices.Remove(index);
    }

    private void OnKeyChanged(int index, string key)
    {
        if (index >= 0 && index < _editRows.Count)
        {
            _editRows[index].Key = key;
        }
    }

    private void OnValueChanged(int index, string value)
    {
        if (index >= 0 && index < _editRows.Count)
        {
            _editRows[index].Value = value;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static string GetFrontMatterKeyClass(int level)
        => $"frontmatter-key frontmatter-key--level-{Math.Clamp(level, 0, 4)}";

    /// <summary>
    /// Mutable row used internally during editing. Not exposed publicly.
    /// </summary>
    private sealed class EditableRow
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public int Level { get; set; }
        public bool IsSection { get; set; }

        public EditableRow(string key, string value, int level, bool isSection)
        {
            Key = key;
            Value = value;
            Level = level;
            IsSection = isSection;
        }
    }
}
