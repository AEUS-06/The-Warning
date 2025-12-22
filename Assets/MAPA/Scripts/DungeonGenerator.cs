using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GeneradorDungeon : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemapSuelo;
    public Tilemap tilemapParedesBase;
    public Tilemap tilemapParedesTop;
    public Tilemap tilemapEsquinasExtBase;
    public Tilemap tilemapEsquinasExtTop;

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

    // Variables internas
    private HashSet<Vector2Int> suelo = new HashSet<Vector2Int>();
    private List<RectInt> salas = new List<RectInt>();

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
    }

    void LimpiarTodo()
    {
        tilemapSuelo.ClearAllTiles();
        tilemapParedesBase.ClearAllTiles();
        tilemapParedesTop.ClearAllTiles();
        tilemapEsquinasExtBase.ClearAllTiles();
        tilemapEsquinasExtTop.ClearAllTiles();
        suelo.Clear();
        salas.Clear();
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
        Vector2Int centroA = Vector2Int.RoundToInt(salas[salaIndex].center);
        int salaMasCercana = salasDisponibles[0];
        float distanciaMinima = float.MaxValue;

        foreach (int idx in salasDisponibles)
        {
            Vector2Int centroB = Vector2Int.RoundToInt(salas[idx].center);
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
        Vector2Int centroA = Vector2Int.RoundToInt(salaA.center);
        Vector2Int centroB = Vector2Int.RoundToInt(salaB.center);

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
        // Encontrar el punto en el borde de la sala más cercano al objetivo
        Vector2Int centro = Vector2Int.RoundToInt(sala.center);
        
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