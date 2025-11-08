//using Unity.Netcode;
//using UnityEngine;
//[RequireComponent(typeof(Player))]
//public class ClientPlayerMove : NetworkBehaviour
//{
//    private Player _playerController;

//    private void Awake()
//    {
//        _playerController = GetComponent<Player>();

//        if (_playerController != null)
//        {
//            _playerController.enabled = false;
//        }
//    }

//    public override void OnNetworkSpawn()
//    {
//        base.OnNetworkSpawn();

//        if (IsOwner)
//        {
//            if (_playerController != null)
//            {
//                _playerController.enabled = true;
//                Debug.Log($"เปิดการควบคุม Player: {gameObject.name} (Client ID: {OwnerClientId})");
//            }

//        }
//        else
//        {
//            Debug.Log($"ปิดการควบคุม Player อื่น: {gameObject.name} (Client ID: {OwnerClientId})");
//        }
//    }

//    public override void OnNetworkDespawn()
//    {
//        if (_playerController != null)
//        {
//            _playerController.enabled = false;
//        }
//        base.OnNetworkDespawn();
//    }
//}