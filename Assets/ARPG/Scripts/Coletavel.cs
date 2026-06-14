using UnityEngine;

public class ObjetivoColetavel : MonoBehaviour
{
    [Header("Rotação")]
    [SerializeField] private float _velocidadeRotacao = 90f;

    private static int _coletados = 0;
    private const  int _total     = 3;

    private void Start()
    {
        _coletados = 0;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _velocidadeRotacao * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            _coletados++;
            Debug.Log($"Coletados: {_coletados}/{_total}");

            gameObject.SetActive(false);

            if (_coletados >= _total)
            {
                PlayerController player = outro.GetComponent<PlayerController>();
                if (player != null)
                    player.Ganhar();
            }
        }
    }
}