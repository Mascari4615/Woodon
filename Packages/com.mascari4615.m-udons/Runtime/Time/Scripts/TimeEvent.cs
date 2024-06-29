﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Mascari4615
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class TimeEvent : MEventSender
	{
		[field: Header("_" + nameof(TimeEvent))]
		[field: SerializeField] public int TimeByDecisecond { get; set; } = 50;
		[SerializeField] private MScore mScore;
		[SerializeField] private CustomBool isCounting;

		[SerializeField] private UITimeEvent[] timeEventUIs;
		[SerializeField] private UITimeEventBar[] timeEventBarUIs;

		[UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(ExpireTime))] private int _expireTime = NONE_INT;
		public int ExpireTime
		{
			get => _expireTime;
			set
			{
				_expireTime = value;
				OnExpireTimeChange();
			}
		}

		[UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(TimeSpeed))] private int _timeSpeed = 1;
		public int TimeSpeed
		{
			get => _timeSpeed;
			set
			{
				_timeSpeed = value;
				// OnExpireTimeChange();
			}
		}

		public bool IsExpired => ExpireTime == NONE_INT;

		private void Start()
		{
			Init();
		}

		private void Init()
		{
			foreach (UITimeEvent uiTimeEvent in timeEventUIs)
				uiTimeEvent.Init(this);

			foreach (UITimeEventBar timeEventBarUI in timeEventBarUIs)
				timeEventBarUI.Init(this);
		}

		private void Update()
		{
			// UpdateUI();

			if (IsOwner() == false)
				return;

			if (ExpireTime == NONE_INT)
				return;

			if (Networking.GetServerTimeInMilliseconds() >= ExpireTime)
			{
				MDebugLog("Expired!");
				SendEvents();
				ResetTime();
			}
		}

		private void UpdateUI()
		{
			foreach (UITimeEvent uiTimeEvent in timeEventUIs)
				uiTimeEvent.UpdateUI();

			foreach (UITimeEventBar timeEventBarUI in timeEventBarUIs)
				timeEventBarUI.UpdateUI();
		}

		private void OnExpireTimeChange()
		{
			MDebugLog($"{nameof(OnExpireTimeChange)} : ChangeTo = {ExpireTime}");

			if (isCounting)
				isCounting.SetValue(ExpireTime != NONE_INT);

			UpdateUI();
		}

		public void SetExpireTime(int newExpireTime)
		{
			SetOwner();
			ExpireTime = newExpireTime;
			RequestSerialization();
		}

		public void ResetTime()
		{
			MDebugLog(nameof(ResetTime));
			SetExpireTime(NONE_INT);
		}

		public void SetTimer()
		{
			MDebugLog(nameof(SetTimer));
			SetExpireTime(Networking.GetServerTimeInMilliseconds() + (TimeByDecisecond * 100));
		}

		public void SetTimeByMScore()
		{
			MDebugLog(nameof(SetTimeByMScore));

			if (mScore != null)
				SetExpireTime(Networking.GetServerTimeInMilliseconds() + (mScore.Score * 100));
		}

		public void AddTime()
		{
			MDebugLog(nameof(AddTime));

			if (ExpireTime != NONE_INT)
				SetExpireTime(ExpireTime + TimeByDecisecond * 100);
		}

		public void AddTimeByMScore()
		{
			MDebugLog(nameof(AddTimeByMScore));

			if (mScore == null)
				return;

			if (ExpireTime == NONE_INT)
				return;

			SetExpireTime(ExpireTime + mScore.Score * 100);
		}

		public void ToggleTime()
		{
			MDebugLog(nameof(ToggleTime));

			if (ExpireTime == NONE_INT)
				SetTimer();
			else
				ResetTime();
		}
	}
}