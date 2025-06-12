using UnityEngine;
using System.Collections.Generic; // Necesario para HashSet

public class HitBox : MonoBehaviour
{
    [Header("Configuración de HitBox")] // Agregado para mejor organización
    public int daño = 10;
    public string oponenteBodyTag; // Asigna "Player1Body" o "Player2Body" en el Inspector para cada HitBox

    // Referencia al GameObject del propio personaje que posee este HitBox
    private GameObject ownerBody;

    private bool activo = false;
    private HashSet<GameObject> objetosGolpeados = new HashSet<GameObject>(); // Para evitar golpear el mismo objeto múltiples veces por ataque

    // Método para que el script Body asigne la referencia a su propio cuerpo
    public void SetOwnerBody(GameObject body)
    {
        ownerBody = body;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activo) return; // Si el hitbox no está activo, ignorar la colisión

        // **IMPORTANTE: Filtro 1 - No me golpeo a mí mismo**
        // Comprueba si el objeto colisionado es el mismo que el "ownerBody" o un hijo de ownerBody
        // Esto es crucial si el Rigidbody del ownerBody puede colisionar con el Rigidbody del HitBox.
        // Si el HitBox SIEMPRE está en un objeto hijo de ownerBody, la segunda parte es más robusta.
        if (ownerBody != null && (other.gameObject == ownerBody || other.transform.IsChildOf(ownerBody.transform)))
        {
            // Debug.Log("HitBox: Colisión detectada con mi propio cuerpo o parte de mi cuerpo. Ignorando daño.");
            return; // Ignora la colisión si es mi propio cuerpo o un hijo
        }

        // **Filtro 2 - Solo golpeo al oponente**
        // Solo procesa la colisión si el tag coincide con el oponenteBodyTag
        if (other.CompareTag(oponenteBodyTag))
        {
            // Buscar el componente Vida en el GameObject colisionado o en sus padres
            // GetComponentInParent es bueno para buscar en la jerarquía si la "Vida" está en el GameObject principal
            Vida oponenteVidaScript = other.GetComponentInParent<Vida>();

            if (oponenteVidaScript != null)
            {
                // Solo golpea si el objeto no ha sido golpeado ya en este ataque
                if (!objetosGolpeados.Contains(other.gameObject))
                {
                    Debug.Log("HitBox: ¡DAÑANDO A " + other.name + " (Padre: " + oponenteVidaScript.name + ")!");
                    oponenteVidaScript.RecibirDaño(daño);
                    objetosGolpeados.Add(other.gameObject); // Añadir a la lista de golpeados para evitar multi-golpes
                }
                // else
                // {
                //     Debug.Log("HitBox: " + other.name + " ya fue golpeado en este ataque."); // Descomentar para depuración
                // }
            }
            else
            {
                Debug.LogWarning("HitBox: Objeto con tag '" + oponenteBodyTag + "' (" + other.name + ") NO TIENE COMPONENTE VIDA en sí mismo o en sus padres.");
            }
        }
        // else
        // {
        //     Debug.Log("HitBox: Colisión con objeto no oponente: " + other.name + " (Tag: " + other.tag + ")"); // Descomentar para depuración
        // }
    }

    // Se recomienda usar OnTriggerEnter para ataques, ya que garantiza que el daño se aplique una vez por colisión inicial.
    // OnTriggerStay puede ser útil para efectos continuos, pero para daño de ataque, puede causar múltiples daños si los colliders se superponen mucho tiempo.
    // Si realmente necesitas OnTriggerStay para el daño, asegúrate de que el cooldown de daño en el script de Vida o aquí lo controle.
    private void OnTriggerStay(Collider other)
    {
        // En la mayoría de los casos de ataque, es mejor no usar OnTriggerStay para daño repetitivo.
        // Si lo usas, DEBES tener un sistema de cooldown de daño dentro de este método o en el script Vida.
        // Por ahora, solo dejaré los filtros.
        if (!activo) return;

        // Filtro 1 - No me golpeo a mí mismo
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
                // Si realmente quieres daño en Stay, asegúrate de un cooldown de daño aquí también,
                // o confía en que el sistema de cooldown de Vida sea suficiente.
                // Debug.Log("HitBox (Stay): Dañando a " + other.name); // Descomentar para depuración
                oponenteVidaScript.RecibirDaño(daño);
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
        // objetosGolpeados.Clear(); // No es estrictamente necesario aquí, ya se limpia al activar
        Debug.Log("HitBox DESACTIVADO");
    }
}