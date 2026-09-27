using UnityEngine;
using UnityEngine.VFX;

public class VFXEventReceiver : MonoBehaviour
{
    [SerializeField] private VisualEffect[] _vfx;

    public void PlayVFX()
    {
        foreach(VisualEffect vfx in _vfx)
        {
            vfx.Play();
        }
    }
}