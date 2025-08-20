using System.Collections.Generic;
using UnityEngine;

public class GeneradorMazmorra : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject sueloPrefab;
    public GameObject paredArribaPrefab;
    public GameObject paredArriba2Prefab; // Nuevo prefab para variar la pared de arriba
    public GameObject paredAbajoPrefab;
    public GameObject paredIzquierdaPrefab;
    public GameObject paredDerechaPrefab;

    public GameObject esquinaSupIzqPrefab;
    public GameObject esquinaSupDerPrefab;
    public GameObject esquinaInfIzqPrefab;
    public GameObject esquinaInfDerPrefab;

    [Header("Tamaño Mazmorra")]
    public int ancho = 20;
    public int alto = 20;

    [Header("Control de Generación")]
    public int cantidadSalas = 5;
    public int tamSalaMin = 3;
    public int tamSalaMax = 6;

    private int[,] mapa; // 0 vacío, 1 suelo
    private List<RectInt> salas = new List<RectInt>();

    void Start()
    {
        GenerarMazmorra();
    }

    void GenerarMazmorra()
    {
        mapa = new int[ancho, alto];
        salas.Clear();

        // Generar salas
        for (int i = 0; i < cantidadSalas; i++)
        {
            int w = Random.Range(tamSalaMin, tamSalaMax + 1);
            int h = Random.Range(tamSalaMin, tamSalaMax + 1);
            int x = Random.Range(1, ancho - w - 1);
            int y = Random.Range(1, alto - h - 1);

            RectInt sala = new RectInt(x, y, w, h);

            // Evitar solapamiento
            bool solapa = false;
            foreach (RectInt s in salas)
            {
                if (s.Overlaps(sala))
                {
                    solapa = true;
                    break;
                }
            }
            if (solapa) continue;

            salas.Add(sala);

            for (int ix = sala.xMin; ix < sala.xMax; ix++)
            {
                for (int iy = sala.yMin; iy < sala.yMax; iy++)
                {
                    mapa[ix, iy] = 1; // suelo
                }
            }
        }

        // Conectar salas con pasillos más cortos
        for (int i = 0; i < salas.Count - 1; i++)
        {
            Vector2Int centroA = new Vector2Int(
                salas[i].xMin + salas[i].width / 2,
                salas[i].yMin + salas[i].height / 2
            );
            Vector2Int centroB = new Vector2Int(
                salas[i + 1].xMin + salas[i + 1].width / 2,
                salas[i + 1].yMin + salas[i + 1].height / 2
            );

            CrearPasillo(centroA, centroB);
        }

        DibujarMapa();
    }

    void CrearPasillo(Vector2Int a, Vector2Int b)
    {
        // Pasillos en L
        if (Random.value < 0.5f)
        {
            CrearLineaHorizontal(a.x, b.x, a.y);
            CrearLineaVertical(a.y, b.y, b.x);
        }
        else
        {
            CrearLineaVertical(a.y, b.y, a.x);
            CrearLineaHorizontal(a.x, b.x, b.y);
        }
    }

    void CrearLineaHorizontal(int x1, int x2, int y)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
        {
            if (x > 0 && x < ancho && y > 0 && y < alto)
                mapa[x, y] = 1;
        }
    }

    void CrearLineaVertical(int y1, int y2, int x)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
        {
            if (x > 0 && x < ancho && y > 0 && y < alto)
                mapa[x, y] = 1;
        }
    }

    void DibujarMapa()
    {
        foreach (Transform hijo in transform)
        {
            Destroy(hijo.gameObject);
        }

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (mapa[x, y] == 1)
                {
                    Instantiate(sueloPrefab, new Vector3(x, y, 0), Quaternion.identity, transform);

                    // Arriba
                    if (y + 1 < alto && mapa[x, y + 1] == 0)
                    {
                        GameObject paredPrefab = (Random.value < 0.5f) ? paredArribaPrefab : paredArriba2Prefab;
                        Instantiate(paredPrefab, new Vector3(x, y + 1, 0), Quaternion.identity, transform);
                    }
                    // Abajo
                    if (y - 1 >= 0 && mapa[x, y - 1] == 0)
                    {
                        Instantiate(paredAbajoPrefab, new Vector3(x, y - 1, 0), Quaternion.identity, transform);
                    }
                    // Izquierda
                    if (x - 1 >= 0 && mapa[x - 1, y] == 0)
                    {
                        Instantiate(paredIzquierdaPrefab, new Vector3(x - 1, y, 0), Quaternion.identity, transform);
                    }
                    // Derecha
                    if (x + 1 < ancho && mapa[x + 1, y] == 0)
                    {
                        Instantiate(paredDerechaPrefab, new Vector3(x + 1, y, 0), Quaternion.identity, transform);
                    }

                    // Esquinas
                    if (mapa[x, y + 1] == 0 && mapa[x - 1, y] == 0) // sup izq
                    {
                        Instantiate(esquinaSupIzqPrefab, new Vector3(x - 1, y + 1, 0), Quaternion.identity, transform);
                    }
                    if (mapa[x, y + 1] == 0 && mapa[x + 1, y] == 0) // sup der
                    {
                        Instantiate(esquinaSupDerPrefab, new Vector3(x + 1, y + 1, 0), Quaternion.identity, transform);
                    }
                    if (mapa[x, y - 1] == 0 && mapa[x - 1, y] == 0) // inf izq
                    {
                        Instantiate(esquinaInfIzqPrefab, new Vector3(x - 1, y - 1, 0), Quaternion.identity, transform);
                    }
                    if (mapa[x, y - 1] == 0 && mapa[x + 1, y] == 0) // inf der
                    {
                        Instantiate(esquinaInfDerPrefab, new Vector3(x + 1, y - 1, 0), Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}  