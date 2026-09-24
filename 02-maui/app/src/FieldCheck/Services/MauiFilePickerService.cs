using FieldCheck.Core.Services;

namespace FieldCheck.Services;

/// <summary>Uses the platform file picker and copies the selection into app storage so the attachment outlives the picker grant.</summary>
public sealed class MauiFilePickerService(IFilePicker filePicker) : IFilePickerService
{
    private static readonly FilePickerFileType PhotoOrDocument = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        [DevicePlatform.Android] = ["image/*", "application/pdf"],
        [DevicePlatform.WinUI] = [".png", ".jpg", ".jpeg", ".gif", ".bmp", ".heic", ".webp", ".pdf"],
    });

    public async Task<PickedFile?> PickPhotoOrFileAsync()
    {
        var result = await filePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Attach photo or file",
            FileTypes = PhotoOrDocument,
        });
        if (result is null)
        {
            return null;
        }

        var directory = Path.Combine(FileSystem.AppDataDirectory, "attachments");
        Directory.CreateDirectory(directory);
        var localPath = Path.Combine(directory, $"{Guid.NewGuid():N}{Path.GetExtension(result.FileName)}");

        await using (var source = await result.OpenReadAsync())
        await using (var target = File.Create(localPath))
        {
            await source.CopyToAsync(target);
        }

        return new PickedFile(result.FileName, localPath);
    }
}
