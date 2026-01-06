using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleRandomWalkParameters_", menuName = "PCG/SimpleRandomWalkData")]
public class SimpleRandomWalkSO : ScriptableObject
{
    [Header("迭代次数")]
    public int iterations = 10;
    [Header("每次行走的步数")]
    public int walkLength = 10;
    [Header("每次迭代是否随机起点")]
    public bool startRandomlyEachIteration = true;
}
