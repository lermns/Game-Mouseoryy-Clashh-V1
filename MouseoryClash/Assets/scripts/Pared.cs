using UnityEngine;
using UnityEngine.UIElements;

public class Pared : MonoBehaviour
{
    [Header("Configuración del coliseo")]
    public Vector3 centro = new Vector3(2f, 2f, 1.7f);
    public float radio = 8.5f;
    public int cantidadSegmentos = 25;
    public float grosorMuro = 1f;
    public float alturaMuro = 2f;

    void Start()
    {
        CrearMurallaCircular();
    }

    void CrearMurallaCircular()
    {
        for (int i = 0; i < cantidadSegmentos; i++)
        {
            float angulo = i * Mathf.PI * 2 / cantidadSegmentos;
            float x = Mathf.Cos(angulo) * radio + centro.x;
            float z = Mathf.Sin(angulo) * radio + centro.z;
            Vector3 posicion = new Vector3(x, centro.y + alturaMuro / 2f, z);

            GameObject muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
            muro.transform.position = posicion;

            // Apuntar hacia el centro del coliseo
            muro.transform.LookAt(new Vector3(centro.x, muro.transform.position.y, centro.z));
            muro.transform.Rotate(0, 90, 0); // Girar 90 grados para que el cubo apunte "de lado"

            // Escalarlo para formar el muro
            muro.transform.localScale = new Vector3(grosorMuro, alturaMuro, (2 * Mathf.PI * radio) / cantidadSegmentos);

            muro.GetComponent<MeshRenderer>().enabled = false; // Hacerlo invisible si quieres
            muro.name = $"Muro_{i}";
            muro.transform.parent = this.transform; // Para mantener jerarquía limpia
        }
    }
}
