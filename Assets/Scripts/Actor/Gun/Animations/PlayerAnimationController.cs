﻿using System.Threading.Tasks;
using Guns.GunParts;
using UnityEngine;
using Utilities;

namespace Actor.Gun.Animations
{
  public class PlayerAnimationController : MonoBehaviour
  {
    private Animator _animator;

    private ArmAnimator _armAnimator;
    
    private PlayerAction _playerAction;
    
    private GunPart _gunPart;

    private Player _player;

    private void Awake()
    {
      GetComponent<PlayerGunSelector>().AddEventListenerOnGunChanged(OnGunChanged);
        
      _playerAction = GetComponent<PlayerAction>();
      _player = GetComponent<Player>();
      _firstPersonController = GetComponent<FirstPersonController>();
    }

    private void OnGunChanged(Transform gunTransform)
    {
      if (_armAnimator != null)
      {
        _armAnimator.RemoveEventListenerOnAnimationEvent(OnAnimationEventDispatch);
      }
      
      _armAnimator = gunTransform.GetComponentInChildren(typeof(ArmAnimator)) as ArmAnimator;

      if (_armAnimator == null) return;
      
      _animator = _armAnimator.GetAnimator();
      _armAnimator.AddEventListenerOnAnimationEvent(OnAnimationEventDispatch);
      
      if (_armAnimator.gameObject.activeInHierarchy)
        _armAnimator.GetAnimator().Update(0);
      _armAnimator.GetAnimator().Rebind();
      _animator.Rebind();

      _gunPart = gunTransform.GetComponentInChildren<GunPart>();
      _gunPart.SetInitialPosition(gunTransform.localPosition);
    }
    
    public void ChangeArmAnimator(Transform gunTransform)
    {
      if (_armAnimator != null)
      {
        _armAnimator.GetAnimator().Rebind();
        _animator.Rebind();
      }
      
      _armAnimator = gunTransform.GetComponentInChildren(typeof(ArmAnimator)) as ArmAnimator;
      
      if (_armAnimator == null) return;
      _animator = _armAnimator.GetAnimator();
      _armAnimator.AddEventListenerOnAnimationEvent(OnAnimationEventDispatch);
    }
    
    public async Task GunChangingAnimation()
    {
      _animator.SetTrigger("ChangeGun");
      float animTime = _animator.GetCurrentAnimatorStateInfo(0).length;
      await Utility.Delay(animTime);
      _armAnimator.RemoveEventListenerOnAnimationEvent(OnAnimationEventDispatch);
      _animator.Rebind();
      _armAnimator.GetAnimator().Rebind();
    }

    public void ResetAnimator()
    {
      _animator.Rebind();
      _armAnimator.GetAnimator().Update(0);
    }
    
    private void OnAnimationEventDispatch(AnimationEventKey eventKey)
    {
      switch (eventKey)
      {
        case AnimationEventKey.Detach_Magazine:
          DetachMagazine();
          break;
        case AnimationEventKey.Drop_Magazine:
          DropMagazine();
          break;
        case AnimationEventKey.Attach_Magazine:
          AttachMagazine();
          break;
        case AnimationEventKey.END_RELOAD:
          EndReload();
          break;
        case AnimationEventKey.End_Pill:
          PillEnd();
          break;
        case AnimationEventKey.End_Radar:
          RadarEnd();
          break;
      }
    }

    #region Pill

    private const string PILL_TRIGGER = "UsePill";
    public void Pill()
    {
      _animator.SetTrigger(PILL_TRIGGER);
    }
    
    private void PillEnd()
    {
      _player.GetPlayerHealth().HealingOverTime();
      
      _player.GetPlayerGunSelector().GetFirstGun();
    }

    #endregion

    #region Radar

    private const string RADAR_TRIGGER = "UseRadar";
    
    public void Radar()
    {
      _animator.SetTrigger(RADAR_TRIGGER);
      
      _player.GetPlayerRadar().StartRadar();
    }
    
    private const string PUT_RADAR_TO_BACKPACK_TRIGGER = "EndUseRadar";
    public void PutRadarToBackpack()
    {
      _animator.SetTrigger(PUT_RADAR_TO_BACKPACK_TRIGGER);
    }
    
    public void RadarEnd()
    {
      _player.GetPlayerRadar().StopRadar();
      
      _player.GetPlayerGunSelector().GetFirstGun();
    }

    #endregion

    #region Reload

    private GameObject _magazine;
      
    private const string RELOAD_TRIGGER = "Reload";
    public void Reload()
    {
      _animator.SetTrigger(RELOAD_TRIGGER);
      _animator.SetBool(SCOPE_TRIGGER, false);

      if (_player.GetIsScoped())
      {
        _player.GetPlayerGunSelector().ActiveGun.Scope();
      }
    }

