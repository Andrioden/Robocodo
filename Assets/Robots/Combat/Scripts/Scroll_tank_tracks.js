var scrollSpeed :float = 18;

private var offset:float;
private var oldPosition:Vector2 = new Vector2(0,0);
private var newPosition:Vector2;
private var distance: float;

function Update () { 

newPosition = Vector2(this.transform.position.x,this.transform.position.z);
distance = Vector2.Distance(oldPosition, newPosition);
oldPosition = newPosition;

offset = (offset + distance * scrollSpeed * Time.deltaTime)%10;
GetComponent.<Renderer>().material.SetTextureOffset ("_MainTex", Vector2(0, offset)); 
GetComponent.<Renderer>().material.SetTextureOffset ("_BumpMap", Vector2(0, offset)); 

}