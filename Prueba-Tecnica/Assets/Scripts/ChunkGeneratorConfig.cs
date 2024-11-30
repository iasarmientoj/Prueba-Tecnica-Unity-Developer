using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chunk Generator Config")]
public class ChunkGeneratorConfig : ScriptableObject
{

    [Header("PAR�METROS PRINCIPALES")]

    [Header("Tama�o del chunk")]
    [Range(9f, 50f)] public int sizeChunks = 13; // m�nimo un tama�o de 9, seg�n los tests
    [Header("Cantidad de chunks a generar")]
    [Range(1f, 500f)] public int cantidadTotalDeChunks = 20;
    [Header("Cantidad m�xima de bifurcaciones")]
    [Range(0f, 3f)] public int cantBifurcacionesPosiblesMax = 3; // m�ximo 3 bifurcaciones
    [Header("Irregularidad de los caminos")]
    [Range(0f, 1f)] public float cantIrregularidadDeCamino = 0.7f; // 0: recto, 1: irregular
    [Header("'Movimiento' de los caminos")]
    [Range(0f, 1f)] public float longMinCamino = 0.5f; // 0: largo, 1: corto

    [Header("----------------")]
    [Header("PAR�METROS SECUNDARIOS")]

    [Header("N�mero m�ximo de intentos para crear un camino en caso de falla")]
    [Range(0f, 50f)] public int numeroDeIntentosCrearCaminoMax = 10;

    [Header("----------------")]
    [Header("PAR�METROS SEED")]
    [Header("Semilla para asegurar aletoriedad y repetibilidad")]
    public int seed_global = 0;

}
