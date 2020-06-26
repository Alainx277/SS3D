using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace SS3D.Content.Systems.Combat
{
    [RequireComponent(typeof(Gun))]
    public class GunFeedback : MonoBehaviour
    {
        public AudioSource AudioSource;
        public AudioClip[] FireSounds;
        public AudioClip EmptySound;
        public float MaxPitch = 1;
        public float MinPitch = 1;

        void Start()
        {
            Gun gun = GetComponent<Gun>();
            Assert.IsNotNull(gun);
            gun.FireCallback += GunOnFireCallback;
            gun.EmptyFire += GunOnEmptyFire;
        }

        private void GunOnEmptyFire(object sender, EventArgs e)
        {
            AudioSource.PlayOneShot(EmptySound);
        }

        private void GunOnFireCallback(object sender, EventArgs e)
        {
            if (AudioSource != null && FireSounds.Length != 0)
            {
                AudioSource.pitch = Random.Range(MinPitch, MaxPitch);
                AudioSource.PlayOneShot(FireSounds[Random.Range(0, FireSounds.Length)]);
            }
        }
    }
}