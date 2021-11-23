using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PotionGenerator : MonoBehaviour
{
    private static readonly int maxColorDifference = 5;

    [SerializeField]
    private GameObject pixel;

    private int height = 10;
    private int bottomWidth = 8;
    private int middleWidth = 4;
    private int topWidth = 6;

    // (bottomWidth, middleWidth, topWidth)
    [SerializeField]
    private List<Vector3> widthVectors = new List<Vector3>();

    private int maxLiquidHeight;

    [SerializeField]
    private List<Color> edgeColors;
    [SerializeField]
    private Color lidColor;

    private List<GameObject> pixelsList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        maxLiquidHeight = (int)Random.Range(height * 0.6f, height);

        List<Color> colors = DetermineColorScheme();
        CreatePixels(colors);
        DesignPotion(colors);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ClearAll();

            maxLiquidHeight = (int)Random.Range(height * 0.6f, height);
            List<Color> colors = DetermineColorScheme();

            CreatePixels(colors);
            DesignPotion(colors);

        }
    }

    private void DesignPotion(List<Color> colors)
    {
        bool isAddLid = Random.value > 0.5f;
        if (isAddLid)
        {
            int width = topWidth;
            int xMin = -width / 2;
            int xMax = xMin + width;

            for (int x = xMin; x < xMax; x++)
            {
                Vector3 position = new Vector3(x, height + 1, 0);

                GameObject pix = Instantiate(pixel, position, Quaternion.identity);
                pixelsList.Add(pix);

                SpriteRenderer sr = pix.GetComponent<SpriteRenderer>();
                sr.color = lidColor;
            }
        }


    }

    private void ClearAll()
    {
        for (int i = 0; i < pixelsList.Count; i++)
        {
            Destroy(pixelsList[i]);
        }
        pixelsList.Clear();
    }

    private List<Color> DetermineColorScheme()
    {
        List<Color> colors = new List<Color>();
        int colorCount = Random.Range(1, 3);

        for (int i = 0; i < colorCount; i++)
        {
            colors.Add(RandomColor());
        }

        return colors;
    }

    private void CreatePixels(List<Color> colors)
    {
        int darkerColorLevel = (int)Random.Range((height / 2) - (height * 0.3f), (height / 2) + (height * 0.3f));
        Vector3 currentWidthVectors = widthVectors[Random.Range(0, widthVectors.Count)];
        bottomWidth = (int)currentWidthVectors.x;
        middleWidth = (int)currentWidthVectors.y;
        topWidth = (int)currentWidthVectors.z;

        for (int level = 0; level <= height; level++)
        {
            int width = GetWidth(level);

            int xMin = -width / 2;
            int xMax = xMin + width;

            for (int x = xMin; x < xMax; x++)
            {
                Vector3 position = new Vector3(x, level, 0);

                GameObject pix = Instantiate(pixel, position, Quaternion.identity);
                pixelsList.Add(pix);

                SpriteRenderer sr = pix.GetComponent<SpriteRenderer>();

                if (x == xMin)
                {
                    sr.color = edgeColors[0];
                }
                else if (x == xMax - 1)
                {
                    sr.color = edgeColors[1];
                }
                else if (level == 0)
                {
                    sr.color = edgeColors[0];
                }
                else if (level > 0 && level < darkerColorLevel)
                {
                    sr.color = DarkenColor(colors[Random.Range(0, colors.Count)]);
                }
                else if (level >= maxLiquidHeight && x != xMax && x != xMin)
                {
                    Color tempColor = colors[Random.Range(0, colors.Count)];
                    tempColor.a = 0;
                    sr.color = tempColor;
                }
                else
                {
                    sr.color = colors[Random.Range(0, colors.Count)];
                }
            }

        }
    }

    private int GetWidth(int level)
    {
        float width;
        if (level == 0)
        {
            width = bottomWidth;
        }
        else if (level > 0 && level < height / 2)
        {
            width = Mathf.Lerp(bottomWidth, middleWidth, level / ((float)height / 2));
        }
        else if (level == height / 2)
        {
            width = middleWidth;
        }
        else if (level > height / 2 && level < height)
        {
            width = Mathf.Lerp(middleWidth, topWidth, level / (float)height);
        }
        else //if (level == height)
        {
            width = topWidth;
        }

        int returnWidth = (int)width;
        Debug.Log(returnWidth);
        if (returnWidth % 2 == 1)
        {
            returnWidth -= 1;
        }
        Debug.Log(returnWidth);
        return returnWidth;
    }

    private Color DarkenColor(Color color)
    {
        float colorChange = 0.8f;

        Color darkerColor = color;
        darkerColor.r *= colorChange;
        darkerColor.g *= colorChange;
        darkerColor.b *= colorChange;

        return darkerColor;
    }

    private Color RandomColor()
    {
        Color color = new Color();
        color.r = Random.value;
        color.g = Random.value;
        color.b = Random.value;
        color.a = 1;

        return color;
    }


    [DllImport("__Internal")]
    private static extern void DownloadFile(byte[] array, int byteLength, string fileName);

    [DllImport("__Internal")]
    private static extern void Hello();

    private IEnumerator DownloadScreenshot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D rawTexture = ScreenCapture.CaptureScreenshotAsTexture(1);

        float width = 0.4f;
        float height = 0.8f;

        int xPos = (int)(Camera.main.pixelWidth * ((1 - width) / 2f));
        int yPos = (int)(Camera.main.pixelHeight * ((1 - height) / 2f));
        int xSize = (int)(Camera.main.pixelWidth * width);
        int ySize = (int)(Camera.main.pixelHeight * height);
        Color[] c = rawTexture.GetPixels(xPos, yPos, xSize, ySize);

        Texture2D croppedTexture = new Texture2D(xSize, ySize);
        croppedTexture.SetPixels(c);
        croppedTexture.Apply();

        croppedTexture = RemoveBackgroundColor(croppedTexture);
        /*
        GameObject test = new GameObject("texture");
        SpriteRenderer test_sr = test.AddComponent<SpriteRenderer>();
        test_sr.sprite = Sprite.Create(
            croppedTexture,
            new Rect(0.0f, 0.0f, croppedTexture.width, croppedTexture.height),
            new Vector2(0.5f, 0.5f));
        */

        byte[] textureBytes = croppedTexture.EncodeToPNG();
        DownloadFile(textureBytes, textureBytes.Length, "Potion.png");
        Destroy(rawTexture);
    }

    private Texture2D RemoveBackgroundColor(Texture2D texture)
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color currentColor = texture.GetPixel(x, y);
                if (CompareColors(currentColor, new Color32(31, 32, 64, 255)))
                {
                    texture.SetPixel(x, y, new Color32(0, 0, 0, 0));
                }
            }

        }

        texture.Apply();
        return texture;
    }

    public void SaveFileButton()
    {
        StartCoroutine(DownloadScreenshot());
    }

    private static bool CompareColors(Color32 color1, Color32 color2)
    {
        int rdiff = Mathf.Abs(color1.r - color2.r);
        int gdiff = Mathf.Abs(color1.g - color2.g);
        int bdiff = Mathf.Abs(color1.b - color2.b);
        int adiff = Mathf.Abs(color1.a - color2.a);

        return rdiff + gdiff + bdiff + adiff < maxColorDifference;
    }





}
