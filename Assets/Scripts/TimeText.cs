using UnityEngine;

// Los tiempos del juego se guardan en segundos porque es lo que cuenta el
// reloj, pero "menos de 1200s" no le dice nada a nadie: hay que dividir
// mentalmente para saber si son dos minutos o veinte. Todo lo que ve el
// jugador pasa por aquí y sale en minutos.
//
// Siempre en formato reloj: "20:00 min", "2:30 min". Ni "1200s", que obliga a
// dividir, ni "8 min 12 s", que se lee como si los 12 segundos sobraran de
// algo. Tampoco se redondea a minutos enteros: un umbral de 150s puesto como
// "3 min" prometería medio minuto que no existe.
public static class TimeText
{
    // "20:00 min", "2:30 min", "0:45 min"
    public static string Format(float seconds)
    {
        int total = Mathf.Max(0, Mathf.RoundToInt(seconds));
        return string.Format("{0}:{1:00} min", total / 60, total % 60);
    }

    // Umbral de estrella: se redondea hacia arriba al segundo, nunca por
    // debajo, para no prometer menos tiempo del que de verdad hay.
    public static string Threshold(float seconds) => Format(Mathf.Ceil(seconds));

    // Tiempo consumido: hacia abajo, el segundo empezado no cuenta.
    public static string Elapsed(float seconds) => Format(Mathf.Floor(seconds));
}
