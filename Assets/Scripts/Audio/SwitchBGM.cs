using UnityEngine;

public class SwitchBGM : MonoBehaviour
{
    [SerializeField] private bool changeBGM = true;
    [SerializeField] private bool changeAmbience = true;
    [SerializeField] private AudioController.BGM bgmTo;
    [SerializeField] private AudioController.BGA ambienceTo;

    //When trigger enter, if it is the player then switch whatever is needed
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (changeBGM) AudioController.Instance.PlayMusic(bgmTo);
            if (changeAmbience) AudioController.Instance.PlayAmbience(ambienceTo);
        }
    }

}
