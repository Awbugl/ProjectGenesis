// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

using ProjectGenesis.Utils;

namespace ProjectGenesis.GoalDeterminator
{
    public class GDGB_GroupPlasmaControl : GD_Group
    {
        public override void OnGameTick(long time)
        {
            if (isFinished || time % 30L != protoId % 30) return;

            Determine();
        }

        public override bool OnDetermineChild()
        {
            GoalData goalDataById1 = goalSystem.GetGoalDataById(1202);
            if (goalDataById1 != null && !goalData.displayingState[(int)gameData.gameDesc.goalLevel]
                                      && GoalTools.ItemProduction(gameData, 1202, 1, 6) > 0L)
            {
                gameData.history.RegFeatureKey(2020001);
                goalDataById1.stage = EGoalStage.Ignored;
                if (!inited || isFinished || goalData.isIgnoredOrCompleted) return true;
            }

            GoalData goalDataById2 = goalSystem.GetGoalDataById(1204);
            if (goalDataById2 != null && !goalData.displayingState[(int)gameData.gameDesc.goalLevel]
                                      && gameData.history.techQueueLength > 0)
            {
                goalDataById2.stage = EGoalStage.Ignored;
                if (!inited || isFinished || goalData.isIgnoredOrCompleted) return true;
            }

            return false;
        }

        public override bool OnDetermineComplete()
        {
            if (!gameData.history.TechUnlocked(ProtoID.T高效电浆控制)) return false;

            ForceComplete();
            return true;
        }
    }
}
