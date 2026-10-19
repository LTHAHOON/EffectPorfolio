using Unity.Cinemachine;
using UnityEngine;

public class CinemachineNoiseController : MonoBehaviour
{
    [SerializeField]
    private CinemachineBasicMultiChannelPerlin _basicPerlin;

    public void SetNoise(NoiseSettings noiseSetting)
    {
        _basicPerlin.NoiseProfile = noiseSetting;
    }
}
