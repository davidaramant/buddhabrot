using System.Reactive.Linq;
using Avalonia.Media;
using BoundaryExplorer.Extensions;
using Buddhabrot.Core.Boundary;
using DynamicData.Binding;
using Humanizer;
using ReactiveUI;

namespace BoundaryExplorer.ViewModels;

public sealed class LegendColorViewModel : ViewModelBase
{
	private readonly ObservableAsPropertyHelper<Brush> _colorProperty;

	public Brush Color => _colorProperty.Value;
	public string Description { get; }

	public LegendColorViewModel(VisualizeViewModel parent, LookupRegionType type)
	{
		Description = type.ToString().Humanize(LetterCasing.Title).Replace("to", "→");

		parent
			.WhenPropertyChanged(x => x.Palette, notifyOnInitialValue: true)
			.Select(p => new SolidColorBrush(p.Value![type].ToAvaloniaColor()))
			.ToProperty(this, x => x.Color, out _colorProperty);
	}
}
