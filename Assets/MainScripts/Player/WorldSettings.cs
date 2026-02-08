using UnityEngine;

public class PlacePlayerCharacter : MonoBehaviour
{
    private GameObject onBoardSpawnPoint;
    private GameObject realSpawnPoint;
    
    private void Awake()
    {
        onBoardSpawnPoint = GameObject.Find("OnBoardSpawnPoint");
        realSpawnPoint = GameObject.Find("FirstSpawnPoint");
    }

    public Transform GetSpawnTransform()
    {
        //PlayerPrefs.SetInt();
        return realSpawnPoint.transform;
    }
}
