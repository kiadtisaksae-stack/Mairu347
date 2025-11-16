using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class NetworkPlayerManager : MonoBehaviour
{
    public Player player;

    private void Start()
    {
        player = GetComponent<Player>();
        
    }






}
