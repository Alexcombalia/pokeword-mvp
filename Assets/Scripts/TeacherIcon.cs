using UnityEngine;

// Silueta de docente —cabeza con birrete y hombros— dibujada por código, misma
// familia que StarIcon, PlayIcon y SoundIcon.
//
// El birrete es lo que hace el trabajo: una cabeza sola es "una persona", y el
// botón tiene que decir "profesor" de un vistazo, sin leer la etiqueta. Se
// dibuja como silueta llena y no con rasgos de cara porque a 100 px unos ojos
// y una boca se emborronan en una mancha sucia.
//
// Si algún día hay un retrato de verdad, basta con dejar el PNG en
// Assets/Resources/teacher_face.png: TeacherPanel lo prefiere a esto.
public static class TeacherIcon
{
    static Sprite cached;

    public static Sprite GetSprite()
    {
        if (cached != null) return cached;

        const int size = 192;   // más resolución que los otros iconos: tiene más detalle
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size * 2f - 1f;
                float v = (y + 0.5f) / size * 2f - 1f;
                var p = new Vector2(u, v);

                float a = 0f;

                // Hombros: elipse muy baja, de la que solo asoma el arco de
                // arriba. Da el remate de busto sin dibujar un cuerpo.
                a = Mathf.Max(a, Ellipse(p, new Vector2(0f, -1.18f), new Vector2(0.92f, 0.74f)));

                // Cuello, para que la cabeza no flote sobre los hombros.
                a = Mathf.Max(a, Box(p, new Vector2(0f, -0.36f), new Vector2(0.14f, 0.16f)));

                // Cabeza.
                a = Mathf.Max(a, Ellipse(p, new Vector2(0f, 0.00f), new Vector2(0.35f, 0.38f)));

                // Copa del birrete, apoyada en la cabeza.
                a = Mathf.Max(a, Ellipse(p, new Vector2(0f, 0.30f), new Vector2(0.34f, 0.20f)));

                // Tabla del birrete: rombo. Ancha a propósito — es la pieza que
                // identifica el icono y tiene que verse a tamaño pequeño.
                a = Mathf.Max(a, Diamond(p, new Vector2(0f, 0.46f), 0.82f, 0.22f));

                // Borla: cuerda a la derecha y nudo al final.
                a = Mathf.Max(a, Stroke(p, new Vector2(0.62f, 0.44f), new Vector2(0.72f, 0.06f), 0.065f));
                a = Mathf.Max(a, Ellipse(p, new Vector2(0.72f, -0.02f), new Vector2(0.10f, 0.12f)));

                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255));
            }
        }

        tex.SetPixels32(pixels);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.Apply();

        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        return cached;
    }

    // Todas devuelven cobertura 0..1 con borde suavizado: a este tamaño, un
    // test binario deja dientes de sierra muy visibles en las curvas.
    const float AA = 40f;

    static float Ellipse(Vector2 p, Vector2 c, Vector2 r)
    {
        Vector2 d = new Vector2((p.x - c.x) / r.x, (p.y - c.y) / r.y);
        // Distancia aproximada al borde, reescalada al radio menor para que el
        // suavizado tenga el mismo grosor en elipses alargadas.
        float k = Mathf.Min(r.x, r.y);
        return Mathf.Clamp01((1f - d.magnitude) * k * AA + 0.5f);
    }

    static float Box(Vector2 p, Vector2 c, Vector2 half)
    {
        float dx = half.x - Mathf.Abs(p.x - c.x);
        float dy = half.y - Mathf.Abs(p.y - c.y);
        return Mathf.Clamp01(Mathf.Min(dx, dy) * AA + 0.5f);
    }

    static float Diamond(Vector2 p, Vector2 c, float halfW, float halfH)
    {
        float d = Mathf.Abs(p.x - c.x) / halfW + Mathf.Abs(p.y - c.y) / halfH;
        return Mathf.Clamp01((1f - d) * Mathf.Min(halfW, halfH) * AA + 0.5f);
    }

    static float Stroke(Vector2 p, Vector2 a, Vector2 b, float thickness)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab));
        float dist = Vector2.Distance(p, a + ab * t);
        return Mathf.Clamp01((thickness * 0.5f - dist) * AA + 0.5f);
    }
}
