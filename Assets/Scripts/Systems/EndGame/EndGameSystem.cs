﻿using Actor;
using HelicopterAction;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Systems.EndGame
{
  public class EndGameSystem : MonoBehaviour
  {
    public static EndGameSystem Instance { get; private set; }
    // Helikopterin inmesi için bütün zombileri öldür yazısı çıkar.
    
    private const string _helicopterAddressable = "Helicopter";
    
    private int _zombieCount;
    
    private Player _player;

    private void Awake()
    {
      if (Instance == null) Instance = this;
      else Destroy(gameObject);
      
      _player = FindObjectOfType<Player>();
      
      _zombieCount = 0;
    }
    
    public void AddZombieCount()
    {
      _zombieCount++;
    }

    public void DeathZombie()
    {
      _zombieCount--;

      if (_zombieCount == 5)
      {
        CallHelicopter();
      }
      else if (_zombieCount == 0)
      {
        _helicopter.Land();
        HelicopterReached();
      }
      else if (_zombieCount < 5)
      {
        HelicopterReached();
      }
    }
    
    private Helicopter _helicopter;
    
    private async void CallHelicopter()
    {
      AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(_helicopterAddressable);

      await handle.Task;

      if (handle.Status != AsyncOperationStatus.Succeeded) return;
      GameObject helicopterInstance = handle.Result;

      _helicopter = helicopterInstance.GetComponent<Helicopter>();
    }

    public void HelicopterReached()
    {
      if (_zombieCount == 0)
      {
        _player.GetPlayerScreenPanel().DisableInfo();
        return;
      }
      
      string helicopterReachedText = $"Secure the whole area for the helicopter to land safely! {_zombieCount} zombies left.";
      
      _player.GetPlayerScreenPanel().OnInfo(helicopterReachedText);
    }

    private const string _helicopterLandedText = "The helicopter has landed! Proceed to the extraction point now!";
    
    public void HelicopterLanded()
    {
      _player.GetPlayerScreenPanel().OnInfo(_helicopterLandedText);
    }

    public void EndGame()
    {
      print("end game");
    }
  }
}