using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{

    [SerializeField] private GameObject prefabCuboVerde;
    [SerializeField] private GameObject prefabCuboCafe;

    [Header("PARAMETROS")]
    [SerializeField] private int sizeChunks = 13;
    [SerializeField] private int cantPosObjetivosSeed = 1;
    [SerializeField] private int seedLado = 0;
    [SerializeField] private int seedUbicacionLado = 1;
    [SerializeField] private int seedFormaCaminos = 0;
    [SerializeField] private float randomness = 0.9f;

    private int[,] chunk;
    private Vector2Int posInicial;
    private List<Vector2Int> posObjetivos = new List<Vector2Int>();
    private List<Vector2Int> posObjetivosAux = new List<Vector2Int>();

    void Start()
    {
        GenerateChunk();
    }

    void GenerateChunk()
    {
        chunk = new int[sizeChunks, sizeChunks];
        posInicial = new Vector2Int(sizeChunks / 2, sizeChunks / 2);
        chunk[posInicial.x, posInicial.y] = 1;

        
        UnityEngine.Random.InitState(cantPosObjetivosSeed);
        int cantPosObjetivos = UnityEngine.Random.Range(1, 6); // Min 1, max 5

        Debug.Log("cantPosObjetivos: " + cantPosObjetivos);

        
        
        int incrementoSeed = 0;

        for (int posObjId = 0; posObjId < cantPosObjetivos; posObjId++)
        {
            UnityEngine.Random.InitState(seedLado + incrementoSeed);
            int lado = UnityEngine.Random.Range(0, 4); // 0: arriba, 1: derecha, 2: abajo, 3: izquierda

            UnityEngine.Random.InitState(seedUbicacionLado + incrementoSeed);
            int ubicacionLado = UnityEngine.Random.Range(1, sizeChunks - 1); // No en esquinas

            Vector2Int posObj = lado switch
            {
                0 => new Vector2Int(0, ubicacionLado),
                1 => new Vector2Int(ubicacionLado, sizeChunks - 1),
                2 => new Vector2Int(sizeChunks - 1, ubicacionLado),
                3 => new Vector2Int(ubicacionLado, 0),
                _ => posInicial // Caso no esperado
            };

            bool permitidoAgregar = true;
            foreach (var posObjVerificar in posObjetivos)
            {
                if (Mathf.Abs(posObj.x - posObjVerificar.x) + Mathf.Abs(posObj.y - posObjVerificar.y) <= 1)
                {
                    permitidoAgregar = false;
                    break;
                }
            }

            if (permitidoAgregar)
            {
                Debug.Log("posObj: " + posObj);
                posObjetivos.Add(posObj);
            }

            incrementoSeed++;
        }

        foreach (var posObj in posObjetivos)
        {
            chunk[posObj.x, posObj.y] = 1;

            Vector2Int posObjAux = posObj.x switch
            {
                0 => new Vector2Int(posObj.x + 1, posObj.y),
                var x when x == sizeChunks - 1 => new Vector2Int(posObj.x - 1, posObj.y),
                _ => posObj.y switch
                {
                    0 => new Vector2Int(posObj.x, posObj.y + 1),
                    var y when y == sizeChunks - 1 => new Vector2Int(posObj.x, posObj.y - 1),
                    _ => posObj // Caso no esperado
                }
            };

            Debug.Log("posObjAux: " + posObjAux);
            posObjetivosAux.Add(posObjAux);
            chunk[posObjAux.x, posObjAux.y] = 1;
        }

        
        UnityEngine.Random.InitState(seedFormaCaminos);
        

        foreach (var posObjAux in posObjetivosAux)
        {
            var pathPoints = ManhattanPathRandomSafe(posInicial, posObjAux, randomness);

            foreach (var point in pathPoints)
            {
                chunk[point.x, point.y] = 1;
            }
        }

        PrintChunk();
        ModelChunk();
    }

    List<Vector2Int> ManhattanPathRandomSafe(Vector2Int start, Vector2Int end, float randomness)
    {
        List<Vector2Int> points = new List<Vector2Int> { start };
        Vector2Int current = start;

        while (current != end)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            if (current.x < end.x) moves.Add(new Vector2Int(current.x + 1, current.y));
            if (current.x > end.x) moves.Add(new Vector2Int(current.x - 1, current.y));
            if (current.y < end.y) moves.Add(new Vector2Int(current.x, current.y + 1));
            if (current.y > end.y) moves.Add(new Vector2Int(current.x, current.y - 1));

            Vector2Int nextStep;
            if (UnityEngine.Random.value < randomness && moves.Count > 1)
                nextStep = moves[UnityEngine.Random.Range(0, moves.Count)];
            else
                nextStep = moves[0];

            current = nextStep;
            points.Add(current);
        }

        return points;
    }

    void PrintChunk()
    {
        string chunkString = "";
        for (int i = 0; i < sizeChunks; i++)
        {
            for (int j = 0; j < sizeChunks; j++)
            {
                chunkString += chunk[i, j] + " ";
            }
            chunkString += "\n";
        }
        Debug.Log(chunkString);
    }

    void ModelChunk()
    {
        if (prefabCuboVerde == null || prefabCuboCafe == null)
        {
            Debug.LogError("Asegúrate de asignar los prefabs en el inspector.");
            return;
        }

        // Crear un objeto vacío para agrupar los cubos
        GameObject chunkParent = new GameObject("Chunk");

        // Crear la base con cubos cafés
        for (int x = 0; x < sizeChunks; x++)
        {
            for (int y = 0; y < sizeChunks; y++)
            {
                Vector3 position = new Vector3(x, 0, y); // Base en y=0
                GameObject cuboCafe = Instantiate(prefabCuboCafe, position, Quaternion.identity);
                cuboCafe.transform.parent = chunkParent.transform; // Asignar el padre
            }
        }

        // Colocar los cubos verdes donde hay ceros en la matriz
        for (int x = 0; x < sizeChunks; x++)
        {
            for (int y = 0; y < sizeChunks; y++)
            {
                if (chunk[x, y] == 0)
                {
                    Vector3 position = new Vector3(x, 1, y); // Altura 1 sobre la base
                    GameObject cuboVerde = Instantiate(prefabCuboVerde, position, Quaternion.identity);
                    cuboVerde.transform.parent = chunkParent.transform; // Asignar el padre
                }
            }
        }
    }
}
