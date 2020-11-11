using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;


public class drawPipeMesh : MonoBehaviour
{

    private GameObject gameObj;
    private MeshRenderer renderer;

    public List<Vector3> points;
    public DrawTunnelMesh pipeGen;
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        gameObj = new GameObject();
        gameObj.AddComponent<MeshFilter>();

        renderer = gameObj.AddComponent<MeshRenderer>();
        renderer.material = mat;
        gameObj.GetComponent<MeshRenderer>().material = mat;
        pipeGen = gameObj.AddComponent<DrawTunnelMesh>();
        pipeGen.GetComponent<MeshRenderer>().material = mat;
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
                pipeGen.points = points;
                points.Add(jointTransformIndex.position);
                pipeGen.RenderPipe();
                gameObj.GetComponent<MeshRenderer>().material = mat;
            }
        }
    }
}
