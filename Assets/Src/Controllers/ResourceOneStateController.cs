using System.Collections;
using System.Collections.Generic;
using Controllers;
using Managers;
using UnityEngine;

namespace Controllers {
    public class ResourceOneStateController : ResourceStateController {
        // Start is called before the first frame update
        [SerializeField]
        public string[] tags;
        public override bool isAvailable () {
            return resourceId > 0 || tags.Length > 0;
        }
        public override void OnUpdateCount (int id, int count) {

            if (!isAvailable ())
                return;

            if (resourceId > 0 && resourceId != id)
                return;

            ResourceData rd = Services.data.ResInfo (id);

            if (tags.Length > 0 && rd.tags != null && Utils.Intersection(tags, rd.tags))
                return;

            int _value = player.AvailableResource (id);

            icon?.SetActive (_value > 0);
            image?.SetActive (_value > 0);
        }

        // Update is called once per frame
        void Update () {

        }
    }
}