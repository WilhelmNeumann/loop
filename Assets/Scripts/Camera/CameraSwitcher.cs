using Unity.Cinemachine;
using UnityEngine;

namespace Camera
{
    public class CameraSwitcher : Singleton<CameraSwitcher>
    {
        public CinemachineCamera freeCam;
        public CinemachineCamera raceCam;

        public void ActivateFreeCam(Transform target = null)
        {
            raceCam.gameObject.SetActive(false);
            freeCam.gameObject.SetActive(true);
            freeCam.GetComponent<FreeCamController>().ShowTarget(target);
        }

        public void ActivateRaceCam()
        {
            freeCam.gameObject.SetActive(false);
            raceCam.gameObject.SetActive(true);
        }

        public void DeactivateAll()
        {
            freeCam.gameObject.SetActive(false);
            raceCam.gameObject.SetActive(false);
        }
    }
}