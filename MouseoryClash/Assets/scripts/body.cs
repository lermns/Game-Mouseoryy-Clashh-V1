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

    [Header("Configuración de Movimiento")] // Agregado para mejor organización
    public float velocidad = 40f;
    public float velocidadRotacion = 100f;

    [Header("Configuración de Animaciones")] // Agregado para mejor organización
    public float velocidadAnimacionWalk = 0.5f; // Hacemos la velocidad de walk ajustable
    public float velocidadAnimacionHitUno = 1.0f; // Para poder ajustar la velocidad de hit_uno desde el Inspector

    private Animation anim;
    private Rigidbody rb;

    private bool atacando = false;       // ¿Está ejecutando ataque?
    public float cooldownGolpe = 1.0f;  // Tiempo mínimo entre golpes en segundos (Hacerlo público para Inspector)
    private float tiempoUltimoGolpe = -10f; // Para controlar cooldown

    // private bool ultimoGolpeUno = false; // No se está utilizando actualmente, se puede eliminar si no hay otra animación de golpe
    public HitBox espadaHitbox; // arrastra aquí el componente de la espada

    void Start()
    {
        anim = GetComponent<Animation>();
        rb = GetComponent<Rigidbody>();

        // Verificar si el componente Animation existe
        if (anim == null)
        {
            Debug.LogError("Error: El componente Animation no se encontró en " + gameObject.name + ". Asegúrate de que esté adjunto.");
            enabled = false; // Desactiva el script si no hay componente Animation para evitar más errores
            return;
        }

        // Asignar la velocidad de walk desde la nueva variable pública
        if (anim["walk"] != null)
        {
            anim["walk"].speed = velocidadAnimacionWalk;
        }
        else
        {
            Debug.LogWarning("La animación 'walk' no está asignada en el componente Animation de " + gameObject.name + ".");
        }

        if (espadaHitbox != null)
        {
            Debug.Log("HitBox asignado correctamente: " + espadaHitbox.name);
            espadaHitbox.Desactivar();
            espadaHitbox.SetOwnerBody(this.gameObject); // Asigna el GameObject del propio personaje
        }
        else
        {
            Debug.LogError("Error: No se ha asignado el HitBox de la espada en el Inspector del script Body en " + gameObject.name + ". ¡Asígnalo!");
            enabled = false; // Desactiva el script si no hay HitBox para evitar más errores
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

        // Mover hacia adelante (Asegúrate de que teclaAtras/Adelante corresponden a tus expectativas)
        if (Input.GetKey(teclaAdelante)) // Asumo que 'W' (teclaAdelante) mueve hacia adelante
        {
            anim.CrossFade("walk");
            Vector3 movimiento = transform.forward * velocidad * Time.deltaTime;
            rb.MovePosition(transform.position + movimiento);
            caminando = true;
        }

        // Mover hacia atrás
        if (Input.GetKey(teclaAtras)) // Asumo que 'S' (teclaAtras) mueve hacia atrás
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

        // Detener animación si no se camina
        if (!caminando && anim.IsPlaying("walk"))
        {
            anim.Stop("walk");
        }

        // Ataque con cooldown
        // Solo permite atacar si no está atacando actualmente y el cooldown ha pasado
        if (Input.GetKeyDown(teclaAtacar) && Time.time >= tiempoUltimoGolpe + cooldownGolpe)
        {
            tiempoUltimoGolpe = Time.time; // Actualiza el tiempo del último golpe
            atacando = true; // Marca que el personaje está en un estado de ataque
            PlayGolpe("hit_uno");    // Siempre se juega hit_uno por ahora
        }
    }

    void PlayGolpe(string nombreAnimacion)
    {
        if (anim[nombreAnimacion] == null)
        {
            Debug.LogError($"La animación '{nombreAnimacion}' no está asignada o tiene un nombre incorrecto en el componente Animation. No se puede reproducir.");
            atacando = false; // Asegura que el estado de ataque se reinicie
            return;
        }

        // Asigna la velocidad de la animación 'hit_uno' desde la nueva variable pública
        // Si tienes más animaciones de golpe, podrías usar un switch o más if/else if
        if (nombreAnimacion == "hit_uno")
        {
            anim[nombreAnimacion].speed = velocidadAnimacionHitUno;
        }
        else
        {
            anim[nombreAnimacion].speed = 1f; // Velocidad por defecto para otras animaciones
        }

        anim.Play(nombreAnimacion);

        // Calcula la duración real de la animación considerando su velocidad
        float duracionRealAnimacion = anim[nombreAnimacion].length / anim[nombreAnimacion].speed;

        StartCoroutine(ActivarHitboxGolpe(duracionRealAnimacion));
    }

    // Corrutina para controlar la activación/desactivación del HitBox durante el golpe
    IEnumerator ActivarHitboxGolpe(float duracionAnimacion)
    {
        // DEBUGGING: Activar el HitBox casi al instante para pruebas
        yield return new WaitForSeconds(duracionAnimacion * 0.05f); // Pequeño delay, ajusta si es necesario
        espadaHitbox.Activar();

        // Mantén el HitBox activo durante un buen porcentaje de la animación de ataque para asegurar el impacto
        yield return new WaitForSeconds(duracionAnimacion * 0.4f); // Ajusta esta duración (ej. 0.3f a 0.5f)
        espadaHitbox.Desactivar();

        // Espera el resto de la duración de la animación antes de permitir otro ataque
        // Asegúrate de que la suma de los porcentajes sea <= 1.0f
        yield return new WaitForSeconds(duracionAnimacion * 0.55f); // Restante: 1 - 0.05 - 0.4 = 0.55

        atacando = false; // Permite al personaje iniciar otro ataque o moverse
    }

    // Esta corrutina ya no es necesaria si todo el control de ataque está en ActivarHitboxGolpe
    /*
    IEnumerator EsperarFinAnimacion(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        atacando = false;
    }
    */
}