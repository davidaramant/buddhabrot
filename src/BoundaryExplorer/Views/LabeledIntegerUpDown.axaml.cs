using Avalonia;
using Avalonia.Controls;

namespace BoundaryExplorer.Views;

public partial class LabeledIntegerUpDown : UserControl
{
	public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<
		LabeledIntegerUpDown,
		string?
	>(nameof(Text), defaultValue: null);

	public string? Text
	{
		get => GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public static readonly StyledProperty<decimal> IncrementProperty =
		NumericUpDown.IncrementProperty.AddOwner<LabeledIntegerUpDown>();

	public decimal Increment
	{
		get => GetValue(IncrementProperty);
		set => SetValue(IncrementProperty, value);
	}

	public static readonly StyledProperty<decimal?> ValueProperty =
		NumericUpDown.ValueProperty.AddOwner<LabeledIntegerUpDown>();

	public decimal? Value
	{
		get => GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public LabeledIntegerUpDown()
	{
		InitializeComponent();
	}
}
