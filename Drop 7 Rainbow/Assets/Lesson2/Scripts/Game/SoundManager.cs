using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lesson2
{
    public sealed class SoundNames
    {
        public static readonly string Music_GameBg = "GameBg";
        public static readonly string Sound_Drop = "Drop";
        public static readonly string Sound_Bomb = "Bomb";
    }

    public sealed class SoundManager
    {
        public static SoundManager Instance = new SoundManager();

        private SoundManager(){}
        
        private AudioSource MusicPlayer;

        private AudioSource SoundPlayer;

        public void Init(AudioSource musicPlayer, AudioSource soundPlayer)
        {
            MusicPlayer = musicPlayer;
            SoundPlayer = soundPlayer;
        }

        public void PlayMusic(string name = "")
        {
            MusicPlayer.loop = true;
            
            if (string.IsNullOrEmpty(name))
            {
                MusicPlayer.Play();
            }
            else
            {
                AudioClip clip = Resources.Load<AudioClip>($"Common/Audios/Musics/{name}");
                MusicPlayer.clip = clip;
                MusicPlayer.Play();
            }
        }

        public void PlaySound(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                MusicPlayer.Play();
            }
            else
            {
                AudioClip clip = Resources.Load<AudioClip>($"Common/Audios/Sounds/{name}");
                MusicPlayer.PlayOneShot(clip);
            }
        }
    }
}
