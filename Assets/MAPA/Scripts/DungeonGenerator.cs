using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GeneradorDungeon : MonoBehaviour
{
    [System.Serializable]
    public class DungeonRoomConfig
    {
        public string nombreTipo;              // "normal", "tesoro", "enemigos", "boss", etc.
        [Range(0f, 1f)]
        public float probabilidadTipo = 0.7f;  // Probabilidad de que aparezca este tipo
        [Range(0, 10)]
        public int minObjetos = 1;             // Mínimo de objetos en esta sala
        [Range(0, 10)]
        public int maxObjetos = 5;             // Máximo de objetos en esta sala
        public GameObject[] objetosPosibles;   // Prefabs que pueden aparecer
        public TileBase[] objetosTiles;        // Tiles alternativos para objetos (si se quiere usar Tilemap)
        public Color colorDebug = Color.white; // Color para debug visual
        [Range(0f, 1f)]
        public float densidadObjetos = 0.3f;   // Qué tan densos son los objetos
        public bool objetosEnCentro = false;   // Si los objetos deben ir más al centro
        public float distanciaMinimaEntreObjetos = 1.5f; // Distancia mínima entre objetos
    }

    [Header("Tilemaps")]
    public Tilemap tilemapSuelo;
    public Tilemap tilemapParedesBase;
    public Tilemap tilemapParedesTop;
    public Tilemap tilemapEsquinasExtBase;
    public Tilemap tilemapEsquinasExtTop;
    public Tilemap tilemapObjetos;

    [Header("Tiles Base")]
    public TileBase tileSuelo;
    public TileBase paredArriba;
    public TileBase paredAbajo;
    public TileBase paredIzquierda;
    public TileBase paredDerecha;
    public TileBase esquinaInfIzq;
    public TileBase esquinaInfDer;
    public TileBase esquinaSupIzqBase;
    public TileBase esquinaSupDerBase;
    public TileBase esquinaSupIzqTop;
    public TileBase esquinaSupDerTop;
    public TileBase paredArribaTop;
    public TileBase extInfIzq;
    public TileBase extInfDer;
    public TileBase extSupIzqBase;
    public TileBase extSupDerBase;
    public TileBase extSupIzqTop;
    public TileBase extSupDerTop;

    [Header("Variaciones de Tiles")]
    public TileBase[] variacionesSuelo;
    public TileBase[] variacionesParedArriba;
    public TileBase[] variacionesParedAbajo;
    public TileBase[] variacionesParedIzquierda;
    public TileBase[] variacionesParedDerecha;
    public TileBase[] variacionesEsquinaInfIzq;
    public TileBase[] variacionesEsquinaInfDer;
    public TileBase[] variacionesEsquinaSupIzqBase;
    public TileBase[] variacionesEsquinaSupDerBase;
    public TileBase[] variacionesEsquinaSupIzqTop;
    public TileBase[] variacionesEsquinaSupDerTop;
    public TileBase[] variacionesParedArribaTop;
    public TileBase[] variacionesExtInfIzq;
    public TileBase[] variacionesExtInfDer;
    public TileBase[] variacionesExtSupIzqBase;
    public TileBase[] variacionesExtSupDerBase;
    public TileBase[] variacionesExtSupIzqTop;
    public TileBase[] variacionesExtSupDerTop;
    
    [Range(0f, 1f)]
    public float probabilidadVariacion = 0.3f;

    [Header("Configuración Salas")]
    [Range(3, 20)]
    public int numeroSalas = 6;
    [Range(4, 8)]
    public int anchoMinSala = 6;
    [Range(4, 8)]
    public int altoMinSala = 6;
    [Range(8, 20)]
    public int anchoMaxSala = 12;
    [Range(8, 20)]
    public int altoMaxSala = 12;
    [Range(1, 10)]
    public int separacionMinima = 2;
    [Range(0, 100)]
    public int seed = 0;
    public bool usarSeedAleatoria = true;

    [Header("Configuración Pasillos")]
    [Range(1, 5)]
    public int anchoPasilloMin = 1;
    [Range(1, 5)]
    public int anchoPasilloMax = 1;
    [Range(1, 10)]
    public int largoMaxPasillo = 3;
    [Range(0f, 1f)]
    public float probabilidadPasilloCurvo = 0.3f;

    [Header("Configuración Dungeon")]
    public Vector2Int areaGeneracionMin = new Vector2Int(-30, -20);
    public Vector2Int areaGeneracionMax = new Vector2Int(30, 20);
    public bool conectarSalaAleatoria = true;
    [Range(0f, 1f)]
    public float densidadPasillos = 1.0f;

    [Header("Configuración Tipos de Sala")]
    public DungeonRoomConfig[] configuracionesSalas = {
        new DungeonRoomConfig { 
            nombreTipo = "normal", 
            probabilidadTipo = 0.6f,
            minObjetos = 1,
            maxObjetos = 4,
            colorDebug = Color.gray
        },
        new DungeonRoomConfig { 
            nombreTipo = "tesoro", 
            probabilidadTipo = 0.15f,
            minObjetos = 2,
            maxObjetos = 6,
            colorDebug = Color.yellow,
            objetosEnCentro = true
        },
        new DungeonRoomConfig { 
            nombreTipo = "enemigos", 
            probabilidadTipo = 0.2f,
            minObjetos = 3,
            maxObjetos = 8,
            colorDebug = Color.red,
            densidadObjetos = 0.4f
        },
        new DungeonRoomConfig { 
            nombreTipo = "boss", 
            probabilidadTipo = 0.05f,
            minObjetos = 5,
            maxObjetos = 10,
            colorDebug = Color.magenta,
            objetosEnCentro = true
        }
    };

    [Header("Debug Visual")]
    public bool mostrarDebugVisual = true;
    public float duracionDebug = 5f;

    // Variables internas
    private HashSet<Vector2Int> suelo = new HashSet<Vector2Int>();
    private List<RectInt> salas = new List<RectInt>();
    private Dictionary<RectInt, string> tiposSalas = new Dictionary<RectInt, string>();
    private Dictionary<RectInt, List<GameObject>> objetosPorSala = new Dictionary<RectInt, List<GameObject>>();
    private Dictionary<RectInt, List<Vector2Int>> objetosTilesPorSala = new Dictionary<RectInt, List<Vector2Int>>();

    void Start()
    {
        GenerarDungeon();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            LimpiarTodo();
            GenerarDungeon();
        }
        
        if (Input.GetKeyDown(KeyCode.D) && mostrarDebugVisual)
        {
            MostrarDebugVisual();
        }
    }

    void LimpiarTodo()
    {
        // Limpiar tilemaps
        tilemapSuelo.ClearAllTiles();
        tilemapParedesBase.ClearAllTiles();
        tilemapParedesTop.ClearAllTiles();
        tilemapEsquinasExtBase.ClearAllTiles();
        tilemapEsquinasExtTop.ClearAllTiles();
        if (tilemapObjetos != null) tilemapObjetos.ClearAllTiles();
        
        // Limpiar objetos
        foreach (var listaObjetos in objetosPorSala.Values)
        {
            foreach (var obj in listaObjetos)
            {
                if (obj != null) Destroy(obj);
            }
        }
        
        suelo.Clear();
        salas.Clear();
        tiposSalas.Clear();
        objetosPorSala.Clear();
        objetosTilesPorSala.Clear();
    }

    void GenerarDungeon()
    {
        // Configurar semilla aleatoria
        if (usarSeedAleatoria)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
        }
        else
        {
            Random.InitState(seed);
        }

        GenerarSalas();
        ConectarSalas();
        GenerarParedes();
        GenerarObjetosPorTipoSala();
        
        Debug.Log($"Dungeon generado: {salas.Count} salas");
        MostrarEstadisticasSalas();
    }

    void GenerarSalas()
    {
        int intentos = 0;
        int maxIntentos = numeroSalas * 20;

        while (salas.Count < numeroSalas && intentos < maxIntentos)
        {
            intentos++;

            // Generar dimensiones aleatorias para la sala
            int ancho = Random.Range(anchoMinSala, anchoMaxSala + 1);
            int alto = Random.Range(altoMinSala, altoMaxSala + 1);
            
            // Calcular posición asegurando margen de separación
            int x = Random.Range(areaGeneracionMin.x + separacionMinima, 
                                areaGeneracionMax.x - ancho - separacionMinima);
            int y = Random.Range(areaGeneracionMin.y + separacionMinima,
                                areaGeneracionMax.y - alto - separacionMinima);

            RectInt nuevaSala = new RectInt(x, y, ancho, alto);

            // Verificar si la nueva sala se superpone con las existentes
            bool solapa = false;
            foreach (var salaExistente in salas)
            {
                // Crear rectángulo expandido para la separación mínima
                RectInt salaExpandida = new RectInt(
                    salaExistente.xMin - separacionMinima,
                    salaExistente.yMin - separacionMinima,
                    salaExistente.width + separacionMinima * 2,
                    salaExistente.height + separacionMinima * 2
                );

                if (salaExpandida.Overlaps(nuevaSala))
                {
                    solapa = true;
                    break;
                }
            }

            if (!solapa)
            {
                salas.Add(nuevaSala);
                
                // Asignar tipo a la sala
                string tipoSala = AsignarTipoSala();
                tiposSalas[nuevaSala] = tipoSala;
                objetosPorSala[nuevaSala] = new List<GameObject>();
                objetosTilesPorSala[nuevaSala] = new List<Vector2Int>();
                
                // Añadir suelo de la sala
                for (int i = nuevaSala.xMin; i < nuevaSala.xMax; i++)
                for (int j = nuevaSala.yMin; j < nuevaSala.yMax; j++)
                {
                    Vector2Int pos = new Vector2Int(i, j);
                    suelo.Add(pos);
                    PonerTileConVariacion(tilemapSuelo, pos, tileSuelo, variacionesSuelo);
                }
            }
        }

        Debug.Log($"Salas generadas: {salas.Count} de {numeroSalas} solicitadas");
    }

    string AsignarTipoSala()
    {
        // Calcular probabilidad total
        float totalProb = 0f;
        foreach (var config in configuracionesSalas)
        {
            totalProb += config.probabilidadTipo;
        }
        
        // Seleccionar tipo basado en probabilidad
        float randomVal = Random.value * totalProb;
        float acumulado = 0f;
        
        foreach (var config in configuracionesSalas)
        {
            acumulado += config.probabilidadTipo;
            if (randomVal <= acumulado)
            {
                return config.nombreTipo;
            }
        }
        
        return "normal"; // Fallback
    }

    void ConectarSalas()
    {
        if (salas.Count < 2) return;

        // Conectar todas las salas en un árbol mínimo
        List<int> salasConectadas = new List<int> { 0 };
        List<int> salasNoConectadas = new List<int>();
        
        for (int i = 1; i < salas.Count; i++)
            salasNoConectadas.Add(i);

        while (salasNoConectadas.Count > 0 && Random.value <= densidadPasillos)
        {
            int idxConectada = salasConectadas[Random.Range(0, salasConectadas.Count)];
            int idxNoConectada;
            
            if (conectarSalaAleatoria)
            {
                idxNoConectada = salasNoConectadas[Random.Range(0, salasNoConectadas.Count)];
            }
            else
            {
                // Conectar con la sala más cercana
                idxNoConectada = EncontrarSalaMasCercana(idxConectada, salasNoConectadas);
            }

            // Conectar las dos salas
            ConectarDosSalas(salas[idxConectada], salas[idxNoConectada]);

            // Actualizar listas
            salasConectadas.Add(idxNoConectada);
            salasNoConectadas.Remove(idxNoConectada);
        }

        // Conexiones adicionales para crear ciclos (opcional)
        if (salas.Count > 2 && Random.value < 0.3f)
        {
            int extraConnections = Random.Range(1, Mathf.Min(3, salas.Count / 2));
            for (int i = 0; i < extraConnections; i++)
            {
                int salaA = Random.Range(0, salas.Count);
                int salaB = Random.Range(0, salas.Count);
                if (salaA != salaB)
                {
                    ConectarDosSalas(salas[salaA], salas[salaB]);
                }
            }
        }
    }

    int EncontrarSalaMasCercana(int salaIndex, List<int> salasDisponibles)
    {
        // Calcular centro de la sala A
        RectInt salaA = salas[salaIndex];
        Vector2Int centroA = new Vector2Int(
            salaA.x + salaA.width / 2,
            salaA.y + salaA.height / 2
        );
        
        int salaMasCercana = salasDisponibles[0];
        float distanciaMinima = float.MaxValue;

        foreach (int idx in salasDisponibles)
        {
            // Calcular centro de la sala B
            RectInt salaB = salas[idx];
            Vector2Int centroB = new Vector2Int(
                salaB.x + salaB.width / 2,
                salaB.y + salaB.height / 2
            );
            
            float distancia = Vector2Int.Distance(centroA, centroB);
            
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                salaMasCercana = idx;
            }
        }

        return salaMasCercana;
    }

    void ConectarDosSalas(RectInt salaA, RectInt salaB)
    {
        // Calcular centros
        Vector2Int centroA = new Vector2Int(
            salaA.x + salaA.width / 2,
            salaA.y + salaA.height / 2
        );
        
        Vector2Int centroB = new Vector2Int(
            salaB.x + salaB.width / 2,
            salaB.y + salaB.height / 2
        );

        // Decidir ancho del pasillo
        int anchoPasillo = Random.Range(anchoPasilloMin, anchoPasilloMax + 1);

        // Punto de inicio y fin del pasillo (en bordes de las salas)
        Vector2Int inicio = ObtenerPuntoConexion(salaA, centroB);
        Vector2Int fin = ObtenerPuntoConexion(salaB, centroA);

        // Crear pasillo recto o con curvas
        if (Random.value > probabilidadPasilloCurvo || Vector2Int.Distance(inicio, fin) < largoMaxPasillo)
        {
            // Pasillo recto
            CrearPasilloRecto(inicio, fin, anchoPasillo);
        }
        else
        {
            // Pasillo con curva en L
            CrearPasilloCurvo(inicio, fin, anchoPasillo);
        }
    }

    Vector2Int ObtenerPuntoConexion(RectInt sala, Vector2Int puntoObjetivo)
    {
        // Calcular centro de la sala
        Vector2Int centro = new Vector2Int(
            sala.x + sala.width / 2,
            sala.y + sala.height / 2
        );
        
        // Calcular dirección desde el centro al objetivo
        Vector2Int direccion = puntoObjetivo - centro;
        
        // Normalizar dirección (mantener en ejes)
        if (Mathf.Abs(direccion.x) > Mathf.Abs(direccion.y))
        {
            // Conectar por lados izquierdo/derecho
            int x = direccion.x > 0 ? sala.xMax - 1 : sala.xMin;
            int y = Mathf.Clamp(puntoObjetivo.y, sala.yMin + 1, sala.yMax - 2);
            return new Vector2Int(x, y);
        }
        else
        {
            // Conectar por lados superior/inferior
            int x = Mathf.Clamp(puntoObjetivo.x, sala.xMin + 1, sala.xMax - 2);
            int y = direccion.y > 0 ? sala.yMax - 1 : sala.yMin;
            return new Vector2Int(x, y);
        }
    }

    void CrearPasilloRecto(Vector2Int inicio, Vector2Int fin, int ancho)
    {
        // Primero horizontal, luego vertical
        for (int x = Mathf.Min(inicio.x, fin.x); x <= Mathf.Max(inicio.x, fin.x); x++)
        {
            for (int w = -ancho/2; w <= ancho/2; w++)
            {
                Vector2Int pos = new Vector2Int(x, inicio.y + w);
                PonerSuelo(pos);
            }
        }

        for (int y = Mathf.Min(inicio.y, fin.y); y <= Mathf.Max(inicio.y, fin.y); y++)
        {
            for (int w = -ancho/2; w <= ancho/2; w++)
            {
                Vector2Int pos = new Vector2Int(fin.x + w, y);
                PonerSuelo(pos);
            }
        }
    }

    void CrearPasilloCurvo(Vector2Int inicio, Vector2Int fin, int ancho)
    {
        // Crear punto de giro aleatorio
        int puntoGiroX = Random.Range(Mathf.Min(inicio.x, fin.x), Mathf.Max(inicio.x, fin.x));
        int puntoGiroY = Random.Range(Mathf.Min(inicio.y, fin.y), Mathf.Max(inicio.y, fin.y));

        Vector2Int giro = new Vector2Int(puntoGiroX, puntoGiroY);

        // Crear tres segmentos: inicio->giro, giro->fin
        CrearSegmentoPasillo(inicio, new Vector2Int(giro.x, inicio.y), ancho);
        CrearSegmentoPasillo(new Vector2Int(giro.x, inicio.y), giro, ancho);
        CrearSegmentoPasillo(giro, new Vector2Int(fin.x, giro.y), ancho);
        CrearSegmentoPasillo(new Vector2Int(fin.x, giro.y), fin, ancho);
    }

    void CrearSegmentoPasillo(Vector2Int inicio, Vector2Int fin, int ancho)
    {
        if (inicio.x == fin.x)
        {
            // Segmento vertical
            for (int y = Mathf.Min(inicio.y, fin.y); y <= Mathf.Max(inicio.y, fin.y); y++)
            {
                for (int w = -ancho/2; w <= ancho/2; w++)
                {
                    Vector2Int pos = new Vector2Int(inicio.x + w, y);
                    PonerSuelo(pos);
                }
            }
        }
        else if (inicio.y == fin.y)
        {
            // Segmento horizontal
            for (int x = Mathf.Min(inicio.x, fin.x); x <= Mathf.Max(inicio.x, fin.x); x++)
            {
                for (int w = -ancho/2; w <= ancho/2; w++)
                {
                    Vector2Int pos = new Vector2Int(x, inicio.y + w);
                    PonerSuelo(pos);
                }
            }
        }
    }

    void PonerSuelo(Vector2Int pos)
    {
        if (suelo.Add(pos))
        {
            PonerTileConVariacion(tilemapSuelo, pos, tileSuelo, variacionesSuelo);
        }
    }

    void GenerarParedes()
    {
        HashSet<Vector2Int> paredes = new HashSet<Vector2Int>();

        // Encontrar todas las posiciones adyacentes al suelo que son paredes
        foreach (var s in suelo)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int p = s + new Vector2Int(dx, dy);
                if (!suelo.Contains(p))
                    paredes.Add(p);
            }
        }

        // Analizar cada posición de pared para decidir qué tile poner
        foreach (var p in paredes)
            AnalizarYPonerPared(p);
    }

    void GenerarObjetosPorTipoSala()
    {
        foreach (var sala in salas)
        {
            if (tiposSalas.ContainsKey(sala))
            {
                string tipo = tiposSalas[sala];
                DungeonRoomConfig config = System.Array.Find(configuracionesSalas, c => c.nombreTipo == tipo);
                
                if (config != null && ((config.objetosPosibles != null && config.objetosPosibles.Length > 0) || (config.objetosTiles != null && config.objetosTiles.Length > 0 && tilemapObjetos != null)))
                {
                    GenerarObjetosEnSala(sala, config);
                }
            }
        }
        
        Debug.Log($"Objetos totales generados: {ContarTotalObjetos()}");
    }

    void GenerarObjetosEnSala(RectInt sala, DungeonRoomConfig config)
    {
        int numObjetosDeseados = Random.Range(config.minObjetos, config.maxObjetos + 1);
        int objetosGenerados = 0;
        int intentosMaximos = numObjetosDeseados * 20;
        int intentos = 0;
        
        List<Vector2Int> posicionesUsadas = new List<Vector2Int>();
        
        while (objetosGenerados < numObjetosDeseados && intentos < intentosMaximos)
        {
            intentos++;
            
            // Obtener posición aleatoria
            Vector2Int posicion = ObtenerPosicionValidaEnSala(sala, config, posicionesUsadas);
            
            if (posicion != Vector2Int.zero)
            {
                // Si hay tiles configurados para objetos y hay Tilemap asignado, colocar tile en vez de instanciar prefab
                if (config.objetosTiles != null && config.objetosTiles.Length > 0 && tilemapObjetos != null)
                {
                    TileBase tile = config.objetosTiles[Random.Range(0, config.objetosTiles.Length)];
                    tilemapObjetos.SetTile((Vector3Int)posicion, tile);
                    objetosTilesPorSala[sala].Add(posicion);
                    posicionesUsadas.Add(posicion);
                    objetosGenerados++;
                }
                else if (config.objetosPosibles != null && config.objetosPosibles.Length > 0)
                {
                    // Seleccionar objeto aleatorio de los posibles
                    GameObject prefab = config.objetosPosibles[Random.Range(0, config.objetosPosibles.Length)];
                    
                    // Spawnear objeto
                    Vector3 spawnPos = new Vector3(posicion.x + 0.5f, posicion.y + 0.5f, 0);
                    GameObject nuevoObj = Instantiate(prefab, spawnPos, Quaternion.identity);
                    nuevoObj.transform.parent = this.transform;
                    
                    // Guardar en lista
                    objetosPorSala[sala].Add(nuevoObj);
                    posicionesUsadas.Add(posicion);
                    objetosGenerados++;
                    
                    // Aplicar rotación aleatoria si es decorativo
                    if (prefab.CompareTag("Decoracion"))
                    {
                        nuevoObj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 4) * 90);
                    }
                }
            }
        }
    }

    Vector2Int ObtenerPosicionValidaEnSala(RectInt sala, DungeonRoomConfig config, List<Vector2Int> posicionesUsadas)
    {
        Vector2Int posicion;
        
        // Determinar área de spawn basada en configuración
        if (config.objetosEnCentro)
        {
            // Solo en el 50% central de la sala
            int margenX = sala.width / 4;
            int margenY = sala.height / 4;
            int x = Random.Range(sala.xMin + margenX, sala.xMax - margenX);
            int y = Random.Range(sala.yMin + margenY, sala.yMax - margenY);
            posicion = new Vector2Int(x, y);
        }
        else
        {
            // En cualquier lugar de la sala (pero no pegado a las paredes)
            int x = Random.Range(sala.xMin + 1, sala.xMax - 1);
            int y = Random.Range(sala.yMin + 1, sala.yMax - 1);
            posicion = new Vector2Int(x, y);
        }
        
        // Verificar condiciones
        if (!suelo.Contains(posicion)) return Vector2Int.zero; // No es suelo
        
        // Verificar distancia mínima entre objetos
        foreach (var posUsada in posicionesUsadas)
        {
            if (Vector2Int.Distance(posicion, posUsada) < config.distanciaMinimaEntreObjetos)
            {
                return Vector2Int.zero;
            }
        }
        
        // Verificar que no esté en una entrada/pasillo
        if (EsPosicionCercaDePasillo(posicion, sala))
        {
            return Vector2Int.zero;
        }
        
        return posicion;
    }

    bool EsPosicionCercaDePasillo(Vector2Int pos, RectInt sala)
    {
        // Verificar si está cerca de los bordes de la sala (posibles entradas)
        int distanciaBorde = 2;
        if (pos.x <= sala.xMin + distanciaBorde || pos.x >= sala.xMax - distanciaBorde - 1 ||
            pos.y <= sala.yMin + distanciaBorde || pos.y >= sala.yMax - distanciaBorde - 1)
        {
            // Verificar si realmente es una entrada (hay suelo continuo fuera)
            int sueloFuera = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int checkPos = pos + new Vector2Int(dx, dy);
                    if (suelo.Contains(checkPos) && !sala.Contains(checkPos))
                    {
                        sueloFuera++;
                    }
                }
            }
            return sueloFuera > 0;
        }
        
        return false;
    }

    void MostrarEstadisticasSalas()
    {
        Dictionary<string, int> conteoPorTipo = new Dictionary<string, int>();
        
        foreach (var tipo in tiposSalas.Values)
        {
            if (conteoPorTipo.ContainsKey(tipo))
                conteoPorTipo[tipo]++;
            else
                conteoPorTipo[tipo] = 1;
        }
        
        Debug.Log("=== ESTADÍSTICAS DE SALAS ===");
        foreach (var kvp in conteoPorTipo)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} salas");
        }
        
        // Mostrar objetos por tipo (prefabs + tiles)
        Debug.Log("=== OBJETOS POR TIPO DE SALA ===");
        foreach (var sala in salas)
        {
            if (tiposSalas.ContainsKey(sala))
            {
                int cuentaPrefabs = objetosPorSala.ContainsKey(sala) ? objetosPorSala[sala].Count : 0;
                int cuentaTiles = objetosTilesPorSala.ContainsKey(sala) ? objetosTilesPorSala[sala].Count : 0;
                int totalSala = cuentaPrefabs + cuentaTiles;
                Debug.Log($"Sala {sala} ({tiposSalas[sala]}): {totalSala} objetos ({cuentaPrefabs} prefabs, {cuentaTiles} tiles)");
            }
        }
    }

    int ContarTotalObjetos()
    {
        int total = 0;
        foreach (var lista in objetosPorSala.Values)
        {
            total += lista.Count;
        }
        foreach (var listaTiles in objetosTilesPorSala.Values)
        {
            total += listaTiles.Count;
        }
        return total;
    }

    void MostrarDebugVisual()
    {
        foreach (var sala in salas)
        {
            if (tiposSalas.ContainsKey(sala))
            {
                string tipo = tiposSalas[sala];
                DungeonRoomConfig config = System.Array.Find(configuracionesSalas, c => c.nombreTipo == tipo);
                
                if (config != null)
                {
                    Vector2 centro = new Vector2(sala.x + sala.width / 2, sala.y + sala.height / 2);
                    Vector2 tamaño = new Vector2(sala.width, sala.height);
                    
                    // Dibujar rectángulo del color del tipo
                    DebugDrawRect(sala, config.colorDebug, duracionDebug);
                    
                    // Mostrar texto con el tipo
                    Debug.Log($"Sala en {centro}: {tipo} ({sala.width}x{sala.height})");
                }
            }
        }
    }

    void DebugDrawRect(RectInt rect, Color color, float duration)
    {
        Vector3 bottomLeft = new Vector3(rect.xMin, rect.yMin, 0);
        Vector3 topLeft = new Vector3(rect.xMin, rect.yMax, 0);
        Vector3 topRight = new Vector3(rect.xMax, rect.yMax, 0);
        Vector3 bottomRight = new Vector3(rect.xMax, rect.yMin, 0);
        
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
    }

    void AnalizarYPonerPared(Vector2Int pos)
    {
        // Verificar vecinos en las 4 direcciones cardinales
        bool A = suelo.Contains(pos + Vector2Int.up);      // Arriba
        bool B = suelo.Contains(pos + Vector2Int.down);    // Abajo
        bool I = suelo.Contains(pos + Vector2Int.left);    // Izquierda
        bool D = suelo.Contains(pos + Vector2Int.right);   // Derecha

        // Verificar vecinos diagonales
        bool AI = suelo.Contains(pos + Vector2Int.up + Vector2Int.left);
        bool AD = suelo.Contains(pos + Vector2Int.up + Vector2Int.right);
        bool BI = suelo.Contains(pos + Vector2Int.down + Vector2Int.left);
        bool BD = suelo.Contains(pos + Vector2Int.down + Vector2Int.right);

        // ESQUINAS INTERNAS SUPERIORES
        if (B && D && !A && !I)
        { 
            PonerTileConVariacion(tilemapParedesBase, pos, esquinaSupIzqBase, variacionesEsquinaSupIzqBase);
            PonerTileConVariacion(tilemapParedesTop, pos + Vector2Int.up, esquinaSupIzqTop, variacionesEsquinaSupIzqTop);
            return;
        }

        if (B && I && !A && !D)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, esquinaSupDerBase, variacionesEsquinaSupDerBase);
            PonerTileConVariacion(tilemapParedesTop, pos + Vector2Int.up, esquinaSupDerTop, variacionesEsquinaSupDerTop);
            return;
        }

        // ESQUINAS INTERNAS INFERIORES
        if (A && D && !B && !I)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, esquinaInfIzq, variacionesEsquinaInfIzq);
            return;
        }

        if (A && I && !B && !D)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, esquinaInfDer, variacionesEsquinaInfDer);
            return;
        }

        // ESQUINAS EXTERNAS SUPERIORES IZQUIERDA
        if (!A && !B && !I && !D && BI && !AI)
        {
            PonerTileConVariacion(tilemapEsquinasExtBase, pos, extSupIzqBase, variacionesExtSupIzqBase);
            PonerTileConVariacion(tilemapEsquinasExtTop, pos + Vector2Int.up, extSupIzqTop, variacionesExtSupIzqTop);
            return;
        }

        // ESQUINAS EXTERNAS SUPERIORES DERECHA
        if (!A && !B && !I && !D && BD && !AD)
        {
            PonerTileConVariacion(tilemapEsquinasExtBase, pos, extSupDerBase, variacionesExtSupDerBase);
            PonerTileConVariacion(tilemapEsquinasExtTop, pos + Vector2Int.up, extSupDerTop, variacionesExtSupDerTop);
            return;
        }

        // ESQUINAS EXTERNAS INFERIORES IZQUIERDA
        if (!A && !B && !I && !D && AI && !BI)
        {
            PonerTileConVariacion(tilemapEsquinasExtBase, pos, extInfIzq, variacionesExtInfIzq);
            return;
        }

        // ESQUINAS EXTERNAS INFERIORES DERECHA
        if (!A && !B && !I && !D && AD && !BD)
        {
            PonerTileConVariacion(tilemapEsquinasExtBase, pos, extInfDer, variacionesExtInfDer);
            return;
        }

        // PARED SUPERIOR
        if (B && !A && !I && !D)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, paredArriba, variacionesParedArriba);
            PonerTileConVariacion(tilemapParedesTop, pos + Vector2Int.up, paredArribaTop, variacionesParedArribaTop);
            return;
        }

        // PARED INFERIOR
        if (A && !B && !I && !D)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, paredAbajo, variacionesParedAbajo);
            return;
        }

        // LATERAL IZQUIERDO
        if (D && !I && !A && !B)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, paredIzquierda, variacionesParedIzquierda);
            return;
        }

        // LATERAL DERECHO
        if (I && !D && !A && !B)
        {
            PonerTileConVariacion(tilemapParedesBase, pos, paredDerecha, variacionesParedDerecha);
            return;
        }
    }

    void PonerTileConVariacion(Tilemap tilemap, Vector2Int pos, TileBase tileBase, TileBase[] variaciones)
    {
        TileBase tileAUsar = tileBase;
        
        if (variaciones != null && variaciones.Length > 0 && Random.value <= probabilidadVariacion)
        {
            tileAUsar = variaciones[Random.Range(0, variaciones.Length)];
        }
        
        tilemap.SetTile((Vector3Int)pos, tileAUsar);
    }
}