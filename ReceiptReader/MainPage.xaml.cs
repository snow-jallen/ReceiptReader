using ReceiptReader.Services;
using ReceiptReader.Models;

namespace ReceiptReader;

public partial class MainPage : ContentPage
{
	private readonly ReceiptService _receiptService;

	public MainPage(ReceiptService receiptService)
	{
		InitializeComponent();
		_receiptService = receiptService;
	}

	private async void OnUploadClicked(object? sender, EventArgs e)
	{
		try
		{
			// Hide previous results and errors
			ResultFrame.IsVisible = false;
			ErrorLabel.IsVisible = false;

			// Pick a photo
			var results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
			{
				Title = "Select a receipt photo"
			});

			var result = results?.FirstOrDefault();
			if (result == null)
				return;

			// Show loading indicator
			LoadingIndicator.IsRunning = true;
			LoadingIndicator.IsVisible = true;
			UploadBtn.IsEnabled = false;

			// Convert image to byte array first
			byte[] imageBytes;
			using (var stream = await result.OpenReadAsync())
			{
				using var memoryStream = new MemoryStream();
				await stream.CopyToAsync(memoryStream);
				imageBytes = memoryStream.ToArray();
			}

			// Process the receipt
			var receipt = await _receiptService.ProcessReceiptImageAsync(imageBytes);

			// Display results
			MerchantSpan.Text = receipt.MerchantName ?? "Unknown";
			DateSpan.Text = receipt.TransactionDate?.ToString("yyyy-MM-dd");
			TotalSpan.Text = $"${receipt.TotalAmount:F2}";
			LineItemsView.ItemsSource = receipt.LineItems;

			ResultFrame.IsVisible = true;
		}
		catch (Exception ex)
		{
			ErrorLabel.Text = $"Error: {ex.Message}";
			ErrorLabel.IsVisible = true;
		}
		finally
		{
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
			UploadBtn.IsEnabled = true;
		}
	}
}
