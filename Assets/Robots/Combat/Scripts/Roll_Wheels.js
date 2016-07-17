#pragma strict

var wheelDiameter :float = 1;
private var wheelLength:float;
private var oldPosition:Vector3;
private var newPosition:Vector3;
private var distance: float;

function Start () {

oldPosition = new Vector3(0,0,0);
wheelLength = wheelDiameter*Mathf.PI;

}

function Update () {

newPosition = this.transform.position;
distance = Vector3.Distance(oldPosition, newPosition);
oldPosition = newPosition;

this.transform.Rotate(distance/wheelLength*360,0,0);
}