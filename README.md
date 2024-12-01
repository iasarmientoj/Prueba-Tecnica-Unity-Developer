# Generación Procedural de Terreno con Caminos en Chunks

![Resultado final](/README-recursos/01-resumen.png "Resultado final")

Este proyecto utiliza un enfoque de generación procedural para crear terrenos representados en chunks interconectados mediante caminos, con alta visual y optimizado. El sistema implementa algoritmos para modelar caminos con bifurcaciones, controlar niveles de detalle, y gestionar eficientemente los recursos gracias a técnicas como el *occlusion culling* y las corrutinas para mejorar el rendimiento.  

---

### Instrucciones de Uso

1. **Clonar o descargar el repositorio del proyecto.**  
   - Versión de Unity: **2022.3.53f1**.
2. **Abrir la escena del proyecto:**  
   - Ubicación: `Assets -> Scenes -> ChunkGenerator.unity`.
3. **Ejecutar el juego** para probar el sistema.
4. **Configurar las propiedades** desde el archivo `ConfigChunk.asset`:  
   - Ubicación: `Assets -> Scripts`.
5. **Generar los chunks** desde la interfaz de usuario:  
   - Presionar el botón "Regenerar chunks".

---

### Demo  

1. **Generación Procedural de 200 Chunks**  
   La animación muestra hasta 3 bifurcaciones por chunk, irregularidades y caminos amplios. 
   
<div align="center"> 

![Demo 1](/README-recursos/02-demo1.gif "Demo 1") 

</div>

2. **Optimización por Occlusion Culling**  
   Solo se renderizan los chunks visibles en pantalla.  
   
<div align="center">

![Demo 2](/README-recursos/02-demo2.gif "Demo 2")

</div>

3. **Modos de Visualización**  
   Modo esquemático vs. HD.  
   
<div align="center">

![Demo 3](/README-recursos/02-demo3.gif "Demo 3")

</div>

4. **Modo Primera Persona**  
   Navegación inmersiva a través de los caminos generados.  
   
<div align="center">

![Demo 4](/README-recursos/02-demo4.gif "Demo 4")

</div>

5. **Caminos menos irregulares y con menor movimiento**  
   Forma de caminos ajustable.  
   
<div align="center">

![Demo 5](/README-recursos/02-demo5.gif "Demo 5")

</div>

---

### Interfaz de Usuario 
Configuración de `ConfigChunk.asset`
Este archivo permite personalizar los parámetros de la generación procedural. Estos son los ajustes disponibles:


<div align="center">
<img src="/README-recursos/03-ui.png" width="600">
</div>


- **Tamaño del Chunk:** Define el tamaño de cada chunk generado.  
- **Cantidad de Chunks a Generar:** Número total de chunks en el mapa.  
- **Máx. Bifurcaciones:** Cantidad máxima de bifurcaciones por chunk.  
- **Irregularidad de los Caminos:** Un valor cercano a 0 produce caminos más rectos, un valor cercano a 1 produce caminos más irregulares.
- **Movimiento de los Caminos:** Un valor cercano a 0 produce caminos más cortos, un valor cercano a 1 produce caminos más largos o amplios dentro del chunk.

- **Intentos para Crear Caminos:** Máximo de intentos al generar caminos, para evitar ciclos infinitos.  
- **Chunks por Frame:** Controla cuántos chunks se procesan por iteración para mantener el rendimiento.  

- **Semilla (`Seed_global`):** Permite repetir la misma generación con un valor específico.  

---

### Funcionamiento del Algoritmo de Generación Procedural  

La lógica principal se encuentra en el script `ProceduralChunkGenerator.cs`. La función **`RegenerarChunks()`** ejecuta el proceso de generación al iniciar el juego o al presionar el botón correspondiente en la UI. A continuación, detallo los pasos principales:  

#### Paso 1: Reinicio de chunks existentes  
Elimina los chunks previos y reinicia variables clave.  
```csharp
public void RegenerarChunks()
{
	EliminarHijosContenedor(chunksContainer);
	ResetVariables();
	GenerarAllChunks();
}
```

#### Paso 2: Generación del primer chunk  
El primer chunk inicia desde su centro y no tiene vecinos. Por eso se debe generar aparte.

1. **Inicialización de la matriz:**  
   Se crea una matriz 2D de ceros según el tamaño definido. Todos los chunks son matrices en c#.

2. **Trazado del camino:**  
   - Función principal: **`GenerarCamino()`**. Traza un camino dentro del chunk a partir de la posición inicial.
   - Evalúa direcciones posibles (**`CalcularPosiblesPasosDelCamino()`**). Evalúa las direcciones posibles que puede tomar el camino en cada uno de sus pasos y evita que el camino sea muy corto y acabe en el mismo borde donde empezó.
   - Filtra direcciones según parámetros del usuario (**`FiltrarPosiblesPasosDelCamino()`**). Filtra las posibles direcciones encontradas anteriormente según los parámetros de irregularidad.

3. **Marcado del camino en la matriz:**  
   Actualiza la matriz con el camino principal generado. Marca con números 1 el camino generado.

4. **Bifurcaciones:**  
   Genera caminos adicionales desde puntos aleatorios del camino principal usando **`GenerarBifurcaciones()`**. Toma como posición inicial un punto aleatorio del camino principal generado anteriormente en el chunk actual y se vuelve a ejecutar la función GenerarCamino() para encontrar un nuevo camino adicional

5. **Almacenamiento:**  
   Guarda la matriz del chunk en el diccionario **`dictChunksCoord`**. También almacena su respectiva coordenada absoluta como llave, la cual servirá para modelar el chubk en unity en su respectiva posición. 

6. **Modelado en Unity:**  
   Usa **`ModelarChunk()`** para instanciar modelos básicos o detallados según la configuración seleccionada. Instancia los modelos o cubo en cada uno de los ceros marcados en la matriz.

```csharp
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
	ModelarChunk(new Vector2Int(0, 0), true);
}
```

#### Paso 3: Generación de chunks vecinos  
La función **`GenerarLosDemasChunks()`** expande la generación:  

1. Evalúa bordes de cada chunk almacenado en el diccionario **`dictChunksCoord`** para detectar finales de caminos. Dependiendo de los finales de camino que encuentre en cada borde procede a calcular un nuevo chunk vecino en esa dirección, por ejemplo, si un camino acabó en el borde derecho del chunk, se calculará un nuevo chunk con posición inicial del camino en el lado izquierdo de este chunk y el chunk resultante se ubicará a la derecha del chunk base.
2. Genera nuevos chunks vecinos usando **`GenerarChunkVecino()`**.  
3. Almacena nuevos chunks en el diccionario y modela sus matrices.  

---

### Optimizaciones Implementadas  

1. **Occlusion Culling**  
   Controlado por el script `CameraCulling.cs`.  
   - Oculta chunks fuera del campo de visión de la cámara.  
   - Garantiza que el chunk actual no desaparezca en el modo primera persona.  

2. **Corrutinas en la Generación de Chunks**  
   - La función **`GenerarLosDemasChunks()`** utiliza corrutinas para evitar congelamientos durante el cálculo intensivo.  
   - Permite observar la creación progresiva de los chunks.  

---

### Recursos Externos Utilizados  

1. **Assets de Vegetación**  
   - *[Stylized Nature Kit Lite](https://assetstore.unity.com/packages/3d/environments/stylized-nature-kit-lite-176906)*

2. **Movimiento en Primera Persona**  
   - Basado en el tutorial: *[First Person Controller - Basic Movement and Mouse Input (EP01) [Unity Tutorial]](https://www.youtube.com/watch?v=2FTDa14nryI&list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21)*

