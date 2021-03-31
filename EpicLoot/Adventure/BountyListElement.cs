﻿using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot.Adventure
{
    public class BountyListElement : BaseMerchantPanelListElement<BountyInfo>
    {
        public Image Icon;
        public Text NameText;
        public Text RewardTextIron;
        public Text RewardTextGold;
        public Button Button;
        public UITooltip Tooltip;
        public GameObject RewardLabel;
        public GameObject InProgressLabel;
        public GameObject CompleteLabel;

        public BountyInfo BountyInfo;
        public bool CanAccept => BountyInfo.State == BountyState.Available;
        public bool CanClaim => BountyInfo.State == BountyState.Complete;

        private static readonly StringBuilder _sb = new StringBuilder();

        public event Action<BountyInfo> OnSelected;

        public void Awake()
        {
            Button = GetComponent<Button>();
            Tooltip = GetComponent<UITooltip>();
            SelectedBackground = transform.Find("Selected").gameObject;
            SelectedBackground.SetActive(false);
            Icon = transform.Find("Icon").GetComponent<Image>();
            NameText = transform.Find("Name").GetComponent<Text>();
            RewardLabel = transform.Find("Rewards/RewardLabel").gameObject;
            InProgressLabel = transform.Find("Rewards/InProgressLabel").gameObject;
            CompleteLabel = transform.Find("Rewards/CompleteLabel").gameObject;
            RewardTextIron = transform.Find("Rewards/IronElement/Amount").GetComponent<Text>();
            RewardTextGold = transform.Find("Rewards/GoldElement/Amount").GetComponent<Text>();

            var iconMaterial = StoreGui.instance.m_listElement.transform.Find("icon").GetComponent<Image>().material;
            if (iconMaterial != null)
            {
                Icon.material = iconMaterial;
            }
        }

        public void SetItem(BountyInfo info)
        {
            BountyInfo = info;

            var displayName = GetDisplayName();
            var canUse = BountyInfo.State == BountyState.Available || BountyInfo.State == BountyState.Complete;

            NameText.text = displayName;
            NameText.color = canUse ? Color.white : Color.gray;

            RewardTextIron.text = BountyInfo.RewardIron.ToString();
            RewardTextIron.transform.parent.gameObject.SetActive(BountyInfo.RewardIron > 0);
            RewardTextGold.text = BountyInfo.RewardGold.ToString();
            RewardTextGold.transform.parent.gameObject.SetActive(BountyInfo.RewardGold > 0);

            Icon.sprite = AdventureDataManager.GetTrophyIconForMonster(BountyInfo.Target.MonsterID);
            Icon.color = canUse ? Color.white : new Color(1.0f, 0.0f, 1.0f, 0.0f);

            RewardLabel.SetActive(BountyInfo.State == BountyState.Available);
            InProgressLabel.SetActive(BountyInfo.State == BountyState.InProgress);
            CompleteLabel.SetActive(BountyInfo.State == BountyState.Complete);

            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(() => OnSelected?.Invoke(BountyInfo));

            Tooltip.m_topic = displayName;
            Tooltip.m_text = Localization.instance.Localize(GetTooltip());
        }

        private string GetDisplayName()
        {
            var typeName = BountyInfo.RewardGold > 0 ? "Gold" : "Iron";
            return Localization.instance.Localize($"{typeName} Bounty: {AdventureDataManager.GetBountyName(BountyInfo)}");
        }

        private string GetTooltip()
        {
            _sb.Clear();
            var biome = $"$biome_{BountyInfo.Biome.ToString().ToLower()}";
            var monsterName = AdventureDataManager.GetMonsterName(BountyInfo.Target.MonsterID).ToLowerInvariant();
            var targetName = string.IsNullOrEmpty(BountyInfo.TargetName) ? "" : $"<color=orange>{BountyInfo.TargetName}</color>, ";
            var adds = BountyInfo.Adds.Count > 0 ? " and its minions" : "";
            _sb.AppendLine($"Travel to the <color=#31eb41>{biome}</color> and locate {targetName}the <color=#f03232>{monsterName}</color>. Slay it{adds}. Return to me when it is done.");
            _sb.AppendLine();

            _sb.AppendLine("<color=#ffc400>Rewards:</color>");
            if (BountyInfo.RewardIron > 0)
            {
                _sb.AppendLine($"  {MerchantPanel.GetIronBountyTokenName()} x{BountyInfo.RewardIron}");
            }
            if (BountyInfo.RewardGold > 0)
            {
                _sb.AppendLine($"  {MerchantPanel.GetGoldBountyTokenName()} x{BountyInfo.RewardGold}");
            }

            _sb.AppendLine();
            _sb.AppendLine("<color=#ffc400>Status:</color>");
            switch (BountyInfo.State)
            {
                case BountyState.Available:
                    _sb.AppendLine("  Available");
                    break;
                case BountyState.InProgress:
                    _sb.AppendLine("  <color=#00f0ff>In Progress</color>");
                    break;
                case BountyState.Complete:
                    _sb.AppendLine("  <color=#70f56c>Vanquished!</color>");
                    break;
                case BountyState.Claimed:
                    _sb.AppendLine("  Claimed");
                    break;
            }

            return _sb.ToString();
        }
    }
}
