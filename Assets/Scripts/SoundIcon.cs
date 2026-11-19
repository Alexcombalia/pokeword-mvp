using UnityEngine;

// Altavoz dibujado por código, en dos versiones: con ondas (sonando) y con
// aspa (silenciado). Mismo motivo que StarIcon y PlayIcon — los glyphs "🔊" y
// "♪" no están en la fuente TMP por defecto y salen como un cuadrado vacío.
//
// Trabaja en coordenadas normalizadas [-1,1] para que el dibujo no dependa del
// tamaño de la textura, y todo se mide con distancias a formas (no con tests
// binarios) para tener el borde suavizado — a 48 px en pantalla, un altavoz
// con dientes de sierra se ve sucio.
public static class SoundIcon
{
    static Sprite cachedOn, cachedOff;

    public static Sprite GetSprite(bool muted)
    {
        if (muted && cachedOff != null) return cachedOff;
        if (!muted && cachedOn  != null) return cachedOn;

        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // +0.5 para muestrear el centro del píxel, no su esquina
                float u = (x + 0.5f) / size * 2f - 1f;
                float v = (y + 0.5f) / size * 2f - 1f;
                var p = new Vector2(u, v);

                float a = SpeakerCoverage(p);

                if (muted)
                {
                    // Aspa a la derecha, donde estarían las ondas. Se prefiere
                    // al típico tachón diagonal completo porque no cruza por
                    // encima del altavoz: a tamaño pequeño, el tachón y el cono
                    // se emborronan en una sola mancha.
                    a = Mathf.Max(a, StrokeCoverage(p, new Vector2(0.34f, -0.30f), new Vector2(0.80f,  0.30f), 0.085f));
                    a = Mathf.Max(a, StrokeCoverage(p, new Vector2(0.34f,  0.30f), new Vector2(0.80f, -0.30f), 0.085f));
                }
                else
                {
                    a = Mathf.Max(a, ArcCoverage(p, 0.50f, 0.085f));
                    a = Mathf.Max(a, ArcCoverage(p, 0.78f, 0.085f));
                }

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        if (muted) cachedOff = sprite; else cachedOn = sprite;
        return sprite;
    }

    // Caja + cono del altavoz, como un solo cuerpo.
    static float SpeakerCoverage(Vector2 p)
    {
        const float aa = 45f;   // ancho del borde suavizado, en unidades de [-1,1]

        // Caja: rectángulo bajito a la izquierda.
        float box = Mathf.Min(
            Mathf.Min(p.x - (-0.62f), (-0.22f) - p.x),
            Mathf.Min(p.y - (-0.26f), 0.26f - p.y));

        // Cono: se ensancha de -0.22 a 0.16. La media altura crece de 0.26 a
        // 0.70, así que el borde inclinado sale de interpolar, no de un
        // triángulo aparte.
        float cone = -1f;
        if (p.x >= -0.62f && p.x <= 0.16f)
        {
            float t = Mathf.InverseLerp(-0.22f, 0.16f, p.x);
            float half = Mathf.Lerp(0.26f, 0.70f, Mathf.Max(0f, t));
            cone = Mathf.Min(
                Mathf.Min(p.x - (-0.62f), 0.16f - p.x),
                half - Mathf.Abs(p.y));
        }

        return Mathf.Clamp01(Mathf.Max(box, cone) * aa + 0.5f);
    }

    // Arco a la derecha del cono: anillo de radio `radius` recortado al sector
    // de ±55°, para que sea una onda y no una circunferencia entera.
    static float ArcCoverage(Vector2 p, float radius, float thickness)
    {
        if (p.x <= 0.22f) return 0f;
        float d = p.magnitude;
        float ring = thickness * 0.5f - Mathf.Abs(d - radius);

        float ang = Mathf.Atan2(p.y, p.x);           // 0 = derecha
        float sector = (55f * Mathf.Deg2Rad) - Mathf.Abs(ang);

        return Mathf.Clamp01(Mathf.Min(ring, sector) * 45f + 0.5f);
    }

    // Trazo recto de a a b con grosor `thickness`, para el aspa.
    static float StrokeCoverage(Vector2 p, Vector2 a, Vector2 b, float thickness)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab));
        float dist = Vector2.Distance(p, a + ab * t);
        return Mathf.Clamp01((thickness * 0.5f - dist) * 45f + 0.5f);
    }
}
