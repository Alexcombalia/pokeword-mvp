using UnityEngine;

// Sprite de estrella generado por código — evita depender del glyph "★" en
// la fuente TMP por defecto, que no lo incluye y se renderiza como un
// cuadrado vacío ("tofu"). Se usa tanto en LevelResultUI como en LevelSelectUI.
public static class StarIcon
{
    static Sprite cached;

    public static Sprite GetSprite()
    {
        if (cached != null) return cached;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        float outerR = size * 0.48f;
        float innerR = outerR * 0.42f;
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - center;
                float dist  = p.magnitude;
                float angle = Mathf.Atan2(p.y, p.x) - Mathf.PI / 2f; // 0 = arriba
                if (angle < 0) angle += Mathf.PI * 2f;

                float sector      = angle / (Mathf.PI * 2f / 10f); // 10 sectores (5 puntas)
                int   sectorIndex = Mathf.FloorToInt(sector);
                float t           = sector - sectorIndex;
                float rA = (sectorIndex % 2 == 0) ? outerR : innerR;
                float rB = (sectorIndex % 2 == 0) ? innerR : outerR;
                float edgeR = Mathf.Lerp(rA, rB, t);

                float aa = Mathf.Clamp01((edgeR - dist) * 2.2f + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(aa * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return cached;
    }
}
