using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;


public class DrawSingleObj : MonoBehaviour
{

    private GameObject gameObj;
    private MeshRenderer renderer;

    public List<Vector3> points;
    public SimpleTunnelMesh pipeGen;
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        gameObj = new GameObject();
        gameObj.AddComponent<MeshFilter>();
        gameObj.name = "SimpleMesh";

        renderer = gameObj.AddComponent<MeshRenderer>();
        
        pipeGen = gameObj.AddComponent<SimpleTunnelMesh>();

        pipeGen.GetComponent<MeshRenderer>().material = mat;
        gameObj.GetComponent<MeshRenderer>().material = mat;
    }

    // Update is called once per frame
    void Update()
    {
        var handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();
        if (handJointService != null)
        {
            var jointTransformIndex = handJointService.RequestJointTransform(TrackedHandJoint.IndexTip, Handedness.Right);
            var jointTransformThumb = handJointService.RequestJointTransform(TrackedHandJoint.ThumbTip, Handedness.Right);

            float dist = Vector3.Distance(jointTransformIndex.position, jointTransformThumb.position);
            if (dist <= .025)
            {
                this.addPoint(jointTransformIndex.position);
            }
        }
    }


    public void addPoint(Vector3 point)
    {
        if (points.Count > 0)
        {
            var lastPoint = points[points.Count - 1];
            if (point != lastPoint)
            {
                points.Add(point);
                pipeGen.points = points;
                pipeGen.AddPoint(point);
                pipeGen.RenderPipe();
            }
        }
        else
        {
            pipeGen.AddPoint(point);
            points.Add(point);
        }
 
    }

}
