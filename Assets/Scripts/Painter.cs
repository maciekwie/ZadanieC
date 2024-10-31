using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class Painter : MonoBehaviour
{
    public Image image;
    public RectTransform imageTransform;

    public RenderTexture renderTexture;
    Texture2D outputTexture;

    public Material mat;
    Vector3 previousDrawingPos;
    Vector3 currentDrawingPos;
    Vector3 mousePos;

    public Color penColor;
    public float penSize;
    public Color canvasColor;

    bool isDrawing = false;
    bool clearRequest = false;

    public float drawTime = 0;
    public bool shapeClassified = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //renderTexture = new RenderTexture((int)imageTransform.sizeDelta.x, (int)imageTransform.sizeDelta.y, 8);
        renderTexture = new RenderTexture((int)imageTransform.rect.width, (int)imageTransform.rect.height, 8);
        GetComponent<Camera>().targetTexture = renderTexture;

        outputTexture = new Texture2D(renderTexture.width, renderTexture.height);

        image.sprite = Sprite.Create(outputTexture, new Rect(0, 0, outputTexture.width, outputTexture.height), new Vector2(0, 0));

        currentDrawingPos = Vector3.zero;

        ClearCanvas();
    }

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }


    void OnPostRenderCallback(Camera camera)
    {
        OnPostRender();
    }

    public void ClearCanvas()
    {
        clearRequest = true;
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Input.mousePosition;
        
        if(Input.GetMouseButton(0))
        {
            Vector3 position = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
            if(isDrawing == false)
            {
                previousDrawingPos = position;
                currentDrawingPos = position;
                isDrawing = true;
            }
            else
            {
                previousDrawingPos = currentDrawingPos;
                currentDrawingPos = position;
            }

            drawTime = Time.time;
            shapeClassified = false;
        }
        else
        {
            if(isDrawing == true) 
            {
                isDrawing = false;
            }
        }

        
        //RenderTexture.active = renderTexture

        if(clearRequest)
        {
            for(int x = 0; x < outputTexture.width; x++)
            {
                for(int y = 0; y < outputTexture.height; y++)
                {
                    outputTexture.SetPixel(x, y, canvasColor);
                }
            }

            clearRequest = false;
        }
        else
        {
            outputTexture.ReadPixels(new Rect(imageTransform.anchoredPosition.x,
                    -imageTransform.anchoredPosition.y,
                    imageTransform.rect.width,
                    imageTransform.rect.height),
                    0, 0);
        }
        outputTexture.Apply();
    }


    void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        // Draw a texture tex inside a rect
        Material guiTexMat = new Material(
            Shader.Find("Hidden/Internal-GUITexture"));

        //GL.Clear(false, true, Color.white);

        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();

        GL.Begin(GL.QUADS);
        guiTexMat.SetTexture("_MainTex", outputTexture);
        guiTexMat.SetPass(0);

        Rect rect = new Rect(imageTransform.anchoredPosition.x / Screen.width,
            1 - (imageTransform.anchoredPosition.y / Screen.height),
            imageTransform.rect.width / Screen.width,
            -imageTransform.rect.height / Screen.height);

        GL.TexCoord2(0, 1);
        GL.Vertex3(rect.x, rect.y, 0);
        GL.TexCoord2(0, 0);
        GL.Vertex3(rect.x, rect.y + rect.height, 0);
        GL.TexCoord2(1, 0);
        GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0);
        GL.TexCoord2(1, 1);
        GL.Vertex3(rect.x + rect.width, rect.y, 0);
        GL.End();

        if(isDrawing)
        {
            DrawThickLine(previousDrawingPos, new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0), penSize, penColor);
        }

        GL.PopMatrix();
    }

    public Texture2D GetScaledImage(int targetWidth, int targetHeight)
    {
        // Create a RenderTexture with the desired dimensions
        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Bilinear;

        // Backup the currently active RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the RenderTexture as active
        RenderTexture.active = rt;

        // Copy the source texture to the RenderTexture
        Graphics.Blit(outputTexture, rt);

        // Create a new Texture2D with the desired dimensions
        Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

        // Read the RenderTexture contents into the Texture2D
        resizedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        resizedTexture.Apply();

        // Restore the previously active RenderTexture
        RenderTexture.active = previous;

        // Clean up
        rt.Release();

        // Return the resized texture
        return resizedTexture;
    }


    public void DrawThickLine(Vector3 start, Vector3 end, float thickness, Color color)
    {
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);

        // Calculate the direction and perpendicular vectors
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Camera.main.transform.forward).normalized * thickness * 1 / Screen.width;

        // Calculate the corner points of the quad
        Vector3 v1 = start - perpendicular - direction * thickness * 1 / Screen.width;
        Vector3 v2 = start + perpendicular - direction * thickness * 1 / Screen.width;;
        Vector3 v3 = end + perpendicular + direction * thickness * 1 / Screen.width;;
        Vector3 v4 = end - perpendicular + direction * thickness * 1 / Screen.width;;

        // First triangle
        GL.Vertex(v1);
        GL.Vertex(v2);
        GL.Vertex(v3);

        // Second triangle
        GL.Vertex(v3);
        GL.Vertex(v4);
        GL.Vertex(v1);

        GL.End();
    }
}

