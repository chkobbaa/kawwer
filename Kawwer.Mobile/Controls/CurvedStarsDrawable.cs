using Microsoft.Maui.Graphics;

namespace Kawwer.Mobile.Controls;

/// <summary>
/// Draws a player's rating as filled stars laid out along an arc that hugs the top of a circular
/// avatar. Supports quarter-star precision (e.g. 3.75 → ★★★¾) and deliberately never draws the
/// trailing empty stars — only the earned portion is shown, centred above the avatar.
/// </summary>
public sealed class CurvedStarsDrawable : IDrawable
{
    /// <summary>Rating in the range [0, <see cref="MaxStars"/>]. Rendered to the nearest quarter.</summary>
    public double Rating { get; set; }

    public int MaxStars { get; set; } = 5;

    /// <summary>Diameter of the avatar the arc curves around, in the same units as the canvas.</summary>
    public double AvatarDiameter { get; set; } = 96;

    /// <summary>Warm gold used for the earned portion of a star.</summary>
    public Color FilledColor { get; set; } = Color.FromArgb("#F6B73C");

    /// <summary>Subtle fill for the remainder of a partially-earned star (the "half" of a half star).</summary>
    public Color TrackColor { get; set; } = Color.FromArgb("#66808080");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Snap to the nearest quarter so the display matches quarter-star precision.
        var rounded = Math.Clamp(Math.Round(Rating * 4.0, MidpointRounding.AwayFromZero) / 4.0, 0, MaxStars);
        if (rounded <= 0)
        {
            return; // No rating yet: draw nothing rather than a row of empty stars.
        }

        // Number of star glyphs to draw: full stars plus one partial star for the fraction.
        var glyphs = Math.Clamp((int)Math.Ceiling(rounded - 1e-6), 0, MaxStars);
        if (glyphs == 0)
        {
            return;
        }

        var a = (float)AvatarDiameter;
        var outerR = a * 0.088f;       // star outer radius — smaller, more refined than before
        var innerR = outerR * 0.42f;   // star inner radius (classic 5-point ratio)
        var gap = a * 0.015f;          // hug the avatar edge closely
        var arcR = a / 2f + gap + outerR;

        var cx = dirtyRect.Width / 2f;
        var cy = dirtyRect.Height - a / 2f; // avatar centre sits at the bottom of the canvas

        // Spread the stars symmetrically around the 12 o'clock position.
        var stepDeg = glyphs <= 1 ? 0.0 : Math.Min(26.0, 116.0 / glyphs);
        var startOffset = -(glyphs - 1) / 2.0;
        // Largest absolute offset from centre, used to normalise the side-star "warp".
        var maxOffset = Math.Max(Math.Abs(startOffset), 1e-6);

        for (var i = 0; i < glyphs; i++)
        {
            var offset = startOffset + i;
            var angleDeg = -90.0 + offset * stepDeg; // -90° = straight up
            var angleRad = angleDeg * Math.PI / 180.0;
            var starX = cx + arcR * (float)Math.Cos(angleRad);
            var starY = cy + arcR * (float)Math.Sin(angleRad);
            var fill = (float)Math.Clamp(rounded - i, 0, 1);

            // Warp the side stars: the further from the centre, the more they lean outward and
            // flatten slightly, so the band reads as a gentle curved crown rather than a flat row.
            var t = offset / maxOffset;                 // -1 (far left) .. 0 (centre) .. 1 (far right)
            var lean = (float)(t * 9.0);                // extra outward tilt in degrees
            var squash = (float)(1.0 - 0.16 * Math.Abs(t)); // subtle vertical compression at the edges

            canvas.SaveState();
            canvas.Translate(starX, starY);
            canvas.Rotate((float)(angleDeg + 90.0) + lean); // follow the arc tangent, plus the lean
            canvas.Scale(1f, squash);
            DrawStar(canvas, outerR, innerR, fill);
            canvas.RestoreState();
        }
    }

    private void DrawStar(ICanvas canvas, float outerR, float innerR, float fill)
    {
        var path = BuildStarPath(outerR, innerR);

        if (fill >= 1f)
        {
            canvas.FillColor = FilledColor;
            canvas.FillPath(path);
            return;
        }

        // The star spans x ∈ [-outerR, outerR] in local space. Fill the left `fill` fraction gold
        // and the remainder with a faint track so a partial star reads as e.g. a clean half star.
        var boundary = -outerR + fill * (2f * outerR);

        canvas.SaveState();
        canvas.ClipRectangle(-outerR, -outerR, boundary + outerR, 2f * outerR);
        canvas.FillColor = FilledColor;
        canvas.FillPath(path);
        canvas.RestoreState();

        canvas.SaveState();
        canvas.ClipRectangle(boundary, -outerR, outerR - boundary, 2f * outerR);
        canvas.FillColor = TrackColor;
        canvas.FillPath(path);
        canvas.RestoreState();
    }

    private static PathF BuildStarPath(float outerR, float innerR)
    {
        var path = new PathF();
        for (var i = 0; i < 10; i++)
        {
            var r = (i % 2 == 0) ? outerR : innerR;
            var angle = -Math.PI / 2 + i * Math.PI / 5; // 36° between each outer/inner vertex
            var x = r * (float)Math.Cos(angle);
            var y = r * (float)Math.Sin(angle);
            if (i == 0)
            {
                path.MoveTo(x, y);
            }
            else
            {
                path.LineTo(x, y);
            }
        }

        path.Close();
        return path;
    }
}
