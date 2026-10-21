using UnityEngine;
using System.Collections.Generic;

public class PartituraMML : MonoBehaviour
{
    [TextArea(5, 20)]
    public string textoMML;

    public Synth7Notas synth;

    public List<Nota> ObtenerNotas()
    {
        if (synth == null)
        {
            Debug.LogError("ERROR: PartituraMML no tiene asignado el Synth7Notas.");
            return new List<Nota>();
        }

        MMLParser parser = new MMLParser(synth);
        return parser.Parsear(textoMML);
    }

}
