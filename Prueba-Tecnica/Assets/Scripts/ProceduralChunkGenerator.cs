using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProceduralsChunkGenerator : MonoBehaviour
{

    [Header("Referencias")]
    [SerializeField] private ChunkGeneratorConfig parameters;
    [SerializeField] private GameObject prefabCuboVerde;
    [SerializeField] private GameObject prefabCuboCafe;
    [SerializeField] private GameObject chunksContainer;


    #region Definición de listas de direcciones de movimiento
    // Listas auxiliares para el funcionamiento del algoritmo

    // Determinar ubicaciones permitidas para continuar el camino desde la posición actual
    private List<Vector2Int> vecinosIds = new List<Vector2Int>
    {
    new Vector2Int(-1, 0), // arriba
    new Vector2Int(0, 1),  // derecha
    new Vector2Int(1, 0),  // abajo
    new Vector2Int(0, -1)  // izquierda
    };

    // Posiciones vecinas diagonales según la dirección en que vaya el camino
    private Dictionary<Vector2Int, List<Vector2Int>> vecinosIdsDiag = new Dictionary<Vector2Int, List<Vector2Int>>
    {
    { new Vector2Int(-1, 0), new List<Vector2Int> { new Vector2Int(-1, -1), new Vector2Int(-1, 1) } }, // dos diagonales de arriba
    { new Vector2Int(0, 1), new List<Vector2Int> { new Vector2Int(-1, 1), new Vector2Int(1, 1) } },    // dos diagonales de derecha
    { new Vector2Int(1, 0), new List<Vector2Int> { new Vector2Int(1, -1), new Vector2Int(1, 1) } },    // dos diagonales de abajo
    { new Vector2Int(0, -1), new List<Vector2Int> { new Vector2Int(-1, -1), new Vector2Int(1, -1) } }  // dos diagonales de izquierda
    };

    //Posiciones vecinas a cualquier chunk
    private List<Vector2Int> vecinosChunksIds = new List<Vector2Int>
    {
        new Vector2Int(0, 1),   // arriba
        new Vector2Int(1, 0),   // derecha
        new Vector2Int(0, -1),  // abajo
        new Vector2Int(-1, 0)   // izquierda
    };

    #endregion

    //variable de cantidad de bifurcaciones, aumenta a medida que aumentan los chunks
    private int cantBifurcacionesPosibles = 0;
    private int contadorDeChunks = 1;
    //diccionario que contiene todos los chunks generados y su coordenada correspondiente para poder reconstruirlos en unity
    private Dictionary<Vector2Int, int[,]> dictChunksCoord = new Dictionary<Vector2Int, int[,]>();


    private void Start()
    {
        RegenerarChunks();
    }

    public void RegenerarChunks()
    {
        EliminarHijosContenedor(chunksContainer);
        ResetVariables();
        GenerarAllChunks();
    }



    private void EliminarHijosContenedor(GameObject chunksContainer)
    {
        for (int i = chunksContainer.transform.childCount - 1; i >= 0; i--) // Loop through all child objects
            Destroy(chunksContainer.transform.GetChild(i).gameObject); // Destroy each child object
    }
    private void ResetVariables()
    {
        dictChunksCoord.Clear();
        cantBifurcacionesPosibles = 0;
        contadorDeChunks = 1;
    }
    private void GenerarAllChunks()
    {
        GenerarChunkBase();
        GenerarLosDemasChunks();
    }



    private void GenerarChunkBase()
    {
        //inicializar el chunk con ceros y el tamaño correcto
        int[,] chunk = new int[parameters.sizeChunks, parameters.sizeChunks];
        //la posición inicial del primer chunk es en el centro
        List<Vector2Int> posInicial = new List<Vector2Int> { new Vector2Int(chunk.GetLength(0) / 2, chunk.GetLength(0) / 2) };

        //generar camino principal
        List<Vector2Int> caminoEncontrado = GenerarCamino(chunk, posInicial, false, 0);
        chunk = ConsolidarChunk(chunk, caminoEncontrado);
        //generar posibles bifurcaciones
        chunk = GenerarBifurcaciones(caminoEncontrado, cantBifurcacionesPosibles, chunk, 0);

        //agregar el chunk generado al diccionario de chunks
        dictChunksCoord[new Vector2Int(0, 0)] = chunk;
        ModelarChunk(new Vector2Int(0, 0));
    }
    private void GenerarLosDemasChunks()
    {
        while (contadorDeChunks < parameters.cantidadTotalDeChunks)
        {
            //PARA CADA CHUNK RESUELTO GENERAR SUS VECINOS si no estan ocupados por otro chunk
            foreach (var kvp in dictChunksCoord.ToList())
            {
                Vector2Int coordResuelto = kvp.Key;
                int[,] chunkResuelto = kvp.Value;


                Vector2Int vecinoChunkId;
                Vector2Int coordAbsolutas;
                int offsetUnicoSemilla;

                //revisar los 4 lados del chunk presente

                // Evaluar el chunk vecino de arriba
                vecinoChunkId = new Vector2Int(0, 1);
                coordAbsolutas = new Vector2Int(coordResuelto.x + vecinoChunkId.x, coordResuelto.y + vecinoChunkId.y);
                offsetUnicoSemilla = coordAbsolutas.x + coordAbsolutas.y;

                List<int> listChunkSup = GetlistChunkSup(chunkResuelto);
                //si un camino terminó en la parte superior del chunk resuelto y si el vecino no existe en el diccionario de chunks, puede proceder a generarlo si es el caso
                if (listChunkSup.Contains(1) && !dictChunksCoord.ContainsKey(coordAbsolutas))
                {
                    Debug.Log("Generar chunk vecino: Parte superior");
                    //inicializar el chunk con ceros
                    int[,] newChunk = new int[parameters.sizeChunks, parameters.sizeChunks];

                    //copiar el borde para empalmar en las dos primeras filas para alejarlo del borde, que siga derecho una celda es el unico paso posible para que no toque el borde
                    for (int i = 0; i < chunkResuelto.GetLength(0) - 1; i++)
                    {
                        newChunk[newChunk.GetLength(0) - 1, i] = chunkResuelto[0, i];
                        newChunk[newChunk.GetLength(0) - 2, i] = chunkResuelto[0, i];
                    }

                    GenerarChunkVecino(newChunk, offsetUnicoSemilla, coordAbsolutas);

                    contadorDeChunks++;
                    if (contadorDeChunks >= parameters.cantidadTotalDeChunks)
                        break;
                }


                // Evaluar el chunk vecino de abajo
                vecinoChunkId = new Vector2Int(0, -1);
                coordAbsolutas = new Vector2Int(coordResuelto.x + vecinoChunkId.x, coordResuelto.y + vecinoChunkId.y);
                offsetUnicoSemilla = coordAbsolutas.x + coordAbsolutas.y;

                List<int> listChunkInf = GetlistChunkInf(chunkResuelto);
                if (listChunkInf.Contains(1) && !dictChunksCoord.ContainsKey(coordAbsolutas))
                {
                    Debug.Log("Generar chunk vecino: Parte inferior");
                    //inicializar el chunk con ceros
                    int[,] newChunk = new int[parameters.sizeChunks, parameters.sizeChunks];

                    for (int i = 0; i < chunkResuelto.GetLength(0) - 1; i++)
                    {
                        newChunk[0, i] = chunkResuelto[newChunk.GetLength(0) - 1, i];
                        newChunk[1, i] = chunkResuelto[newChunk.GetLength(0) - 1, i];
                    }

                    GenerarChunkVecino(newChunk, offsetUnicoSemilla, coordAbsolutas);

                    contadorDeChunks++;
                    if (contadorDeChunks >= parameters.cantidadTotalDeChunks)
                        break;
                }


                // Evaluar el chunk vecino de la derecha
                vecinoChunkId = new Vector2Int(1, 0);
                coordAbsolutas = new Vector2Int(coordResuelto.x + vecinoChunkId.x, coordResuelto.y + vecinoChunkId.y);
                offsetUnicoSemilla = coordAbsolutas.x + coordAbsolutas.y;

                List<int> listChunkDer = GetlistChunkDer(chunkResuelto);
                //si un camino terminó en la parte derecha del chunk resuelto
                if (listChunkDer.Contains(1) && !dictChunksCoord.ContainsKey(coordAbsolutas))
                {
                    Debug.Log("Generar chunk vecino: Parte derecha");
                    //inicializar el chunk con ceros
                    int[,] newChunk = new int[parameters.sizeChunks, parameters.sizeChunks];

                    for (int i = 0; i < chunkResuelto.GetLength(0) - 1; i++)
                    {
                        //#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
                        newChunk[i, 0] = chunkResuelto[i, chunkResuelto.GetLength(0) - 1];
                        newChunk[i, 1] = chunkResuelto[i, chunkResuelto.GetLength(0) - 1];
                    }

                    GenerarChunkVecino(newChunk, offsetUnicoSemilla, coordAbsolutas);

                    contadorDeChunks++;
                    if (contadorDeChunks >= parameters.cantidadTotalDeChunks)
                        break;
                }


                // Evaluar el chunk vecino de la izquierda
                vecinoChunkId = new Vector2Int(-1, 0);
                coordAbsolutas = new Vector2Int(coordResuelto.x + vecinoChunkId.x, coordResuelto.y + vecinoChunkId.y);
                offsetUnicoSemilla = coordAbsolutas.x + coordAbsolutas.y;

                List<int> listChunkIzq = GetlistChunkIzq(chunkResuelto);
                if (listChunkIzq.Contains(1) && !dictChunksCoord.ContainsKey(coordAbsolutas))
                {
                    Debug.Log("Generar chunk vecino: Parte izquierda");
                    //inicializar el chunk con ceros
                    int[,] newChunk = new int[parameters.sizeChunks, parameters.sizeChunks];

                    for (int i = 0; i < chunkResuelto.GetLength(0) - 1; i++)
                    {
                        newChunk[i, chunkResuelto.GetLength(0) - 1] = chunkResuelto[i, 0];
                        newChunk[i, chunkResuelto.GetLength(0) - 2] = chunkResuelto[i, 0];
                    }

                    GenerarChunkVecino(newChunk, offsetUnicoSemilla, coordAbsolutas);

                    contadorDeChunks++;
                    if (contadorDeChunks >= parameters.cantidadTotalDeChunks)
                        break;
                }



                //aumentar la probabilidad de haber bifurcaciones
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



    private List<Vector2Int> GenerarCamino(int[,] chunkInput, List<Vector2Int> posInicial, bool esBifurcacion, int offsetUnicoSemilla)
    {

        #region Parámetros y variables

        //Semillas con ligeros cambios con propiedades unicas de chunk para obtener resultados diferentes en cada chunk
        int seed_cantIrregularidadDeCamino = parameters.seed_global + (int)(parameters.cantIrregularidadDeCamino * 100) + offsetUnicoSemilla;
        int seed_siguientePasoDireccion = parameters.seed_global + offsetUnicoSemilla + (int)(parameters.cantIrregularidadDeCamino * 100) + (int)(parameters.longMinCamino * 100);

        //hacer una copia del chunk o si no se conservan caminos fallidos
        int[,] chunk = HacerCopiaChunk(chunkInput);

        //hacer una copia del chunk o si no se conservan caminos fallidos
        int[,] chunkOrig = HacerCopiaChunk(chunkInput);

        int initId = 0;
        Vector2Int posOrig = posInicial[initId];
        Vector2Int posActual = posInicial[initId];
        chunk[posActual.x,posActual.y] = 1;

        List<Vector2Int> caminoEncontrado = new List<Vector2Int>();
        caminoEncontrado.Add(posActual);
        int incrementoSeed = 0;
        int incrementoSeedIrr = 0;

        //hallar los bordes de la matriz que ya tienen un 1 o fin de camino, para evitar terminar en el mismo borde
        List<Vector2Int> bordesProhibidosList = new List<Vector2Int>();
        List<Vector2Int> bordesProhibidosListLongCamino = new List<Vector2Int>();

        int numeroDeIntentosCrearCamino = 0;

        #endregion


        #region Obtener bordes del chunk

        //obtener los bordes del chunk para validar si tienen fines de canimo
        List<int> listChunkSup = GetlistChunkSup(chunk);
        List<int> listChunkInf = GetlistChunkInf(chunk);
        List<int> listChunkDer = GetlistChunkDer(chunk);
        List<int> listChunkIzq = GetlistChunkIzq(chunk);

        #endregion


        #region Validar bordes del chunk

        // Si hay un 1 en el borde superior, se prohíbe terminar en ese borde, y así con los demás bordes
        if (listChunkSup.Contains(1)) // Borde superior
            for (int i = 0; i < chunk.GetLength(0); i++) //agregar todas las posiciones del borde de arriba
                bordesProhibidosList.Add(new Vector2Int (0, i));

        if (listChunkDer.Contains(1)) // Borde derecho
            for (int i = 0; i < chunk.GetLength(0); i++)
                bordesProhibidosList.Add(new Vector2Int(i, chunk.GetLength(0) - 1));

        if (listChunkInf.Contains(1)) // Borde inferior
            for (int i = 0; i < chunk.GetLength(0); i++)
                bordesProhibidosList.Add(new Vector2Int(chunk.GetLength(0) - 1, i));

        if (listChunkIzq.Contains(1)) // Borde izquierdo
            for (int i = 0; i < chunk.GetLength(0); i++)
                bordesProhibidosList.Add(new Vector2Int(i, 0));

        #endregion

        //mientras el camino no haya llegado a un borde del chunk
        while (posActual.x != 0 && posActual.x != chunk.GetLength(0) - 1 && posActual.y != 0 && posActual.y != chunk.GetLength(0) - 1)
        {

            // para la celda actual hallar las posibles direcciones a las que se puede mover y almacenarlas en posiblesPasosId
            var (posiblesPasosId, posicionAnterior) = CalcularPosiblesPasosDelCamino(chunk, posActual, bordesProhibidosList, bordesProhibidosListLongCamino);


            // filtrar las posibles direcciones de movimiento segun el parámetro de IRREGULARIDAD que se definió y almacenarlas en posiblesPasosIdIrr
            List<Vector2Int> posiblesPasosIdIrr = new List<Vector2Int>();


            //si no hay posibles pasos y no ha terminado el while, es que se quedó sin camino y toca recalcular el camino con otra semilla
            if (posiblesPasosId.Count == 0)
            {
                Debug.Log("CAMINO COLAPSADO, VOLVER A INTENTAR con otro seed");
                seed_siguientePasoDireccion++;

                //si es una bifurcación y colapsó, intentarla crear desde otro punto inicial
                if (esBifurcacion)
                    initId++;

                //la bifurcacion no pudo iniciarse en ninguna celda del camino
                if (initId > posInicial.Count - 1)
                    return new List<Vector2Int>();

                //restaurar el chunk original, la posicion inicial y el camino generado que colapsó
                chunk = chunkOrig;
                posActual = posInicial[initId];
                chunk[posActual.x,posActual.y] = 1;

                caminoEncontrado.Clear();
                caminoEncontrado.Add(posActual);

                incrementoSeed = 0;
                incrementoSeedIrr = 0;

                numeroDeIntentosCrearCamino++;

            }
            else
            {

                //Filtrar posibles direcciones del camino dependiendo de qué tan irregular se desee
                posiblesPasosIdIrr = FiltrarPosiblesPasosDelCamino( posiblesPasosId,  posActual,  posicionAnterior,  seed_cantIrregularidadDeCamino,  offsetUnicoSemilla);



                // si se solicitan caminos muy rectos y una longitud muy alta y luego del filtro no hay pasos disponibles, agregar las direcciones diferente a la recta o si no se choca contra el borde antes de tener la longitud solicitada
                if (parameters.cantIrregularidadDeCamino < 0.6f && parameters.longMinCamino > 0.4f && posiblesPasosIdIrr.Count == 0)
                {
                    // Hallar posición donde sería el camino recto
                    Vector2Int direccionRecta = new Vector2Int(posActual.x - posicionAnterior.x, posActual.y - posicionAnterior.y);

                    foreach (Vector2Int posiblePaso in posiblesPasosId)
                        if (!posiblePaso.Equals(direccionRecta)) // agregar las direcciones diferente a la recta
                            posiblesPasosIdIrr.Add(posiblePaso);

                }


                // Si no hay posibles pasos y no ha terminado el while, es que se quedó sin camino y toca recalcular el camino con otra semilla
                if (posiblesPasosIdIrr.Count == 0)
                {
                    Debug.Log("CAMINO COLAPSADO, VOLVER A INTENTAR con otro seed posiblesPasosIdIrr");
                    seed_siguientePasoDireccion++;

                    // Si es bifurcación recalcular la bifurcación desde otro punto de inicio
                    if (esBifurcacion)
                        initId++;

                    // La bifurcación no pudo iniciarse en ninguna celda del camino
                    if (initId > posInicial.Count - 1)
                        return new List<Vector2Int>();

                    chunk = chunkOrig;
                    posActual = posInicial[initId];
                    chunk[posActual.x,posActual.y] = 1;

                    caminoEncontrado.Clear();
                    caminoEncontrado.Add(posActual);
                    incrementoSeed = 0;
                    incrementoSeedIrr = 0;

                    numeroDeIntentosCrearCamino++;
                }
                else
                {
                    //FINALMENTE seleccionar aleatoriamente el siguiente paso dentro de los pasos posibles filtrados para ir armando el camino
                    Random.InitState(seed_siguientePasoDireccion + posActual.x + posActual.y + incrementoSeed + offsetUnicoSemilla + posiblesPasosIdIrr.Count + posiblesPasosId.Count); //tantas sumas para asegurarse que en cada celda del camino se use una semilla diferente pero esté bien definida para la repetibilidad
                    incrementoSeed++;
                    Vector2Int siguientePasoDireccion = posiblesPasosIdIrr[Random.Range(0, posiblesPasosIdIrr.Count)];
                    posActual = new Vector2Int(posActual.x + siguientePasoDireccion.x, posActual.y + siguientePasoDireccion.y);
                    caminoEncontrado.Add(posActual);
                    chunk[posActual.x,posActual.y] = 1;
                   // Debug.Log(posActual.x.ToString() + " " + posActual.y.ToString());
                }

            }

            // condicion para que si no encuentra ningun camino no se quede en bucle, retorna un camino vacio
            if (numeroDeIntentosCrearCamino > parameters.numeroDeIntentosCrearCaminoMax)
                return new List<Vector2Int>();

            // PARA EVITAR CAMINOS CORTOS marcar zonas prohibidas hasta que alcance una longitud minima
            bordesProhibidosListLongCamino = ZonasProhibidasParaEvitarCaminosCortos(chunk);


        }

        return caminoEncontrado;


    }
    private int[,] ConsolidarChunk(int[,] chunk, List<Vector2Int> caminoEncontrado)
    {
        // Marcar las posiciones del camino en la matriz
        foreach (Vector2Int posCamino in caminoEncontrado)
        {
            chunk[posCamino.x, posCamino.y] = 1;
        }

        // Construir el string para imprimir la matriz
        string printChunk = "chunk\n";
        for (int i = 0; i < chunk.GetLength(0); i++) // Recorre las filas
        {
            for (int j = 0; j < chunk.GetLength(1); j++) // Recorre las columnas
            {
                printChunk += chunk[i, j] + " ";
            }
            printChunk += "\n"; // Nueva línea después de cada fila
        }

        // Imprimir en la consola
        Debug.Log(printChunk);

        return chunk;
    }
    private int[,] GenerarBifurcaciones(List<Vector2Int> caminoEncontrado, int cantBifurcacionesPosibles, int[,] chunkInput, int offsetUnicoSemilla)
    {

        //hacer una copia del chunk o si no se conservan caminos fallidos
        int[,] chunk = HacerCopiaChunk(chunkInput);

        //definir semilla
        Random.InitState(parameters.seed_global + caminoEncontrado.Count + offsetUnicoSemilla + cantBifurcacionesPosibles);
        int cantBifurcaciones = Random.Range(0, cantBifurcacionesPosibles + 1);
        Debug.Log("cantBifurcaciones: " + cantBifurcaciones);

        // Agregar bifurcaciones si las hay
        for (int num = 0; num < cantBifurcaciones; num++)
        {
            // Tomar una posición aleatoria de los caminos existentes, excluyendo los 2 bordes externos
            List<Vector2Int> posicionesCaminosExistentes = new List<Vector2Int>();

            for (int filaCount = 0; filaCount < chunk.GetLength(0); filaCount++)
                for (int celdaCount = 0; celdaCount < chunk.GetLength(0); celdaCount++)
                    if (chunk[filaCount,celdaCount] == 1)
                        if (filaCount > 1 && filaCount < chunk.GetLength(0) - 2 && celdaCount > 1 && celdaCount < chunk.GetLength(0) - 2) //#si son celdas del centro, para que las bifurcaciones inicien desde el centro
                            posicionesCaminosExistentes.Add(new Vector2Int(filaCount, celdaCount));
          

            // Si no hay celdas en el centro para escoger, tomar cualquier celda
            if (posicionesCaminosExistentes.Count == 0)
                posicionesCaminosExistentes = ObtenerCoordenadasEn1(chunk);

            // Barajar las posiciones de los caminos existentes
            Random.InitState(parameters.seed_global + offsetUnicoSemilla);
            posicionesCaminosExistentes = posicionesCaminosExistentes.OrderBy(_ => Random.value).ToList();

            // Encontrar y añadir el camino
            caminoEncontrado = GenerarCamino(chunk, posicionesCaminosExistentes, true, offsetUnicoSemilla);
            chunk = ConsolidarChunk(chunk, caminoEncontrado);
        }

        return chunk;
    }
    private void GenerarChunkVecino(int[,] newChunk, int offsetUnicoSemilla, Vector2Int coordAbsolutas)
    {

        List<Vector2Int> coordenadasEn1 = ObtenerCoordenadasEn1(newChunk);

        //la celda inicial es la siguiente a la del borde
        List<Vector2Int> posInicial = new List<Vector2Int>();
        foreach (Vector2Int coor1 in coordenadasEn1)
            if (coor1.x != 0 && coor1.x != newChunk.GetLength(0) - 1 && coor1.y != 0 && coor1.y != newChunk.GetLength(0) - 1) // Verificar si la coordenada no está en los bordes del chunk
                posInicial.Add(new Vector2Int(coor1.x, coor1.y)); // Agregar la coordenada válida a la lista

        Debug.Log($"posInicial: {string.Join(", ", posInicial)}");


        List<Vector2Int> caminoEncontrado = GenerarCamino(newChunk, posInicial, false, offsetUnicoSemilla);
        ConsolidarChunk(newChunk, caminoEncontrado);
        newChunk = GenerarBifurcaciones(caminoEncontrado, cantBifurcacionesPosibles, newChunk, offsetUnicoSemilla);

        dictChunksCoord[coordAbsolutas] = newChunk;
        ModelarChunk(coordAbsolutas);
    }


    private List<int> GetlistChunkSup(int[,] chunk)
    {
        List<int> listChunkSup = new List<int>();
        // Recorrer todas las columnas de la primera fila (índice 0)
        for (int j = 0; j < chunk.GetLength(1); j++) // matrix.GetLength(1) es el número de columnas
            listChunkSup.Add(chunk[0, j]); // Agregar el valor de la primera fila

        return listChunkSup;
    }
    private List<int> GetlistChunkInf(int[,] chunk)
    {
        List<int> listChunkInf = new List<int>();
        for (int j = 0; j < chunk.GetLength(1); j++)
            listChunkInf.Add(chunk[chunk.GetLength(0) - 1, j]);

        return listChunkInf;
    }
    private List<int> GetlistChunkDer(int[,] chunk)
    {
        List<int> listChunkDer = new List<int>();
        // Recorrer todas las filas y obtener el valor de la última columna (índice matrix.GetLength(1) - 1)
        for (int i = 0; i < chunk.GetLength(0); i++) // matrix.GetLength(0) es el número de filas
            listChunkDer.Add(chunk[i, chunk.GetLength(1) - 1]); // Agregar el valor de la última columna

        return listChunkDer;
    }
    private List<int> GetlistChunkIzq(int[,] chunk)
    {
        List<int> listChunkIzq = new List<int>();
        for (int i = 0; i < chunk.GetLength(0); i++)
            listChunkIzq.Add(chunk[i, 0]);

        return listChunkIzq;
    }



    private (List<Vector2Int>, Vector2Int) CalcularPosiblesPasosDelCamino(int[,] chunk,    Vector2Int posActual,    List<Vector2Int> bordesProhibidosList,    List<Vector2Int> bordesProhibidosListLongCamino)
    {
        // para la celda actual hallar las posibles direcciones a las que se puede mover y almacenarlas en posiblesPasosId
        List<Vector2Int> posiblesPasosId = new List<Vector2Int>();

        Vector2Int posicionAnterior = new Vector2Int(-1, -1);

        //por cada vecino hallar sus vecinos para evitar que el camino tome un camino junto al camino recorrido, es decir, para evitar que haga una U cerrada
        foreach (Vector2Int vecinoId in vecinosIds)
        {
            // posicion absoluta del vecino
            Vector2Int posVecino = new Vector2Int(posActual.x + vecinoId.x, posActual.y + vecinoId.y);

            // solo tener en cuenta los vecinos directos que están dentro de la matriz
            if (posVecino.x >= 0 && posVecino.x < chunk.GetLength(0) && posVecino.y >= 0 && posVecino.y < chunk.GetLength(0))
            {
                //si el vecino directo está libre, sin camino, puede ser candidato a poner camino por ahí
                if (chunk[posVecino.x, posVecino.y] == 0)
                {
                    // esta suma indica si los vecinos del vecino de la celda actual están ocupado con camino, para evitar hacer giros que toquen laterales de camino entre sí, es decir, para evitar que haga una U cerrada
                    int sumaVecinos2 = 0;
                    foreach (Vector2Int vecinoId2 in vecinosIds)
                    {
                        // posicion absoluta del vecino del vecino
                        Vector2Int posVecino2 = new Vector2Int(posVecino.x + vecinoId2.x, posVecino.y + vecinoId2.y);
                        if (posVecino2.x >= 0 && posVecino2.x < chunk.GetLength(0) && posVecino2.y >= 0 && posVecino2.y < chunk.GetLength(0)) // si esta dentro del chunk
                            sumaVecinos2 += chunk[posVecino2.x, posVecino2.y]; // sumar ceros y unos existentes
                    }

                    // esta suma indica si los vecinos diagonales al vecino de la celda actual están ocupado con camino, para evitar que el camino se toque en esquinas
                    int sumaVecinos2Diag = 0;
                    foreach (Vector2Int vecinoId2Diag in vecinosIdsDiag[vecinoId])
                    {
                        Vector2Int posVecino2Diag = new Vector2Int(posVecino.x + vecinoId2Diag.x, posVecino.y + vecinoId2Diag.y);
                        if (posVecino2Diag.x >= 0 && posVecino2Diag.x < chunk.GetLength(0) && posVecino2Diag.y >= 0 && posVecino2Diag.y < chunk.GetLength(0))
                            sumaVecinos2Diag += chunk[posVecino2Diag.x, posVecino2Diag.y];
                    }

                    //condicion para evitar giros de camino junto a caminos existentes, y que ningun camino se junte por las diagonales
                    if (sumaVecinos2 <= 1 && sumaVecinos2Diag == 0)
                    {
                        //#condicion para que no acabe en el mismo borde que ya esta ocupado
                        Vector2Int posAbsolutaPaso = new Vector2Int(posActual.x + vecinoId.x, posActual.y + vecinoId.y);
                        if (!bordesProhibidosList.Contains(posAbsolutaPaso))
                            if (!bordesProhibidosListLongCamino.Contains(posAbsolutaPaso)) //#evitar caminos muy cortos
                                posiblesPasosId.Add(vecinoId);
                    }
                }
                //si el vecino no está libre, quiere decir que ya tiene camino pasando por ahí, es decir, es el camino por el que viene creandose
                else
                {
                    posicionAnterior = posVecino;
                }
            }
        }

        return (posiblesPasosId, posicionAnterior);

    }
    private List<Vector2Int> FiltrarPosiblesPasosDelCamino(List<Vector2Int> posiblesPasosId, Vector2Int posActual, Vector2Int posicionAnterior, int seed_cantIrregularidadDeCamino, int offsetUnicoSemilla)
    {

        // filtrar las posibles direcciones de movimiento segun el parámetro de IRREGULARIDAD que se definió y almacenarlas en posiblesPasosIdIrr
        List<Vector2Int> posiblesPasosIdIrr = new List<Vector2Int>();

        if (posicionAnterior.x != -1) // si el camino al menos tiene 2 unidades, ya se puede calcular hacia qué direccion el camino sería "recto"
        {
            // Hallar posición donde sería el camino recto
            Vector2Int direccionRecta = new Vector2Int(posActual.x - posicionAnterior.x, posActual.y - posicionAnterior.y);

            // Elimina los posibles pasos dependiendo de la cantidad de irregularidad que se desee
            if (posiblesPasosId.Count > 1) //si hay varios pasos
            {

                // solo elimino los pasos diferentes al recto, cantIrregularidadDeCamino = 0, elimino todos los pasos diferentes al recto ; cantIrregularidadDeCamino = 0, dejo todos los pasos diferentes al recto y el recto tambien


                int incrementoSeedIrr = 0; //para que en cada paso del camino la probabilidad de escoger un paso distinto cambie

                foreach (Vector2Int posiblePaso in posiblesPasosId)
                {
                    Random.InitState(seed_cantIrregularidadDeCamino + incrementoSeedIrr + offsetUnicoSemilla + posiblesPasosId.Count);
                    if (posiblePaso.Equals(direccionRecta))
                    {
                        //siempre mantener el paso a la direccion recta
                        posiblesPasosIdIrr.Add(posiblePaso);
                    }
                    else if (!posiblePaso.Equals(direccionRecta) && Random.value <= parameters.cantIrregularidadDeCamino)
                    {
                        //0 no agrega ningun paso diferente del recto, 1 agrega todos los pasos
                        posiblesPasosIdIrr.Add(posiblePaso);
                    }
                    incrementoSeedIrr++;
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

        return posiblesPasosIdIrr;
    }
    private List<Vector2Int> ZonasProhibidasParaEvitarCaminosCortos(int[,] chunk)
    {
        List<Vector2Int> bordesProhibidosListLongCamino = new List<Vector2Int>();

        // calcular la Cantidad de 1 (unos) en la matriz
        int numeroDe1enMatriz = 0;
        for (int i = 0; i < chunk.GetLength(0); i++)
            for (int j = 0; j < chunk.GetLength(1); j++)
                if (chunk[i, j] == 1)
                    numeroDe1enMatriz++;

        float longMinima = (parameters.sizeChunks * 1.5f * parameters.longMinCamino);
        // Si la longitud es muy corta, tiene prohibido los bordes
        if (numeroDe1enMatriz <= longMinima)
        {
            // Agregar todas las posiciones de todos los bordes, evitar el borde de grosor 2
            for (int i = 0; i < chunk.GetLength(0); i++) // Borde superior
            {
                bordesProhibidosListLongCamino.Add(new Vector2Int(0, i));
                bordesProhibidosListLongCamino.Add(new Vector2Int(1, i));
            }
            for (int i = 0; i < chunk.GetLength(0); i++) // Borde derecho
            {
                bordesProhibidosListLongCamino.Add(new Vector2Int(i, chunk.GetLength(0) - 1));
                bordesProhibidosListLongCamino.Add(new Vector2Int(i, chunk.GetLength(0) - 2));
            }
            for (int i = 0; i < chunk.GetLength(0); i++) // Borde inferior
            {
                bordesProhibidosListLongCamino.Add(new Vector2Int(chunk.GetLength(0) - 1, i));
                bordesProhibidosListLongCamino.Add(new Vector2Int(chunk.GetLength(0) - 2, i));
            }
            for (int i = 0; i < chunk.GetLength(0); i++) // Borde izquierdo
            {
                bordesProhibidosListLongCamino.Add(new Vector2Int(i, 0));
                bordesProhibidosListLongCamino.Add(new Vector2Int(i, 1));
            }
        }
        else
        {
            bordesProhibidosListLongCamino.Clear();
        }

        return bordesProhibidosListLongCamino;

    }



    private int[,] HacerCopiaChunk(int[,] chunkInput)
    {
        //hacer una copia del chunk o si no se conservan caminos fallidos
        int[,] chunk = new int[parameters.sizeChunks, parameters.sizeChunks];
        // Copiar los valores de la matriz de entrada
        for (int i = 0; i < chunkInput.GetLength(0); i++)
            for (int j = 0; j < chunkInput.GetLength(1); j++)
                chunk[i, j] = chunkInput[i, j];

        return chunk;
    }
    private List<Vector2Int> ObtenerCoordenadasEn1(int[,] matrix)
    {
        List<Vector2Int> coordinates = new List<Vector2Int>();

        for (int i = 0; i < matrix.GetLength(0); i++) // Filas
        {
            for (int j = 0; j < matrix.GetLength(1); j++) // Columnas
            {
                if (matrix[i,j] == 1) // Verificamos si es un 1
                {
                    coordinates.Add(new Vector2Int(i, j));
                }
            }
        }

        return coordinates;
    }



    private void ModelarChunk(Vector2Int coordResuelto)
    {

        // modelar en unity el chunk del diccionario de la coordenada solicitada
        int[,] chunkResuelto = dictChunksCoord[coordResuelto];


        // Crear un objeto vacío para agrupar los cubos
        GameObject chunkParent = new GameObject("Chunk (" + coordResuelto.x.ToString() + "," + coordResuelto.y.ToString() + ")");
        chunkParent.transform.position = new Vector3(coordResuelto.y * parameters.sizeChunks, 0, coordResuelto.x * parameters.sizeChunks);

        // Crear la base con un solo cubo café para optimizar
        Vector3 position = new Vector3(parameters.sizeChunks/2 - chunkParent.transform.position.x, 0, parameters.sizeChunks/2 + chunkParent.transform.position.z); // Base en y=0
        GameObject cuboCafe = Instantiate(prefabCuboCafe, position, Quaternion.identity);
        cuboCafe.transform.localScale = new Vector3(parameters.sizeChunks,1, parameters.sizeChunks);
        cuboCafe.transform.parent = chunkParent.transform; // Asignar el padre


        // Colocar los cubos verdes donde hay ceros en la matriz
        for (int x = 0; x < parameters.sizeChunks; x++)
        {
            for (int y = 0; y < parameters.sizeChunks; y++)
            {
                if (chunkResuelto[x,y] == 0)
                {
                    Vector3 position2 = new Vector3(x - chunkParent.transform.position.x, 1, y + chunkParent.transform.position.z); // Altura 1 sobre la base
                    GameObject cuboVerde = Instantiate(prefabCuboVerde, position2, Quaternion.identity);
                    cuboVerde.transform.parent = chunkParent.transform; // Asignar el padre
                }
            }
        }

        chunkParent.transform.parent = chunksContainer.transform; // Asignar el padre de todos los chunks



    }



}