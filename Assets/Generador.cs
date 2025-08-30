using System.Collections.Generic;
using UnityEngine;

public class GeneradorMazmorra : MonoBehaviour
{
    [Header("Prefabs - Suelo y Paredes")]
    public GameObject sueloPrefab;

    [Tooltip("Pared superior: principal (visible encima)")]
    public GameObject paredArribaPrefab;
    [Tooltip("Pared superior: secundario (visible entre suelo y principal)")]
    public GameObject paredArriba2Prefab;

    public GameObject paredAbajoPrefab;
    public GameObject paredIzquierdaPrefab;
    public GameObject paredDerechaPrefab;

    [Header("Prefabs - Esquinas (superiores con secundario debajo)")]
    public GameObject esquinaSupIzqPrefab;
    public GameObject esquinaSupIzq2Prefab;
    public GameObject esquinaSupDerPrefab;
    public GameObject esquinaSupDer2Prefab;

    [Header("Esquinas inferiores (simples)")]
    public GameObject esquinaInfIzqPrefab;
    public GameObject esquinaInfDerPrefab;

    [Header("Esquinas internas")]
    public GameObject esquinaIntArribaIzqPrefab;
    public GameObject esquinaIntArribaDerPrefab;
    public GameObject esquinaIntAbajoIzqPrefab;
    public GameObject esquinaIntAbajoDerPrefab;

    [Header("Coberturas")]
    public GameObject coberturaPrefab;
    [Range(0, 4)] public int coberturasMaxPorSala = 3;

    [Header("Tamaño mapa")]
    public int ancho = 60;
    public int alto = 60;

    [Header("Salas (rectangulares)")]
    public int cantidadSalas = 6;
    public int tamSalaMin = 5;
    public int tamSalaMax = 10;
    [Tooltip("Margen entre salas para que no se encimen")]
    public int separacionSalas = 1;

    [Header("Offsets Y para secundarios (opcional)")]
    public float yOffsetParedArriba2 = -0.20f;
    public float yOffsetEsquinaArriba2 = -0.20f;

    [Header("Orden de dibujo (SpriteRenderer.sortingOrder)")]
    public int orderSuelo = 0;
    public int orderSecundario = 1; // prefab2
    public int orderPrincipal = 2;  // prefab principal
    public int orderCobertura = 3;

    //mapa interno
    private int[,] mapa; // 0 vacío, 1 suelo
    private readonly List<RectInt> salas = new List<RectInt>();
    private readonly HashSet<Vector2Int> celdasSuelo = new HashSet<Vector2Int>();

    //paredes y esquinas 
    private readonly HashSet<Vector2Int> bordesArriba = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> bordesAbajo = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> bordesIzq = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> bordesDer = new HashSet<Vector2Int>();

    private readonly HashSet<Vector2Int> esquSupIzq = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> esquSupDer = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> esquInfIzq = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> esquInfDer = new HashSet<Vector2Int>();

    private readonly HashSet<Vector2Int> esquIntArribaIzq = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> esquIntArribaDer = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> esquIntAbajoIzq = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> esquIntAbajoDer = new HashSet<Vector2Int>();

    //celdas ya ocupadas por coberturas
    private readonly HashSet<Vector2Int> celdasCobertura = new HashSet<Vector2Int>();

    //evitar que se amontonen los prefabs
    [System.Flags]
    enum CeldaEstado { Ninguno = 0, Suelo = 1, Secundario = 2, Principal = 4, Cobertura = 8 }
    private readonly Dictionary<Vector2Int, CeldaEstado> estados = new Dictionary<Vector2Int, CeldaEstado>();

    void Start()
    {
        GenerarMazmorra();
    }

    public void GenerarMazmorra()
    {
        //reinicia todo
        mapa = new int[ancho, alto];
        salas.Clear();
        celdasSuelo.Clear();
        bordesArriba.Clear(); bordesAbajo.Clear(); bordesIzq.Clear(); bordesDer.Clear();
        esquSupIzq.Clear(); esquSupDer.Clear(); esquInfIzq.Clear(); esquInfDer.Clear();
        esquIntArribaIzq.Clear(); esquIntArribaDer.Clear(); esquIntAbajoIzq.Clear(); esquIntAbajoDer.Clear();
        celdasCobertura.Clear();
        estados.Clear();

        LimpiarHijos();

        GenerarSalasRectangularesConPadding();
        ConectarSalasConMST();
        PlanificarBordes();
        InstanciarSuelo();
        InstanciarBordesSecundarios();
        InstanciarBordesPrincipales();
        InstanciarEsquinasInternas();
        GenerarCoberturasEnSalas();
    }

