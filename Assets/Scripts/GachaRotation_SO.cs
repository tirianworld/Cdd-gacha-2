using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRotation", menuName = "Gacha/Rotation")]
public class GachaRotation_SO : ScriptableObject
{
    public List<GachaRotationStep> steps = new List<GachaRotationStep>();
}
