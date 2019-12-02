using UnityEngine;

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using mattatz.Utils;
using mattatz.Triangulation2DSystem;

namespace mattatz.TeddySystem.Example
{

    public class SpaceColonyDrawer : Drawer
    {
        Generator g;

        void Awake()
        {
            g = FindObjectOfType<Generator>();
            TreeDrawer t = FindObjectOfType<TreeDrawer>();

            if (g)
                g.enabled = false;
            if (t)
                t.enabled = false;
        }

        void LateUpdate()
        {
            if (g && g._attractors.Count > 0)
                mode = OperationMode.Default;
        }

    }

}

