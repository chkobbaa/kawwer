using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Kawwer.Mobile.Views;

/// <summary>
/// A modal circular photo editor. The image can be zoomed (pinch) and repositioned (drag) inside a
/// circular window; confirming renders a circular PNG crop that is returned to the caller via
/// <see cref="Result"/>.
/// </summary>
public partial class CropPhotoPage : ContentPage
{
    // Keep the on-screen viewport and the output geometry in the same coordinate system so the
    // rendered crop matches exactly what the user positioned.
    private const double CropSize = 300;
    private const int OutputSize = 512;

    private readonly TaskCompletionSource<byte[]?> _result = new();
    private readonly byte[] _sourceBytes;
    private Microsoft.Maui.Graphics.IImage? _source;

    private double _scale = 1;
    private double _startScale = 1;
    private double _translationX;
    private double _translationY;
    private double _panStartX;
    private double _panStartY;
    private bool _completed;

    public CropPhotoPage(byte[] sourceBytes)
    {
        InitializeComponent();
        _sourceBytes = sourceBytes;
        MaskCanvas.Drawable = new CropMaskDrawable();
        LoadSource();
    }

    /// <summary>Completes with the edited PNG bytes, or null if the user cancelled.</summary>
    public Task<byte[]?> Result => _result.Task;

    private void LoadSource()
    {
        try
        {
            using var stream = new MemoryStream(_sourceBytes);
            var image = PlatformImage.FromStream(stream);
            // Cap the working resolution so preview + render stay light on memory.
            _source = image.Width > 1600 || image.Height > 1600
                ? image.Downsize(1600, disposeOriginal: true)
                : image;
        }
        catch
        {
            _source = null;
        }

        PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(_sourceBytes));
    }

    private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        if (e.Status == GestureStatus.Started)
        {
            _startScale = _scale;
            PreviewImage.AnchorX = 0.5;
            PreviewImage.AnchorY = 0.5;
        }
        else if (e.Status == GestureStatus.Running)
        {
            // e.Scale is the incremental pinch ratio per event; accumulate from the gesture start.
            _scale += (e.Scale - 1) * _startScale;
            _scale = Math.Clamp(_scale, 1.0, 5.0);
            PreviewImage.Scale = _scale;
        }
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartX = _translationX;
                _panStartY = _translationY;
                break;
            case GestureStatus.Running:
                _translationX = _panStartX + e.TotalX;
                _translationY = _panStartY + e.TotalY;
                PreviewImage.TranslationX = _translationX;
                PreviewImage.TranslationY = _translationY;
                break;
        }
    }

    private async void OnUseClicked(object? sender, EventArgs e)
    {
        byte[]? cropped;
        try
        {
            cropped = RenderCrop();
        }
        catch
        {
            cropped = _sourceBytes; // Never lose the user's photo if rendering fails.
        }

        Complete(cropped);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        Complete(null);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        Complete(null);
        return base.OnBackButtonPressed();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Complete(null); // Safety net so the awaiting caller never hangs.
    }

    private void Complete(byte[]? value)
    {
        if (!_completed)
        {
            _completed = true;
            _result.TrySetResult(value);
        }
    }

    private byte[]? RenderCrop()
    {
        if (_source is null)
        {
            return _sourceBytes;
        }

        // Base fit: the image is laid out AspectFit inside the square viewport, then scaled and
        // translated by the user. Reproduce that transform onto the output bitmap.
        var baseFit = Math.Min(CropSize / _source.Width, CropSize / _source.Height);
        var baseW = _source.Width * baseFit;
        var baseH = _source.Height * baseFit;
        var displayW = baseW * _scale;
        var displayH = baseH * _scale;
        var left = (CropSize / 2) + _translationX - (displayW / 2);
        var top = (CropSize / 2) + _translationY - (displayH / 2);
        var k = (double)OutputSize / CropSize;

        // Create an off-screen bitmap the size of the exported avatar. (MAUI 10 replaced the old
        // GraphicsPlatform.CurrentService.CreateBitmapExportContext helper with the platform bitmap
        // export service; a display scale of 1 keeps the drawing coordinates 1:1 with pixels.)
        using var context = new PlatformBitmapExportService().CreateContext(OutputSize, OutputSize, 1f);
        var canvas = context.Canvas;

        // Clip to a circle so the exported PNG is genuinely circular (transparent corners).
        var circle = new PathF();
        circle.AppendCircle(OutputSize / 2f, OutputSize / 2f, OutputSize / 2f);
        canvas.ClipPath(circle);

        canvas.DrawImage(_source, (float)(left * k), (float)(top * k), (float)(displayW * k), (float)(displayH * k));

        using var memory = new MemoryStream();
        context.WriteToStream(memory);
        var bytes = memory.ToArray();

        // Guard against a degenerate render (e.g. a platform hiccup produced an empty/near-empty
        // bitmap): fall back to the untouched source so the user never loses their photo.
        return bytes.Length > 256 ? bytes : _sourceBytes;
    }
}

/// <summary>Dims everything outside the circular crop window and outlines it with the Volt ring.</summary>
public sealed class CropMaskDrawable : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var cx = dirtyRect.Width / 2f;
        var cy = dirtyRect.Height / 2f;
        var radius = Math.Min(cx, cy);

        // Rectangle + circle filled even-odd => the circle becomes a clear "hole" in the dark scrim.
        var mask = new PathF();
        mask.AppendRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);
        mask.AppendCircle(cx, cy, radius);
        canvas.FillColor = Color.FromRgba(0f, 0f, 0f, 0.5f);
        canvas.FillPath(mask, WindingMode.EvenOdd);

        canvas.StrokeColor = Color.FromArgb("#CDF564"); // Volt
        canvas.StrokeSize = 3;
        canvas.DrawCircle(cx, cy, radius);
    }
}
