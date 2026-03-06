public interface IChatView
{
    void DisplayText(string message, string senderLabel, bool isMine);
    void DisplayImage(byte[] imageData, string senderLabel, bool isMine);
    void DisplayFile(byte[] fileData, string fileName, string senderLabel, bool isMine);

    void ShowError(string message);

    void SetConnectionStatus(ConnectionStatus status);
}