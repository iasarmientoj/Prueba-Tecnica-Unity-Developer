using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ChunkGenerator : MonoBehaviour
{

    [Header("Referencias")]
    [SerializeField] private ChunkGeneratorConfig parameters;
    [SerializeField] private GameObject prefabCuboVerde;
    [SerializeField] private GameObject prefabCuboCafe;
    [SerializeField] private GameObject chunksContainer;


    #region Definición de listas de direcciones de movimiento
    // Listas auxiliares para el funcionamiento del algoritmo

    // Determinar ubicaciones permitidas para continuar el camino desde la posición actual
    private List<(int, int)> vecinosIds = new List<(int, int)>
    {
    (-1, 0), // arriba
    (0, 1),  // derecha
    (1, 0),  // abajo
    (0, -1)  // izquierda
    };

    // Posiciones vecinas diagonales según la dirección en que vaya el camino
    private Dictionary<(int, int), List<(int, int)>> vecinosIdsDiag = new Dictionary<(int, int), List<(int, int)>>
    {
    { (-1, 0), new List<(int, int)> { (-1, -1), (-1, 1) } }, // dos diagonales de arriba
    { (0, 1), new List<(int, int)> { (-1, 1), (1, 1) } },    // dos diagonales de derecha
    { (1, 0), new List<(int, int)> { (1, -1), (1, 1) } },    // dos diagonales de abajo
    { (0, -1), new List<(int, int)> { (-1, -1), (1, -1) } }  // dos diagonales de izquierda
    };

    //Posiciones vecinas a cualquier chunk
    private List<(int, int)> vecinosChunksIds = new List<(int, int)>
    {
        (0, 1),   // arriba
        (1, 0),   // derecha
        (0, -1),  // abajo
        (-1, 0)   // izquierda
    };

    #endregion

    //variable de cantidad de bifurcaciones, aumenta a medida que aumentan los chunks
    private int cantBifurcacionesPosibles = 0;
    private int contadorDeChunks = 1;
    //diccionario que contiene todos los chunks generados y su coordenada correspondiente para poder reconstruirlos en unity
    private Dictionary<(int, int), List<List<int>>> dictChunksCoord = new Dictionary<(int, int), List<List<int>>>();

    private void Start()
    {
        GenerarChunks();
        ModelChunk();
    }

    public void RegenerarChunks()
    {
        DeleteChildren(chunksContainer);
        ResetVariables();
        GenerarChunks();
        ModelChunk();

    }

    private void ResetVariables()
    {
        dictChunksCoord.Clear();
        cantBifurcacionesPosibles = 0;
        contadorDeChunks = 1;
    }

    private void DeleteChildren(GameObject chunksContainer)
    {
        // Loop through all child objects
        for (int i = chunksContainer.transform.childCount - 1; i >= 0; i--)
        {
            // Destroy each child object
            Destroy(chunksContainer.transform.GetChild(i).gameObject);
        }
    }

    private List<(int, int)> EncontrarCamino(List<List<int>> chunkInput, List<(int, int)> posInicial, bool esBifurcacion, int offsetUnicoSemilla)
    {
        //hacer una copia del chunk o si no se conservan caminos fallidos
        List<List<int>> chunk = new List<List<int>>();
        foreach (var fila in chunkInput)
            chunk.Add(new List<int>(fila));

        int initId = 0;

        //List<List<int>> chunkOrig = chunk;

        //hacer una copia del chunk o si no se conservan caminos fallidos
        List<List<int>> chunkOrig = new List<List<int>>();
        foreach (var fila in chunkInput)
            chunkOrig.Add(new List<int>(fila));

        (int, int) posOrig = posInicial[initId];

        //PARAMETROS
        int seed_cantIrregularidadDeCamino = parameters.seed_global + (int)(parameters.cantIrregularidadDeCamino * 100);
        int seed_siguientePasoDireccion = parameters.seed_global;



        (int, int) posActual = posInicial[initId];
        chunk[posActual.Item1][posActual.Item2] = 1;



        List<(int, int)> caminoEncontrado = new List<(int, int)>();
        caminoEncontrado.Add(posActual);
        int incrementoSeed = 0;
        int incrementoSeedIrr = 0;


        //hallar los bordes de la matriz que ya tienen un 1 o fin de camino, para evitar terminar en el mismo borde
        List<(int, int)> bordesProhibidosList = new List<(int, int)>();
        List<(int, int)> bordesProhibidosListLongCamino = new List<(int, int)>();


        List<int> listChunkDer = new List<int>();
        foreach (List<int> fila in chunk)
            listChunkDer.Add(fila[chunk.Count - 1]);

        List<int> listChunkIzq = new List<int>();
        foreach (List<int> fila in chunk)
            listChunkIzq.Add(fila[0]);

        // Si hay un 1 en el borde superior, se prohíbe terminar en ese borde, y así con los demás bordes
        if (chunk[0].Contains(1)) // Borde superior
            for (int i = 0; i < chunk.Count; i++) //agregar todas las posiciones del borde de arriba
                bordesProhibidosList.Add((0, i));

        if (listChunkDer.Contains(1)) // Borde derecho
            for (int i = 0; i < chunk.Count; i++)
                bordesProhibidosList.Add((i, chunk.Count - 1));

        if (chunk[chunk.Count - 1].Contains(1)) // Borde inferior
            for (int i = 0; i < chunk.Count; i++)
                bordesProhibidosList.Add((chunk.Count - 1, i));

        if (listChunkIzq.Contains(1)) // Borde izquierdo
            for (int i = 0; i < chunk.Count; i++)
                bordesProhibidosList.Add((i, 0));


        int numeroDeIntentosCrearCamino = 0;
        int numeroDe1enMatriz = 0;




        while (posActual.Item1 != 0 && posActual.Item1 != chunk.Count - 1 && posActual.Item2 != 0 && posActual.Item2 != chunk[0].Count - 1)
        {
            List<(int, int)> posiblesPasosId = new List<(int, int)>();
            (int, int) posicionAnterior = (-1, -1);

            foreach ((int, int) vecinoId in vecinosIds)
            {
                //#por cada vecino hallar sus vecinos para evitar que el camino tome un camino junto al camino recorrido
                (int, int) posVecino = (posActual.Item1 + vecinoId.Item1, posActual.Item2 + vecinoId.Item2);

                //#vecinos directos que están dentro de la matriz
                if (posVecino.Item1 >= 0 && posVecino.Item1 < chunk.Count && posVecino.Item2 >= 0 && posVecino.Item2 < chunk[0].Count)
                {
                    //#si el vecino directo está libre, sin camino, puede ser candidato a poner camino por ahí
                    if (chunk[posVecino.Item1][posVecino.Item2] == 0)
                    {
                        // esta suma indica si los vecinos del vecino de la celda actual están ocupado con camino, para evitar hacer giros que toquen laterales de camino entre sí
                        int sumaVecinos2 = 0;
                        foreach ((int, int) vecinoId2 in vecinosIds)
                        {
                            (int, int) posVecino2 = (posVecino.Item1 + vecinoId2.Item1, posVecino.Item2 + vecinoId2.Item2);
                            if (posVecino2.Item1 >= 0 && posVecino2.Item1 < chunk.Count && posVecino2.Item2 >= 0 && posVecino2.Item2 < chunk[0].Count)
                                sumaVecinos2 += chunk[posVecino2.Item1][posVecino2.Item2];
                        }

                        // esta suma indica si los vecinos diagonales al vecino de la celda actual están ocupado con camino, para evitar que el camino se toque en esquinas
                        int sumaVecinos2Diag = 0;
                        foreach ((int, int) vecinoId2Diag in vecinosIdsDiag[vecinoId])
                        {
                            (int, int) posVecino2Diag = (posVecino.Item1 + vecinoId2Diag.Item1, posVecino.Item2 + vecinoId2Diag.Item2);
                            if (posVecino2Diag.Item1 >= 0 && posVecino2Diag.Item1 < chunk.Count && posVecino2Diag.Item2 >= 0 && posVecino2Diag.Item2 < chunk[0].Count)
                                sumaVecinos2Diag += chunk[posVecino2Diag.Item1][posVecino2Diag.Item2];
                        }

                        //#condicion para evitar giros de camino junto a caminos existentes, y que ningun camino se junte por las diagonales
                        if (sumaVecinos2 <= 1 && sumaVecinos2Diag == 0)
                        {
                            //#condicion para que no acabe en el mismo borde que ya esta ocupado
                            (int, int) posAbsolutaPaso = (posActual.Item1 + vecinoId.Item1, posActual.Item2 + vecinoId.Item2);
                            if (!bordesProhibidosList.Contains(posAbsolutaPaso) )
                                if (!bordesProhibidosListLongCamino.Contains(posAbsolutaPaso)) //#evitar caminos muy cortos
                                    posiblesPasosId.Add(vecinoId);
                        }
                    }
                    else
                    {
                        posicionAnterior = posVecino;
                    }
                }
            }



            List<(int, int)> posiblesPasosIdIrr = new List<(int, int)>();
            //#si no hay posibles pasos y no ha terminado el while, es que se quedó sin camino y toca recalcular el camino con otra semilla
            if (posiblesPasosId.Count == 0)
            {
                Debug.Log("CAMINO COLAPSADO, VOLVER A INTENTAR con otro seed");
                seed_siguientePasoDireccion++;

                if (esBifurcacion)
                {
                    initId++;
                }

                //#la bifurcacion no pudo iniciarse en ninguna celda del camino
                if (initId > posInicial.Count - 1) 
                    return new List<(int, int)>();

                chunk = chunkOrig;
                posActual = posInicial[initId];
                chunk[posActual.Item1][posActual.Item2] = 1;

                caminoEncontrado.Clear();
                caminoEncontrado.Add(posActual);

                incrementoSeed = 0;
                incrementoSeedIrr = 0;

                numeroDeIntentosCrearCamino++;

            }
            else
            {

                // Hallar posición donde sería el camino recto
                if (posicionAnterior.Item1 != -1)
                {
                    (int,int) direccionRecta = (posActual.Item1 - posicionAnterior.Item1, posActual.Item2 - posicionAnterior.Item2);

                    // Elimina los posibles pasos dependiendo de la cantidad de irregularidad que se desee
                    if (posiblesPasosId.Count > 1)
                    {
                        // Si cantIrregularidadDeCamino es baja, es probable eliminar los pasos diferentes a la dirección recta
                        if (parameters.cantIrregularidadDeCamino < 0.5f)
                        {
                            incrementoSeedIrr = 0;
                            foreach ((int, int) posiblePaso in posiblesPasosId)
                            {

                                Random.InitState(seed_cantIrregularidadDeCamino + incrementoSeedIrr + offsetUnicoSemilla);
                                if (posiblePaso.Equals(direccionRecta))
                                {
                                    posiblesPasosIdIrr.Add(posiblePaso);
                                }
                                else if (!posiblePaso.Equals(direccionRecta) && Random.value < parameters.cantIrregularidadDeCamino)
                                {
                                    posiblesPasosIdIrr.Add(posiblePaso);
                                }
                                incrementoSeedIrr++;
                            }
                        }
                        // Si cantIrregularidadDeCamino es alta, es probable eliminar la dirección recta
                        else
                        {
                            incrementoSeedIrr = 0;
                            float probEliminarPaso = (parameters.cantIrregularidadDeCamino - 0.5f) * 2;
                            foreach ((int, int) posiblePaso in posiblesPasosId)
                            {
                                Random.InitState(seed_cantIrregularidadDeCamino + incrementoSeedIrr);
                                if (posiblePaso.Equals(direccionRecta) && Random.value > probEliminarPaso)
                                {
                                    posiblesPasosIdIrr.Add(posiblePaso);
                                }
                                else if (!posiblePaso.Equals(direccionRecta))
                                {
                                    posiblesPasosIdIrr.Add(posiblePaso);
                                }
                                incrementoSeedIrr++;
                            }
                        }
                    }
                    else
                    {
                        posiblesPasosIdIrr = posiblesPasosId;
                    }
                }
                else
                {
                    posiblesPasosIdIrr = posiblesPasosId;
                }

                // Si es bifurcación, no tener en cuenta tantas reglas
                if (esBifurcacion)
                    posiblesPasosIdIrr = posiblesPasosId;

                // Si no hay posibles pasos y no ha terminado el while, es que se quedó sin camino y toca recalcular el camino con otra semilla
                if (posiblesPasosIdIrr.Count == 0)
                {
                    Debug.Log("CAMINO COLAPSADO, VOLVER A INTENTAR con otro seed posiblesPasosIdIrr");
                    seed_siguientePasoDireccion++;

                    // Si es bifurcación recalcular la bifurcación desde otro punto de inicio
                    if (esBifurcacion)
                    {
                        initId++;
                    }

                    // La bifurcación no pudo iniciarse en ninguna celda del camino
                    if (initId > posInicial.Count - 1)
                    {
                        return new List<(int, int)>();
                    }

                    chunk = chunkOrig;
                    posActual = posInicial[initId];
                    chunk[posActual.Item1][posActual.Item2] = 1;

                    caminoEncontrado.Clear();
                    caminoEncontrado.Add(posActual);
                    incrementoSeed = 0;
                    incrementoSeedIrr = 0;

                    numeroDeIntentosCrearCamino++;
                }
                else
                {
                    Random.InitState((int)(seed_siguientePasoDireccion + posActual.Item1 + posActual.Item2 + incrementoSeed + offsetUnicoSemilla));
                    incrementoSeed++;
                    //seleccionar aleatoriamente el siguiente paso dentro de los pasos posibles para ir armando el camino
                    (int, int) siguientePasoDireccion = posiblesPasosIdIrr[Random.Range(0, posiblesPasosIdIrr.Count-1)];
                    posActual = (posActual.Item1 + siguientePasoDireccion.Item1, posActual.Item2 + siguientePasoDireccion.Item2);
                    caminoEncontrado.Add(posActual);
                    chunk[posActual.Item1][posActual.Item2] = 1;
                }

            }


            if (numeroDeIntentosCrearCamino > parameters.numeroDeIntentosCrearCaminoMax)
                return new List<(int, int)>();



            // PARA EVITAR CAMINOS CORTOS
            // calcular la Cantidad de 1 (unos) en la matriz
            numeroDe1enMatriz = 0;
            foreach (List<int> fila in chunk)
                foreach (int celda in fila)
                    if (celda == 1)
                        numeroDe1enMatriz++;


            // Si la longitud es muy corta, tiene prohibido los bordes
            if (numeroDe1enMatriz < parameters.longMinCamino)
            {
                // Agregar todas las posiciones de todos los bordes, evitar el borde de grosor 2
                for (int i = 0; i < chunk.Count; i++) // Borde superior
                {
                    bordesProhibidosListLongCamino.Add((0, i));
                    bordesProhibidosListLongCamino.Add((1, i));
                }
                for (int i = 0; i < chunk.Count; i++) // Borde derecho
                {
                    bordesProhibidosListLongCamino.Add((i, chunk.Count - 1));
                    bordesProhibidosListLongCamino.Add((i, chunk.Count - 2));
                }
                for (int i = 0; i < chunk.Count; i++) // Borde inferior
                {
                    bordesProhibidosListLongCamino.Add((chunk.Count - 1, i));
                    bordesProhibidosListLongCamino.Add((chunk.Count - 2, i));
                }
                for (int i = 0; i < chunk.Count; i++) // Borde izquierdo
                {
                    bordesProhibidosListLongCamino.Add((i, 0));
                    bordesProhibidosListLongCamino.Add((i, 1));
                }
            }
            else
            {
                bordesProhibidosListLongCamino.Clear();
            }



        }

        return caminoEncontrado;


    }

    
    private List<List<int>> PrintChunk(List<List<int>> chunk, List<(int, int)> caminoEncontrado)
    {
        foreach ((int, int) posCamino in caminoEncontrado)
        {
            chunk[posCamino.Item1][posCamino.Item2] = 1;
        }

        // Imprimir el chunk en la consola
        string printChunk = "chunk\n";
        foreach (List<int> row in chunk)
            printChunk += string.Join(", ", row) + "\n";

         Debug.Log(printChunk);

        return chunk;
    }


    private List<List<int>> GenerarBifurcacionese(List<(int, int)> caminoEncontrado, int cantBifurcacionesPosibles, List<List<int>> chunkInput, int offsetUnicoSemilla)
    {

        //hacer una copia del chunk o si no se conservan caminos fallidos
        List<List<int>> chunk = new List<List<int>>();
        foreach (var fila in chunkInput)
            chunk.Add(new List<int>(fila));



        Random.InitState(parameters.seed_global + caminoEncontrado.Count + offsetUnicoSemilla);
        int cantBifurcaciones = Random.Range(0, cantBifurcacionesPosibles + 1);
        Debug.Log("cantBifurcaciones: " + cantBifurcaciones);

        // Agregar bifurcaciones si las hay
        for (int num = 0; num < cantBifurcaciones; num++)
        {
            // Tomar una posición aleatoria de los caminos existentes
            List<(int, int)> posicionesCaminosExistentes = new List<(int, int)>();

            for (int filaCount = 0; filaCount < chunk.Count; filaCount++)
            {
                for (int celdaCount = 0; celdaCount < chunk[filaCount].Count; celdaCount++)
                {
                    if (chunk[filaCount][celdaCount] == 1)
                    {
                        if (filaCount > (int)(chunk.Count/4) && filaCount < chunk.Count - (int)(chunk.Count/4) && celdaCount > (int)(chunk.Count/4) && celdaCount < chunk.Count - (int)(chunk.Count/4)) //#si son celdas del centro
                        {
                            posicionesCaminosExistentes.Add((filaCount, celdaCount));
                        }
                    }
                }
            }

            // Si no hay celdas en el centro para escoger, tomar cualquier celda
            if (posicionesCaminosExistentes.Count == 0)
            {
                for (int filaCount = 0; filaCount < chunk.Count; filaCount++)
                {
                    for (int celdaCount = 0; celdaCount < chunk[filaCount].Count; celdaCount++)
                    {
                        if (chunk[filaCount][celdaCount] == 1)
                        {
                            posicionesCaminosExistentes.Add((filaCount, celdaCount));
                        }
                    }
                }
            }

            // Barajar las posiciones de los caminos existentes
            Random.InitState(parameters.seed_global + offsetUnicoSemilla);
            posicionesCaminosExistentes = posicionesCaminosExistentes.OrderBy(_ => Random.value).ToList();

            // Encontrar y añadir el camino
            caminoEncontrado = EncontrarCamino(chunk, posicionesCaminosExistentes, true , offsetUnicoSemilla);
            chunk = PrintChunk(chunk, caminoEncontrado);
        }

        return chunk;
    }


    private List<(int, int)> ObtenerCoordenadasEn1(List<List<int>> matrix)
    {
        List<(int, int)> coordinates = new List<(int, int)>();

        for (int i = 0; i < matrix.Count; i++) // Filas
        {
            for (int j = 0; j < matrix.Count; j++) // Columnas
            {
                if (matrix[i][j] == 1) // Verificamos si es un 1
                {
                    coordinates.Add((i, j));
                }
            }
        }

        return coordinates;
    }


    private void GenerarChunks()
    {
        // PARA EL PRIMER CHUNK
        // PARA EL PRIMER CHUNK
        // PARA EL PRIMER CHUNK

        //inicializar el chunk con ceros
        List<List<int>> chunk = new List<List<int>>(parameters.sizeChunks);
        for (int i = 0; i < parameters.sizeChunks; i++)
            chunk.Add(new List<int>(new int[parameters.sizeChunks]));
        //la posición inicial del primer chunk es en el centro
        List<(int, int)> posInicial = new List<(int, int)> { ((int)(chunk.Count / 2), (int)(chunk.Count / 2)) };


        //generar camino principal
        List<(int, int)> caminoEncontrado = EncontrarCamino(chunk, posInicial, false , 0);
        chunk = PrintChunk(chunk, caminoEncontrado);
        //generar posibles bifurcaciones
        chunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, chunk, 0);

        //agregar el chunk generado al diccionario de chunks
        dictChunksCoord[(0, 0)] = chunk;
        //Debug.Log(dictChunksCoord[(0, 0)]);


        //PARA LOS DEMAS CHUNK
        //PARA LOS DEMAS CHUNK
        //PARA LOS DEMAS CHUNK

        //PARA CADA CHUNK RESUELTO GENERAR SUS VECINOS


        while (contadorDeChunks < parameters.cantidadTotalDeChunks)
        {
            foreach (var kvp in dictChunksCoord.ToList())
            {
                (int, int) coordResuelto = kvp.Key;
                List<List<int>> chunkResuelto = kvp.Value;

                foreach ((int, int) vecinoChunkId in vecinosChunksIds)
                {
                    (int, int) coordAbsolutas = (coordResuelto.Item1 + vecinoChunkId.Item1, coordResuelto.Item2 + vecinoChunkId.Item2);
                    int offsetUnicoSemilla = coordAbsolutas.Item1 + coordAbsolutas.Item2;

                    //# si el vecino no existe en el diccionario de chunks, puede proceder a generarlo si es el caso
                    if (!dictChunksCoord.ContainsKey(coordAbsolutas))
                    {

                        // Evaluar el chunk vecino de arriba
                        if (vecinoChunkId == (0, 1))
                        {
                            //#si un camino terminó en la parte superior del chunk resuelto
                            if (chunkResuelto[0].Contains(1))
                            {
                                Debug.Log("Parte superior");
                                //inicializar el chunk con ceros
                                List<List<int>> newChunk = new List<List<int>>(parameters.sizeChunks);
                                for (int i = 0; i < parameters.sizeChunks; i++)
                                    newChunk.Add(new List<int>(new int[parameters.sizeChunks]));

                                //#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
                                for (int i = 0; i < chunkResuelto.Count - 1; i++)
                                {
                                    newChunk[newChunk.Count - 1][ i] = chunkResuelto[0][ i];
                                    newChunk[newChunk.Count - 2][ i] = chunkResuelto[0][ i];
                                }

                                List<(int, int)> coordenadasEn1 = ObtenerCoordenadasEn1(newChunk);

                                //#la celda inicial es la siguiente a la del borde
                                posInicial = new List<(int, int)>();
                                foreach ((int, int) coor1 in coordenadasEn1)
                                {
                                    // Verificar si la coordenada no está en los bordes del chunk
                                    if (coor1.Item1 != 0 && coor1.Item1 != newChunk.Count - 1 && coor1.Item2 != 0 && coor1.Item2 != newChunk.Count - 1)
                                    {
                                        // Agregar la coordenada válida a la lista
                                        posInicial.Add((coor1.Item1, coor1.Item2));
                                    }
                                }
                                Debug.Log($"posInicial: {string.Join(", ", posInicial)}");


                                caminoEncontrado = EncontrarCamino(newChunk, posInicial, false, offsetUnicoSemilla);
                                PrintChunk(newChunk, caminoEncontrado);
                                newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk, offsetUnicoSemilla);

                                dictChunksCoord[coordAbsolutas] = newChunk;

                                contadorDeChunks++;
                                if (contadorDeChunks >= parameters.cantidadTotalDeChunks) 
                                    break;
                            }
                        }

                        // Evaluar el chunk vecino de abajo
                        if (vecinoChunkId == (0, -1))
                        {
                            if (chunkResuelto[chunkResuelto.Count - 1].Contains(1))
                            {
                                Debug.Log("Parte inferior");
                                //inicializar el chunk con ceros
                                List<List<int>> newChunk = new List<List<int>>(parameters.sizeChunks);
                                for (int i = 0; i < parameters.sizeChunks; i++)
                                    newChunk.Add(new List<int>(new int[parameters.sizeChunks]));

                                for (int i = 0; i < chunkResuelto.Count - 1; i++)
                                {
                                    newChunk[0][ i] = chunkResuelto[newChunk.Count - 1][ i];
                                    newChunk[1][ i] = chunkResuelto[newChunk.Count - 1][ i];
                                }

                                List<(int, int)> coordenadasEn1 = ObtenerCoordenadasEn1(newChunk);

                                //#la celda inicial es la siguiente a la del borde
                                posInicial = new List<(int, int)>();
                                foreach ((int, int) coor1 in coordenadasEn1)
                                {
                                    // Verificar si la coordenada no está en los bordes del chunk
                                    if (coor1.Item1 != 0 && coor1.Item1 != newChunk.Count - 1 && coor1.Item2 != 0 && coor1.Item2 != newChunk.Count - 1)
                                    {
                                        // Agregar la coordenada válida a la lista
                                        posInicial.Add((coor1.Item1, coor1.Item2));
                                    }
                                }
                                Debug.Log($"posInicial: {string.Join(", ", posInicial)}");

                                caminoEncontrado = EncontrarCamino(newChunk, posInicial, false, offsetUnicoSemilla);
                                PrintChunk(newChunk, caminoEncontrado);
                                newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk, offsetUnicoSemilla);

                                dictChunksCoord[coordAbsolutas] = newChunk;

                                contadorDeChunks++;
                                if (contadorDeChunks >= parameters.cantidadTotalDeChunks) 
                                    break;
                            }
                        }

                        // Evaluar el chunk vecino de la derecha
                        if (vecinoChunkId == (1, 0))
                        {
                            //#si un camino terminó en la parte derecha del chunk resuelto
                            List<int> listChunkDer = new List<int>();
                            foreach (List<int> fila in chunkResuelto)
                                listChunkDer.Add(fila[chunkResuelto.Count - 1]);


                            if (listChunkDer.Contains(1))
                            {
                                Debug.Log("Parte derecha");
                                //inicializar el chunk con ceros
                                List<List<int>> newChunk = new List<List<int>>(parameters.sizeChunks);
                                for (int i = 0; i < parameters.sizeChunks; i++)
                                    newChunk.Add(new List<int>(new int[parameters.sizeChunks]));

                                for (int i = 0; i < chunkResuelto.Count - 1; i++)
                                {
                                    //#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
                                    newChunk[i][ 0] = chunkResuelto[i][chunkResuelto.Count - 1];
                                    newChunk[i][ 1] = chunkResuelto[i][chunkResuelto.Count - 1];
                                }

                                List<(int, int)> coordenadasEn1 = ObtenerCoordenadasEn1(newChunk);

                                //#la celda inicial es la siguiente a la del borde
                                posInicial = new List<(int, int)>();
                                foreach ((int, int) coor1 in coordenadasEn1)
                                {
                                    // Verificar si la coordenada no está en los bordes del chunk
                                    if (coor1.Item1 != 0 && coor1.Item1 != newChunk.Count - 1 && coor1.Item2 != 0 && coor1.Item2 != newChunk.Count - 1)
                                    {
                                        // Agregar la coordenada válida a la lista
                                        posInicial.Add((coor1.Item1, coor1.Item2));
                                    }
                                }
                                Debug.Log($"posInicial: {string.Join(", ", posInicial)}");

                                caminoEncontrado = EncontrarCamino(newChunk, posInicial, false, offsetUnicoSemilla);
                                PrintChunk(newChunk, caminoEncontrado);
                                newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk, offsetUnicoSemilla);

                                dictChunksCoord[coordAbsolutas] = newChunk;

                                contadorDeChunks++;
                                if (contadorDeChunks >= parameters.cantidadTotalDeChunks) 
                                    break;
                            }
                        }

                        // Evaluar el chunk vecino de la izquierda
                        if (vecinoChunkId == (-1, 0))
                        {
                            List<int> listChunkIzq = new List<int>();
                            foreach (List<int> fila in chunkResuelto)
                                listChunkIzq.Add(fila[0]);

                            if (listChunkIzq.Contains(1))
                            {
                                Debug.Log("Parte izquierda");
                                //inicializar el chunk con ceros
                                List<List<int>> newChunk = new List<List<int>>(parameters.sizeChunks);
                                for (int i = 0; i < parameters.sizeChunks; i++)
                                    newChunk.Add(new List<int>(new int[parameters.sizeChunks]));

                                for (int i = 0; i < chunkResuelto.Count - 1; i++)
                                {
                                    newChunk[i][chunkResuelto.Count - 1] = chunkResuelto[i][ 0];
                                    newChunk[i][chunkResuelto.Count - 2] = chunkResuelto[i][ 0];
                                }

                                List<(int, int)> coordenadasEn1 = ObtenerCoordenadasEn1(newChunk);

                                //#la celda inicial es la siguiente a la del borde
                                posInicial = new List<(int, int)>();
                                foreach ((int, int) coor1 in coordenadasEn1)
                                {
                                    // Verificar si la coordenada no está en los bordes del chunk
                                    if (coor1.Item1 != 0 && coor1.Item1 != newChunk.Count - 1 && coor1.Item2 != 0 && coor1.Item2 != newChunk.Count - 1)
                                    {
                                        // Agregar la coordenada válida a la lista
                                        posInicial.Add((coor1.Item1, coor1.Item2));
                                    }
                                }
                                Debug.Log($"posInicial: {string.Join(", ", posInicial)}");

                                caminoEncontrado = EncontrarCamino(newChunk, posInicial, false, offsetUnicoSemilla);
                                PrintChunk(newChunk, caminoEncontrado);
                                newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk, offsetUnicoSemilla);

                                dictChunksCoord[coordAbsolutas] = newChunk;

                                contadorDeChunks++;
                                if (contadorDeChunks >= parameters.cantidadTotalDeChunks) 
                                    break;
                            }
                        }
                    }
                }



                cantBifurcacionesPosibles += 1;

                if (cantBifurcacionesPosibles > parameters.cantBifurcacionesPosiblesMax)
                    cantBifurcacionesPosibles = parameters.cantBifurcacionesPosiblesMax;


                if (contadorDeChunks >= parameters.cantidadTotalDeChunks) 
                    break;
            }

            if (contadorDeChunks >= parameters.cantidadTotalDeChunks) 
                break;
        }






    }


    private void ModelChunk()
    {

        // por cada chunk del diccionario
        foreach (var kvp in dictChunksCoord.ToList())
        {
            (int, int) coordResuelto = kvp.Key;
            List<List<int>> chunkResuelto = kvp.Value;

            // Crear un objeto vacío para agrupar los cubos
            GameObject chunkParent = new GameObject("Chunk ("+ coordResuelto.Item1.ToString()+","+ coordResuelto.Item2.ToString() + ")");
            chunkParent.transform.position = new Vector3(coordResuelto.Item2* parameters.sizeChunks, 0 , coordResuelto.Item1* parameters.sizeChunks);

            // Crear la base con cubos cafés
            for (int x = 0; x < parameters.sizeChunks; x++)
            {
                for (int y = 0; y < parameters.sizeChunks; y++)
                {
                    Vector3 position = new Vector3(x - chunkParent.transform.position.x, 0, y + chunkParent.transform.position.z); // Base en y=0
                    GameObject cuboCafe = Instantiate(prefabCuboCafe, position, Quaternion.identity);
                    cuboCafe.transform.parent = chunkParent.transform; // Asignar el padre
                }
            }

            // Colocar los cubos verdes donde hay ceros en la matriz
            for (int x = 0; x < parameters.sizeChunks; x++)
            {
                for (int y = 0; y < parameters.sizeChunks; y++)
                {
                    if (chunkResuelto[x][y] == 0)
                    {
                        Vector3 position = new Vector3(x - chunkParent.transform.position.x, 1, y + chunkParent.transform.position.z); // Altura 1 sobre la base
                        GameObject cuboVerde = Instantiate(prefabCuboVerde, position, Quaternion.identity);
                        cuboVerde.transform.parent = chunkParent.transform; // Asignar el padre
                    }
                }
            }

            chunkParent.transform.parent = chunksContainer.transform; // Asignar el padre de todos los chunks


        }


    }



}