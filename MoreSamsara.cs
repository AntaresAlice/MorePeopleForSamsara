using TaiwuModdingLib.Core.Plugin;
using GameData.Domains.Building;
using GameData.Utilities;
using HarmonyLib;
using GameData.Domains;
using GameData.Domains.Character.Display;
using GameData.Domains.Character;
using GameData.Domains.Taiwu.Profession;
using GameData.GameDataBridge;
using GameData.Domains.Global;
using GameData.Domains.World;

namespace MorePeopleForSamsara
{
    [PluginConfig(pluginName: "MorePeopleForSamsara", creatorId: "Antares", pluginVersion: "1.0.0")]
    public class MoreSamsara : TaiwuRemakePlugin
    {
        Harmony harmony;
        public override void Dispose()
        {
            if (harmony != null)
            {
                harmony.UnpatchSelf();
            }
        }

        public override void Initialize()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(MoreSamsara));
            DomainManager.Mod.GetSetting(ModIdStr, "genderSetting", ref MoreSamsara.genderSetting);
            DomainManager.Mod.GetSetting(ModIdStr, "SamsaraRangeSetting", ref MoreSamsara.samsaraRangeSetting);
            DomainManager.Mod.GetSetting(ModIdStr, "DeaMonth", ref MoreSamsara.deaMonthSetting);
            DomainManager.Mod.GetSetting(ModIdStr, "gradeSetting", ref MoreSamsara.gradeSetting);
            AdaptableLog.Info("More Samsara online.");
        }

        public override void OnModSettingUpdate()
        {
            harmony.UnpatchSelf();
            harmony.PatchAll(typeof(MoreSamsara));
            DomainManager.Mod.GetSetting(ModIdStr, "genderSetting", ref MoreSamsara.genderSetting);
            DomainManager.Mod.GetSetting(ModIdStr, "SamsaraRangeSetting", ref MoreSamsara.samsaraRangeSetting);
            DomainManager.Mod.GetSetting(ModIdStr, "DeaMonth", ref MoreSamsara.deaMonthSetting);
            DomainManager.Mod.GetSetting(ModIdStr, "gradeSetting", ref MoreSamsara.gradeSetting);
        }


    [HarmonyPrefix]
    public static bool PrePatch(
        BuildingDomain __instance,
        DataContext context,
        bool excludeCharactersInSlot,
        ref List<SamsaraPlatformCharDisplayData> __result)
    {
        if (MoreSamsara.samsaraRangeSetting == 0)
        {
            return true;
        }

        sbyte gender = (sbyte)(MoreSamsara.genderSetting);
        __result = new List<SamsaraPlatformCharDisplayData>();

        HashSet<int> waitingReincarnationChars = ObjectPool<HashSet<int>>.Instance.Get();
        DomainManager.Character.GetAllWaitingReincarnationCharIds(waitingReincarnationChars);

        int currDate = WorldDomain.GetWorldInfo().CurrDate;
        int durDate = currDate - (deaMonthSetting != 0 && deaMonthSetting <= currDate ? deaMonthSetting : 0);

        foreach (int charId in waitingReincarnationChars)
        {
            bool isValidChar = __instance.IsCharOnSamsaraPlatform(charId, false) ||
                               ProfessionSkillHandle.BuddhistMonkSkill_IsDirectedSamsaraCharacter(charId);

            if (!isValidChar)
            {
                DeadCharacter deadChar = DomainManager.Character.GetDeadCharacter(charId);

                if ((gender == 0 || gender == 1) && deadChar.Gender != gender)
                {
                    continue;
                }

                int taiwuSettlementId = DomainManager.Taiwu.GetTaiwuVillageSettlementId();
                if (MoreSamsara.samsaraRangeSetting == 2 && deadChar.OrganizationInfo.SettlementId != taiwuSettlementId)
                {
                    continue;
                }

                if (deaMonthSetting != 0 && deadChar.DeathDate < durDate)
                {
                    continue;
                }

                if (gradeSetting != 9 && deadChar.OrganizationInfo.Grade != gradeSetting)
                {
                    continue;
                }

                SamsaraPlatformCharDisplayData displayData = new SamsaraPlatformCharDisplayData
                {
                    Id = charId,
                    TemplateId = deadChar.TemplateId,
                    NameRelatedData = DomainManager.Character.GetNameRelatedData(charId),
                    AvatarRelatedData = new AvatarRelatedData(deadChar),
                    MainAttributes = deadChar.BaseMainAttributes,
                    CombatSkillQualifications = deadChar.BaseCombatSkillQualifications,
                    LifeSkillQualifications = deadChar.BaseLifeSkillQualifications
                };
                __result.Add(displayData);
            }
        }

        ObjectPool<HashSet<int>>.Instance.Return(waitingReincarnationChars);
        return false; // Skip the original method
    }

        // [HarmonyPrefix, HarmonyPatch(
        //     declaringType: typeof(BuildingDomain),
        //     methodName: "GetSamsaraPlatformCharList")]
        // public static bool BuildingDomain_GetSamsaraPlatformCharList_PrePatch(
        // BuildingDomain __instance,
        // ref List<SamsaraPlatformCharDisplayData> __result
        //     )
        // {
        //     if (MoreSamsara.samsaraRangeSetting == 0) {
        //         return true;
        //     }
        //     // gender setting
        //     sbyte gender = (sbyte)(MoreSamsara.genderSetting);

        //     // List<SamsaraPlatformCharDisplayData> dataList = new List<SamsaraPlatformCharDisplayData>();
        //     __result = new List<SamsaraPlatformCharDisplayData>();
        //     HashSet<int> waitingReincarnationChars = ObjectPool<HashSet<int>>.Instance.Get();
        //     DomainManager.Character.GetAllWaitingReincarnationCharIds(waitingReincarnationChars);
        //     int currDate = WorldDomain.GetWorldInfo().CurrDate;
        //     int durDate = 0;
        //     if (deaMonthSetting != 0)
        //     {
        //         if (deaMonthSetting > currDate)
        //         {
        //             durDate = 0;
        //         }
        //         else
        //         {
        //             durDate = currDate - deaMonthSetting;
        //         }
        //     }
        //     foreach (int charId in waitingReincarnationChars)
        //     {
        //         bool flag = __instance.IsCharOnSamsaraPlatform(charId, false) || ProfessionSkillHandle.BuddhistMonkSkill_IsDirectedSamsaraCharacter(charId);
        //         if (!flag)
        //         {
        //             DeadCharacter deadChar = DomainManager.Character.GetDeadCharacter(charId);

        //             if (gender == 0 || gender == 1)
        //             {
        //                 if (deadChar.Gender == gender) continue;
        //             }

        //             // int taiwuId = DomainManager.Taiwu.GetTaiwuCharId();
        //             int taiwuSettlementId = DomainManager.Taiwu.GetTaiwuVillageSettlementId();
        //             if (MoreSamsara.samsaraRangeSetting == 2)
        //             {
        //                 if (deadChar.OrganizationInfo.SettlementId != taiwuSettlementId) continue;
        //             }

        //             if (deaMonthSetting != 0)
        //             {
        //                 if (deadChar.DeathDate < durDate) continue;
        //             }

        //             if (gradeSetting != 9)
        //             {
        //                 if (deadChar.OrganizationInfo.Grade != gradeSetting) continue;
        //             }

        //             //AdaptableLog.Info(DomainManager.Character.GetNameRelatedData(charId).GetRealName().surname + DomainManager.Character.GetNameRelatedData(charId).GetRealName().givenName);
        //             //AdaptableLog.Info(deadChar.DeathDate.ToString());
        //             //AdaptableLog.Info(deadChar.OrganizationInfo.SettlementId.ToString());
        //             //AdaptableLog.Info(deadChar.OrganizationInfo.Grade.ToString());

        //             SamsaraPlatformCharDisplayData displayData = new SamsaraPlatformCharDisplayData
        //             {
        //                 Id = charId,
        //                 NameRelatedData = DomainManager.Character.GetNameRelatedData(charId),
        //                 AvatarRelatedData = new AvatarRelatedData(deadChar),
        //                 MainAttributes = deadChar.BaseMainAttributes,
        //                 CombatSkillQualifications = deadChar.BaseCombatSkillQualifications,
        //                 LifeSkillQualifications = deadChar.BaseLifeSkillQualifications
        //             };
        //             __result.Add(displayData);
        //         }
        //         ObjectPool<HashSet<int>>.Instance.Return(waitingReincarnationChars);
        //     }
        //     // AdaptableLog.Info("More Samsara now.");
        //     return false;
        // }

        private static int genderSetting;
        private static int samsaraRangeSetting;
        private static int deaMonthSetting;
        private static int gradeSetting;
    }
}
