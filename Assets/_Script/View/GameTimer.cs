using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI m_Time1Display;
	[SerializeField] TextMeshProUGUI m_Time2Display;

	public Action OnEnd;

	private float m_TimeLeft1 = 0; // in seconds
	private float m_TimeLeft2 = 0; // in seconds
	private bool m_IsTime1Init = true;
	private bool m_IsTime1 = true;
	private bool m_IsRunning = false;

	private float m_Increment = 0;

	public void Init(float iInitialTime, float iIncrement, bool iStartTimer1 = true)
	{
		m_TimeLeft1 = Math.Max(iInitialTime, 0);
		m_TimeLeft2 = m_TimeLeft1;
		m_IsRunning = false;
		m_Increment = iIncrement;
		m_IsTime1 = iStartTimer1;
		m_IsTime1Init = iStartTimer1;
		UpdateDisplay();
	}

	public void Switch()
	{
		m_IsTime1 = !m_IsTime1;
		if(!m_IsRunning)
		{
			if(m_IsTime1Init == m_IsTime1)
				m_IsRunning = true;
			return;
		}

		if(m_IsTime1)
			m_TimeLeft2 += m_Increment;
		else
			m_TimeLeft1 += m_Increment;
		UpdateDisplay();
	}

	public void Pause()
	{
		m_IsRunning = false;
	}

	public void Resume()
	{
		m_IsRunning = false;
	}

	public void End()
	{
		if(!m_IsRunning)
			return;

		m_IsRunning = false;
		OnEnd?.Invoke();
	}

	private void Update()
	{
		if(!m_IsRunning)
			return;

		if(m_IsTime1)
			m_TimeLeft1 -= Time.deltaTime;
		else
			m_TimeLeft2 -= Time.deltaTime;
		UpdateDisplay();

		if(m_TimeLeft1 <= 0 || m_TimeLeft2 <= 0)
			End();
	}

	private void UpdateDisplay()
	{
		m_Time1Display.text = _FormatTime(m_TimeLeft1);
		m_Time2Display.text = _FormatTime(m_TimeLeft2);
	}

	private string _FormatTime(float iSeconds)
	{
		TimeSpan span = TimeSpan.FromSeconds(iSeconds);
		return span.ToString(@"m\:ss");
	}
}
