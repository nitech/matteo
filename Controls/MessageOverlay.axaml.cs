using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace Matteo.Controls;

public partial class MessageOverlay : UserControl
{
    public event EventHandler? MessageClosed;

    public MessageOverlay()
    {
        InitializeComponent();
        OkButton.Click += OkButton_Click;
    }

    public void ShowMessage(string title, string message)
    {
        TitleText.Text = title;
        MessageText.Text = message;
        OverlayGrid.IsVisible = true;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        OverlayGrid.IsVisible = false;
        MessageClosed?.Invoke(this, EventArgs.Empty);
    }
} 