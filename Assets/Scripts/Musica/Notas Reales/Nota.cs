using UnityEngine;

[System.Serializable]
public class Nota
{
    public string nombre;
    public int octava;
    public float inicio;
    public float duracion;

    // Nuevos campos optimizados
    public float pitch;
    public AudioClip clip;
}
