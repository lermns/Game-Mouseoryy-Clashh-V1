using UnityEngine;
using System.Collections;
using UnityEngine.UI; // ¡Importante! Necesario para trabajar con componentes UI

public class Vida : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int saludMaxima = 100;
    private int saludActual;

    [Header("Configuración de UI")] // Nueva sección para la barra de vida
    public Image barraDeVidaUI; // Asigna aquí el componente Image de la barra de relleno (Fill)
    public Color colorSaludCompleta = Color.green;
    public Color colorSaludBaja = Color.red;
    public float umbralSaludBaja = 0.3f; // Umbral para cambiar a rojo (30% de vida)

    private Renderer[] renderers;
    private Color[] coloresOriginales;

    // --- CÓDIGO NUEVO AÑADIDO PARA LA NOTIFICACIÓN DE MUERTE ---
    public delegate void OnDeath(GameObject deadCharacter); // Define un delegado para el evento de muerte
    public static event OnDeath OnCharacterDeath; // Evento estático que otros scripts pueden suscribir
    // -----------------------------------------------------------

    void Start()
    {
        saludActual = saludMaxima;

        renderers = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                coloresOriginales[i] = renderers[i].material.color;
            }
            else
            {
                Debug.LogWarning($"El Renderer en {renderers[i].gameObject.name} no tiene un material asignado. No se podrá guardar su color original.");
                coloresOriginales[i] = Color.white;
            }
        }

        ActualizarBarraDeVidaUI(); // Asegura que la barra se muestre al inicio
    }

    public void RecibirDaño(int cantidad)
    {
        saludActual -= cantidad;
        Debug.Log(gameObject.name + " recibió " + cantidad + " de daño. Salud restante: " + saludActual);

        ActualizarBarraDeVidaUI(); // ¡Actualiza la UI cada vez que recibe daño!

        StopAllCoroutines();
        StartCoroutine(ParpadearRojo());

        if (saludActual <= 0)
        {
            Morir();
        }
    }

    void ActualizarBarraDeVidaUI()
    {
        if (barraDeVidaUI != null)
        {
            // Calcula el porcentaje de vida (0.0 a 1.0)
            float porcentajeVida = (float)saludActual / saludMaxima;

            // Actualiza el Fill Amount de la Image UI
            barraDeVidaUI.fillAmount = porcentajeVida;

            // Cambia el color de la barra según la vida restante
            barraDeVidaUI.color = Color.Lerp(colorSaludBaja, colorSaludCompleta, porcentajeVida);

            // También puedes agregar un texto si quieres mostrar el número exacto de vida
            // Ejemplo: if (textoVidaUI != null) textoVidaUI.text = saludActual.ToString();
        }
        else
        {
            Debug.LogWarning($"La barra de vida UI no está asignada para {gameObject.name}.");
        }
    }

    IEnumerator ParpadearRojo()
    {
        foreach (Renderer rend in renderers)
        {
            if (rend.material != null)
            {
                rend.material.color = Color.red;
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                renderers[i].material.color = coloresOriginales[i];
            }
        }
    }

    void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto.");
        // Opcional: Desactivar la barra de vida al morir
        if (barraDeVidaUI != null)
        {
            barraDeVidaUI.gameObject.SetActive(false);
        }

        // --- CÓDIGO NUEVO AÑADIDO PARA LA NOTIFICACIÓN DE MUERTE ---
        // Notificar a cualquiera que esté escuchando que este personaje ha muerto
        if (OnCharacterDeath != null)
        {
            OnCharacterDeath(this.gameObject);
        }
        // -----------------------------------------------------------

        Destroy(gameObject);
    }
}