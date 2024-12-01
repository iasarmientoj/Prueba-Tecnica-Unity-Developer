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
   
   ![Demo 1](/README-recursos/02-demo1.gif "Demo 1")

2. **Optimización por Occlusion Culling**  
   Solo se renderizan los chunks visibles en pantalla.  
   
   ![Demo 2](/README-recursos/02-demo2.gif "Demo 2")

3. **Modos de Visualización**  
   - Modo esquemático vs. HD.  
   
   ![Demo 3](/README-recursos/02-demo3.gif "Demo 3")

4. **Modo Primera Persona**  
   Navegación inmersiva a través de los caminos generados.  
   
   ![Demo 4](/README-recursos/02-demo4.gif "Demo 4")

5. **Caminos menos irregulares y con menor movimiento**  
   Navegación inmersiva a través de los caminos generados.  
   
   ![Demo 5](/README-recursos/02-demo5.gif "Demo 54")

---

### Funcionamiento del Algoritmo de Generación Procedural  

La lógica principal se encuentra en el script `ProceduralChunkGenerator.cs`. La función **`RegenerarChunks()`** ejecuta el proceso de generación al iniciar el juego o al presionar el botón correspondiente en la UI. A continuación, detallo los pasos principales:  

#### Paso 1: Reinicio de chunks existentes  
Elimina los chunks previos y reinicia variables clave.  
```csharp
// Ejemplo de código de reinicio
void ReiniciarChunks() {
    foreach (Transform child in chunkContainer.transform) {
        Destroy(child.gameObject);
    }
    dictChunksCoord.Clear();
    // Otras variables reseteadas
}
```

#### Paso 2: Generación del primer chunk  
El primer chunk inicia desde su centro y no tiene vecinos.  

1. **Inicialización de la matriz:**  
   Se crea una matriz 2D de ceros según el tamaño definido.  

2. **Trazado del camino:**  
   - Función principal: **`GenerarCamino()`**.  
   - Evalúa direcciones posibles (**`CalcularPosiblesPasosDelCamino()`**).  
   - Filtra direcciones según parámetros del usuario (**`FiltrarPosiblesPasosDelCamino()`**).  

3. **Marcado del camino en la matriz:**  
   Actualiza la matriz con el camino principal generado.  

4. **Bifurcaciones:**  
   Genera caminos adicionales desde puntos aleatorios del camino principal usando **`GenerarBifurcaciones()`**.  

5. **Almacenamiento:**  
   Guarda la matriz del chunk en el diccionario **`dictChunksCoord`**.  

6. **Modelado en Unity:**  
   Usa **`ModelarChunk()`** para instanciar modelos básicos o detallados según la configuración seleccionada.  

```csharp
// Ejemplo de función de generación de chunks
void GenerarPrimerChunk() {
    int[,] matrizChunk = new int[tamaño, tamaño];
    GenerarCamino(matrizChunk, posicionInicial);
    dictChunksCoord.Add(coordAbsoluta, matrizChunk);
    ModelarChunk(coordAbsoluta, matrizChunk);
}
```

#### Paso 3: Generación de chunks vecinos  
La función **`GenerarLosDemasChunks()`** expande la generación:  

1. Evalúa bordes de cada chunk para detectar finales de caminos.  
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
   - *Stylized Nature Kit Lite*.  

2. **Movimiento en Primera Persona**  
   - Basado en el tutorial: *First Person Controller - Basic Movement and Mouse Input (EP01) [Unity Tutorial]*.  

