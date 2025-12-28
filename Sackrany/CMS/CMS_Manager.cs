using Sackrany.Utils;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Sackrany.CMS
{
    public class CMS_Manager : AManager<CMS_Manager>
    {
        public CMS Cms;
        public static CMS CMS => Instance.Cms;
        private protected override void OnManagerAwake()
        {
            CMS.UpdateDictionary(true);
        }
    }
}