using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class MoveRemote : MonoBehaviour
    {
        private Rigidbody rb;
        private SpriteRenderer sr;

        private Vector3 _pos = Vector3.zero;
        private bool _isFaceRight = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        private void HandlePlayerMoveUpdated()
        {
            try
            {
                var otherP = SignalRConnectionManager.MyRoom.Players.FirstOrDefault(p => p.Id != SignalRConnectionManager.MyPlayer.Id);
                if (otherP == null || otherP.Position == null)
                    return;
                Debug.Log($"HandlePlayerMoveUpdated: {otherP}");
                _pos.x = otherP.Position.X;
                _pos.y = otherP.Position.Y;
                _pos.z = otherP.Position.Z;
                _isFaceRight = otherP.IsFaceRight;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {ex.Message}");
            }
        }

        private void Update()
        {
            rb.MovePosition(_pos);
            sr.flipX = _isFaceRight;
        }


        private void OnEnable()
        {
            if (SignalRConnectionManager.Instance != null)
            {
                SignalRConnectionManager.Instance.OnPlayerMoveUpdated += HandlePlayerMoveUpdated;
            }
        }

        private void OnDisable()
        {
            if (SignalRConnectionManager.Instance != null)
            {
                SignalRConnectionManager.Instance.OnPlayerMoveUpdated += HandlePlayerMoveUpdated;
            }
        }

    }
}
