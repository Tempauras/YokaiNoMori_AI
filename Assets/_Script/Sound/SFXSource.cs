using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSource : MonoBehaviour
{
	[SerializeField] private AudioClip m_Sound;

	private AudioSource m_SFXSpeaker;

	// Start is called before the first frame update
	void Start()
	{
		m_SFXSpeaker = GameObject.FindGameObjectWithTag("SFX")?.GetComponent<AudioSource>();
		Debug.Assert(m_SFXSpeaker != null, "No gameobject tagged SFX with an audio source");
	}

	public void Play()
	{
		m_SFXSpeaker.PlayOneShot(m_Sound);
	}
}
