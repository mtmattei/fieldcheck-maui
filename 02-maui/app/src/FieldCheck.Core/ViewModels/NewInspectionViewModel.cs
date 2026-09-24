using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldCheck.Core.Models;
using FieldCheck.Core.Services;

namespace FieldCheck.Core.ViewModels;

public sealed partial class NewInspectionViewModel(
    IFieldCheckRepository repository,
    INavigationService navigation,
    IFilePickerService filePicker) : StatefulViewModel
{
    public const double MinTemperature = -50;
    public const double MaxTemperature = 250;

    public const string GuardsItem = "Guards and covers secure";
    public const string LeaksItem = "No visible leaks or damage";
    public const string AreaItem = "Area clear and accessible";

    private bool _temperatureTouched;
    private bool _issueTouched;
    private bool _checklistTouched;
    private int _submitGate;

    [ObservableProperty]
    public partial Asset? Asset { get; private set; }

    public string AssetLine => Asset is null ? string.Empty : $"{Asset.Name} · {Asset.Id}";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGood), nameof(IsAttention), nameof(IsCritical))]
    public partial InspectionCondition? Condition { get; set; }

    public bool IsGood => Condition == InspectionCondition.Good;
    public bool IsAttention => Condition == InspectionCondition.Attention;
    public bool IsCritical => Condition == InspectionCondition.Critical;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OperatingNormallyText))]
    public partial bool OperatingNormally { get; set; } = true;

    public string OperatingNormallyText => OperatingNormally ? "Yes" : "No";

    [ObservableProperty]
    public partial string TemperatureText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool GuardsSecure { get; set; }

    [ObservableProperty]
    public partial bool NoVisibleLeaks { get; set; }

    [ObservableProperty]
    public partial bool AreaClear { get; set; }

    [ObservableProperty]
    public partial string Notes { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string IssueDescription { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAttachment), nameof(AttachmentName))]
    public partial PickedFile? Attachment { get; private set; }

    public bool HasAttachment => Attachment is not null;
    public string AttachmentName => Attachment?.FileName ?? string.Empty;

    [ObservableProperty]
    public partial string? AttachmentError { get; private set; }

    [ObservableProperty]
    public partial bool IsPicking { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand), nameof(CancelCommand))]
    [NotifyPropertyChangedFor(nameof(SubmitText))]
    public partial bool IsSubmitting { get; private set; }

    public string SubmitText => IsSubmitting ? "Saving…" : "Submit inspection";

    [ObservableProperty]
    public partial string? SubmitError { get; private set; }

    /// <summary>Issue description is required (and shown) for Attention/Critical or when not operating normally.</summary>
    public bool IsIssueDescriptionRequired =>
        Condition is InspectionCondition.Attention or InspectionCondition.Critical || !OperatingNormally;

    public string? TemperatureError
    {
        get
        {
            var error = ValidateTemperature(TemperatureText, out _);
            return _temperatureTouched || TemperatureText.Length > 0 ? error : null;
        }
    }

    public string? IssueDescriptionError =>
        _issueTouched && IsIssueDescriptionRequired && string.IsNullOrWhiteSpace(IssueDescription)
            ? "Describe the issue before submitting."
            : null;

    public string? ChecklistError =>
        _checklistTouched && !IsChecklistComplete ? "All three checklist items must be confirmed." : null;

    public bool IsChecklistComplete => GuardsSecure && NoVisibleLeaks && AreaClear;

    /// <summary>Plain-language list of what still blocks submission.</summary>
    public IReadOnlyList<string> MissingRequirements
    {
        get
        {
            var missing = new List<string>();
            if (Condition is null)
            {
                missing.Add("Select a condition");
            }

            if (ValidateTemperature(TemperatureText, out _) is not null)
            {
                missing.Add("Enter a temperature from -50 to 250 °C");
            }

            var unchecked_ = new[] { GuardsSecure, NoVisibleLeaks, AreaClear }.Count(c => !c);
            if (unchecked_ > 0)
            {
                missing.Add(unchecked_ == 1 ? "Confirm 1 checklist item" : $"Confirm {unchecked_} checklist items");
            }

            if (IsIssueDescriptionRequired && string.IsNullOrWhiteSpace(IssueDescription))
            {
                missing.Add("Describe the issue");
            }

            return missing;
        }
    }

    public bool IsValid => Asset is not null && MissingRequirements.Count == 0;

    public string RequirementsSummary => IsValid ? string.Empty : "To submit: " + string.Join(" · ", MissingRequirements) + ".";

    public async Task LoadAsync(string assetId)
    {
        State = ViewState.Loading;
        try
        {
            Asset = await repository.GetAssetAsync(assetId);
            State = Asset is null ? ViewState.Empty : ViewState.Content;
        }
        catch (DataLoadException)
        {
            State = ViewState.Error;
        }

        OnPropertyChanged(nameof(AssetLine));
        RefreshValidation();
    }

    [RelayCommand]
    private void SelectCondition(string condition)
    {
        if (Enum.TryParse<InspectionCondition>(condition, out var parsed))
        {
            Condition = parsed;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        // Guard against re-entry from rapid repeated activation before CanExecute propagates to the view.
        if (Interlocked.Exchange(ref _submitGate, 1) == 1)
        {
            return;
        }

        try
        {
            if (!IsValid || Asset is null || ValidateTemperature(TemperatureText, out var temperature) is not null)
            {
                _temperatureTouched = _issueTouched = _checklistTouched = true;
                RefreshValidation();
                return;
            }

            IsSubmitting = true;
            SubmitError = null;
            var saved = await repository.AddInspectionAsync(new InspectionDraft
            {
                AssetId = Asset.Id,
                Condition = Condition!.Value,
                OperatingNormally = OperatingNormally,
                TemperatureC = temperature,
                Notes = Notes,
                IssueDescription = IsIssueDescriptionRequired ? IssueDescription : null,
                AttachmentFileName = Attachment?.FileName,
                AttachmentPath = Attachment?.LocalPath,
            });
            await navigation.GoToInspectionSuccessAsync(saved.Id);
        }
        catch (DataPersistenceException)
        {
            SubmitError = "The inspection wasn't saved. Your entries are still here — try submitting again.";
        }
        catch (DataLoadException)
        {
            SubmitError = "The inspection wasn't saved because local data couldn't be read. Try again.";
        }
        finally
        {
            IsSubmitting = false;
            Interlocked.Exchange(ref _submitGate, 0);
        }
    }

    private bool CanSubmit() => IsValid && !IsSubmitting;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private Task CancelAsync() => navigation.GoBackAsync();

    private bool CanCancel() => !IsSubmitting;

    [RelayCommand]
    private async Task ChooseFileAsync()
    {
        if (IsPicking)
        {
            return;
        }

        IsPicking = true;
        AttachmentError = null;
        try
        {
            var picked = await filePicker.PickPhotoOrFileAsync();
            if (picked is not null)
            {
                Attachment = picked;
            }
        }
        catch (OperationCanceledException)
        {
            // A superseded or dismissed picker is a cancellation; keep the current attachment.
        }
        catch (Exception)
        {
            AttachmentError = "The file couldn't be attached. Try again or choose a different file.";
        }
        finally
        {
            IsPicking = false;
        }
    }

    [RelayCommand]
    private void RemoveAttachment()
    {
        Attachment = null;
        AttachmentError = null;
    }

    partial void OnConditionChanged(InspectionCondition? value) => RefreshValidation();

    partial void OnOperatingNormallyChanged(bool value) => RefreshValidation();

    partial void OnTemperatureTextChanged(string value)
    {
        _temperatureTouched = true;
        RefreshValidation();
    }

    partial void OnGuardsSecureChanged(bool value) => ChecklistChanged();

    partial void OnNoVisibleLeaksChanged(bool value) => ChecklistChanged();

    partial void OnAreaClearChanged(bool value) => ChecklistChanged();

    partial void OnIssueDescriptionChanged(string value)
    {
        _issueTouched = true;
        RefreshValidation();
    }

    private void ChecklistChanged()
    {
        _checklistTouched = true;
        RefreshValidation();
    }

    private void RefreshValidation()
    {
        OnPropertyChanged(nameof(IsIssueDescriptionRequired));
        OnPropertyChanged(nameof(TemperatureError));
        OnPropertyChanged(nameof(IssueDescriptionError));
        OnPropertyChanged(nameof(ChecklistError));
        OnPropertyChanged(nameof(IsChecklistComplete));
        OnPropertyChanged(nameof(MissingRequirements));
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(RequirementsSummary));
        SubmitCommand.NotifyCanExecuteChanged();
    }

    public static string? ValidateTemperature(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return "Temperature is required.";
        }

        var normalized = text.Trim().Replace(',', '.').Replace('−', '-');
        if (!double.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out value) || !double.IsFinite(value))
        {
            return "Temperature must be a number, for example 27 or -4.5.";
        }

        return value is < MinTemperature or > MaxTemperature
            ? "Temperature must be between -50 and 250 °C."
            : null;
    }
}
