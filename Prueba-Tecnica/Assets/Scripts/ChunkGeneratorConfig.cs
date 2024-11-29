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
    [Range(1f, 100f)] public int cantidadTotalDeChunks = 20;
    [Header("Cantidad de bifurcaciones")]
    [Range(0f, 3f)] public int cantBifurcacionesPosiblesMax = 3; // máximo 3 bifurcaciones
    [Header("Irregularidad de los caminos")]
    [Range(0f, 1f)] public float cantIrregularidadDeCamino = 0.7f; // 0: recto, 1: irregular

    [Header("----------------")]
    [Header("PARÁMETROS SECUNDARIOS")]

    [Header("Longitud mínima de los caminos dentro del chunk (Tip: mismo valor que sizeChunks)")]
    [Range(9f, 50f)] public int longMinCamino = 13;
    [Header("Número máximo de intentos para crear un camino en caso de falla")]
    [Range(0f, 10f)] public int numeroDeIntentosCrearCaminoMax = 7;

    [Header("----------------")]
    [Header("PARÁMETROS SEED")]
    [Header("Afecta el recorido de los caminos")]
    public int seed_siguientePasoDireccion = 3;
    [Header("Afecta la posible cantidad de bifurcaciones por chunk")]
    public int seed_cantBifurcaciones = 3;
    [Header("Afecta el origen de las bifurcaciones dentro del chunk")]
    public int seed_origenBifurcaciones = 0;
    [Header("Afecta la irrecularidad de los caminos")]
    public int seed_cantIrregularidadDeCamino = 0;



}
