﻿using UnityEngine;
using UnityEngine.Rendering;

namespace UserInterface.Panel.Base
{
  [RequireComponent(typeof(SortingGroup))]
  public abstract class BasePanel : MonoBehaviour
  {
    protected SortingGroup SortingGroup { get; private set; }

    public virtual void Awake()
    {
      SortingGroup = gameObject.GetComponent<SortingGroup>();
      
      ChangePanelLayer();
    }

    protected abstract void ChangePanelLayer();
  }
}