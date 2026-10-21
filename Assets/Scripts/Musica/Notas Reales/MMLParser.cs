using System.Collections.Generic;
using UnityEngine;

public class MMLParser
{
    private float tempo = 120f;
    private Synth7Notas synth;

    public float cuantizacion = 0f;   // 0 = sin cuantizar
    public float duracionMinima = 0.02f; // evita notas imposibles
    public bool normalizar = true;    // ajusta micro-errores acumulados

    public MMLParser(Synth7Notas s)
    {
        synth = s;
    }

    public List<Nota> Parsear(string mml)
    {
        List<Nota> notas = new List<Nota>();

        // Limpieza suave
        mml = mml.ToLower()
                 .Replace(" ", "")
                 .Replace("\n", "")
                 .Replace("\r", "");

        if (mml.StartsWith("7ml@"))
            mml = mml.Substring(4);

        int i = 0;
        int octava = 4;
        float tiempoPista = 0f;
        int longitudDefault = 4;

        while (i < mml.Length)
        {
            char c = mml[i];

            if (c == ',')
            {
                tiempoPista = 0f;
                octava = 4;
                longitudDefault = 4;
                i++; continue;
            }

            if (c == '<') { octava--; i++; continue; }
            if (c == '>') { octava++; i++; continue; }

            if (c == 'o') { i++; octava = (int)LeerNumero(mml, ref i); continue; }
            if (c == 't') { i++; tempo = LeerNumero(mml, ref i); continue; }
            if (c == 'l') { i++; longitudDefault = (int)LeerNumero(mml, ref i); continue; }

            if ((c >= 'a' && c <= 'g') || c == 'r')
            {
                string nombre = c.ToString();
                i++;

                if (i < mml.Length && (mml[i] == '#' || mml[i] == '+'))
                {
                    nombre += "#";
                    i++;
                }

                float figura = LeerNumero(mml, ref i);
                if (figura <= 0) figura = longitudDefault;

                float dur = (60f / tempo) * (4f / figura);

                if (i < mml.Length && mml[i] == '.')
                {
                    dur *= 1.5f;
                    i++;
                }

                // 🔹 Duración mínima para evitar notas imposibles
                if (dur < duracionMinima)
                    dur = duracionMinima;

                // 🔹 Cuantización opcional
                if (cuantizacion > 0f)
                {
                    dur = Mathf.Round(dur / cuantizacion) * cuantizacion;
                }

                if (nombre != "r")
                {
                    var n = new Nota
                    {
                        nombre = nombre,
                        octava = octava,
                        inicio = tiempoPista,
                        duracion = dur
                    };

                    (n.clip, n.pitch) = synth.Precalcular(nombre, octava);
                    notas.Add(n);
                }

                tiempoPista += dur;
                continue;
            }

            i++;
        }

        // 🔹 Normalización opcional (evita acumulación de errores)
        if (normalizar)
        {
            for (int k = 0; k < notas.Count; k++)
            {
                notas[k].inicio = Mathf.Round(notas[k].inicio * 1000f) / 1000f;
                notas[k].duracion = Mathf.Round(notas[k].duracion * 1000f) / 1000f;
            }
        }

        notas.Sort((a, b) => a.inicio.CompareTo(b.inicio));
        return notas;
    }

    private float LeerNumero(string s, ref int i)
    {
        string num = "";
        while (i < s.Length && char.IsDigit(s[i]))
        {
            num += s[i];
            i++;
        }
        return num == "" ? 0 : float.Parse(num);
    }
}
