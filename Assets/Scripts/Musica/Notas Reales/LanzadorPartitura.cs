using UnityEngine;

public class LanzadorPartitura : MonoBehaviour
{
    public PartituraMML partitura;
    public ReproductorPartituras reproductor;

    void Start()
    {
        var notas = partitura.ObtenerNotas();
        reproductor.Iniciar(notas);
    }

}
