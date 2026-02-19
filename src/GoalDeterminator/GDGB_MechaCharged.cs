// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

namespace ProjectGenesis.GoalDeterminator
{
    public class GDGB_MechaCharged : global::GoalDeterminator
    {
        public override bool OnInit()
        {
            if (!base.OnInit() || gameData == null) return false;

            goalData.SetProgress(0L, 1L);
            return true;
        }

        public override void OnGameTick(long gameTick)
        {
            if (gameTick % 30L != protoId % 30) return;

            Determine();
        }

        public override void Determine()
        {
            if (GameMain.mainPlayer.mecha.chargerCount <= 0) return;

            goalData.SetProgress(1L, 1L);
            Complete();
        }
    }
}
