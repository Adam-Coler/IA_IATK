using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;


public class DrawLineMeshFromRenderer : MonoBehaviour
{

    private GameObject gameObj;
    private MeshLineRenderer currentLine;
    private bool lineMade = false;
    private bool hasCollider = false;
    private int numPoints = 0;
    private BoundingBox boundbox;

    private Bounds newbound;


    // Start is called before the first frame update
    void Start()
    {
        
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
                if (!lineMade)
                {
                    GameObject.Destroy(gameObj);
                    //make new line
                    gameObj = new GameObject();

                    gameObj.AddComponent<MeshFilter>();
                    gameObj.AddComponent<MeshRenderer>();

                    gameObj.AddComponent<ManipulationHandler>();
                    boundbox = gameObj.AddComponent<BoundingBox>();
                    

                    currentLine = gameObj.AddComponent<MeshLineRenderer>();
                    currentLine.setWidth(.01f);

                    hasCollider = false;
                    lineMade = true;
                    numPoints = 0;
                }

                gameObject.transform.position = jointTransformIndex.position;
                // should allocate in the future
                currentLine.AddPoint(gameObject.transform.position);
                numPoints++;

                
            }
            else
            {
                if (!hasCollider)
                {
                    MeshCollider meshc = gameObj.AddComponent(typeof(MeshCollider)) as MeshCollider;
                    meshc.sharedMesh = gameObj.GetComponent<MeshFilter>().sharedMesh;
                    boundbox.BoundsOverride = gameObj.GetComponent<BoxCollider>();


                    newbound = gameObj.GetComponent<MeshFilter>().mesh.bounds;
                    gameObj.GetComponent<BoxCollider>().size = newbound.size;
                    gameObj.GetComponent<BoxCollider>().center = newbound.center;
                                        gameObj.AddComponent<NearInteractionGrabbable>();
                    boundbox.ProximityEffectActive = true;
                    boundbox.FarScale = .25f;

                    gameObj.AddComponent<NearInteractionGrabbable>();

                    hasCollider = true;
                }

                lineMade = false;
                numPoints = 0;
            }

        }
    }
}