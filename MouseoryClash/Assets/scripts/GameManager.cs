using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para Coroutines
using TMPro; // Si usas TextMeshPro
using UnityEngine.SceneManagement; // �IMPORTANTE! Necesario para cambiar de escena

public class GameManager : MonoBehaviour
{
    public GameObject princesaGameObject;
    public GameObject romanoGameObject;

    public Text victoriaText_UGUI;
    public TextMeshProUGUI victoriaText_TMP;

    // --- NUEVAS VARIABLES PARA EL CONTADOR ---
    public Text countdownText_UGUI; // Asigna aqu� tu GameObject CountdownText (si usas Text de Unity)
    public TextMeshProUGUI countdownText_TMP; // Asigna aqu� tu GameObject CountdownText (si usas TextMeshPro)
    public float tiempoEsperaEntreNumeros = 1f; // Tiempo que espera entre 3, 2, 1
    // ------------------------------------------

    // --- NUEVAS VARIABLES PARA EL BOT�N DE MEN� ---
    [Header("UI de Fin de Partida")] // Nueva secci�n para el bot�n de men�
    public GameObject botonVolverMenu; // Asigna aqu� tu GameObject BotonVolverMenu
    public string nombreEscenaMenuPrincipal = "Menu"; // �CAMBIA ESTO AL NOMBRE REAL DE TU ESCENA EN EL EDITOR!
    // ----------------------------------------------

    private bool juegoTerminado = false;
    private bool juegoIniciado = false; // Nueva bandera para saber si el juego ha comenzado

    // Declaraciones de los componentes Body (para habilitar/deshabilitar controles)
    private Body princesaBody;
    private Body romanoBody;

    void Awake() // Usamos Awake para asegurarnos de que se encuentran antes de Start
    {
        // Encontrar los componentes Body
        if (princesaGameObject != null)
        {
            princesaBody = princesaGameObject.GetComponent<Body>();
        }
        if (romanoGameObject != null)
        {
            romanoBody = romanoGameObject.GetComponent<Body>();
        }
    }

    void OnEnable()
    {
        Vida.OnCharacterDeath += HandleCharacterDeath;

        // --- C�DIGO NUEVO: Suscribir el bot�n a la funci�n VolverAMenuPrincipal cuando se activa ---
        if (botonVolverMenu != null)
        {
            Button btn = botonVolverMenu.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(VolverAMenuPrincipal);
            }
        }
        // ------------------------------------------------------------------------------------------
    }

    void OnDisable()
    {
        Vida.OnCharacterDeath -= HandleCharacterDeath;

        // --- C�DIGO NUEVO: Desuscribir el bot�n para evitar problemas si el GameManager se destruye antes que el bot�n ---
        if (botonVolverMenu != null)
        {
            Button btn = botonVolverMenu.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(VolverAMenuPrincipal);
            }
        }
        // ------------------------------------------------------------------------------------------------------------
    }

    void Start()
    {
        // Aseg�rate de que los mensajes est�n ocultos al inicio
        if (victoriaText_UGUI != null) victoriaText_UGUI.gameObject.SetActive(false);
        if (victoriaText_TMP != null) victoriaText_TMP.gameObject.SetActive(false);
        if (countdownText_UGUI != null) countdownText_UGUI.gameObject.SetActive(false);
        if (countdownText_TMP != null) countdownText_TMP.gameObject.SetActive(false);

        // --- C�DIGO NUEVO: Asegura que el bot�n est� oculto al inicio ---
        if (botonVolverMenu != null) botonVolverMenu.SetActive(false);
        // ---------------------------------------------------------------

        juegoTerminado = false;
        juegoIniciado = false; // Inicialmente el juego no ha iniciado

        // Pausar los controles de los ratones al inicio
        if (princesaBody != null) princesaBody.enabled = false;
        if (romanoBody != null) romanoBody.enabled = false;

        StartCoroutine(IniciarCuentaRegresiva()); // Iniciar la cuenta regresiva
    }

    IEnumerator IniciarCuentaRegresiva()
    {
        // Activar el texto del contador
        if (countdownText_UGUI != null) countdownText_UGUI.gameObject.SetActive(true);
        if (countdownText_TMP != null) countdownText_TMP.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            // Actualizar el texto del contador
            if (countdownText_UGUI != null) countdownText_UGUI.text = i.ToString();
            if (countdownText_TMP != null) countdownText_TMP.text = i.ToString();
            yield return new WaitForSeconds(tiempoEsperaEntreNumeros);
        }

        // Mensaje final "�PELEA!" o "�GO!"
        if (countdownText_UGUI != null) countdownText_UGUI.text = "�PELEA!";
        if (countdownText_TMP != null) countdownText_TMP.text = "�PELEA!";
        yield return new WaitForSeconds(tiempoEsperaEntreNumeros);

        // Desactivar el texto del contador
        if (countdownText_UGUI != null) countdownText_UGUI.gameObject.SetActive(false);
        if (countdownText_TMP != null) countdownText_TMP.gameObject.SetActive(false);

        // --- HABILITAR CONTROLES Y MARCAR JUEGO INICIADO ---
        juegoIniciado = true;
        if (princesaBody != null) princesaBody.enabled = true;
        if (romanoBody != null) romanoBody.enabled = true;
        // ----------------------------------------------------
    }

    void HandleCharacterDeath(GameObject deadCharacter)
    {
        if (juegoTerminado) return; // Si el juego ya termin�, ignora m�s muertes

        juegoTerminado = true; // Marca el juego como terminado

        // --- DESHABILITAR CONTROLES AL FINALIZAR EL JUEGO ---
        if (princesaBody != null) princesaBody.enabled = false;
        if (romanoBody != null) romanoBody.enabled = false;
        // ----------------------------------------------------

        string ganador = "";
        if (deadCharacter == romanoGameObject)
        {
            ganador = "Princesa";
        }
        else if (deadCharacter == princesaGameObject)
        {
            ganador = "Romano";
        }
        else
        {
            ganador = "Alguien"; // En caso de que muera otro objeto
        }

        MostrarMensajeVictoria(ganador);
    }

    void MostrarMensajeVictoria(string ganadorNombre)
    {
        // Aseg�rate de que el contador est� oculto si por alguna raz�n sigue visible
        if (countdownText_UGUI != null) countdownText_UGUI.gameObject.SetActive(false);
        if (countdownText_TMP != null) countdownText_TMP.gameObject.SetActive(false);

        if (victoriaText_UGUI != null)
        {
            victoriaText_UGUI.text = $"�{ganadorNombre} GANA!";
            victoriaText_UGUI.gameObject.SetActive(true);
        }
        if (victoriaText_TMP != null)
        {
            victoriaText_TMP.text = $"�{ganadorNombre} GANA!";
            victoriaText_TMP.gameObject.SetActive(true);
        }
        // Puedes agregar m�s l�gica aqu�, como detener el tiempo (si no lo detuviste antes), etc.

        // --- C�DIGO NUEVO: Mostrar el bot�n de volver al men� ---
        if (botonVolverMenu != null)
        {
            botonVolverMenu.SetActive(true);
        }
        // --------------------------------------------------------

        Time.timeScale = 0f; // Detiene el juego completamente
    }

    // --- NUEVO M�TODO PARA VOLVER AL MEN� PRINCIPAL ---
    public void VolverAMenuPrincipal()
    {
        Debug.Log("Volviendo al men� principal...");
        Time.timeScale = 1f; // Es crucial restablecer Time.timeScale antes de cambiar de escena
        SceneManager.LoadScene(nombreEscenaMenuPrincipal); // Carga la escena del men� principal
    }
    // ----------------------------------------------------
}