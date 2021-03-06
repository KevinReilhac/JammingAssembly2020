﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] Animator animator = null;
    [SerializeField] Light2D light2d = null;
    [SerializeField] Color chargedLightColor = Color.red;
    [Header("Gameplay")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float interactionRange = 1f;
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab = null;
    [SerializeField] private float chargeTime = 3f;
    [SerializeField] private float minSpeed = 0.3f;
    [SerializeField] private float maxSpeed = 0.3f;
    [SerializeField] private float minDist = 1f;
    [SerializeField] private float maxDist = 3f;


    private Rigidbody2D _rigidbody = null;
    private Vector2 _currentdir = Vector2.zero;
    private IInteractable _targetInteractable = null;
    private Vector2 _velocity = Vector2.zero;
    private bool _canMove = true;
    private float? timeStartCharge = null;
    private Color _defaultLightColor = Color.white;
    private bool _isDead = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _defaultLightColor = light2d.color;
        SetupControls();
    }

    public void Kill()
    {
        _isDead = true;
        _canMove = false;
        GameManager.instance.AddDeath();
        GameManager.instance.StarCount = 0;
        GameManager.instance.GetSceneManager().ReloadScene();
        _rigidbody.velocity = Vector2.zero;
    }


    private float GetChargeState()
    {
        if (timeStartCharge == null)
            return 0;
        return Mathf.Clamp((Time.time - timeStartCharge.Value), 0, chargeTime) / chargeTime;
    }

    private void Update()
    {
        if (GameManager.instance.isPhonePlay)
            UpdateMove(GameManager.instance.GetHUD().MobileInputs.GetStickValue());
        SearchInteractable();
        _rigidbody.velocity = _velocity;
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        light2d.color = Color.Lerp(_defaultLightColor, chargedLightColor, GetChargeState());
        animator.SetFloat("SpeedX", _currentdir.x);
        animator.SetFloat("SpeedY", _currentdir.y);
        animator.SetBool("Idle", _rigidbody.velocity.magnitude < 0.2f || !_canMove);
        animator.SetFloat("chargeVal", GetChargeState());;
        animator.SetBool("isCharge", timeStartCharge.HasValue);;
        animator.SetBool("isDead", _isDead);
    }
    
    private void SearchInteractable()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _currentdir, interactionRange);
        IInteractable newInteractable = null;

        if (hit.collider != null)
            newInteractable = hit.collider.GetComponent<IInteractable>();
        if (newInteractable == null)
        {
            _targetInteractable?.StopHighlight();
        }
        else if (newInteractable != _targetInteractable)
        {
            newInteractable.StartHighlight();
            _targetInteractable?.StopHighlight();
        }

        _targetInteractable = newInteractable;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_currentdir * interactionRange);
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    #region INPUTS_HANDLING
    private void UpdateMove(InputAction.CallbackContext context)
    {
        UpdateMove(context.ReadValue<Vector2>().Get4Direction());
    }

    private void UpdateMove(Vector2 value)
    {
        if (value != Vector2.zero)
            _currentdir = value;
        _velocity = value * speed * (_canMove ? 1 : 0);
    }

    private void Interact(InputAction.CallbackContext context = default(InputAction.CallbackContext))
    {
        if (_targetInteractable == null)
            return;
        _targetInteractable.Interact();
    }

    private void StartCharge(InputAction.CallbackContext context = default(InputAction.CallbackContext))
    {
        if (!GameManager.instance.HaveStar())
            return;
        timeStartCharge = Time.time;
        _canMove = false;
    }

    private void ThrowStar(InputAction.CallbackContext context = default(InputAction.CallbackContext))
    {
        if (!GameManager.instance.HaveStar())
            return;
        GameManager.instance.RemoveStar();
        timeStartCharge = null;
        _canMove = true;
        animator.SetTrigger("throw");
        StartCoroutine(ThrowStarCoroutine());
    }

    private IEnumerator ThrowStarCoroutine()
    {
        float state = GetChargeState();
        yield return new WaitForSeconds(0.3f);
        _canMove = true;
        Projectile instance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        instance.Throw(Mathf.Lerp(minDist, maxDist, 1 - state), _currentdir, Mathf.Lerp(minSpeed, maxSpeed, 1 - state));
        
    }

    private void InteractMobile(bool value)
    {
        if (value == true)
            StartCharge();
        else
            ThrowStar();
    }

    private void SetupControls()
    {
        PlayerControls controls = GameManager.instance.Controls;

        controls.Enable();
        controls.MainGameplay.Movements.performed += UpdateMove;
        controls.MainGameplay.Movements.canceled += UpdateMove;

        controls.MainGameplay.Interact.started += Interact;
        controls.MainGameplay.Attack.performed += StartCharge;
        controls.MainGameplay.Attack.canceled += ThrowStar;

        MobileInputs mobileInputs = GameManager.instance.GetHUD().MobileInputs;

        mobileInputs.onInteraction.AddListener((_) => Interact());
        mobileInputs.onAttack.AddListener(InteractMobile);
    }

    private void OnDestroy()
    {
        PlayerControls controls = GameManager.instance.Controls;


        controls.MainGameplay.Movements.performed -= UpdateMove;
        controls.MainGameplay.Movements.canceled -= UpdateMove;

        controls.MainGameplay.Interact.started -= Interact;
        controls.MainGameplay.Attack.performed -= StartCharge;
        controls.MainGameplay.Attack.canceled -= ThrowStar;
    }

    #endregion
}
