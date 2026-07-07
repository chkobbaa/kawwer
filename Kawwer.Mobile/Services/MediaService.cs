using Kawwer.Mobile.Views;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Raised when the user is unable to complete a camera/gallery action (permission denied or the
/// feature is unsupported). Carries a friendly, display-ready message for the caller to surface.
/// </summary>
public sealed class MediaAccessException : Exception
{
    public MediaAccessException(string message) : base(message)
    {
    }
}

/// <summary>
/// Wraps the platform camera/gallery pickers with graceful permission handling and hands the
/// chosen image to the circular crop editor before it is uploaded.
/// </summary>
public sealed class MediaService
{
    /// <summary>Takes a photo with the camera. Returns null if the user cancels.</summary>
    public async Task<FileResult?> CapturePhotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            throw new MediaAccessException("This device doesn't have a camera available.");
        }

        var status = await EnsureCameraPermissionAsync();
        if (status != PermissionStatus.Granted)
        {
            throw new MediaAccessException("Camera access is off. Enable it in Settings to take a photo.");
        }

        try
        {
            return await MediaPicker.Default.CapturePhotoAsync();
        }
        catch (FeatureNotSupportedException)
        {
            throw new MediaAccessException("This device doesn't have a camera available.");
        }
        catch (PermissionException)
        {
            throw new MediaAccessException("Camera access is off. Enable it in Settings to take a photo.");
        }
    }

    /// <summary>Picks an existing photo from the gallery. Returns null if the user cancels.</summary>
    public async Task<FileResult?> PickPhotoAsync()
    {
        try
        {
            return await MediaPicker.Default.PickPhotoAsync();
        }
        catch (FeatureNotSupportedException)
        {
            throw new MediaAccessException("Picking photos isn't supported on this device.");
        }
        catch (PermissionException)
        {
            throw new MediaAccessException("Photo access is off. Enable it in Settings to choose a photo.");
        }
    }

    /// <summary>
    /// Opens the circular crop/zoom editor for the given image and returns the edited PNG bytes,
    /// or null if the user backed out.
    /// </summary>
    public async Task<byte[]?> EditPhotoAsync(byte[] source)
    {
        var navigation = Shell.Current?.Navigation
                         ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;
        if (navigation is null)
        {
            return source; // No host to present the editor: fall back to the untouched image.
        }

        var page = new CropPhotoPage(source);
        await navigation.PushModalAsync(page);
        return await page.Result;
    }

    private static async Task<PermissionStatus> EnsureCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        return status;
    }
}
