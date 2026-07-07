using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class LineupPage : ContentPage
{
    // Avatar footprint on the board (circle + name label), used for both layout and drag math.
    private const double AvatarWidth = 64;
    private const double AvatarHeight = 72;

    private static readonly Color TeamAColor = Color.FromArgb("#CDF564");
    private static readonly Color TeamBColor = Color.FromArgb("#FB8C00");

    private readonly LineupViewModel _viewModel;
    private readonly PitchDrawable _halfDrawable = new() { Mode = PitchMode.HalfVertical };
    private readonly PitchDrawable _fullDrawable = new() { Mode = PitchMode.FullHorizontal };

    private double _pageWidth;
    private double _pageHeight;

    public LineupPage(LineupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        HalfPitchView.Drawable = _halfDrawable;
        FullPitchView.Drawable = _fullDrawable;

        _viewModel.BoardChanged += OnBoardChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SubscribeRealtime();
        RenderBoards();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.UnsubscribeRealtime();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (width <= 0 || height <= 0)
        {
            return;
        }

        _pageWidth = width;
        _pageHeight = height;
        SizeFullPitchContainer();
        RenderBoards();
    }

    private void OnBoardChanged() => MainThread.BeginInvokeOnMainThread(RenderBoards);

    /// <summary>
    /// Sizes the full-pitch container so that, after the 90° rotation, its on-screen bounding box
    /// fills the (still portrait) window: a view rotated 90° swaps its width and height on screen,
    /// so we request Width = page height and Height = page width. The device never rotates.
    /// </summary>
    private void SizeFullPitchContainer()
    {
        if (_pageWidth <= 0 || _pageHeight <= 0)
        {
            return;
        }

        const double margin = 16;
        FullPitchContainer.WidthRequest = _pageHeight - margin;
        FullPitchContainer.HeightRequest = _pageWidth - margin;
    }

    private void RenderBoards()
    {
        if (_viewModel.ShowFullPitch)
        {
            SizeFullPitchContainer();
            RenderFullBoard();
            FullPitchView.Invalidate();
        }
        else
        {
            RenderHalfBoard();
            HalfPitchView.Invalidate();
        }
    }

    // ----- Portrait half board (interactive) -----

    private void RenderHalfBoard()
    {
        HalfSurface.Clear();

        var team = _viewModel.ViewingTeam;
        var color = team == TeamSide.TeamB ? TeamBColor : TeamAColor;
        var players = _viewModel.Tokens.Where(t => t.Team == team).ToList();

        HalfEmptyLabel.IsVisible = players.Count == 0;

        foreach (var token in players)
        {
            var avatar = CreateAvatar(token, color);

            // Depth (own goal 0 -> halfway 1) maps to bottom->top; width maps straight across.
            var fracX = token.PositionY;
            var fracY = 1 - token.PositionX;
            AbsoluteLayout.SetLayoutFlags(avatar, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(avatar, new Rect(fracX, fracY, AvatarWidth, AvatarHeight));

            if (_viewModel.IsOrganizer)
            {
                AttachDrag(avatar, token, team);
            }

            HalfSurface.Add(avatar);
        }
    }

    /// <summary>Turns pan gestures into board moves, saving the new coordinates when the drag ends.</summary>
    private void AttachDrag(View avatar, LineupToken token, TeamSide team)
    {
        var pan = new PanGestureRecognizer();
        double startX = 0, startY = 0;

        pan.PanUpdated += (_, e) =>
        {
            var width = HalfSurface.Width;
            var height = HalfSurface.Height;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    var bounds = AbsoluteLayout.GetLayoutBounds(avatar);
                    startX = bounds.X;
                    startY = bounds.Y;
                    break;

                case GestureStatus.Running:
                    var nx = Math.Clamp(startX + e.TotalX / Math.Max(1, width - AvatarWidth), 0, 1);
                    var ny = Math.Clamp(startY + e.TotalY / Math.Max(1, height - AvatarHeight), 0, 1);
                    AbsoluteLayout.SetLayoutBounds(avatar, new Rect(nx, ny, AvatarWidth, AvatarHeight));
                    break;

                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    var final = AbsoluteLayout.GetLayoutBounds(avatar);
                    token.PositionY = Math.Clamp(final.X, 0, 1);
                    token.PositionX = Math.Clamp(1 - final.Y, 0, 1);
                    token.Team = team;
                    _ = _viewModel.SaveSlotAsync(token);
                    break;
            }
        };

        avatar.GestureRecognizers.Add(pan);
    }

    // ----- Simulated-landscape full board (read-only overview) -----

    private void RenderFullBoard()
    {
        FullSurface.Clear();

        foreach (var token in _viewModel.Tokens.Where(t => t.Team != TeamSide.Unassigned))
        {
            var isTeamA = token.Team == TeamSide.TeamA;
            var color = isTeamA ? TeamAColor : TeamBColor;

            // Team A defends the far left (X 0 -> left goal), Team B the far right (mirrored).
            var fracX = isTeamA
                ? token.PositionX * 0.5
                : 1 - token.PositionX * 0.5;
            var fracY = token.PositionY;

            var avatar = CreateAvatar(token, color);
            AbsoluteLayout.SetLayoutFlags(avatar, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(avatar, new Rect(fracX, fracY, AvatarWidth, AvatarHeight));
            FullSurface.Add(avatar);
        }
    }

    // ----- Avatar factory -----

    private static View CreateAvatar(LineupToken token, Color ringColor)
    {
        var content = new Grid();
        content.Add(new Label
        {
            Text = token.Initials,
            TextColor = Colors.White,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        });

        if (!string.IsNullOrWhiteSpace(token.ProfilePictureUrl))
        {
            content.Add(new Image
            {
                Source = token.ProfilePictureUrl,
                Aspect = Aspect.AspectFill
            });
        }

        var ring = new Border
        {
            WidthRequest = 46,
            HeightRequest = 46,
            StrokeThickness = 2.5,
            Stroke = ringColor,
            BackgroundColor = Color.FromArgb("#0C2014"),
            StrokeShape = new RoundRectangle { CornerRadius = 23 },
            HorizontalOptions = LayoutOptions.Center,
            Content = content
        };

        var name = new Label
        {
            Text = token.ShortName,
            FontSize = 10,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        };

        var stack = new VerticalStackLayout
        {
            Spacing = 2,
            WidthRequest = AvatarWidth,
            HeightRequest = AvatarHeight
        };
        stack.Add(ring);
        stack.Add(name);
        return stack;
    }
}
