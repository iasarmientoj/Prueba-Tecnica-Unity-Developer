using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chunk Generator Config")]
public class ChunkGeneratorConfig : ScriptableObject
{

    [Header("PARÁMETROS PRINCIPALES")]

    [Header("Tamaño del chunk")]
    [Range(9f, 50f)] public int sizeChunks = 13; // mínimo un tamaño de 9, según los tests
    [Header("Cantidad de chunks a generar")]
    [Range(1f, 500f)] public int cantidadTotalDeChunks = 20;
    [Header("Cantidad máxima de bifurcaciones")]
    [Range(0f, 3f)] public int cantBifurcacionesPosiblesMax = 3; // máximo 3 bifurcaciones
    [Header("Irregularidad de los caminos")]
    [Range(0f, 1f)] public float cantIrregularidadDeCamino = 0.7f; // 0: recto, 1: irregular
    [Header("'Movimiento' de los caminos")]
    [Range(0f, 1f)] public float longMinCamino = 0.5f; // 0: largo, 1: corto

    [Header("----------------")]
    [Header("PARÁMETROS SECUNDARIOS")]

    [Header("Número máximo de intentos para crear un camino en caso de falla")]
    [Range(0f, 50f)] public int numeroDeIntentosCrearCaminoMax = 10;

    [Header("----------------")]
    [Header("PARÁMETROS SEED")]
    [Header("Semilla para asegurar aletoriedad y repetibilidad")]
    public int seed_global = 0;

}
