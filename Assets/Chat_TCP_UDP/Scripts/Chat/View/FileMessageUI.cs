using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;

public class FileMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text fileNameText;
    [SerializeField] private TMP_Text fileSizeText;
    [SerializeField] private Button saveButton;

    private byte[] fileData;
    private string fileName;

    public void Initialize(byte[] data, string name)
    {
        fileData = data;
        fileName = name;

        fileNameText.text = name;
        fileSizeText.text = $"{(data.Length / 1024f):F1} KB";

        saveButton.onClick.AddListener(SaveFile);
    }

    private void SaveFile()
    {
        string extension = System.IO.Path.GetExtension(fileName);

        var extensionList = new[]
        {
            new ExtensionFilter("File", extension.Replace(".", ""))
        };

        string path = StandaloneFileBrowser.SaveFilePanel(
            "Save File",
            "",
            fileName,
            extensionList
        );

        if (string.IsNullOrEmpty(path))
            return;

        System.IO.File.WriteAllBytes(path, fileData);
    }
}