using Microsoft.Maui.Graphics;
using System.Timers;

namespace ClockApp;

public partial class MainPage : ContentPage
{
    private System.Timers.Timer _tickTimer;
    private ClockRenderer _clockRenderer;

    public MainPage()
    {
        InitializeComponent();
        SetupClock();
    }

    private void SetupClock()
    {
        _clockRenderer = new ClockRenderer();
        ClockCanvas.Drawable = _clockRenderer;
        InitializeTimer();          
    }

    private void InitializeTimer()
    {
        _tickTimer = new System.Timers.Timer(1000);
        _tickTimer.Elapsed += OnTick;
        _tickTimer.Start();
    }

    private void OnTick(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateDisplay);
    }

    private void UpdateDisplay()
    {
        _clockRenderer.DisplayedTime = DateTime.Now;
        ClockCanvas.Invalidate();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PauseTimer();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ResumeTimer();
    }

    private void ResumeTimer() => _tickTimer?.Start();
    private void PauseTimer() => _tickTimer?.Stop();
}

public class ClockRenderer : IDrawable
{
    public DateTime DisplayedTime { get; set; } = DateTime.Now;

    private float _segmentLength = 40;
    private float _segmentThickness = 10;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        
        canvas.FillColor = Colors.Black;
        canvas.FillRectangle(dirtyRect);

        canvas.StrokeColor = Colors.HotPink;
        canvas.StrokeSize = _segmentThickness;

        RenderTime(canvas, DisplayedTime, dirtyRect);
    }

    private void RenderTime(ICanvas canvas, DateTime time, RectF bounds)
    {
        string timeFormat = time.ToString("HH:mm:ss");
        float posX = 120;
        float posY = 60;
        float padding = 15;

        foreach (char character in timeFormat)
        {
            if (character == ':')
            {
                DrawColonSymbol(canvas, posX, posY, _segmentLength);
                posX += padding+5;
            }
            else
            {
                DrawCharacter(canvas, character - '0', posX, posY, _segmentLength);
                posX += _segmentLength + padding;
            }
        }
    }

    private void DrawCharacter(ICanvas canvas, int number, float x, float y, float size)
    {
        float halfSize = size / 2;

        var segments = new[]
        {
            (x, y, x + size, y), // Верхняя
            (x + size, y, x + size, y + halfSize), // Верхний правый
            (x + size, y + halfSize, x + size, y + size), // Нижний правый
            (x, y + size, x + size, y + size), // Нижняя
            (x, y + halfSize, x, y + size), // Нижний левый
            (x, y, x, y + halfSize), // Верхний левый
            (x, y + halfSize, x + size, y + halfSize), // Средняя
        };

        bool[] segmentActive = GetSegments(number);

        for (int i = 0; i < segments.Length; i++)
        {
            if (segmentActive[i])
            {
                var (sx1, sy1, sx2, sy2) = segments[i];
                canvas.DrawLine(sx1, sy1, sx2, sy2);
            }
        }
    }

    private bool[] GetSegments(int number)
    {
        return number switch
        {
            0 => new[] { true, true, true, true, true, true, false },
            1 => new[] { false, true, true, false, false, false, false },
            2 => new[] { true, true, false, true, true, false, true },
            3 => new[] { true, true, true, true, false, false, true },
            4 => new[] { false, true, true, false, false, true, true },
            5 => new[] { true, false, true, true, false, true, true },
            6 => new[] { true, false, true, true, true, true, true },
            7 => new[] { true, true, true, false, false, false, false },
            8 => new[] { true, true, true, true, true, true, true },
            9 => new[] { true, true, true, true, false, true, true },
            _ => new[] { false, false, false, false, false, false, false },
        };
    }

    private void DrawColonSymbol(ICanvas canvas, float x, float y, float size)
    {
        float dotSize = 3;
        float gap = size / 4;

        float topDotY = y + gap;
        float bottomDotY = y + 3 * gap;

        canvas.DrawEllipse(x, topDotY, dotSize, dotSize);
        canvas.DrawEllipse(x, bottomDotY, dotSize, dotSize);
    }
}