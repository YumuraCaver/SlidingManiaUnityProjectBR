using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public string SFXname;

    public void PlaySfx()
    { 
        AudioManager.Instance.Play(SFXname);
    }
    public void PlaySfxByName(string s)
    {
        AudioManager.Instance.Play(s);
    }

}
