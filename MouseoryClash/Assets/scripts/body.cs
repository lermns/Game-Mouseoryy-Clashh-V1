using UnityEngine;
using System.Collections; // Necesario para Coroutines

public class Body : MonoBehaviour
{
    [Header("Controles personalizados")]
    public KeyCode teclaAdelante = KeyCode.S;
    public KeyCode teclaAtras = KeyCode.W;
    public KeyCode teclaIzquierda = KeyCode.A;
    public KeyCode teclaDerecha = KeyCode.D;
    public KeyCode teclaAtacar = KeyCode.Q;

    [Header("Configuraci�n de Movimiento")] // Agregado para mejor organizaci�n
    public float velocidad = 40f;
    public float velocidadRotacion = 100f;

    [Header("Configuraci�n de Animaciones")] // Agregado para mejor organizaci�n
    public float velocidadAnimacionWalk = 0.5f; // Hacemos la velocidad de walk ajustable
    public float velocidadAnimacionHitUno = 1.0f; // Para poder ajustar la velocidad de hit_uno desde el Inspector

    private Animation anim;
    private Rigidbody rb;

    private bool atacando = false;       // �Est� ejecutando ataque?
    public float cooldownGolpe = 1.0f;  // Tiempo m�nimo entre golpes en segundos (Hacerlo p�blico para Inspector)
    private float tiempoUltimoGolpe = -10f; // Para controlar cooldown

    // private bool ultimoGolpeUno = false; // No se est� utilizando actualmente, se puede eliminar si no hay otra animaci�n de golpe
    public HitBox espadaHitbox; // arrastra aqu� el componente de la espada

    void Start()
    {
        anim = GetComponent<Animation>();
        rb = GetComponent<Rigidbody>();

        // Verificar si el componente Animation existe
        if (anim == null)
        {
            Debug.LogError("Error: El componente Animation no se encontr� en " + gameObject.name + ". Aseg�rate de que est� adjunto.");
            enabled = false; // Desactiva el script si no hay componente Animation para evitar m�s errores
            return;
        }

        // Asignar la velocidad de walk desde la nueva variable p�blica
        if (anim["walk"] != null)
        {
            anim["walk"].speed = velocidadAnimacionWalk;
        }
        else
        {
            Debug.LogWarning("La animaci�n 'walk' no est� asignada en el componente Animation de " + gameObject.name + ".");
        }

        if (espadaHitbox != null)
        {
            Debug.Log("HitBox asignado correctamente: " + espadaHitbox.name);
            espadaHitbox.Desactivar();
            espadaHitbox.SetOwnerBody(this.gameObject); // Asigna el GameObject del propio personaje
        }
        else
        {
            Debug.LogError("Error: No se ha asignado el HitBox de la espada en el Inspector del script Body en " + gameObject.name + ". �As�gnalo!");
            enabled = false; // Desactiva el script si no hay HitBox para evitar m�s errores
        }
    }

    void Update()
    {
        if (atacando)
        {
            // Mientras atacamos, no permitimos caminar ni iniciar otro ataque
            return;
        }

        bool caminando = false;

        // Mover hacia adelante (Aseg�rate de que teclaAtras/Adelante corresponden a tus expectativas)
        if (Input.GetKey(teclaAdelante)) // Asumo que 'W' (teclaAdelante) mueve hacia adelante
        {
            anim.CrossFade("walk");
            Vector3 movimiento = transform.forward * velocidad * Time.deltaTime;
            rb.MovePosition(transform.position + movimiento);
            caminando = true;
        }

        // Mover hacia atr�s
        if (Input.GetKey(teclaAtras)) // Asumo que 'S' (teclaAtras) mueve hacia atr�s
        {
            anim.CrossFade("walk");
            Vector3 movimiento = -transform.forward * velocidad * Time.deltaTime;
            rb.MovePosition(transform.position + movimiento);
            caminando = true;
        }

        // Rotar a la izquierda
        if (Input.GetKey(teclaIzquierda))
        {
            transform.Rotate(Vector3.up, -velocidadRotacion * Time.deltaTime);
        }

        // Rotar a la derecha
        if (Input.GetKey(teclaDerecha))
        {
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
        }

        // Detener animaci�n si no se camina
        if (!caminando && anim.IsPlaying("walk"))
        {
            anim.Stop("walk");
        }

        // Ataque con cooldown
        // Solo permite atacar si no est� atacando actualmente y el cooldown ha pasado
        if (Input.GetKeyDown(teclaAtacar) && Time.time >= tiempoUltimoGolpe + cooldownGolpe)
        {
            tiempoUltimoGolpe = Time.time; // Actualiza el tiempo del �ltimo golpe
            atacando = true; // Marca que el personaje est� en un estado de ataque
            PlayGolpe("hit_uno");    // Siempre se juega hit_uno por ahora
        }
    }

    void PlayGolpe(string nombreAnimacion)
    {
        if (anim[nombreAnimacion] == null)
        {
            Debug.LogError($"La animaci�n '{nombreAnimacion}' no est� asignada o tiene un nombre incorrecto en el componente Animation. No se puede reproducir.");
            atacando = false; // Asegura que el estado de ataque se reinicie
            return;
        }

        // Asigna la velocidad de la animaci�n 'hit_uno' desde la nueva variable p�blica
        // Si tienes m�s animaciones de golpe, podr�as usar un switch o m�s if/else if
        if (nombreAnimacion == "hit_uno")
        {
            anim[nombreAnimacion].speed = velocidadAnimacionHitUno;
        }
        else
        {
            anim[nombreAnimacion].speed = 1f; // Velocidad por defecto para otras animaciones
        }

        anim.Play(nombreAnimacion);

        // Calcula la duraci�n real de la animaci�n considerando su velocidad
        float duracionRealAnimacion = anim[nombreAnimacion].length / anim[nombreAnimacion].speed;

        StartCoroutine(ActivarHitboxGolpe(duracionRealAnimacion));
    }

    // Corrutina para controlar la activaci�n/desactivaci�n del HitBox durante el golpe
    IEnumerator ActivarHitboxGolpe(float duracionAnimacion)
    {
        // DEBUGGING: Activar el HitBox casi al instante para pruebas
        yield return new WaitForSeconds(duracionAnimacion * 0.05f); // Peque�o delay, ajusta si es necesario
        espadaHitbox.Activar();

        // Mant�n el HitBox activo durante un buen porcentaje de la animaci�n de ataque para asegurar el impacto
        yield return new WaitForSeconds(duracionAnimacion * 0.4f); // Ajusta esta duraci�n (ej. 0.3f a 0.5f)
        espadaHitbox.Desactivar();

        // Espera el resto de la duraci�n de la animaci�n antes de permitir otro ataque
        // Aseg�rate de que la suma de los porcentajes sea <= 1.0f
        yield return new WaitForSeconds(duracionAnimacion * 0.55f); // Restante: 1 - 0.05 - 0.4 = 0.55

        atacando = false; // Permite al personaje iniciar otro ataque o moverse
    }

    // Esta corrutina ya no es necesaria si todo el control de ataque est� en ActivarHitboxGolpe
    /*
    IEnumerator EsperarFinAnimacion(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        atacando = false;
    }
    */
}