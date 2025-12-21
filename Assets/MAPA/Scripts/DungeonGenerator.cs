using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GeneradorDungeon : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemapSuelo;
    public Tilemap tilemapParedesBase;
    public Tilemap tilemapParedesTop;

    [Header("Tilemaps Esquinas Externas")]
    public Tilemap tilemapEsquinasExtBase;
    public Tilemap tilemapEsquinasExtTop;

    [Header("Suelo")]
    public TileBase tileSuelo;
    [Header("Variaciones Suelo")]
    public TileBase[] variacionesSuelo;
    [Range(0f, 1f)]
    public float probabilidadVariacionSuelo = 0.3f;

    [Header("Paredes Simples")]
    public TileBase paredArriba;
    public TileBase paredAbajo;
    public TileBase paredIzquierda;
    public TileBase paredDerecha;
    
    [Header("Variaciones Pared Arriba")]
    public TileBase[] variacionesParedArriba;
    [Header("Variaciones Pared Abajo")]
    public TileBase[] variacionesParedAbajo;
    [Header("Variaciones Pared Izquierda")]
    public TileBase[] variacionesParedIzquierda;
    [Header("Variaciones Pared Derecha")]
    public TileBase[] variacionesParedDerecha;
    [Range(0f, 1f)]
    public float probabilidadVariacionPared = 0.2f;

    [Header("Esquinas Internas Inferiores")]
    public TileBase esquinaInfIzq;
    public TileBase esquinaInfDer;
    [Header("Variaciones Esquina Inf Izq")]
    public TileBase[] variacionesEsquinaInfIzq;
    [Header("Variaciones Esquina Inf Der")]
    public TileBase[] variacionesEsquinaInfDer;

    [Header("Esquinas Internas Superiores BASE")]
    public TileBase esquinaSupIzqBase;
    public TileBase esquinaSupDerBase;
    [Header("Variaciones Esquina Sup Izq Base")]
    public TileBase[] variacionesEsquinaSupIzqBase;
    [Header("Variaciones Esquina Sup Der Base")]
    public TileBase[] variacionesEsquinaSupDerBase;

    [Header("Esquinas Internas Superiores TOP")]
    public TileBase esquinaSupIzqTop;
    public TileBase esquinaSupDerTop;
    [Header("Variaciones Esquina Sup Izq Top")]
    public TileBase[] variacionesEsquinaSupIzqTop;
    [Header("Variaciones Esquina Sup Der Top")]
    public TileBase[] variacionesEsquinaSupDerTop;

    [Header("Pared Superior TOP")]
    public TileBase paredArribaTop;
    [Header("Variaciones Pared Arriba Top")]
    public TileBase[] variacionesParedArribaTop;

    [Header("Esquinas Externas Inferiores")]
    public TileBase extInfIzq;
    public TileBase extInfDer;
    [Header("Variaciones Esquina Ext Inf Izq")]
    public TileBase[] variacionesExtInfIzq;
    [Header("Variaciones Esquina Ext Inf Der")]
    public TileBase[] variacionesExtInfDer;

    [Header("Esquinas Externas Superiores BASE")]
    public TileBase extSupIzqBase;
    public TileBase extSupDerBase;
    [Header("Variaciones Esquina Ext Sup Izq Base")]
    public TileBase[] variacionesExtSupIzqBase;
    [Header("Variaciones Esquina Ext Sup Der Base")]
    public TileBase[] variacionesExtSupDerBase;

    [Header("Esquinas Externas Superiores TOP")]
    public TileBase extSupIzqTop;
    public TileBase extSupDerTop;
    [Header("Variaciones Esquina Ext Sup Izq Top")]
    public TileBase[] variacionesExtSupIzqTop;
    [Header("Variaciones Esquina Ext Sup Der Top")]
    public TileBase[] variacionesExtSupDerTop;

    [Header("Dungeon")]
    public int numeroSalas = 6;
    public int anchoMinSala = 6;
    public int altoMinSala = 6;
    public int anchoMaxSala = 12;
    public int altoMaxSala = 12;

    private HashSet<Vector2Int> suelo = new HashSet<Vector2Int>();
    private List<RectInt> salas = new List<RectInt>();
    
    // Método auxiliar para obtener variaciones aleatorias
    private TileBase ObtenerVariacion(TileBase tileBase, TileBase[] variaciones, float probabilidad = 0.3f)
    {
        // Si no hay variaciones o no se activa la probabilidad, usar el tile base
        if (variaciones == null || variaciones.Length == 0 || Random.value > probabilidad)
            return tileBase;
        
        // Seleccionar una variación aleatoria
        return variaciones[Random.Range(0, variaciones.Length)];
    }

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

    //GENERACIÓN

    void GenerarDungeon()
    {
        GenerarSalas();
        ConectarSalas();
        GenerarParedes();
    }

    void GenerarSalas()
    {
        int intentos = 0;

        while (salas.Count < numeroSalas && intentos < 200)
        {
            intentos++;

            int w = Random.Range(anchoMinSala, anchoMaxSala + 1);
            int h = Random.Range(altoMinSala, altoMaxSala + 1);
            int x = Random.Range(-30, 30);
            int y = Random.Range(-20, 20);

            RectInt sala = new RectInt(x, y, w, h);

            bool solapa = false;
            foreach (var s in salas)
                if (s.Overlaps(sala)) solapa = true;

            if (!solapa)
            {
                salas.Add(sala);
                for (int i = sala.xMin; i < sala.xMax; i++)
                for (int j = sala.yMin; j < sala.yMax; j++)
                {
                    Vector2Int p = new Vector2Int(i, j);
                    suelo.Add(p);
                    // Usar variaciones para el suelo
                    tilemapSuelo.SetTile((Vector3Int)p, 
                        ObtenerVariacion(tileSuelo, variacionesSuelo, probabilidadVariacionSuelo));
                }
            }
        }
    }

    void ConectarSalas()
    {
        for (int i = 0; i < salas.Count - 1; i++)
        {
            Vector2Int a = Vector2Int.RoundToInt(salas[i].center);
            Vector2Int b = Vector2Int.RoundToInt(salas[i + 1].center);

            for (int x = a.x; x != b.x; x += x < b.x ? 1 : -1)
                PonerSuelo(new Vector2Int(x, a.y));

            for (int y = a.y; y != b.y; y += y < b.y ? 1 : -1)
                PonerSuelo(new Vector2Int(b.x, y));
        }
    }

    void PonerSuelo(Vector2Int p)
    {
        if (suelo.Add(p))
            tilemapSuelo.SetTile((Vector3Int)p, 
                ObtenerVariacion(tileSuelo, variacionesSuelo, probabilidadVariacionSuelo));
    }

    //PAREDES

    void GenerarParedes()
    {
        HashSet<Vector2Int> paredes = new HashSet<Vector2Int>();

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

        foreach (var p in paredes)
            AnalizarYPonerPared(p);
    }

    //LÓGICA CORREGIDA PARA ESQUINAS EXTERNAS

    void AnalizarYPonerPared(Vector2Int pos)
    {
        bool A = suelo.Contains(pos + Vector2Int.up);
        bool B = suelo.Contains(pos + Vector2Int.down);
        bool I = suelo.Contains(pos + Vector2Int.left);
        bool D = suelo.Contains(pos + Vector2Int.right);

        bool AI = suelo.Contains(pos + Vector2Int.up + Vector2Int.left);
        bool AD = suelo.Contains(pos + Vector2Int.up + Vector2Int.right);
        bool BI = suelo.Contains(pos + Vector2Int.down + Vector2Int.left);
        bool BD = suelo.Contains(pos + Vector2Int.down + Vector2Int.right);

        // ================= ESQUINAS INTERNAS SUPERIORES =================
        if (B && D && !A && !I)
        { 
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(esquinaSupIzqBase, variacionesEsquinaSupIzqBase, probabilidadVariacionPared));
            tilemapParedesTop.SetTile((Vector3Int)(pos + Vector2Int.up), 
                ObtenerVariacion(esquinaSupIzqTop, variacionesEsquinaSupIzqTop, probabilidadVariacionPared));
            return;
        }

        if (B && I && !A && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(esquinaSupDerBase, variacionesEsquinaSupDerBase, probabilidadVariacionPared));
            tilemapParedesTop.SetTile((Vector3Int)(pos + Vector2Int.up), 
                ObtenerVariacion(esquinaSupDerTop, variacionesEsquinaSupDerTop, probabilidadVariacionPared));
            return;
        }

        // ================= ESQUINAS INTERNAS INFERIORES =================
        if (A && D && !B && !I)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(esquinaInfIzq, variacionesEsquinaInfIzq, probabilidadVariacionPared));
            return;
        }

        if (A && I && !B && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(esquinaInfDer, variacionesEsquinaInfDer, probabilidadVariacionPared));
            return;
        }

        // ================= ESQUINAS EXTERNAS SUPERIORES IZQUIERDA =================
        if (!A && !B && !I && !D && BI && !AI)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(extSupIzqBase, variacionesExtSupIzqBase, probabilidadVariacionPared));
            tilemapEsquinasExtTop.SetTile((Vector3Int)(pos + Vector2Int.up), 
                ObtenerVariacion(extSupIzqTop, variacionesExtSupIzqTop, probabilidadVariacionPared));
            return;
        }

        // ================= ESQUINAS EXTERNAS SUPERIORES DERECHA =================
        if (!A && !B && !I && !D && BD && !AD)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(extSupDerBase, variacionesExtSupDerBase, probabilidadVariacionPared));
            tilemapEsquinasExtTop.SetTile((Vector3Int)(pos + Vector2Int.up), 
                ObtenerVariacion(extSupDerTop, variacionesExtSupDerTop, probabilidadVariacionPared));
            return;
        }

        // ================= ESQUINAS EXTERNAS INFERIORES IZQUIERDA =================
        if (!A && !B && !I && !D && AI && !BI)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(extInfIzq, variacionesExtInfIzq, probabilidadVariacionPared));
            return;
        }

        // ================= ESQUINAS EXTERNAS INFERIORES DERECHA =================
        if (!A && !B && !I && !D && AD && !BD)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(extInfDer, variacionesExtInfDer, probabilidadVariacionPared));
            return;
        }

        // ================= PARED SUPERIOR =================
        if (B && !A && !I && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(paredArriba, variacionesParedArriba, probabilidadVariacionPared));
            tilemapParedesTop.SetTile((Vector3Int)(pos + Vector2Int.up), 
                ObtenerVariacion(paredArribaTop, variacionesParedArribaTop, probabilidadVariacionPared));
            return;
        }

        // ================= PARED INFERIOR =================
        if (A && !B && !I && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(paredAbajo, variacionesParedAbajo, probabilidadVariacionPared));
            return;
        }

        // ================= LATERALES =================
        if (D && !I && !A && !B)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(paredIzquierda, variacionesParedIzquierda, probabilidadVariacionPared));
            return;
        }

        if (I && !D && !A && !B)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, 
                ObtenerVariacion(paredDerecha, variacionesParedDerecha, probabilidadVariacionPared));
            return;
        }
    }
}