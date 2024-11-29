import random
import numpy as np


#PARAMETROS
sizeChunks = 13 # min 9
#determinar ubicaciones permitidas para continuar el camino desde la posicion actual
vecinosIds = [(-1,0),(0,1),(1,0),(0,-1)] #posiciones vecinas a cualquier celda arriba, derecha, abajo, izquierda correspondientemente
#posiciones vecinas diagonales segun la direccion en que vaya el camino
vecinosIdsDiag = {
					(-1,0): [(-1,-1), (-1,1)],
					(0,1): [(-1,1), (1,1)],
					(1,0): [(1,-1), (1,1)],
					(0,-1): [(-1,-1), (1,-1)],
				}

seed_cantBifurcaciones = 3
seed_origenBifurcaciones = 0
cantBifurcacionesPosiblesMax = 3 # maximo 3 bifurcaciones
cantBifurcacionesPosibles = 3
numeroDeIntentosCrearCaminoMax = 7
longMinCamino = sizeChunks

cantidadTotalDeChunks = 20
contadorDeChunks = 1

dictChunksCoord = {}


def EncontrarCamino(chunk, posInicial, esBifurcacion):
	
	initId=0
	
	chunkOrig = chunk.copy()
	chunk = chunk.copy()
	posOrig = posInicial[initId]
	
	
	#PARAMETROS
	seed_siguientePasoDireccion = 0
	cantIrregularidadDeCamino = 0.7 # 0: recto, 1: irregular
	seed_cantIrregularidadDeCamino = 0+cantIrregularidadDeCamino*100

	
	

	posActual = posInicial[initId]
	# print('posActual',posActual)
	chunk[posActual] = 1
	# print('\nchunk')
	# print(chunk)

	caminoEncontrado = [posActual]
	incrementoSeed = 0
	incrementoSeedIrr = 0
	
	#hallar los bordes de la matriz que ya tienen un 1 o fin de camino, para evitar terminar en el mismo borde
	
	bordesProhibidosList = []
	bordesProhibidosListLongCamino = []
	
	listChunkDer = []
	for fila in chunk:
		listChunkDer.append(fila[len(chunk)-1])

	listChunkIzq = []
	for fila in chunk:
		listChunkIzq.append(fila[0])
	
	#si hay un 1 en el borde de arriba, se prohibe terminar en ese borde, y así con los demas bordes
	if 1 in chunk[0]: #borde arriba
		#agregar todas las posiciones del borde de arriba
		for i in range(len(chunk)):
			bordesProhibidosList.append((0,i))
	if 1 in listChunkDer: #borde derecha
		for i in range(len(chunk)):
			bordesProhibidosList.append((i,(len(chunk)-1)))
	if 1 in chunk[len(chunk)-1]: #borde abajo
		for i in range(len(chunk)):
			bordesProhibidosList.append(((len(chunk)-1),i))
	if 1 in listChunkIzq: #borde izquierda
		for i in range(len(chunk)):
			bordesProhibidosList.append((i,0))
	
	# print('bordesProhibidosList',bordesProhibidosList)
	
	numeroDeIntentosCrearCamino = 0
	numeroDe1enMatriz = 0

	while posActual[0] != 0 and posActual[0] != len(chunk)-1 and posActual[1] != 0 and posActual[1] != len(chunk)-1:

		posiblesPasosId = []
		posicionAnterior = (-1,-1)
		for vecinoId in vecinosIds:
			#por cada vecino hallar sus vecinos para evitar que el camino tome un camino junto al camino recorrido
			# print()
			# print('vecinoId',vecinoId)
			posVecino = (posActual[0] + vecinoId[0], posActual[1] + vecinoId[1])
			# print('posVecino',posVecino)
			
			#vecinos directos que están dentro de la matriz
			if posVecino[0]>=0 and posVecino[0]<len(chunk) and posVecino[1]>=0 and posVecino[1]<len(chunk):
			
				#si el vecino directos está libre sin camino
				if chunk[posVecino] == 0:
					
					sumaVecinos2 = 0
					for vecinoId2 in vecinosIds:
						# print('vecinoId2',vecinoId2)
						posVecino2 = (posVecino[0] + vecinoId2[0], posVecino[1] + vecinoId2[1])
						# print('posVecino2',posVecino2)
						if posVecino2[0]>=0 and posVecino2[0]<len(chunk) and posVecino2[1]>=0 and posVecino2[1]<len(chunk):
							sumaVecinos2 = sumaVecinos2 + chunk[posVecino2]
						
						
					
					sumaVecinos2Diag = 0
					for vecinoId2Diag in vecinosIdsDiag[vecinoId]:
						# print('vecinoId2Diag',vecinoId2Diag)
						posVecino2Diag = (posVecino[0] + vecinoId2Diag[0], posVecino[1] + vecinoId2Diag[1])
						# print('posVecino2Diag',posVecino2Diag)
						if posVecino2Diag[0]>=0 and posVecino2Diag[0]<len(chunk) and posVecino2Diag[1]>=0 and posVecino2Diag[1]<len(chunk):
							sumaVecinos2Diag = sumaVecinos2Diag + chunk[posVecino2Diag]
						
						
					# print('sumaVecinos2',sumaVecinos2,'sumaVecinos2Diag',sumaVecinos2Diag)
					#para evitar giros de camino junto a caminos existentes, y que ningun camino se junte por las diagonales
					if sumaVecinos2 <= 1 and sumaVecinos2Diag == 0:
						#condicion para que no acabe en el mismo borde que ya esta ocupado
						posAbsolutaPaso = (posActual[0] + vecinoId[0], posActual[1] + vecinoId[1])
						if not posAbsolutaPaso in bordesProhibidosList:
							if not posAbsolutaPaso in bordesProhibidosListLongCamino: #evitar caminos muy cortos
								# print('posAbsolutaPaso',posAbsolutaPaso)
								posiblesPasosId.append(vecinoId)
				else:
					posicionAnterior = posVecino
				
			
		posiblesPasosIdIrr = []
		#si no hay posibles pasos y no ha terminado el while, es que se quedó sin camino y toca recalcular el camino con otra semilla
		if len(posiblesPasosId) == 0:
			print('\n\tCAMINO COLAPSADO, VOLVER A INTENTAR con otro seed\n')
			seed_siguientePasoDireccion = seed_siguientePasoDireccion + 1
			
			if esBifurcacion:
				initId = initId+1
			
			#la bifurcacion no pudo iniciarse en ninguna celda del camino
			if initId > len(posInicial)-1:
				return []
			
			chunk = chunkOrig
			# posInicial = posOrig[]
			posActual = posInicial[initId]
			# print('posActual',posActual)
			chunk[posActual] = 1
			
			caminoEncontrado = [posActual]
			incrementoSeed = 0
			incrementoSeedIrr = 0
			
			
			numeroDeIntentosCrearCamino = numeroDeIntentosCrearCamino + 1
			
			
		else:
			#hallar posicion donde sería el camino recto
			if posicionAnterior[0] != -1:
				direccionRecta = (posActual[0]-posicionAnterior[0], posActual[1]-posicionAnterior[1])
				# print('posicionAnterior',posicionAnterior)
				# print('posActual',posActual)
				# print('direccionRecta',direccionRecta)
			
				#elimina los posibles pasos dependiendo de la cantidad de irregularidad que se desee
				if len(posiblesPasosId) > 1:
					#si cantIrregularidadDeCamino es baja, es probable eliminar los pasos diferentes a la direccion recta
					if cantIrregularidadDeCamino < 0.5:
						incrementoSeedIrr = 0
						for posiblePaso in posiblesPasosId:
							random.seed(seed_cantIrregularidadDeCamino+incrementoSeedIrr)
							if posiblePaso == direccionRecta:
								posiblesPasosIdIrr.append(posiblePaso)
							elif posiblePaso != direccionRecta and random.random() < cantIrregularidadDeCamino:
								posiblesPasosIdIrr.append(posiblePaso)
							
							incrementoSeedIrr = incrementoSeedIrr + 1
					
					else:
						incrementoSeedIrr = 0
						probEliminarPaso = (cantIrregularidadDeCamino-0.5)*2
						for posiblePaso in posiblesPasosId:
							random.seed(seed_cantIrregularidadDeCamino+incrementoSeedIrr)
							if posiblePaso == direccionRecta and random.random() > probEliminarPaso:
								posiblesPasosIdIrr.append(posiblePaso)
							elif posiblePaso != direccionRecta:
								posiblesPasosIdIrr.append(posiblePaso)
							
							incrementoSeedIrr = incrementoSeedIrr + 1
			
				else:
					posiblesPasosIdIrr = posiblesPasosId.copy()
			else:
				posiblesPasosIdIrr = posiblesPasosId.copy()
			
			# si es bifurcacion no  tener en cuenta tantas reglas
			if esBifurcacion:
				posiblesPasosIdIrr = posiblesPasosId.copy()
				
			
			#si no hay posibles pasos y no ha terminado el while, es que se quedó sin camino y toca recalcular el camino con otra semilla
			if len(posiblesPasosIdIrr) == 0:
				print('\n\tCAMINO COLAPSADO, VOLVER A INTENTAR con otro seed posiblesPasosIdIrr\n')
				seed_siguientePasoDireccion = seed_siguientePasoDireccion + 1
				
				#si es bifurcacion recalcular la bifurcacion desde otro punto de inicio
				if esBifurcacion:
					initId = initId+1
				
				#la bifurcacion no pudo iniciarse en ninguna celda del camino
				if initId > len(posInicial)-1:
					return []
				
				chunk = chunkOrig
				# print('chunk',chunk)
				# posInicial = posOrig[]
				posActual = posInicial[initId]
				# print('posActual',posActual)
				chunk[posActual] = 1
				
				caminoEncontrado = [posActual]
				# print('caminoEncontrado',caminoEncontrado)
				incrementoSeed = 0
				incrementoSeedIrr = 0
				
				
				numeroDeIntentosCrearCamino = numeroDeIntentosCrearCamino + 1
			
			else:
			
				# print('posiblesPasosId',posiblesPasosId)
				# print('posiblesPasosIdIrr',posiblesPasosIdIrr)
				random.seed(int(seed_siguientePasoDireccion+posActual[0]+posActual[1]+incrementoSeed))
				incrementoSeed = incrementoSeed + 1
				siguientePasoDireccion = posiblesPasosIdIrr[random.randint(0, len(posiblesPasosIdIrr)-1)]
				# print('siguientePasoDireccion',siguientePasoDireccion)
				posActual = (posActual[0] + siguientePasoDireccion[0], posActual[1] + siguientePasoDireccion[1])
				# print('posActual',posActual)
				caminoEncontrado.append(posActual)
				# print('caminoEncontrado',caminoEncontrado)
				chunk[posActual] = 1
				# print('\nchunk')
				# print(chunk)
		
		if numeroDeIntentosCrearCamino > numeroDeIntentosCrearCaminoMax:
			return []
		
		#PARA EVITAR CAMINOS CORTOS
		#cantidad de 1 en la matriz
		numeroDe1enMatriz = 0
		for fila in chunk:
			for celda in fila:
				if celda == 1:
					numeroDe1enMatriz = numeroDe1enMatriz + 1
		
		# print('numeroDe1enMatriz',numeroDe1enMatriz)
		# print('posiblesPasosId',posiblesPasosId)
		# print('posiblesPasosIdIrr',posiblesPasosIdIrr)
		# print('chunk-----------------\n',chunk)
		
		#si la longitud es muy corta, tiene prohibido los bordes
		if numeroDe1enMatriz < longMinCamino:
			#agregar todas las posiciones de todos los bordes
			for i in range(len(chunk)): #borde arriba
				bordesProhibidosListLongCamino.append((0,i))
				bordesProhibidosListLongCamino.append((1,i))
			for i in range(len(chunk)): #borde derecha
				bordesProhibidosListLongCamino.append((i,(len(chunk)-1)))
				bordesProhibidosListLongCamino.append((i,(len(chunk)-2)))
			for i in range(len(chunk)): #borde abajo
				bordesProhibidosListLongCamino.append(((len(chunk)-1),i))
				bordesProhibidosListLongCamino.append(((len(chunk)-2),i))
			for i in range(len(chunk)): #borde izquierda
				bordesProhibidosListLongCamino.append((i,0))
				bordesProhibidosListLongCamino.append((i,1))
			
		else:
			bordesProhibidosListLongCamino = []
		
	
	return caminoEncontrado

