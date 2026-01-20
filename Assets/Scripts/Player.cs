using UnityEngine;

/// <summary>
/// 플레이어의 정적 참조를 제공하는 클래스
/// 다른 클래스에서 Player.Instance로 접근 가능
/// </summary>
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}