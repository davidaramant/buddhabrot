using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace BoundaryExplorer.Views;

public partial class LabeledIntegerUpDown : UserControl
{
	public static readonly DirectProperty<LabeledIntegerUpDown, string?> TextProperty =
		TextBlock.TextProperty.AddOwner<LabeledIntegerUpDown>(o => o.Text, (o, v) => o.Text = v);

	private string? _text = null;

	public string? Text
	{
		get => _text;
		set => SetAndRaise(TextProperty, ref _text, value);
	}

	public static readonly StyledProperty<double> IncrementProperty =
		NumericUpDown.IncrementProperty.AddOwner<LabeledIntegerUpDown>();

	public double Increment
	{
		get => GetValue(IncrementProperty);
		set => SetValue(IncrementProperty, value);
	}

	public static readonly DirectProperty<LabeledIntegerUpDown, double> ValueProperty =
		NumericUpDown.ValueProperty.AddOwner<LabeledIntegerUpDown>(o => o.Value, (o, v) => o.Value = v);

	private double _value = 0;

	public double Value
	{
		get => _value;
		set => SetAndRaise(ValueProperty, ref _value, value);
	}

	public LabeledIntegerUpDown()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
