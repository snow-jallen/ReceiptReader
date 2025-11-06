using ReceiptReader.Services;
using ReceiptReader.Models;
using System.Collections.ObjectModel;

namespace ReceiptReader;

public partial class MainPage : ContentPage
{
	private readonly ReceiptService _receiptService;
	private readonly ObservableCollection<ProcessedReceipt> _processedReceipts = [];

	public MainPage(ReceiptService receiptService)
	{
		InitializeComponent();
		_receiptService = receiptService;
		ReceiptsCollection.ItemsSource = _processedReceipts;
	}

	private async void OnUploadClicked(object? sender, EventArgs e)
	{
		try
		{
			// Hide previous results and errors
			ReceiptsCollection.IsVisible = false;
			ErrorLabel.IsVisible = false;

			// Pick photos
			var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
			{
				Title = "Select receipt photos",
				SelectionLimit = 64
			});

			if (results == null || !results.Any())
				return;

			// Show loading panel
			LoadingPanel.IsVisible = true;
			LoadingIndicator.IsRunning = true;
			UploadBtn.IsEnabled = false;

			// Clear previous receipts
			_processedReceipts.Clear();

			var totalCount = results.Count();
			var completedCount = 0;

			// Update progress
			UpdateProgress(completedCount, totalCount);

			// Show collection immediately (empty at first)
			ReceiptsCollection.IsVisible = true;

			// Process all images in parallel, but add results as they complete
			var processingTasks = results.Select(async result =>
			{
				try
				{
					// Convert image to byte array
					byte[] imageBytes;
					using (var stream = await result.OpenReadAsync())
					{
						using var memoryStream = new MemoryStream();
						await stream.CopyToAsync(memoryStream);
						imageBytes = memoryStream.ToArray();
					}

					// Process the receipt
					var receipt = await _receiptService.ProcessReceiptImageAsync(imageBytes);

					var processedReceipt = new ProcessedReceipt
					{
						ImageBytes = imageBytes,
						Receipt = receipt
					};

					// Add to UI immediately (on main thread)
					await MainThread.InvokeOnMainThreadAsync(() =>
					{
						_processedReceipts.Add(processedReceipt);
						completedCount++;
						UpdateProgress(completedCount, totalCount);
					});
				}
				catch (Exception ex)
				{
					// Add error receipt to UI (on main thread)
					await MainThread.InvokeOnMainThreadAsync(() =>
					{
						_processedReceipts.Add(new ProcessedReceipt
						{
							ImageBytes = [],
							Receipt = new Receipt
							{
								MerchantName = $"Error: {ex.Message}",
								TotalAmount = 0,
								LineItems = []
							}
						});
						completedCount++;
						UpdateProgress(completedCount, totalCount);
					});
				}
			}).ToList();

			// Wait for all processing to complete
			await Task.WhenAll(processingTasks);
		}
		catch (Exception ex)
		{
			ErrorLabel.Text = $"Error: {ex.Message}";
			ErrorLabel.IsVisible = true;
		}
		finally
		{
			LoadingPanel.IsVisible = false;
			LoadingIndicator.IsRunning = false;
			UploadBtn.IsEnabled = true;
		}
	}

	private void UpdateProgress(int completed, int total)
	{
		ProgressLabel.Text = $"Processing: {completed} of {total}";
		ProgressBar.Progress = total > 0 ? (double)completed / total : 0;
	}

	private void OnReceiptItemSizeChanged(object? sender, EventArgs e)
	{
		if (sender is not Grid grid)
			return;

		// Get the image and details borders
		var imageBorder = grid.FindByName<Border>("ImageBorder");
		var detailsBorder = grid.FindByName<Border>("DetailsBorder");

		if (imageBorder == null || detailsBorder == null)
			return;

		// Determine layout based on width
		// Wide screen (>= 800px): side-by-side layout
		// Narrow screen (< 800px): stacked layout
		if (grid.Width >= 800)
		{
			// Side-by-side layout
			Grid.SetRow(imageBorder, 0);
			Grid.SetColumn(imageBorder, 0);
			Grid.SetRow(detailsBorder, 0);
			Grid.SetColumn(detailsBorder, 1);
		}
		else
		{
			// Stacked layout
			Grid.SetRow(imageBorder, 0);
			Grid.SetColumn(imageBorder, 0);
			Grid.SetRow(detailsBorder, 1);
			Grid.SetColumn(detailsBorder, 0);
		}
	}
}
