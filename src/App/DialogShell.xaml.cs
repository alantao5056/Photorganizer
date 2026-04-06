using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Alan.Sortify.App;

public sealed partial class DialogShell : UserControl
{
    public DialogShell()
    {
        InitializeComponent();
    }

    // ── Icon ──

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(DialogShell),
            new PropertyMetadata("\uE946", OnIconGlyphChanged));

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.HeaderIcon.Glyph = (string)e.NewValue;
    }

    public static readonly DependencyProperty IconForegroundProperty =
        DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(DialogShell),
            new PropertyMetadata(null, OnIconForegroundChanged));

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    private static void OnIconForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.HeaderIcon.Foreground = (Brush)e.NewValue;
    }

    public static readonly DependencyProperty IconBackgroundProperty =
        DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(DialogShell),
            new PropertyMetadata(null, OnIconBackgroundChanged));

    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    private static void OnIconBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.IconBorder.Background = (Brush)e.NewValue;
    }

    public static readonly DependencyProperty IconBorderBrushProperty =
        DependencyProperty.Register(nameof(IconBorderBrush), typeof(Brush), typeof(DialogShell),
            new PropertyMetadata(null, OnIconBorderBrushChanged));

    public Brush IconBorderBrush
    {
        get => (Brush)GetValue(IconBorderBrushProperty);
        set => SetValue(IconBorderBrushProperty, value);
    }

    private static void OnIconBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.IconBorder.BorderBrush = (Brush)e.NewValue;
    }

    // ── Title / Subtitle ──

    public static readonly DependencyProperty DialogTitleProperty =
        DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(DialogShell),
            new PropertyMetadata("", OnDialogTitleChanged));

    public string DialogTitle
    {
        get => (string)GetValue(DialogTitleProperty);
        set => SetValue(DialogTitleProperty, value);
    }

    private static void OnDialogTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.TitleBlock.Text = (string)e.NewValue;
    }

    public static readonly DependencyProperty DialogSubtitleProperty =
        DependencyProperty.Register(nameof(DialogSubtitle), typeof(string), typeof(DialogShell),
            new PropertyMetadata("", OnDialogSubtitleChanged));

    public string DialogSubtitle
    {
        get => (string)GetValue(DialogSubtitleProperty);
        set => SetValue(DialogSubtitleProperty, value);
    }

    private static void OnDialogSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.SubtitleBlock.Text = (string)e.NewValue;
    }

    // ── Width ──

    public static readonly DependencyProperty DialogWidthProperty =
        DependencyProperty.Register(nameof(DialogWidth), typeof(double), typeof(DialogShell),
            new PropertyMetadata(420.0, OnDialogWidthChanged));

    public double DialogWidth
    {
        get => (double)GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    private static void OnDialogWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.Card.Width = (double)e.NewValue;
    }

    // ── Body content ──

    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(object), typeof(DialogShell),
            new PropertyMetadata(null, OnBodyChanged));

    public object Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.BodyPresenter.Content = e.NewValue;
    }

    // ── Footer ──

    public static readonly DependencyProperty PrimaryButtonTextProperty =
        DependencyProperty.Register(nameof(PrimaryButtonText), typeof(string), typeof(DialogShell),
            new PropertyMetadata("OK", OnPrimaryButtonTextChanged));

    public string PrimaryButtonText
    {
        get => (string)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    private static void OnPrimaryButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.PrimaryBtn.Content = (string)e.NewValue;
    }

    public static readonly DependencyProperty IsPrimaryButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsPrimaryButtonEnabled), typeof(bool), typeof(DialogShell),
            new PropertyMetadata(true, OnIsPrimaryButtonEnabledChanged));

    public bool IsPrimaryButtonEnabled
    {
        get => (bool)GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    private static void OnIsPrimaryButtonEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.PrimaryBtn.IsEnabled = (bool)e.NewValue;
    }

    public static readonly DependencyProperty FooterExtraProperty =
        DependencyProperty.Register(nameof(FooterExtra), typeof(object), typeof(DialogShell),
            new PropertyMetadata(null, OnFooterExtraChanged));

    public object FooterExtra
    {
        get => GetValue(FooterExtraProperty);
        set => SetValue(FooterExtraProperty, value);
    }

    private static void OnFooterExtraChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.FooterExtraPresenter.Content = e.NewValue;
    }

    public static readonly DependencyProperty FooterTopMarginProperty =
        DependencyProperty.Register(nameof(FooterTopMargin), typeof(double), typeof(DialogShell),
            new PropertyMetadata(0.0, OnFooterTopMarginChanged));

    public double FooterTopMargin
    {
        get => (double)GetValue(FooterTopMarginProperty);
        set => SetValue(FooterTopMarginProperty, value);
    }

    private static void OnFooterTopMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.FooterGrid.Margin = new Thickness(0, (double)e.NewValue, 0, 0);
    }

    // ── Cancel button visibility ──

    public static readonly DependencyProperty IsCancelButtonVisibleProperty =
        DependencyProperty.Register(nameof(IsCancelButtonVisible), typeof(bool), typeof(DialogShell),
            new PropertyMetadata(true, OnIsCancelButtonVisibleChanged));

    public bool IsCancelButtonVisible
    {
        get => (bool)GetValue(IsCancelButtonVisibleProperty);
        set => SetValue(IsCancelButtonVisibleProperty, value);
    }

    private static void OnIsCancelButtonVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DialogShell shell)
            shell.CancelBtn.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Events ──

    public event RoutedEventHandler? CancelClick;
    public event RoutedEventHandler? PrimaryClick;

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
        => CancelClick?.Invoke(this, e);

    private void PrimaryBtn_Click(object sender, RoutedEventArgs e)
        => PrimaryClick?.Invoke(this, e);
}
