﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventTarget : Graphic
{
    public override void SetMaterialDirty(){}
    public override void SetVerticesDirty(){}
    protected override void OnPopulateMesh(VertexHelper vh) {}
}
