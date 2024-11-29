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
    [Range(1f, 100f)] public int cantidadTotalDeChunks = 20;
    [Header("Cantidad de bifurcaciones")]
    [Range(0f, 3f)] public int cantBifurcacionesPosiblesMax = 3; // m�ximo 3 bifurcaciones
    [Header("Irregularidad de los caminos")]
    [Range(0f, 1f)] public float cantIrregularidadDeCamino = 0.7f; // 0: recto, 1: irregular

    [Header("----------------")]
    [Header("PAR�METROS SECUNDARIOS")]

    [Header("Longitud m�nima de los caminos dentro del chunk (Tip: mismo valor que sizeChunks)")]
    [Range(9f, 50f)] public int longMinCamino = 13;
    [Header("N�mero m�ximo de intentos para crear un camino en caso de falla")]
    [Range(0f, 10f)] public int numeroDeIntentosCrearCaminoMax = 7;

    [Header("----------------")]
    [Header("PAR�METROS SEED")]
    [Header("Afecta el recorido de los caminos")]
    public int seed_siguientePasoDireccion = 3;
    [Header("Afecta la posible cantidad de bifurcaciones por chunk")]
    public int seed_cantBifurcaciones = 3;
    [Header("Afecta el origen de las bifurcaciones dentro del chunk")]
    public int seed_origenBifurcaciones = 0;
    [Header("Afecta la irrecularidad de los caminos")]
    public int seed_cantIrregularidadDeCamino = 0;



}
