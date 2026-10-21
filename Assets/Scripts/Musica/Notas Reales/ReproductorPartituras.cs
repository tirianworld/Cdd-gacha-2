using UnityEngine;
using System.Collections.Generic;

public class ReproductorPartituras : MonoBehaviour
{
    public bool loop = true;

    private List<Nota> notas;
    private double dspInicio;
    private bool reproduciendo = false;
    private int indiceNota = 0;

    public Synth7Notas synth;

    public void Iniciar(List<Nota> lista)
    {
        notas = lista;
        indiceNota = 0;
        dspInicio = AudioSettings.dspTime;
        reproduciendo = true;
    }

    void Update()
    {
        if (!reproduciendo || notas == null || notas.Count == 0)
            return;

        double dspActual = AudioSettings.dspTime;
        double tiempoRel = dspActual - dspInicio;

        while (indiceNota < notas.Count && notas[indiceNota].inicio <= tiempoRel)
        {
            var n = notas[indiceNota];
            synth.TocarNotaProgramada(n, dspActual + 0.01);
            indiceNota++;
        }

        if (indiceNota >= notas.Count)
        {
            if (loop)
            {
                dspInicio = AudioSettings.dspTime;
                indiceNota = 0;
            }
            else
            {
                reproduciendo = false;
            }
        }
    }
}
