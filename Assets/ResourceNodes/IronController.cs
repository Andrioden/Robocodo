using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronController : ResourceController
{

    public override string SerializedInventoryType() { return IronItem.SerializedType; }

}
