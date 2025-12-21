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

    [Header("Paredes Simples")]
    public TileBase paredArriba;
    public TileBase paredAbajo;
    public TileBase paredIzquierda;
    public TileBase paredDerecha;

    [Header("Esquinas Internas Inferiores")]
    public TileBase esquinaInfIzq;
    public TileBase esquinaInfDer;

    [Header("Esquinas Internas Superiores BASE")]
    public TileBase esquinaSupIzqBase;
    public TileBase esquinaSupDerBase;

    [Header("Esquinas Internas Superiores TOP")]
    public TileBase esquinaSupIzqTop;
    public TileBase esquinaSupDerTop;

    [Header("Pared Superior TOP")]
    public TileBase paredArribaTop;

    [Header("Esquinas Externas Inferiores")]
    public TileBase extInfIzq;
    public TileBase extInfDer;

    [Header("Esquinas Externas Superiores BASE")]
    public TileBase extSupIzqBase;
    public TileBase extSupDerBase;

    [Header("Esquinas Externas Superiores TOP")]
    public TileBase extSupIzqTop;
    public TileBase extSupDerTop;

    [Header("Dungeon")]
    public int numeroSalas = 6;
    public int anchoMinSala = 6;
    public int altoMinSala = 6;
    public int anchoMaxSala = 12;
    public int altoMaxSala = 12;

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

    //GENERACION

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
                    tilemapSuelo.SetTile((Vector3Int)p, tileSuelo);
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
            tilemapSuelo.SetTile((Vector3Int)p, tileSuelo);
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

    //LOGICA

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

        //ESQUINAS INTERNAS SUPERIORES
        if (B && D && !A && !I)
        { 
            tilemapParedesBase.SetTile((Vector3Int)pos, esquinaSupIzqBase);
            tilemapParedesTop.SetTile((Vector3Int)(pos + Vector2Int.up), esquinaSupIzqTop);
            return;
        }

        if (B && I && !A && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, esquinaSupDerBase);
            tilemapParedesTop.SetTile((Vector3Int)(pos + Vector2Int.up), esquinaSupDerTop);
            return;
        }

        //ESQUINAS INTERNAS INFERIORES
        if (A && D && !B && !I)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, esquinaInfIzq);
            return;
        }

        if (A && I && !B && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, esquinaInfDer);
            return;
        }

        //ESQUINAS EXTERNAS SUPERIORES IZQUIERDA
        if (!A && !B && !I && !D && BI && !AI)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, extSupIzqBase);
            tilemapEsquinasExtTop.SetTile((Vector3Int)(pos + Vector2Int.up), extSupIzqTop);
            return;
        }

        //ESQUINAS EXTERNAS SUPERIORES DERECHA
        if (!A && !B && !I && !D && BD && !AD)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, extSupDerBase);
            tilemapEsquinasExtTop.SetTile((Vector3Int)(pos + Vector2Int.up), extSupDerTop);
            return;
        }

        //ESQUINAS EXTERNAS INFERIORES IZQUIERDA
        if (!A && !B && !I && !D && AI && !BI)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, extInfIzq);
            return;
        }

        //ESQUINAS EXTERNAS INFERIORES DERECHA
        if (!A && !B && !I && !D && AD && !BD)
        {
            tilemapEsquinasExtBase.SetTile((Vector3Int)pos, extInfDer);
            return;
        }

        //PARED SUPERIOR
        if (B && !A && !I && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, paredArriba);
            tilemapParedesTop.SetTile((Vector3Int)(pos + Vector2Int.up), paredArribaTop);
            return;
        }

        //PARED INFERIOR
        if (A && !B && !I && !D)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, paredAbajo);
            return;
        }

        //LATERALES
        if (D && !I && !A && !B)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, paredIzquierda);
            return;
        }

        if (I && !D && !A && !B)
        {
            tilemapParedesBase.SetTile((Vector3Int)pos, paredDerecha);
            return;
        }
    }
}