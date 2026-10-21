using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Synth7Notas : MonoBehaviour
{
    public AudioClip doBase, reBase, miBase, faBase, solBase, laBase, siBase;

    private Dictionary<string, (AudioClip clip, int midi)> baseNotas;

    // Pool de AudioSources
    private List<AudioSource> pool = new List<AudioSource>();
    public int poolSize = 32;
    public float volumen = 1f; // 0 = silencio, 1 = volumen original

    void Start()
    {
        Inicializar();
        CrearPool();
    }

    // -----------------------------
    //   INICIALIZAR NOTAS BASE
    // -----------------------------
    private void Inicializar()
    {
        baseNotas = new Dictionary<string, (AudioClip clip, int midi)>();

        // Notas naturales base
        var naturales = new Dictionary<string, (AudioClip, int)>
    {
        { "c", (doBase, 60) },
        { "d", (reBase, 62) },
        { "e", (miBase, 64) },
        { "f", (faBase, 65) },
        { "g", (solBase, 67) },
        { "a", (laBase, 69) },
        { "b", (siBase, 71) }
    };

        // Generar TODAS las notas MIDI (0–127)
        for (int midi = 0; midi <= 127; midi++)
        {
            int octava = (midi / 12) - 1;
            int semitono = midi % 12;

            string nombre = semitono switch
            {
                0 => "c",
                1 => "c#",
                2 => "d",
                3 => "d#",
                4 => "e",
                5 => "f",
                6 => "f#",
                7 => "g",
                8 => "g#",
                9 => "a",
                10 => "a#",
                11 => "b",
                _ => "c"
            };

            string natural = nombre.Replace("#", "");
            var baseData = naturales[natural];

            baseNotas[nombre + octava] = (baseData.Item1, baseData.Item2);
        }
    }



    // -----------------------------
    //   POOL DE AUDIOSOURCES
    // -----------------------------
    void CrearPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0;
            pool.Add(src);
        }
    }

    AudioSource PedirSource()
    {
        foreach (var src in pool)
            if (!src.isPlaying)
            {
                src.volume = volumen;
                return src;
            }

        var nuevo = gameObject.AddComponent<AudioSource>();
        nuevo.playOnAwake = false;
        nuevo.spatialBlend = 0;
        nuevo.volume = volumen;
        pool.Add(nuevo);
        return nuevo;
    }


    // -----------------------------
    //   PRECALCULAR NOTA
    // -----------------------------
    public (AudioClip, float) Precalcular(string nombre, int octava)
    {
        if (baseNotas == null || baseNotas.Count == 0)
            Inicializar();

        string clave = nombre + octava;

        if (!baseNotas.ContainsKey(clave))
        {
            Debug.LogWarning("Nota fuera del rango de 32 notas: " + clave);
            return (null, 1f);
        }

        var baseData = baseNotas[clave];

        int midiBase = baseData.midi;
        int midiObjetivo = ConvertirANumeroMidi(nombre, octava);

        float pitch = Mathf.Pow(2f, (midiObjetivo - midiBase) / 12f);

        return (baseData.clip, pitch);
    }


    // -----------------------------
    //   TOCAR NOTA PROGRAMADA
    // -----------------------------
    public void TocarNotaProgramada(Nota n, double dspInicio)
    {
        AudioSource src = PedirSource();
        src.clip = n.clip;
        src.pitch = n.pitch;
        src.spatialBlend = 0;
        src.volume = volumen;

        src.PlayScheduled(dspInicio);

        // Duración real del clip ajustada por pitch
        float durReal = (src.clip.length / src.pitch) + 0.05f;

        StartCoroutine(DevolverTrasTiempo(src, durReal));
    }

    IEnumerator DevolverTrasTiempo(AudioSource src, float t)
    {
        yield return new WaitForSeconds(t);

        // Seguridad extra: no devolver si sigue sonando
        if (!src.isPlaying)
            src.Stop();
    }

    // -----------------------------
    //   MIDI
    // -----------------------------
    private int ConvertirANumeroMidi(string nota, int oct) =>
        (oct + 1) * 12 + (nota switch
        {
            "c" => 0,
            "c#" => 1,
            "d" => 2,
            "d#" => 3,
            "e" => 4,
            "f" => 5,
            "f#" => 6,
            "g" => 7,
            "g#" => 8,
            "a" => 9,
            "a#" => 10,
            "b" => 11,
            _ => 0
        });
}
