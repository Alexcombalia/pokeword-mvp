using UnityEngine;

// Sprite de rectángulo redondeado generado por código, con 9-slice para que
// se pueda estirar a cualquier tamaño de burbuja sin deformar las esquinas.
// Mismo enfoque que StarIcon.cs — evita depender de un sprite importado.
public static class RoundedRectSprite
{
    static Sprite cached;

    public static Sprite GetSprite()
    {
        if (cached != null) return cached;

        const int size   = 64;
        const int radius = 20;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = 1f;
                int cx = -1, cy = -1;

                if (x < radius && y < radius) { cx = radius; cy = radius; }
                else if (x >= size - radius && y < radius) { cx = size - radius; cy = radius; }
                else if (x < radius && y >= size - radius) { cx = radius; cy = size - radius; }
                else if (x >= size - radius && y >= size - radius) { cx = size - radius; cy = size - radius; }

                if (cx >= 0)
                {
                    float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    alpha = Mathf.Clamp01((radius - dist) + 0.5f);
                }

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return cached;
    }
}
