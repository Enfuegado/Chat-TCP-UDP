using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIView : MonoBehaviour, IChatView
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private GameObject filePrefab;

    [Header("Connection Status")]
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Error Popup")]
    [SerializeField] private ErrorPopupUI errorPopup;

    private Color connectedColor = new Color32(0, 168, 13, 255);
    private Color disconnectedColor = new Color32(168, 3, 0, 255);
    private Color clientMessageColor = new Color32(180, 210, 230, 255);
    private Color serverMessageColor = new Color32(240, 240, 240, 255);

    private Sprite roundedSprite;

    private void Awake()
    {
        roundedSprite = CreateRoundedSprite(7);

        VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
        if (contentLayout == null)
            contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();

        contentLayout.spacing = 6;
        contentLayout.padding = new RectOffset(8, 8, 8, 8);
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
    }

    private Sprite CreateRoundedSprite(int radius)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x, radius, size - radius);
                float cy = Mathf.Clamp(y, radius, size - radius);
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(1f - (dist - (radius - 1f)));
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius)
        );
    }

    private GameObject CreateRow(bool isMine)
    {
        GameObject row = new GameObject("Row", typeof(RectTransform));
        row.transform.SetParent(content, false);

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = isMine ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        ContentSizeFitter rowFitter = row.AddComponent<ContentSizeFitter>();
        rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        return row;
    }

    private GameObject CreateBubble(GameObject row, bool isMine)
    {
        GameObject bubble = new GameObject("Bubble", typeof(RectTransform));
        bubble.transform.SetParent(row.transform, false);

        Image bg = bubble.AddComponent<Image>();
        bg.sprite = roundedSprite;
        bg.type = Image.Type.Sliced;
        bg.color = isMine ? clientMessageColor : serverMessageColor;

        VerticalLayoutGroup vlg = bubble.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 8, 8);
        vlg.spacing = 4;

        ContentSizeFitter fitter = bubble.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement bubbleLayout = bubble.AddComponent<LayoutElement>();
        bubbleLayout.flexibleWidth = 0;

        return bubble;
    }

    private void AddTimestamp(GameObject parent, string senderLabel, bool isMine)
    {
        GameObject timeGO = new GameObject("Timestamp", typeof(RectTransform));
        timeGO.transform.SetParent(parent.transform, false);

        ContentSizeFitter timeFitter = timeGO.AddComponent<ContentSizeFitter>();
        timeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        timeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        TMP_Text timeText = timeGO.AddComponent<TextMeshProUGUI>();
        timeText.richText = true;
        timeText.text = "<b>" + senderLabel + "</b>  " + DateTime.Now.ToString("HH:mm");
        timeText.fontSize = 9;
        timeText.color = new Color32(100, 100, 100, 255);
        timeText.alignment = isMine ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
    }

    public void DisplayText(string message, string senderLabel, bool isMine)
    {
        GameObject row = CreateRow(isMine);
        GameObject bubble = CreateBubble(row, isMine);

        GameObject textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(bubble.transform, false);

        ContentSizeFitter textFitter = textGO.AddComponent<ContentSizeFitter>();
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        TMP_Text text = textGO.AddComponent<TextMeshProUGUI>();
        text.richText = true;
        text.text = "<b>" + senderLabel + "</b>\n" + message;
        text.color = Color.black;
        text.fontSize = 14;

        AddTimestamp(bubble, senderLabel, isMine);
    }

    public void DisplayImage(byte[] data, string senderLabel, bool isMine)
    {
        Texture2D texture = new Texture2D(2, 2);

        if (!texture.LoadImage(data))
        {
            Debug.LogWarning("Failed to load image data");
            return;
        }

        GameObject row = CreateRow(isMine);
        GameObject bubble = CreateBubble(row, isMine);

        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(bubble.transform, false);
        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.richText = true;
        label.text = "<b>" + senderLabel + "</b>";
        label.fontSize = 11;
        label.color = new Color32(80, 80, 80, 255);

        GameObject imageGO = Instantiate(imagePrefab, bubble.transform);
        Image img = imageGO.GetComponent<Image>();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        img.sprite = sprite;
        img.preserveAspect = true;

        RectTransform contentRect = content as RectTransform;
        float maxWidth = contentRect.rect.width - 20f;
        float aspectRatio = (float)texture.height / texture.width;
        float calculatedHeight = maxWidth * aspectRatio;

        LayoutElement layout = imageGO.GetComponent<LayoutElement>();
        if (layout == null)
            layout = imageGO.AddComponent<LayoutElement>();

        layout.preferredWidth = maxWidth * 0.6f;
        layout.preferredHeight = calculatedHeight * 0.6f;

        AddTimestamp(bubble, senderLabel, isMine);
    }

    public void DisplayFile(byte[] data, string fileName, string senderLabel, bool isMine)
    {
        if (filePrefab == null)
        {
            Debug.LogError("File prefab not assigned in ChatUIView");
            return;
        }

        GameObject row = CreateRow(isMine);
        GameObject bubble = CreateBubble(row, isMine);

        // ── Ancho del bubble proporcional al contenedor ──────────────────────
        RectTransform contentRect = content as RectTransform;
        float bubbleTargetWidth = contentRect.rect.width * 0.62f;

        LayoutElement bubbleLayout = bubble.GetComponent<LayoutElement>();
        if (bubbleLayout == null)
            bubbleLayout = bubble.AddComponent<LayoutElement>();
        bubbleLayout.minWidth     = bubbleTargetWidth;
        bubbleLayout.preferredWidth = bubbleTargetWidth;

        // ── Padding interno con más aire ─────────────────────────────────────
        VerticalLayoutGroup vlg = bubble.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
            vlg.padding = new RectOffset(12, 12, 10, 10);

        // ── Etiqueta del remitente alineada al lado correcto ─────────────────
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(bubble.transform, false);

        ContentSizeFitter labelFitter = labelGO.AddComponent<ContentSizeFitter>();
        labelFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        labelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.richText  = true;
        label.text      = "<b>" + senderLabel + "</b>";
        label.fontSize  = 11;
        label.color     = new Color32(80, 80, 80, 255);
        label.alignment = isMine ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;

        // ── Prefab del archivo con tamaño proporcional al bubble ─────────────
        GameObject fileGO = Instantiate(filePrefab, bubble.transform);

        float innerWidth = bubbleTargetWidth - 24f; // descontando padding lateral

        LayoutElement fileLayout = fileGO.GetComponent<LayoutElement>();
        if (fileLayout == null)
            fileLayout = fileGO.AddComponent<LayoutElement>();
        fileLayout.minWidth      = innerWidth;
        fileLayout.preferredWidth  = innerWidth;
        fileLayout.minHeight     = 64f;
        fileLayout.preferredHeight = 64f;

        // ── Texto interno legible con ellipsis si el nombre es largo ─────────
        foreach (TMP_Text t in fileGO.GetComponentsInChildren<TMP_Text>())
        {
            t.fontSize     = 12;
            t.overflowMode = TextOverflowModes.Ellipsis;
        }

        FileMessageUI fileUI = fileGO.GetComponent<FileMessageUI>();
        if (fileUI == null)
        {
            Debug.LogError("FileMessageUI component missing in filePrefab");
            return;
        }

        fileUI.Initialize(data, fileName);

        AddTimestamp(bubble, senderLabel, isMine);

        LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.GetComponent<RectTransform>());
    }

    public void ShowError(string message)
    {
        if (errorPopup == null)
        {
            Debug.LogError("ErrorPopupUI not assigned in ChatUIView");
            return;
        }

        errorPopup.Show(message);
    }

    public void SetConnectionStatus(ConnectionStatus status)
    {
        if (connectionStatusText == null)
            return;

        switch (status)
        {
            case ConnectionStatus.Connected:
                connectionStatusText.text  = "Connected";
                connectionStatusText.color = connectedColor;
                break;

            case ConnectionStatus.Disconnected:
                connectionStatusText.text  = "Disconnected";
                connectionStatusText.color = disconnectedColor;
                break;
        }
    }
}