using Unity.Cinemachine;

namespace Camera
{
    public class CameraSwitcher : Singleton<CameraSwitcher>
    {
        public CinemachineCamera freeCam;
        public CinemachineCamera raceCam;

        public void ActivateFreeCam(bool isActive)
        {
            raceCam.gameObject.SetActive(!isActive);
            freeCam.gameObject.SetActive(isActive);
        }

        public void ActivateRaceCam(bool isActive)
        {
            freeCam.gameObject.SetActive(!isActive);
            raceCam.gameObject.SetActive(isActive);
        }
    }
}