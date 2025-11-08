// using Unity.Netcode;
// using UnityEngine;

// public class ClientAnimation : NetworkBehaviour
// {
//     [SerializeField] private Animator animator;

//     private void Awake()
//     {
//         if (animator == null)
//             animator = GetComponent<Animator>();
//     }

//     public override void OnNetworkSpawn()
//     {
//         base.OnNetworkSpawn();

//         // สำหรับ Client เท่านั้น
//         if (!IsOwner)
//         {
//             enabled = false;
//         }
//     }

//     [Rpc(SendTo.Everyone)]
//     public void PlayAnimationRpc(string animationName)
//     {
//         //if (IsOwner) return;
//         //animator.CrossFade(animationName, 0f);
//     }


// }
