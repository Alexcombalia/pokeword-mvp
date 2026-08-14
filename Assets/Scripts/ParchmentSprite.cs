using UnityEngine;

// Textura de papel envejecido generada por código: color base cálido +
// variación de brillo por ruido Perlin (dos frecuencias, para veteado
// grueso y manchas finas) y un oscurecimiento sutil hacia los bordes
// ("quemado"). Mismo 9-slice de esquinas redondeadas que RoundedRectSprite.cs,
// para poder estirarlo a cualquier tamaño de panel sin deformar las esquinas.
//
// No es una textura pintada real (esto no puede generar imágenes, solo
// píxeles por código) — es una aproximación plana/ilustrada al estilo
// pergamino, no fotorrealista.
public static class ParchmentSprite
{
    static Sprite cachedPaper;
    static Sprite cachedInk;

    public static Sprite GetPaper()
    {
        if (cachedPaper != null) return cachedPaper;
        cachedPaper = Build(new Color(0.851f, 0.769f, 0.616f), edgeDarken: 0.30f, noisy: true);
        return cachedPaper;
    }

    // Variante oscura y lisa (sin veteado) — para bordes tipo cuero/tinta
    // alrededor del papel.
    public static Sprite GetInk()
    {
        if (cachedInk != null) return cachedInk;
        cachedInk = Build(new Color(0.267f, 0.184f, 0.114f), edgeDarken: 0f, noisy: false);
        return cachedInk;
    }

    static Sprite Build(Color baseColor, float edgeDarken, bool noisy)
    {
        const int size   = 128;
        const int radius = 26;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        float seedX = Random.Range(0f, 1000f);
        float seedY = Random.Range(0f, 1000f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float shade = 1f;
                if (noisy)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.035f + seedX, y * 0.035f + seedY);
                    float n2 = Mathf.PerlinNoise(x * 0.12f + seedY, y * 0.12f + seedX);
                    shade += (n1 - 0.5f) * 0.16f + (n2 - 0.5f) * 0.10f;
                }

                if (edgeDarken > 0f)
                {
                    float dx = Mathf.Abs(x - size * 0.5f) / (size * 0.5f);
                    float dy = Mathf.Abs(y - size * 0.5f) / (size * 0.5f);
                    float edge = Mathf.Max(dx, dy);
                    shade -= Mathf.Clamp01((edge - 0.55f) / 0.45f) * edgeDarken;
                }

                float r = Mathf.Clamp01(baseColor.r * shade);
                float g = Mathf.Clamp01(baseColor.g * shade);
                float b = Mathf.Clamp01(baseColor.b * shade);

                // Alpha por esquina redondeada (9-slice) — igual técnica que RoundedRectSprite.
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

                pixels[y * size + x] = new Color32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(alpha * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
