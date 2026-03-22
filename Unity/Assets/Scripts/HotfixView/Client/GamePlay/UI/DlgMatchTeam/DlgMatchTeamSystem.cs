using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[FriendOf(typeof(DlgMatchTeam))]
	public static class DlgMatchTeamSystem
	{

		public static void RegisterUIEvent(this DlgMatchTeam self)
        {
            self.View.E_ConfirmButton.AddListener(self.Root(), self.OnStartMatchClick);
            self.View.E_CancelButton.AddListener(self.Root(), self.OnCancelClick);

            self.View.ELoopScrollList_RolesLoopHorizontalScrollRect.AddItemRefreshListener(self.OnScrollItemRefreshHandler);

        }

		public static void ShowWindow(this DlgMatchTeam self, Entity contextData = null)
		{
			self.IsMatching = false;
            self.MemberIds.Clear();
            long myId = self.Root().GetComponent<PlayerComponent>().MyId;
            self.MemberIds.Add(myId);

			self.RefreshMembers();
			self.View.ECountDownText.text = string.Empty;
            self.StartMatchAsync().Coroutine();
		}


        public static void OnScrollItemRefreshHandler(this DlgMatchTeam self, Transform transform, int index)
        {
            if (self.ScrollItemRoles == null || !self.ScrollItemRoles.ContainsKey(index))
            {
                return;
            }

            Scroll_Item_role itemRole = self.ScrollItemRoles[index].BindTrans(transform);

			if (self.MemberIds.Count > index)
			{
				long memberId = self.MemberIds[index];
				itemRole.E_RoleNameText.text = memberId.ToString();

				int avatarIndex = (index % 9) + 1;
				itemRole.E_AvatarImage.sprite = self.Root().GetComponent<ResourcesLoaderComponent>().LoadAssetSync<Sprite>($"Avatar{avatarIndex}");
			}
		}

        public static void OnStartMatchClick(this DlgMatchTeam self)
        {
            if (self.IsMatching)
            {
                return;
            }

            self.StartMatchAsync().Coroutine();
        }

        public static void OnCancelClick(this DlgMatchTeam self)
        {
            self.CancelMatchAsync().Coroutine();
        }

		public static void RefreshMembers(this DlgMatchTeam self)
		{
			int count = self.MemberIds.Count;

			self.RemoveUIScrollItems(ref self.ScrollItemRoles);
			self.AddUIScrollItems(ref self.ScrollItemRoles, count);
			self.View.ELoopScrollList_RolesLoopHorizontalScrollRect.SetVisible(true, count);
		}

		public static async ETTask StartCountDown(this DlgMatchTeam self)
		{
            Scene root = self.Root();
            EntityRef<DlgMatchTeam> selfRef = self;

			for (int i = ConstValue.StateSyncMatchTimeoutTime / 1000; i > 0; i--)
			{
                self = selfRef;
                if (self == null || self.IsDisposed)
                {
                    return;
                }

				if (!self.IsMatching)
				{
					self.View.ECountDownText.text = string.Empty;
					return;
				}

                self.View.ECountDownText.text = i.ToString();
                await root.GetComponent<TimerComponent>().WaitAsync(1000);
            }

            self = selfRef;
            if (self == null || self.IsDisposed)
            {
                return;
            }

			self.View.ECountDownText.text = string.Empty;
        }

        private static async ETTask StartMatchAsync(this DlgMatchTeam self)
        {
            EntityRef<DlgMatchTeam> selfRef = self;
            self.IsMatching = true;
            try
            {
                await EnterMapHelper.StateSyncMatch(self.Fiber());
            }
            catch (Exception)
            {
                self = selfRef;
                if (self == null || self.IsDisposed)
                {
                    return;
                }

                self.IsMatching = false;
                self.View.ECountDownText.text = string.Empty;
                return;
            }

            self = selfRef;
            if (self == null || self.IsDisposed || !self.IsMatching)
            {
                return;
            }

            self.StartCountDown().Coroutine();
        }

        private static async ETTask CancelMatchAsync(this DlgMatchTeam self)
        {
            bool wasMatching = self.IsMatching;
            self.IsMatching = false;
            self.View.ECountDownText.text = string.Empty;
            self.Root().GetComponent<UIComponent>().HideWindow(WindowID.WindowID_MatchTeam);

            if (!wasMatching)
            {
                return;
            }

            try
            {
                await EnterMapHelper.CancelMatchAsync(self.Fiber());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

    }
}

