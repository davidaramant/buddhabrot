using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using BoundaryExplorer.Models;
using Buddhabrot.Core.Boundary;
using Buddhabrot.Core.DataStorage;
using DynamicData;
using DynamicData.Binding;
using Humanizer;
using ReactiveUI;

namespace BoundaryExplorer.ViewModels;

public sealed class DiffViewModel : ViewModelBase
{
	private readonly BorderDataProvider _dataProvider;
	private readonly Action<string> _addToSystemLog;
	private BoundaryDataSet? _selectedLeft;
	private BoundaryDataSet? _selectedRight;
	private readonly ReadOnlyObservableCollection<BoundaryDataSet> _leftDataSets;
	private readonly ReadOnlyObservableCollection<BoundaryDataSet> _rightDataSets;
	private string _log = string.Empty;

	public ReadOnlyObservableCollection<BoundaryDataSet> LeftDataSets => _leftDataSets;
	public ReadOnlyObservableCollection<BoundaryDataSet> RightDataSets => _rightDataSets;

	public BoundaryDataSet? SelectedLeft
	{
		get => _selectedLeft;
		set => this.RaiseAndSetIfChanged(ref _selectedLeft, value);
	}

	public BoundaryDataSet? SelectedRight
	{
		get => _selectedRight;
		set => this.RaiseAndSetIfChanged(ref _selectedRight, value);
	}

	public ReactiveCommand<Unit, Unit> ComputeDiffCommand { get; }

	public string LogOutput
	{
		get => _log;
		private set => this.RaiseAndSetIfChanged(ref _log, value);
	}

	public DiffViewModel(BorderDataProvider dataProvider, Action<string> addToSystemLog)
	{
		_dataProvider = dataProvider;
		_addToSystemLog = addToSystemLog;

		dataProvider
			.SavedBoundaries.Connect()
			.Filter(b => !b.IsDiff)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Bind(out _leftDataSets)
			.Subscribe();

		var selectedLeftObservable = this.WhenValueChanged(t => t.SelectedLeft);

		_dataProvider
			.SavedBoundaries.Connect()
			.AutoRefreshOnObservable(_ => selectedLeftObservable)
			.Filter(b => !b.IsDiff && b != SelectedLeft)
			.Sort(BoundaryDataSet.Comparer)
			.ObserveOn(RxApp.MainThreadScheduler)
			.Bind(out _rightDataSets)
			.Subscribe();

		var canDiff = this.WhenAnyValue(
			t => t.SelectedLeft,
			t => t.SelectedRight,
			(left, right) => left != null && right != null
		);

		ComputeDiffCommand = ReactiveCommand.CreateFromTask(ComputeDiffAsync, canDiff);
	}

	private async Task ComputeDiffAsync()
	{
		try
		{
			var diff = await Task.Run(() =>
			{
				var left = Time(
					() => _dataProvider.LoadLookup(SelectedLeft!),
					(elapsed, _) => $"Loaded left in {elapsed}"
				);
				var right = Time(
					() => _dataProvider.LoadLookup(SelectedRight!),
					(elapsed, _) => $"Loaded right in {elapsed}"
				);
				var diff = Time(
					() => RegionLookupDiffer.Diff(left, right),
					(elapsed, diff) => $"Took {elapsed} to calculate diff of {diff.NodeCount:N0} nodes"
				);
				return diff;
			});

			_dataProvider.SaveDiff(SelectedLeft!.Parameters, SelectedRight!.Parameters, diff);
		}
		catch (Exception e)
		{
			var error = "Failed creating diff: " + e;
			AddToLog(error);
			_addToSystemLog(error);
		}

		T Time<T>(Func<T> generate, Func<string, T, string> makeMsg)
		{
			var timer = Stopwatch.StartNew();
			var item = generate();
			AddToLog(makeMsg(timer.Elapsed.Humanize(), item));
			return item;
		}
	}

	private void AddToLog(string msg) => Dispatcher.UIThread.Post(() => LogOutput += msg + Environment.NewLine);
}
