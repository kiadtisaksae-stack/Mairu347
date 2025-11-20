// using TMPro;
// using Unity.Netcode;
// using UnityEngine;

// public class EnemyTalktoFight : Enemy, IInteractable
// {
    
//     private readonly NetworkVariable<bool> _networkIsOn = new(
//         false, 
//         NetworkVariableReadPermission.Everyone, 
//         NetworkVariableWritePermission.Server);

//     public bool canTalk = true;
    
//     public bool isInteractable { get => canTalk; set => canTalk = value; } 
//     public TMP_Text interactionTextUI;
//     public TMP_Text WordTextUI;

//     public void Update()
//     {
//         if (LocalPlayer == null)
//         {
//             animator.SetBool("Attack", false);
//             return;
//         }
//         Turn(LocalPlayer.transform.position - transform.position);

//         if (currentState == State.Idel)
//         {
//             IdelState();
//         }
//         else if (currentState == State.Attack) {
//             attakeState();
//         }
        
//     }

//     private void IdelState()
//     {
//         if (GetDistanClosestPlayer() >= 2f || !canTalk)
//         {
//             interactionTextUI.gameObject.SetActive(false);
//         }
//         else
//         {
//             interactionTextUI.gameObject.SetActive(true);
//         }
//     }
//     private void attakeState()
//     {
//         if (LocalPlayer == null)
//         {
//             animator.SetBool("Attack", false);
//             return;
//         }
//         timer -= Time.deltaTime;

//         if (GetDistanClosestPlayer() < 1.5)
//         {
//             Attack(LocalPlayer);
//         }
//         else
//         {
//             animator.SetBool("Attack", false);
//             Vector3 direction = (LocalPlayer.transform.position - transform.position).normalized;
//             Move(direction);
//         }

//     }

//     public void Interact(Player player)
//     {
//         Debug.Log("Interact");
//         if (currentState == State.Idel) {
//             interactionTextUI.gameObject.SetActive(false);
//             currentState = State.Attack;
//         }
//         WordTextUI.gameObject.SetActive(true);

//         Invoke("CloseWord",3);
//     }
//     void CloseWord() {
//         WordTextUI.gameObject.SetActive(false);
//     }
// }