    //creacion de las salas
    void GenerarSalasRectangularesConPadding()
    {
        int intentos = 0;
        while (salas.Count < cantidadSalas && intentos < cantidadSalas * 40)
        {
            intentos++;
            int w = Random.Range(tamSalaMin, tamSalaMax + 1);
            int h = Random.Range(tamSalaMin, tamSalaMax + 1);

            int maxXExclusive = Mathf.Max(2, ancho - w);
            int maxYExclusive = Mathf.Max(2, alto - h);
            if (maxXExclusive <= 1 || maxYExclusive <= 1) break;

            int x = Random.Range(1, maxXExclusive);
            int y = Random.Range(1, maxYExclusive);

            var nueva = new RectInt(x, y, w, h);

            bool solapa = false;
            foreach (var s in salas)
            {
                var sPad = new RectInt(s.xMin - separacionSalas, s.yMin - separacionSalas, s.width + separacionSalas * 2, s.height + separacionSalas * 2);
                if (sPad.Overlaps(nueva)) { solapa = true; break; }
            }
            if (solapa) continue;

            salas.Add(nueva);

            // Rellenar suelo
            for (int ix = nueva.xMin; ix < nueva.xMax; ix++)
                for (int iy = nueva.yMin; iy < nueva.yMax; iy++)
                    if (Dentro(ix, iy))
                    {
                        mapa[ix, iy] = 1;
                        celdasSuelo.Add(new Vector2Int(ix, iy));
                    }
        }
    }

    //control de los pasillos
    void ConectarSalasConMST()
    {
        if (salas.Count <= 1) return;
        var conectadas = new HashSet<int> { 0 };

        while (conectadas.Count < salas.Count)
        {
            int mejorA = -1, mejorB = -1, mejorD = int.MaxValue;
            foreach (int i in conectadas)
            {
                for (int j = 0; j < salas.Count; j++)
                {
                    if (conectadas.Contains(j)) continue;
                    int d = DistManhattan(Centro(salas[i]), Centro(salas[j]));
                    if (d < mejorD) { mejorD = d; mejorA = i; mejorB = j; }
                }
            }
            if (mejorA == -1) break;
            CrearPasilloEnL(Centro(salas[mejorA]), Centro(salas[mejorB]));
            conectadas.Add(mejorB);
        }
    }