def PrintChunk(chunk, caminoEncontrado):
	for posCamino in caminoEncontrado:
		chunk[posCamino] = 1
	# print('caminoEncontrado',caminoEncontrado)

	print('\nchunk')
	print(chunk)

def GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, chunk):
		
	chunk = chunk.copy()
	
	random.seed(int(seed_cantBifurcaciones+len(caminoEncontrado)))
	cantBifurcaciones = random.randint(0, cantBifurcacionesPosibles)
	print('cantBifurcaciones',cantBifurcaciones)
	#agregar bifurcaciones si las hay
	for num in range(cantBifurcaciones):

		#tomar una posicion aleatoria de los caminos existentes
		posicionesCaminosExistentes = []
		filaCount=0
		celdaCount=0
		for fila in chunk:
			celdaCount=0
			for celda in fila:
				if celda == 1:
					if filaCount > len(chunk)//4 and filaCount < len(chunk)-len(chunk)//4 and celdaCount > len(chunk)//4 and celdaCount < len(chunk)-len(chunk)//4: #si son celdas del centro
						posicionesCaminosExistentes.append((filaCount,celdaCount))
				celdaCount=celdaCount+1
			filaCount=filaCount+1
		
		
		#si no hay celdas en el centro para escoger, tomar cualquier celda
		if len(posicionesCaminosExistentes) == 0:
			filaCount=0
			celdaCount=0
			for fila in chunk:
				celdaCount=0
				for celda in fila:
					if celda == 1:
						posicionesCaminosExistentes.append((filaCount,celdaCount))
					celdaCount=celdaCount+1
				filaCount=filaCount+1
			
		
		
		# print('posicionesCaminosExistentes',posicionesCaminosExistentes)
		random.seed(seed_origenBifurcaciones)
		random.shuffle(posicionesCaminosExistentes)
		
		caminoEncontrado = EncontrarCamino(chunk, posicionesCaminosExistentes, True)
		PrintChunk(chunk, caminoEncontrado)

	return chunk



