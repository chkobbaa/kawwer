namespace Kawwer.Mobile.Views;

/// <summary>Which pitch to draw: one team's half stacked vertically, or the full pitch laid out horizontally.</summary>
public enum PitchMode
{
    HalfVertical,
    FullHorizontal
}

/// <summary>
/// Draws a football pitch as vectors (grass stripes + white markings) so the board looks crisp at any
/// size without shipping a bitmap. In <see cref="PitchMode.HalfVertical"/> the halfway line sits along
/// the top and the goal along the bottom (a coach looking at one half); in
/// <see cref="PitchMode.FullHorizontal"/> the whole pitch runs left-to-right with both goals.
/// </summary>
public sealed class PitchDrawable : IDrawable
{
    public PitchMode Mode { get; set; } = PitchMode.HalfVertical;

    private static readonly Color Grass = Color.FromArgb("#137A41");
    private static readonly Color GrassAlt = Color.FromArgb("#106A38");
    private static readonly Color Line = Color.FromArgb("#EAF7EE");

    public void Draw(ICanvas canvas, RectF rect)
    {
        DrawGrass(canvas, rect);

        canvas.StrokeColor = Line;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineCap = LineCap.Square;

        var margin = MathF.Min(rect.Width, rect.Height) * 0.05f + 6f;
        var field = new RectF(rect.X + margin, rect.Y + margin, rect.Width - 2 * margin, rect.Height - 2 * margin);
        canvas.DrawRectangle(field);

        if (Mode == PitchMode.HalfVertical)
        {
            DrawHalf(canvas, field);
        }
        else
        {
            DrawFull(canvas, field);
        }
    }

    private static void DrawGrass(ICanvas canvas, RectF rect)
    {
        const int stripes = 8;
        if (rect.Height >= rect.Width)
        {
            var band = rect.Height / stripes;
            for (var i = 0; i < stripes; i++)
            {
                canvas.FillColor = i % 2 == 0 ? Grass : GrassAlt;
                canvas.FillRectangle(rect.X, rect.Y + i * band, rect.Width, band + 1);
            }
        }
        else
        {
            var band = rect.Width / stripes;
            for (var i = 0; i < stripes; i++)
            {
                canvas.FillColor = i % 2 == 0 ? Grass : GrassAlt;
                canvas.FillRectangle(rect.X + i * band, rect.Y, band + 1, rect.Height);
            }
        }
    }

    private static void DrawHalf(ICanvas canvas, RectF f)
    {
        // Halfway line is the top edge; draw the center-circle arc bulging down into the half.
        var centerR = f.Width * 0.16f;
        canvas.DrawArc(f.Center.X - centerR, f.Top - centerR, centerR * 2, centerR * 2, 180, 360, true, false);
        FillSpot(canvas, f.Center.X, f.Top, 2.5f);

        // Penalty area + goal area along the bottom (the defended goal line).
        var penW = f.Width * 0.58f;
        var penH = f.Height * 0.22f;
        canvas.DrawRectangle(f.Center.X - penW / 2, f.Bottom - penH, penW, penH);

        var goalAreaW = f.Width * 0.30f;
        var goalAreaH = f.Height * 0.09f;
        canvas.DrawRectangle(f.Center.X - goalAreaW / 2, f.Bottom - goalAreaH, goalAreaW, goalAreaH);

        FillSpot(canvas, f.Center.X, f.Bottom - penH * 0.55f, 2.5f);

        // Penalty arc at the top edge of the box.
        var arcR = f.Width * 0.12f;
        canvas.DrawArc(f.Center.X - arcR, f.Bottom - penH - arcR, arcR * 2, arcR * 2, 200, 340, true, false);

        // Goal.
        var goalW = f.Width * 0.18f;
        canvas.StrokeSize = 4f;
        canvas.DrawLine(f.Center.X - goalW / 2, f.Bottom, f.Center.X + goalW / 2, f.Bottom);
        canvas.StrokeSize = 2.5f;
    }

    private static void DrawFull(ICanvas canvas, RectF f)
    {
        // Halfway line + center circle.
        canvas.DrawLine(f.Center.X, f.Top, f.Center.X, f.Bottom);
        var centerR = f.Height * 0.16f;
        canvas.DrawEllipse(f.Center.X - centerR, f.Center.Y - centerR, centerR * 2, centerR * 2);
        FillSpot(canvas, f.Center.X, f.Center.Y, 2.5f);

        var penW = f.Width * 0.16f;
        var penH = f.Height * 0.55f;
        var goalW = f.Width * 0.06f;
        var goalH = f.Height * 0.28f;

        // Left box + goal area.
        canvas.DrawRectangle(f.Left, f.Center.Y - penH / 2, penW, penH);
        canvas.DrawRectangle(f.Left, f.Center.Y - goalH / 2, goalW, goalH);

        // Right box + goal area.
        canvas.DrawRectangle(f.Right - penW, f.Center.Y - penH / 2, penW, penH);
        canvas.DrawRectangle(f.Right - goalW, f.Center.Y - goalH / 2, goalW, goalH);

        // Goals.
        var goalMouth = f.Height * 0.16f;
        canvas.StrokeSize = 4f;
        canvas.DrawLine(f.Left, f.Center.Y - goalMouth / 2, f.Left, f.Center.Y + goalMouth / 2);
        canvas.DrawLine(f.Right, f.Center.Y - goalMouth / 2, f.Right, f.Center.Y + goalMouth / 2);
        canvas.StrokeSize = 2.5f;
    }

    private static void FillSpot(ICanvas canvas, float x, float y, float r)
    {
        canvas.FillColor = Line;
        canvas.FillEllipse(x - r, y - r, r * 2, r * 2);
    }
}