    Vector2Int Centro(RectInt r) => new Vector2Int(r.xMin + r.width / 2, r.yMin + r.height / 2);
    int DistManhattan(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    void CrearPasilloEnL(Vector2Int a, Vector2Int b)
    {
        if (Random.value < 0.5f) { CrearLineaH(a.x, b.x, a.y); CrearLineaV(a.y, b.y, b.x); }
        else { CrearLineaV(a.y, b.y, a.x); CrearLineaH(a.x, b.x, b.y); }
    }

    void CrearLineaH(int x1, int x2, int y)
    {
        int min = Mathf.Min(x1, x2), max = Mathf.Max(x1, x2);
        for (int x = min; x <= max; x++)
            if (Dentro(x, y)) { mapa[x, y] = 1; celdasSuelo.Add(new Vector2Int(x, y)); }
    }

    void CrearLineaV(int y1, int y2, int x)
    {
        int min = Mathf.Min(y1, y2), max = Mathf.Max(y1, y2);
        for (int y = min; y <= max; y++)
            if (Dentro(x, y)) { mapa[x, y] = 1; celdasSuelo.Add(new Vector2Int(x, y)); }
    }

    //creacion de las paredes y esquinas
    void PlanificarBordes()
    {
        foreach (var s in celdasSuelo)
        {
            int x = s.x, y = s.y;

            if (EsVacio(x, y + 1)) bordesArriba.Add(new Vector2Int(x, y + 1));
            if (EsVacio(x, y - 1)) bordesAbajo.Add(new Vector2Int(x, y - 1));
            if (EsVacio(x - 1, y)) bordesIzq.Add(new Vector2Int(x - 1, y));
            if (EsVacio(x + 1, y)) bordesDer.Add(new Vector2Int(x + 1, y));

            //esquinas principales externas
            if (EsVacio(x, y + 1) && EsVacio(x - 1, y) && EsVacio(x - 1, y + 1)) esquSupIzq.Add(new Vector2Int(x - 1, y + 1));
            if (EsVacio(x, y + 1) && EsVacio(x + 1, y) && EsVacio(x + 1, y + 1)) esquSupDer.Add(new Vector2Int(x + 1, y + 1));
            if (EsVacio(x, y - 1) && EsVacio(x - 1, y) && EsVacio(x - 1, y - 1)) esquInfIzq.Add(new Vector2Int(x - 1, y - 1));
            if (EsVacio(x, y - 1) && EsVacio(x + 1, y) && EsVacio(x + 1, y - 1)) esquInfDer.Add(new Vector2Int(x + 1, y - 1));

            //esquinas internas
            if (!EsVacio(x - 1, y) && !EsVacio(x, y + 1) && EsVacio(x - 1, y + 1))
                esquIntArribaIzq.Add(new Vector2Int(x - 1, y + 1));
            if (!EsVacio(x + 1, y) && !EsVacio(x, y + 1) && EsVacio(x + 1, y + 1))
                esquIntArribaDer.Add(new Vector2Int(x + 1, y + 1));
            if (!EsVacio(x - 1, y) && !EsVacio(x, y - 1) && EsVacio(x - 1, y - 1))
                esquIntAbajoIzq.Add(new Vector2Int(x - 1, y - 1));
            if (!EsVacio(x + 1, y) && !EsVacio(x, y - 1) && EsVacio(x + 1, y - 1))
                esquIntAbajoDer.Add(new Vector2Int(x + 1, y - 1));
        }
    }

    //instanciar prefabs
    void InstanciarSuelo()
    {
        foreach (var p in celdasSuelo)
            InstanciarConOrdenSiNoExiste(sueloPrefab, p, orderSuelo, CeldaEstado.Suelo);
    }

    void InstanciarBordesSecundarios()
    {
        if (paredArriba2Prefab != null)
            foreach (var p in bordesArriba)
                InstanciarConOrdenSiNoExiste(paredArriba2Prefab, p, orderSecundario, CeldaEstado.Secundario, yOffsetParedArriba2);

        if (esquinaSupIzq2Prefab != null)
            foreach (var p in esquSupIzq)
                InstanciarConOrdenSiNoExiste(esquinaSupIzq2Prefab, p, orderSecundario, CeldaEstado.Secundario, yOffsetEsquinaArriba2);

        if (esquinaSupDer2Prefab != null)
            foreach (var p in esquSupDer)
                InstanciarConOrdenSiNoExiste(esquinaSupDer2Prefab, p, orderSecundario, CeldaEstado.Secundario, yOffsetEsquinaArriba2);
    }

    void InstanciarBordesPrincipales()
    {
        if (esquinaSupIzqPrefab != null)
            foreach (var p in esquSupIzq)
                InstanciarConOrdenSiNoExiste(esquinaSupIzqPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (esquinaSupDerPrefab != null)
            foreach (var p in esquSupDer)
                InstanciarConOrdenSiNoExiste(esquinaSupDerPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (paredArribaPrefab != null)
            foreach (var p in bordesArriba)
                InstanciarConOrdenSiNoExiste(paredArribaPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (paredAbajoPrefab != null)
            foreach (var p in bordesAbajo)
                InstanciarConOrdenSiNoExiste(paredAbajoPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (paredIzquierdaPrefab != null)
            foreach (var p in bordesIzq)
                InstanciarConOrdenSiNoExiste(paredIzquierdaPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (paredDerechaPrefab != null)
            foreach (var p in bordesDer)
                InstanciarConOrdenSiNoExiste(paredDerechaPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (esquinaInfIzqPrefab != null)
            foreach (var p in esquInfIzq)
                InstanciarConOrdenSiNoExiste(esquinaInfIzqPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (esquinaInfDerPrefab != null)
            foreach (var p in esquInfDer)
                InstanciarConOrdenSiNoExiste(esquinaInfDerPrefab, p, orderPrincipal, CeldaEstado.Principal);
    }

    void InstanciarEsquinasInternas()
    {
        if (esquinaIntArribaIzqPrefab != null)
            foreach (var p in esquIntArribaIzq)
                InstanciarConOrdenSiNoExiste(esquinaIntArribaIzqPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (esquinaIntArribaDerPrefab != null)
            foreach (var p in esquIntArribaDer)
                InstanciarConOrdenSiNoExiste(esquinaIntArribaDerPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (esquinaIntAbajoIzqPrefab != null)
            foreach (var p in esquIntAbajoIzq)
                InstanciarConOrdenSiNoExiste(esquinaIntAbajoIzqPrefab, p, orderPrincipal, CeldaEstado.Principal);

        if (esquinaIntAbajoDerPrefab != null)
            foreach (var p in esquIntAbajoDer)
                InstanciarConOrdenSiNoExiste(esquinaIntAbajoDerPrefab, p, orderPrincipal, CeldaEstado.Principal);
    }

    //creacion de coberturas
    void GenerarCoberturasEnSalas()
    {
        if (coberturaPrefab == null || coberturasMaxPorSala <= 0) return;

        foreach (var sala in salas)
        {
            int faltan = Random.Range(0, coberturasMaxPorSala + 1);
            int intentos = 0;
            while (faltan > 0 && intentos < 60)
            {
                intentos++;
                int cx = Random.Range(sala.xMin + 2, sala.xMax - 2);
                int cy = Random.Range(sala.yMin + 2, sala.yMax - 2);
                var basePos = new Vector2Int(cx, cy);

                if (!celdasSuelo.Contains(basePos)) continue;

                int patron = Random.Range(0, 4);
                int rot = Random.Range(0, 4);
                var celdas = ObtenerCeldasCobertura(basePos, patron, rot);

                bool valido = true;
                foreach (var c in celdas)
                {
                    if (!Dentro(c.x, c.y) || !celdasSuelo.Contains(c) || celdasCobertura.Contains(c)) { valido = false; break; }
                    if (EsCeldaBordePrincipal(c)) { valido = false; break; }
                }
                if (!valido) continue;

                foreach (var c in celdas)
                {
                    InstanciarConOrdenSiNoExiste(coberturaPrefab, c, orderCobertura, CeldaEstado.Cobertura);
                    celdasCobertura.Add(c);
                }
                faltan--;
            }
        }
    }

    List<Vector2Int> ObtenerCeldasCobertura(Vector2Int basePos, int patron, int rot)
    {
        var lista = new List<Vector2Int> { basePos };
        switch (patron)
        {
            case 1:
                lista.Add(basePos + Vector2Int.left);
                lista.Add(basePos + Vector2Int.right);
                break;
            case 2:
                lista.Add(basePos + Vector2Int.down);
                lista.Add(basePos + Vector2Int.up);
                break;
            case 3:
                lista.Add(basePos + Vector2Int.right);
                lista.Add(basePos + Vector2Int.up);
                break;
        }

        if (rot % 4 != 0)
        {
            var rotadas = new List<Vector2Int>();
            foreach (var p in lista)
            {
                Vector2Int d = p - basePos;
                Vector2Int r = d;
                for (int i = 0; i < (rot % 4); i++) r = new Vector2Int(-r.y, r.x);
                rotadas.Add(basePos + r);
            }
            lista = rotadas;
        }

        return lista;
    }

    bool EsCeldaBordePrincipal(Vector2Int c)
    {
        return bordesArriba.Contains(c) || bordesAbajo.Contains(c) || bordesIzq.Contains(c) || bordesDer.Contains(c)
            || esquSupIzq.Contains(c) || esquSupDer.Contains(c) || esquInfIzq.Contains(c) || esquInfDer.Contains(c);
    }

    //instanciar un prefab en una celda concreta, con orden y marca, si no existe ya uno con esa marca
    void InstanciarConOrdenSiNoExiste(GameObject prefab, Vector2Int celda, int sortingOrder, CeldaEstado marca, float yOffset = 0f)
    {
        if (prefab == null) return;

        if (!estados.ContainsKey(celda)) estados[celda] = CeldaEstado.Ninguno;
        var actual = estados[celda];

        if ((actual & marca) != 0) return;

        estados[celda] = actual | marca;

        var obj = Instantiate(prefab, new Vector3(celda.x, celda.y + yOffset, 0f), Quaternion.identity, transform);
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder = sortingOrder;
    }

    bool Dentro(int x, int y) => x >= 0 && y >= 0 && x < ancho && y < alto;
    bool EsVacio(int x, int y) => Dentro(x, y) && mapa[x, y] == 0;

    void LimpiarHijos()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var go = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(go); else DestroyImmediate(go);
        }
    }
}