    private void DetachMagazine()
    {
      _magazine = Instantiate(_gunPart.Magazine, _gunPart.LeftHand, true);
      _gunPart.Magazine.SetActive(false);
      _playerAction.Reloading(0);
    }
    
    private void DropMagazine()
    {
      // GameObject droppedMagazine = Instantiate(_magazine, _magazine.transform.position, new Quaternion(0,0,90,1));
      // droppedMagazine.AddComponent<Rigidbody>();
      // droppedMagazine.AddComponent<BoxCollider>().size = _gunPart.MagazineColliderSize;
      // droppedMagazine.transform.gameObject.layer = (int)Layer.OnlyVisual;
    }

    private void AttachMagazine()
    {
      _gunPart.Magazine.SetActive(true);
      _playerAction.Reloading(1);
      Destroy(_magazine);
    }
    
    private void EndReload()
    {
      _playerAction.EndReload();
    }
    
    #endregion

    #region Run & Walk
    
    private bool _isRunning => _firstPersonController.GetIsRunning();

    private bool _movingAnimationValue;
    
    private Vector3 _initialGunPosition;

    private Vector3 _defaultGunPosition;
    
    private FirstPersonController _firstPersonController;

    private const string _move = "Move";
    private void FixedUpdate()
    {
      if(!_player.GetPlayerGunSelector().HasGun)
        return;
      
      if (_isRunning && !_movingAnimationValue)
      {
        _animator.SetBool(_move, true);
        _movingAnimationValue = true;
      }
      else if (!_isRunning && _movingAnimationValue)
      {
        _animator.SetBool(_move, false);
        _movingAnimationValue = false;
        _defaultGunPosition = _gunPart.GetInitialPosition();
      }
    }
    
    public void EndGame()
    {
      _animator.SetBool(_move, false);
      _movingAnimationValue = false;
    }

    #endregion

    #region Bobbing

    private float timer;
    
    [SerializeField] private float _scopeBobAmount = 0.01f;
    [SerializeField] private float _scopeBobSpeed = 2f;
    
    [SerializeField] private float _breathBobAmount = 0.1f;
    [SerializeField] private float _breathBobSpeed = 1f;
    
    [SerializeField] private float _crouchBobAmount = 0.5f;
    [SerializeField] private float _crouchBobSpeed = 2f;

    [SerializeField] private float _walkBobAmount = 1f;
    [SerializeField] private float _walkBobSpeed = 3f;
    
    [SerializeField] private float _runBobAmount = 5f;
    [SerializeField] private float _runBobSpeed = 5f;
    
    private float _bobAmount  => 
      _player.GetIsScoped() ? _scopeBobAmount / 1000 :
      !_firstPersonController.GetIsGrounded() ? _walkBobAmount / 1000 :
      !_firstPersonController.GetIsMoving() ? _breathBobAmount / 1000:
      _firstPersonController.GetIsCrouching() ? _crouchBobAmount / 1000: 
      _firstPersonController.GetIsRunning() ? _runBobAmount / 1000 : _walkBobAmount / 1000;
    
    private float _bobSpeed => 
      _player.GetIsScoped() ? _scopeBobSpeed :
      !_firstPersonController.GetIsGrounded() ? _walkBobSpeed :
      !_firstPersonController.GetIsMoving() ? _breathBobSpeed :
      _firstPersonController.GetIsCrouching() ? _crouchBobSpeed :
      _firstPersonController.GetIsRunning() ? _runBobSpeed : _walkBobSpeed;
    
    public void ApplyBob()
    {
      if (!_player.GetPlayerGunSelector().HasGun) return;
      
      if (timer >= 360) timer = 0;
      
      _defaultGunPosition = _gunPart.GetInitialPosition();
      
      timer += Time.deltaTime * _bobSpeed;
      
      _gunPart.transform.localPosition = new Vector3(
        _defaultGunPosition.x + Mathf.Sin(timer) * _bobAmount,
        _defaultGunPosition.y + Mathf.Sin(timer) * _bobAmount,
        _defaultGunPosition.z + Mathf.Sin(timer) * _bobAmount);
    }

    #endregion

    #region Scope
    
    private const string SCOPE_TRIGGER = "Scope";
    
    public void OnScopeOpen(bool isScoped)
    {
      _animator.SetBool(SCOPE_TRIGGER, isScoped);
    }

    #endregion

    #region Getters & Setters

    public GunPart GetGunPart()
    {
      return _gunPart;
    }

    #endregion
  }
}