#PARA EL PRIMER CHUNK
#PARA EL PRIMER CHUNK
#PARA EL PRIMER CHUNK
chunk = np.zeros((sizeChunks, sizeChunks))
posInicial = [(len(chunk)//2,len(chunk)//2)]

caminoEncontrado = EncontrarCamino(chunk, posInicial, False)
PrintChunk(chunk, caminoEncontrado)
chunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, chunk)

dictChunksCoord[(0,0)] = chunk

print('-------------------------------------------------------------------------------------')
print()
print('dictChunksCoord')
print(dictChunksCoord)
print('-------------------------------------------------------------------------------------')

#PARA LOS DEMAS CHUNK
#PARA LOS DEMAS CHUNK
#PARA LOS DEMAS CHUNK

#PARA CADA CHUNK RESUELTO GENERAR SUS VECINOS
vecinosChunksIds = [(0,1),(1,0),(0,-1),(-1,0)] #posiciones vecinas a cualquier chunk arriba, derecha, abajo, izquierda correspondientemente


while contadorDeChunks < cantidadTotalDeChunks:


	for coordResuelto, chunkResuelto in list(dictChunksCoord.items()):
		# print()
		# print('AAAAAAAAAAAAAAAAAA',coordResuelto, chunkResuelto)
		
		
		for vecinoChunkId in vecinosChunksIds:
			coordAbsolutas = (coordResuelto[0]+vecinoChunkId[0],coordResuelto[1]+vecinoChunkId[1])
			# si el vecino no existe en el diccionario de chunks, puede proceder a generarlo si es el caso
			if not coordAbsolutas in dictChunksCoord:
				#evaluar el chunk vecino de arriba
				if vecinoChunkId == (0,1):
					#si un camino terminó en la parte superior del chunk resuelto
					if 1 in chunkResuelto[0]:
						print('parte superior')
						newChunk = np.zeros((sizeChunks, sizeChunks))
						for i in range(len(chunkResuelto)-1):
							#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
							newChunk[len(newChunk)-1][i] = chunkResuelto[0][i]
							newChunk[len(newChunk)-2][i] = chunkResuelto[0][i]
							
						coordenadasEn1 = list(zip(*np.where(newChunk == 1)))
						
						#la celda inicial es la siguiente a la del borde
						posInicial = []
						for coor1 in coordenadasEn1:
							if coor1[0] != 0 and coor1[0] != len(newChunk)-1 and  coor1[1] != 0 and coor1[1] != len(newChunk)-1:
								posInicial.append(coor1)
						
						print('posInicial',posInicial)
						
						caminoEncontrado = EncontrarCamino(newChunk, posInicial, False)
						PrintChunk(newChunk, caminoEncontrado)
						newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk)
						
						dictChunksCoord[coordAbsolutas] = newChunk
						
						contadorDeChunks = contadorDeChunks + 1
						if contadorDeChunks >= cantidadTotalDeChunks:
							break
						
				#evaluar el chunk vecino de abajo
				if vecinoChunkId == (0,-1):
					#si un camino terminó en la parte inferior del chunk resuelto
					if 1 in chunkResuelto[len(chunkResuelto)-1]:
						print('parte inferior')
						newChunk = np.zeros((sizeChunks, sizeChunks))
						for i in range(len(chunkResuelto)-1):
							#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
							newChunk[0][i] = chunkResuelto[len(newChunk)-1][i]
							newChunk[1][i] = chunkResuelto[len(newChunk)-1][i]
						
						coordenadasEn1 = list(zip(*np.where(newChunk == 1)))
						
						#la celda inicial es la siguiente a la del borde
						posInicial = []
						for coor1 in coordenadasEn1:
							if coor1[0] != 0 and coor1[0] != len(newChunk)-1 and  coor1[1] != 0 and coor1[1] != len(newChunk)-1:
								posInicial.append(coor1)
						
						print('posInicial',posInicial)
						
						caminoEncontrado = EncontrarCamino(newChunk, posInicial, False)
						PrintChunk(newChunk, caminoEncontrado)
						newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk)
						
						dictChunksCoord[coordAbsolutas] = newChunk
						
						contadorDeChunks = contadorDeChunks + 1
						if contadorDeChunks >= cantidadTotalDeChunks:
							break
						
				#evaluar el chunk vecino de derecha
				if vecinoChunkId == (1,0):
					#si un camino terminó en la parte derecha del chunk resuelto

					listChunkDer = []
					for fila in chunkResuelto:
						listChunkDer.append(fila[len(chunkResuelto)-1])
					
					if 1 in listChunkDer:
						print('parte derecha')
						newChunk = np.zeros((sizeChunks, sizeChunks))
						for i in range(len(chunkResuelto)-1):
							#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
							newChunk[i][0] = chunkResuelto[i][len(newChunk)-1]
							newChunk[i][1] = chunkResuelto[i][len(newChunk)-1]
						
						coordenadasEn1 = list(zip(*np.where(newChunk == 1)))
						
						#la celda inicial es la siguiente a la del borde
						posInicial = []
						for coor1 in coordenadasEn1:
							if coor1[0] != 0 and coor1[0] != len(newChunk)-1 and  coor1[1] != 0 and coor1[1] != len(newChunk)-1:
								posInicial.append(coor1)
						
						print('posInicial',posInicial)
						
						caminoEncontrado = EncontrarCamino(newChunk, posInicial, False)
						PrintChunk(newChunk, caminoEncontrado)
						newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk)
						
						dictChunksCoord[coordAbsolutas] = newChunk
						
						contadorDeChunks = contadorDeChunks + 1
						if contadorDeChunks >= cantidadTotalDeChunks:
							break
						
						
				#evaluar el chunk vecino de izquierda
				if vecinoChunkId == (-1,0):
					#si un camino terminó en la parte izquierda del chunk resuelto

					listChunkIzq = []
					for fila in chunkResuelto:
						listChunkIzq.append(fila[0])
					
					if 1 in listChunkIzq:
						print('parte izquierda')
						newChunk = np.zeros((sizeChunks, sizeChunks))
						for i in range(len(chunkResuelto)-1):
							#copiarlo en las dos primeras filas para alejarlo del borde, es el unico paso posible
							newChunk[i][len(newChunk)-1] = chunkResuelto[i][0]
							newChunk[i][len(newChunk)-2] = chunkResuelto[i][0]
						
						coordenadasEn1 = list(zip(*np.where(newChunk == 1)))
						
						#la celda inicial es la siguiente a la del borde
						posInicial = []
						for coor1 in coordenadasEn1:
							if coor1[0] != 0 and coor1[0] != len(newChunk)-1 and  coor1[1] != 0 and coor1[1] != len(newChunk)-1:
								posInicial.append(coor1)
						
						print('posInicial',posInicial)
						
						caminoEncontrado = EncontrarCamino(newChunk, posInicial, False)
						PrintChunk(newChunk, caminoEncontrado)
						newChunk = GenerarBifurcacionese(caminoEncontrado, cantBifurcacionesPosibles, newChunk)
						
						dictChunksCoord[coordAbsolutas] = newChunk
						
						contadorDeChunks = contadorDeChunks + 1
						if contadorDeChunks >= cantidadTotalDeChunks:
							break
						
						
		cantBifurcacionesPosibles = cantBifurcacionesPosibles + 1
		if cantBifurcacionesPosibles > cantBifurcacionesPosiblesMax:
			cantBifurcacionesPosibles = cantBifurcacionesPosiblesMax

	
		if contadorDeChunks >= cantidadTotalDeChunks:
			break
			
	if contadorDeChunks >= cantidadTotalDeChunks:
		break
	

print('-------------------------------------------------------------------------------------')
print()
print('dictChunksCoord')
print(dictChunksCoord)
print('-------------------------------------------------------------------------------------')
print('contadorDeChunks',contadorDeChunks)





