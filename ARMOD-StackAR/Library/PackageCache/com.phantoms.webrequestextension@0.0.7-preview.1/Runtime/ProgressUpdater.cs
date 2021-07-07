using System.Collections.Generic;
using UnityEngine;

namespace com.Phantoms.WebRequestExtension.Runtime
{
    public class ProgressUpdater : MonoBehaviour
    {
        private const string CONST_PROGRESS_NAME = "ProgressUpdater";
        private static ProgressUpdater INSTANCE;
        readonly List<AsyncOperationProgressNotifier> items = new List<AsyncOperationProgressNotifier>();

        public static ProgressUpdater Instance
        {
            get
            {
                if (INSTANCE == null)
                {
                    INSTANCE = new GameObject(CONST_PROGRESS_NAME).AddComponent<ProgressUpdater>();
                }

                return INSTANCE;
            }
        }

        public void AddItem(AsyncOperationProgressNotifier _asyncItem)
        {
            if (!_asyncItem.NotifyProgress())
            {
                items.Add(_asyncItem);
            }
        }

        void Update()
        {
            for (var tmp_I = 0; tmp_I < items.Count; tmp_I++)
            {
                var tmp_Item = items[tmp_I];

                if (tmp_Item.NotifyProgress())
                {
                    items[tmp_I] = null;
                }
            }

            items.RemoveAll(_item => _item == null);
        }
    }
}