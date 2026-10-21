using System.Collections.Generic;
using UnityEngine;

public class PartituraTemplate : MonoBehaviour
{
    [TextArea(5, 20)]
    public string textoPartitura;

    public List<Nota> Parsear()
    {
        List<Nota> notas = new List<Nota>();

        string[] lineas = textoPartitura.Split('\n');

        foreach (string linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea))
                continue;

            string[] partes = linea.Trim().Split(' ');

            if (partes.Length < 3)
                continue;

            string nombre = partes[0].ToLower();   // c, d#, a, r
            int octava = nombre == "r" ? 0 : int.Parse(nombre.Substring(nombre.Length - 1).ToString());
            nombre = nombre == "r" ? "r" : nombre.Substring(0, nombre.Length - 1);

            float inicio = float.Parse(partes[1]);
            float duracion = float.Parse(partes[2]);

            Nota n = new Nota();
            n.nombre = nombre;
            n.octava = octava;
            n.inicio = inicio;
            n.duracion = duracion;

            notas.Add(n);
        }

        return notas;
    }
}
