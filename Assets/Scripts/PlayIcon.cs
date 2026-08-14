using UnityEngine;

// Sprite de triángulo "▶" generado por código — mismo motivo que StarIcon:
// el glyph de texto no está garantizado en la fuente TMP por defecto y se
// renderiza como tofu. Usado en el botón de escuchar pronunciación de
// BeginnerTurnController.
public static class PlayIcon
{
    static Sprite cached;

    public static Sprite GetSprite()
    {
        if (cached != null) return cached;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];

        // Triángulo equilátero apuntando a la derecha, centrado con un
        // pequeño desplazamiento a la izquierda para que el centroide visual
        // quede centrado en el botón (la punta "pesa" menos que la base).
        Vector2 a = new Vector2(size * 0.28f, size * 0.16f);
        Vector2 b = new Vector2(size * 0.28f, size * 0.84f);
        Vector2 c = new Vector2(size * 0.82f, size * 0.50f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float aa = TriangleCoverage(p, a, b, c);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(aa * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return cached;
    }

    // 0/1 con un borde de 1px de anti-aliasing en vez de un test binario —
    // evita dientes de sierra visibles en el borde diagonal del triángulo.
    static float TriangleCoverage(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 centroid = (a + b + c) / 3f;
        float d0 = SignedInsideDistance(p, a, b, centroid);
        float d1 = SignedInsideDistance(p, b, c, centroid);
        float d2 = SignedInsideDistance(p, c, a, centroid);
        float minD = Mathf.Min(d0, Mathf.Min(d1, d2));
        return Mathf.Clamp01(minD + 0.5f);
    }

    // Distancia con signo a la recta v0-v1, positiva hacia "inside" — sin
    // esto, según el orden de los vértices, la normal podía apuntar hacia
    // AFUERA del triángulo y todo el interior salía con cobertura 0 (el
    // triángulo se dibujaba invisible, solo el cuadrado de fondo del botón).
    static float SignedInsideDistance(Vector2 p, Vector2 v0, Vector2 v1, Vector2 inside)
    {
        Vector2 edge = v1 - v0;
        Vector2 normal = new Vector2(-edge.y, edge.x).normalized;
        if (Vector2.Dot(inside - v0, normal) < 0f) normal = -normal;
        return Vector2.Dot(p - v0, normal);
    }
}
