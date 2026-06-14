using UnityEngine;
using UnityEngine.AI;

public class Inimigo : MonoBehaviour
{
    [Header("IA")]
    public Transform player;
    [SerializeField] private float _distanciaDeteccao = 10f;
    [SerializeField] private float _wanderRadius      = 15f;
    [SerializeField] private float _wanderTimer       = 5f;

    [Header("Ataque")]
    [SerializeField] private float _cooldownAtaque = 1f;
    [SerializeField] private int   _danoAtaque     = 1;
    private float                  _timerAtaque    = 0f;
    public bool                   _playerNaArea   = false;

    [Header("Vida")]
    [SerializeField] private int _vida   = 3;
    private bool                 _morreu = false;

    private NavMeshAgent     _agent;
    private float            _timer;
    private PlayerController _playerController;
    private Animator         _animator;

    private void Start()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _timer    = _wanderTimer;

        if (_animator == null)
            Debug.LogError("Animator não encontrado no Inimigo!");

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
            {
                player            = p.transform;
                _playerController = p.GetComponent<PlayerController>();
            }
            else Debug.LogError("Player não encontrado!");
        }
        else
        {
            _playerController = player.GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (player == null || _morreu) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= _distanciaDeteccao)
        {
            if (_playerNaArea)
            {
                if (_agent != null) _agent.SetDestination(transform.position);
                SetAnim("IsRunning", false);

                _timerAtaque += Time.deltaTime;
                if (_timerAtaque >= _cooldownAtaque)
                {
                    _timerAtaque = 0f;
                   AtacarPlayer();
                }
            }
            else
            {
                if (_agent != null) _agent.SetDestination(player.position);
                SetAnim("IsRunning", true);
                _timerAtaque = 0f;
            }
        }
        else
        {
            SetAnim("IsRunning", false);
            _timerAtaque = 0f;

            _timer += Time.deltaTime;
            if (_timer >= _wanderTimer)
            {
                Vector3 newPos;
                if (TryGetRandomNavPoint(transform.position, _wanderRadius, out newPos))
                {
                    if (_agent != null) _agent.SetDestination(newPos);
                    SetAnim("IsRunning", true);
                }
                _timer = 0f;
            }
        }
    }

    // Helper para evitar erros de null
    private void SetAnim(string param, bool value)
    {
        if (_animator != null)
            _animator.SetBool(param, value);
    }

    private void TriggerAnim(string param)
    {
        if (_animator != null)
            _animator.SetTrigger(param);
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (outro.CompareTag("Player"))
            _playerNaArea = true;
    }

    private void OnTriggerExit(Collider outro)
    {
        if (outro.CompareTag("Player"))
        {
            _playerNaArea = false;
            _timerAtaque  = 0f;
        }
    }

    private void AtacarPlayer()
    {
        TriggerAnim("IsAttack");

        if (_playerController != null)
            _playerController.TomarDano(_danoAtaque);
    }

    private bool TryGetRandomNavPoint(Vector3 origin, float radius, out Vector3 result)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius;
        randomDir += origin;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDir, out hit, radius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = origin;
        return false;
    }

    public void TomarDano(int dano)
    {
        if (_morreu) return;

        _vida -= dano;
        Debug.Log($"Inimigo tomou dano! Vida: {_vida}");

        if (_vida <= 0)
            Morrer();
    }

    private void Morrer()
    {
        _morreu = true;

        if (_agent != null)
        {
            _agent.SetDestination(transform.position);
            _agent.isStopped = true;
        }

        TriggerAnim("IsDead");
        Debug.Log("Inimigo morreu!");
        Destroy(gameObject, 2f);
    }
}