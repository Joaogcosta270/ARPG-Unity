using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayControls _controls;

    [SerializeField] private Vector2 _moveInput;

    private Vector3             _movementDirection;
    private CharacterController _characterController;
    private Animator            _animator;

    [Header("Movement")]
    public float                   _walkSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private bool  _isWalk;

    [Header("Attack")]
    [SerializeField] private bool      _isAttack;
    [SerializeField] private float     _attackTime    = 1f;
    [SerializeField] public float     _alcanceAtaque = 1.5f;
    [SerializeField] private int       _danoAtaque    = 1;
    [SerializeField] private LayerMask _layerInimigo;

    [Header("Vida")]
    [SerializeField] private int _vidas = 3;
    private float                _cooldown = 0f;
    private bool                 _morreu   = false;

    [Header("UI Vidas")]
    [SerializeField] private GameObject _trofeu1;
    [SerializeField] private GameObject _trofeu2;
    [SerializeField] private GameObject _trofeu3;

    [Header("Painéis UI")]
    [SerializeField] private GameObject _painelDerrota;
    [SerializeField] private GameObject _painelVitoria;

    private InputAction _actionAttack;

    private void Awake()
    {
        _controls = new PlayControls();

        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled  += ctx => _moveInput = Vector2.zero;

        _actionAttack = _controls.FindAction("Attack");
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator            = GetComponent<Animator>();

        if (_painelDerrota != null) _painelDerrota.SetActive(false);
        if (_painelVitoria != null) _painelVitoria.SetActive(false);

        if (_trofeu1 != null) _trofeu1.SetActive(true);
        if (_trofeu2 != null) _trofeu2.SetActive(true);
        if (_trofeu3 != null) _trofeu3.SetActive(true);
    }

    private void Update()
    {
        if (_morreu) return;

        MovePlayer();
        UpdateAnimation();

        if (_actionAttack != null && _actionAttack.WasPressedThisFrame())
            AttackPlayer();

        UpdateAttackState();

        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;
    }

    private void OnEnable()
    {
        if (_controls != null)
            _controls.Enable();
    }

    private void OnDisable()
    {
        if (_controls != null)
            _controls.Disable();
    }

    private void MovePlayer()
    {
        _movementDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        if (_movementDirection != Vector3.zero && !_isAttack)
        {
            _characterController.Move(_movementDirection * _walkSpeed * Time.deltaTime);

            bool isRotate = new Vector2(_movementDirection.x, _movementDirection.z).magnitude > 0.1f;
            if (isRotate)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_movementDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }

            _isWalk = true;
        }
        else
        {
            _isWalk = false;
        }
    }

    private void UpdateAnimation()
    {
        _animator.SetBool("isWalk", _isWalk);
    }

    private void AttackPlayer()
    {
        if (!_isAttack)
        {
            _movementDirection = Vector3.zero;
            _animator.SetTrigger("isAttack1");
            _isAttack = true;

            Collider[] atingidos = Physics.OverlapSphere(
                transform.position + transform.forward * 1f,
                _alcanceAtaque,
                _layerInimigo
            );

            foreach (Collider alvo in atingidos)
            {
                Inimigo inimigo = alvo.GetComponent<Inimigo>();
                if (inimigo != null)
                    inimigo.TomarDano(_danoAtaque);
            }
        }
    }

    private void UpdateAttackState()
    {
        if (_isAttack)
        {
            _attackTime -= Time.deltaTime;
            if (_attackTime <= 0f)
            {
                _isAttack   = false;
                _attackTime = 1f;
            }
        }
    }

    // --- Vida ---

    public void TomarDano(int dano)
    {
        if (_cooldown > 0f || _morreu) return;

        _vidas--;
        _cooldown = 2f;
        Debug.Log($"Tomou dano! Vidas restantes: {_vidas}");

        if (_vidas == 2 && _trofeu3 != null) _trofeu3.SetActive(false);
        if (_vidas == 1 && _trofeu2 != null) _trofeu2.SetActive(false);
        if (_vidas <= 0)
        {
            if (_trofeu1 != null) _trofeu1.SetActive(false);
            Morrer();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("Inimigo") && _cooldown <= 0f)
            TomarDano(1);
    }

    private void Morrer()
    {
        _morreu = true;
        _moveInput = Vector2.zero;

        // Toca animação de morte
        if (_animator != null)
            _animator.SetTrigger("IsDead");

        Debug.Log("O jogador perdeu!");

        // Espera a animação terminar antes de mostrar o painel
        StartCoroutine(MostrarPainelDerrota());
    }

    private System.Collections.IEnumerator MostrarPainelDerrota()
    {
        // Espera 2 segundos para a animação de morte tocar
        yield return new WaitForSecondsRealtime(2f);

        if (_painelDerrota != null)
        {
            _painelDerrota.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // --- Vitória ---

    public void Ganhar()
    {
        if (_morreu) return;

        Debug.Log("Parabéns! Você venceu!");
        if (_painelVitoria != null)
        {
            _painelVitoria.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position + transform.forward * 1f,
            _alcanceAtaque
        );
    }
}