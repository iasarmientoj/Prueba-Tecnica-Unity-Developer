using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCulling : MonoBehaviour
{
    // Referencia a los par�metros de los chunks
    [SerializeField] private ChunkGeneratorConfig parameters;
    // Referencia a la c�mara
    [SerializeField] private Camera mainCamera;
    // Referencia al objeto que contiene los cubos
    [SerializeField] private Transform chunksContainer;
    // Intervalo de actualizaci�n en segundos (para mejorar rendimiento)
    [SerializeField] private float cullingUpdateInterval = 0.1f;
    private float nextUpdateTime = 0f;
    // Margen de visi�n m�s all� de los l�mites de la c�mara
    [SerializeField] private float margenVision = 0.14f;

    void Update()
    {
        // Solo actualizamos el culling seg�n el intervalo definido
        if (Time.time >= nextUpdateTime)
        {
            PerformCulling();
            nextUpdateTime = Time.time + cullingUpdateInterval;
        }
    }

    void PerformCulling()
    {
        // Itera sobre cada hijo del contenedor
        foreach (Transform child in chunksContainer)
        {
            // Comprueba si el cubo est� dentro del campo de visi�n de la c�mara, mas un margen 
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(child.position);
            // o si la distancia entre el player y el chunk es menor al tama�o de los chunks
            float distancia = Vector3.Distance(mainCamera.transform.position, child.position);

            bool isVisible = (( viewportPoint.x > 0- margenVision && viewportPoint.x < 1+ margenVision && viewportPoint.y > 0- margenVision && viewportPoint.y < 1+ margenVision && viewportPoint.z > 0) || distancia <= parameters.sizeChunks*1.5f); 

            child.gameObject.SetActive(isVisible);
        }
    }

    // Intercambia entre la c�mara del modo noormal y del modo FPS
    public void SetCamFPS(Camera cam)
    {
        mainCamera = cam;
    }

}
