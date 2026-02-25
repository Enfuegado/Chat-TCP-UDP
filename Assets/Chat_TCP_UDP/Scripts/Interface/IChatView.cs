public interface IChatView
{
    void DisplayText(string message);
    void DisplayImage(byte[] data);
    void DisplayFile(byte[] data, string fileName);
    void DisplayAudio(byte[] data, string fileName);
}