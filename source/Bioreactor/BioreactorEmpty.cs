using TUNING;
using UnityEngine;

namespace Bioreactor
{
    public class BioreactorEmpty : Workable
    {
        private static readonly HashedString[] CLEAN_ANIMS = new HashedString[2]
        {
    (HashedString) "sponge_pre",
    (HashedString) "sponge_loop"
        };
        private static readonly HashedString PST_ANIM = new HashedString("sponge_pst");

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            this.workerStatusItem = Db.Get().DuplicantStatusItems.Cleaning;
            this.workingStatusItem = Db.Get().MiscStatusItems.Cleaning;
            this.attributeConverter = Db.Get().AttributeConverters.TidyingSpeed;
            this.attributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
            this.workAnims = BioreactorEmpty.CLEAN_ANIMS;
            this.workingPstComplete = new HashedString[1]
            {
      BioreactorEmpty.PST_ANIM
            };
            this.workingPstFailed = new HashedString[1]
            {
      BioreactorEmpty.PST_ANIM
            };
            this.synchronizeAnims = false;
        }
    }
}