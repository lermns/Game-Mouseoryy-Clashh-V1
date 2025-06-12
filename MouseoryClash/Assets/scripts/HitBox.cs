using UnityEngine;
using System.Collections.Generic; // Necesario para HashSet

public class HitBox : MonoBehaviour
{
    [Header("Configuraci�n de HitBox")] // Agregado para mejor organizaci�n
    public int da�o = 10;
    public string oponenteBodyTag; // Asigna "Player1Body" o "Player2Body" en el Inspector para cada HitBox

    // Referencia al GameObject del propio personaje que posee este HitBox
    private GameObject ownerBody;

    private bool activo = false;
    private HashSet<GameObject> objetosGolpeados = new HashSet<GameObject>(); // Para evitar golpear el mismo objeto m�ltiples veces por ataque

    // M�todo para que el script Body asigne la referencia a su propio cuerpo
    public void SetOwnerBody(GameObject body)
    {
        ownerBody = body;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activo) return; // Si el hitbox no est� activo, ignorar la colisi�n

        // **IMPORTANTE: Filtro 1 - No me golpeo a m� mismo**
        // Comprueba si el objeto colisionado es el mismo que el "ownerBody" o un hijo de ownerBody
        // Esto es crucial si el Rigidbody del ownerBody puede colisionar con el Rigidbody del HitBox.
        // Si el HitBox SIEMPRE est� en un objeto hijo de ownerBody, la segunda parte es m�s robusta.
        if (ownerBody != null && (other.gameObject == ownerBody || other.transform.IsChildOf(ownerBody.transform)))
        {
            // Debug.Log("HitBox: Colisi�n detectada con mi propio cuerpo o parte de mi cuerpo. Ignorando da�o.");
            return; // Ignora la colisi�n si es mi propio cuerpo o un hijo
        }

        // **Filtro 2 - Solo golpeo al oponente**
        // Solo procesa la colisi�n si el tag coincide con el oponenteBodyTag
        if (other.CompareTag(oponenteBodyTag))
        {
            // Buscar el componente Vida en el GameObject colisionado o en sus padres
            // GetComponentInParent es bueno para buscar en la jerarqu�a si la "Vida" est� en el GameObject principal
            Vida oponenteVidaScript = other.GetComponentInParent<Vida>();

            if (oponenteVidaScript != null)
            {
                // Solo golpea si el objeto no ha sido golpeado ya en este ataque
                if (!objetosGolpeados.Contains(other.gameObject))
                {
                    Debug.Log("HitBox: �DA�ANDO A " + other.name + " (Padre: " + oponenteVidaScript.name + ")!");
                    oponenteVidaScript.RecibirDa�o(da�o);
                    objetosGolpeados.Add(other.gameObject); // A�adir a la lista de golpeados para evitar multi-golpes
                }
                // else
                // {
                //     Debug.Log("HitBox: " + other.name + " ya fue golpeado en este ataque."); // Descomentar para depuraci�n
                // }
            }
            else
            {
                Debug.LogWarning("HitBox: Objeto con tag '" + oponenteBodyTag + "' (" + other.name + ") NO TIENE COMPONENTE VIDA en s� mismo o en sus padres.");
            }
        }
        // else
        // {
        //     Debug.Log("HitBox: Colisi�n con objeto no oponente: " + other.name + " (Tag: " + other.tag + ")"); // Descomentar para depuraci�n
        // }
    }

    // Se recomienda usar OnTriggerEnter para ataques, ya que garantiza que el da�o se aplique una vez por colisi�n inicial.
    // OnTriggerStay puede ser �til para efectos continuos, pero para da�o de ataque, puede causar m�ltiples da�os si los colliders se superponen mucho tiempo.
    // Si realmente necesitas OnTriggerStay para el da�o, aseg�rate de que el cooldown de da�o en el script de Vida o aqu� lo controle.
    private void OnTriggerStay(Collider other)
    {
        // En la mayor�a de los casos de ataque, es mejor no usar OnTriggerStay para da�o repetitivo.
        // Si lo usas, DEBES tener un sistema de cooldown de da�o dentro de este m�todo o en el script Vida.
        // Por ahora, solo dejar� los filtros.
        if (!activo) return;

        // Filtro 1 - No me golpeo a m� mismo
        if (ownerBody != null && (other.gameObject == ownerBody || other.transform.IsChildOf(ownerBody.transform)))
        {
            return;
        }

        // Filtro 2 - Solo golpeo al oponente
        if (other.CompareTag(oponenteBodyTag))
        {
            Vida oponenteVidaScript = other.GetComponentInParent<Vida>();
            if (oponenteVidaScript != null && !objetosGolpeados.Contains(other.gameObject))
            {
                // Si realmente quieres da�o en Stay, aseg�rate de un cooldown de da�o aqu� tambi�n,
                // o conf�a en que el sistema de cooldown de Vida sea suficiente.
                // Debug.Log("HitBox (Stay): Da�ando a " + other.name); // Descomentar para depuraci�n
                oponenteVidaScript.RecibirDa�o(da�o);
                objetosGolpeados.Add(other.gameObject);
            }
        }
    }

    public void Activar()
    {
        activo = true;
        objetosGolpeados.Clear(); // Limpia la lista de objetos golpeados al inicio de cada ataque
        Debug.Log("HitBox ACTIVADO");
    }

    public void Desactivar()
    {
        activo = false;
        // objetosGolpeados.Clear(); // No es estrictamente necesario aqu�, ya se limpia al activar
        Debug.Log("HitBox DESACTIVADO");
    }
}