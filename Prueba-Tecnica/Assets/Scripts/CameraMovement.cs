using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragMovement : MonoBehaviour
{
    [SerializeField] private Transform cam;

    // Velocidad de movimiento de la cámara al arrastrar
    [SerializeField] private float dragSpeed = 2f;

    // Velocidad del zoom
    [SerializeField] private float zoomSpeed = 10f;

    // Variable para almacenar la posición inicial del mouse al arrastrar
    private Vector3 lastMousePosition;

    void Update()
    {
        // Movimiento con clic derecho y arrastre
        if (Input.GetMouseButtonDown(0)) // Botón izq del mouse presionado
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0)) // Mantener presionado el botón izq
        {
            Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // Movimiento en el plano XZ según el delta del mouse
            Vector3 movement = new Vector3(-deltaMousePosition.x, 0, -deltaMousePosition.y) * dragSpeed * Time.deltaTime;
            transform.Translate(movement);
        }

        // Zoom con la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // Valor de la rueda del ratón
        if (scroll != 0)
        {
            // Calcula el movimiento en el eje local Z
            Vector3 zoomDirection = cam.transform.forward * scroll * zoomSpeed;

            // Calcula la nueva posición potencial
            Vector3 newPosition = cam.transform.position + zoomDirection;

            cam.transform.position = newPosition;
        }
    }
}
