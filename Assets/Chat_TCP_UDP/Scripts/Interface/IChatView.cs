public interface IChatView
{
    void DisplayText(string message);
    void DisplayImage(byte[] imageData);
    void DisplayFile(byte[] fileData, string fileName);

    void ShowError(string message);

    void SetConnectionStatus(string status);
}