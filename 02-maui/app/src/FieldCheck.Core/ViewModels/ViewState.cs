using CommunityToolkit.Mvvm.ComponentModel;

namespace FieldCheck.Core.ViewModels;

public enum ViewState
{
    Loading,
    Content,
    /// <summary>The repository returned no records.</summary>
    Empty,
    /// <summary>Records exist but the current search/filter matches none.</summary>
    NoResults,
    Error,
}

public abstract partial class StatefulViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading), nameof(IsContent), nameof(IsEmpty), nameof(IsNoResults), nameof(IsError))]
    public partial ViewState State { get; protected set; } = ViewState.Loading;

    public bool IsLoading => State == ViewState.Loading;
    public bool IsContent => State == ViewState.Content;
    public bool IsEmpty => State == ViewState.Empty;
    public bool IsNoResults => State == ViewState.NoResults;
    public bool IsError => State == ViewState.Error;

    public const string LoadErrorMessage = "FieldCheck couldn't read local data. Your saved inspections are not affected.";
}
