using UnityEngine;
using System.Collections;

/*
TubeRenderer.js
This script is created by Ray Nothnagel of Last Bastion Games. It is
free for use and available on the Unify Wiki.
For other components I've created, see:
http://lastbastiongames.com/middleware/
(C) 2008 Last Bastion Games
--------------------------------------------------------------
EDIT: MODIFIED BY JACOB FLETCHER FOR USE WITH THE ROPE SCRIPT
http://www.reverieinteractive.com
*/

public class TubeVertex
{
	public Vector3 point = Vector3.zero;
	public float radius = 1;
	public Color color = Color.white;

	public TubeVertex(Vector3 pt, float r, Color c)
	{
		point = pt;
		radius = r;
		color = c;
	}
}
