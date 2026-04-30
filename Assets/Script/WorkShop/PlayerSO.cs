using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Game/Player Config")]
public class PlayerSO : ScriptableObject
{
    [Header("Base Stats")]
    public int initialMaxHealth = 100;
    public int baseDamage = 10;
    public int baseDefence = 10;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float rotationSmoothing = 15f;

    [Header("Interact")]
    public float interactSphereRadius = 0.8f;
    public float interactMaxDistance = 2.0f;

    [Header("Combat")]
    public List<string> attackAnimations = new List<string>();

    [Header("Level Up Multipliers")]
    [Range(1f, 2f)] public float healthMultiplier = 1.2f;
    [Range(1f, 2f)] public float damageMultiplier = 1.1f;
    [Range(1f, 2f)] public float defenceMultiplier = 1.1f;
}
