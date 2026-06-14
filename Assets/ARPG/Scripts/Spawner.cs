using UnityEngine;

public class ObjetivoSpawner : MonoBehaviour
{
    [Header("Prefab do Objeto")]
    [SerializeField] private GameObject _objetoPrefab;
    [SerializeField] private int        _quantidade = 3;

    [Header("Área de Spawn Aleatório")]
    [SerializeField] private float _areaX = 20f; // largura da área
    [SerializeField] private float _areaZ = 20f; // comprimento da área
    [SerializeField] private float _alturaNoChao = 1f;

    private void Start()
    {
        if (_objetoPrefab == null)
        {
            Debug.LogError("Arraste o Prefab no Inspector!");
            return;
        }

        for (int i = 0; i < _quantidade; i++)
        {
            // Posição aleatória dentro da área definida
            float x = transform.position.x + Random.Range(-_areaX, _areaX);
            float z = transform.position.z + Random.Range(-_areaZ, _areaZ);

            Vector3 posicao = new Vector3(x, _alturaNoChao, z);
            Instantiate(_objetoPrefab, posicao, Quaternion.identity);

            Debug.Log($"Objeto {i+1} spawnado em: {posicao}");
        }
    }

    // Desenha a área de spawn no Editor para visualizar
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(_areaX * 2, 1f, _areaZ * 2)
        );
    }
}