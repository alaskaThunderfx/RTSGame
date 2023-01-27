using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Units
{
    public class Unit : NetworkBehaviour
    {
        [SerializeField] private UnityEvent onSelected = null;
        [SerializeField] private UnityEvent onDeselected = null;

        #region Client

        [Client]
        public void Select()
        {
            if (!isOwned) return;
            
            onSelected?.Invoke();
        }

        [Client]
        public void Deselect()
        {
            if (!isOwned) return;
            
            onDeselected?.Invoke();
        }

        #endregion
    }
}