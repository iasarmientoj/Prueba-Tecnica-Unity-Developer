using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform cam;

    // Velocidad de movimiento de la cámara
    [SerializeField] private float moveSpeed = 30f;

    // Velocidad del zoom
    [SerializeField] private float zoomSpeed = 10f;



    void Update()
    {
        // Movimiento en el plano XZ
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D o flechas izquierda/derecha
        float moveVertical = Input.GetAxis("Vertical");     // W/S o flechas arriba/abajo

        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime);

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
