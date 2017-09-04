using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelf : MonoBehaviour {
    public float delay = 2f;

	private void Start () {
        DestroyObject(this.gameObject, delay);
	}
}