using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

public class AnchorCreatedEvent : UnityEvent<Transform> {}

public class ARCloudAnchorManager : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera = null;
    [SerializeField]
    private float resolveAnchorPassedTimeout = 10.0f;
    private ARAnchorManager aRAnchorManager = null;
    ARAnchor pendingHostAnchor = null;
    ARCloudAnchor cloudAnchor = null;
    private string anchorIDToResolve;
    bool anchorResolveInProgress = false;
    bool anchorHostInProgress = false;
    float safeToResolvePassed = 0;
    AnchorCreatedEvent cloudAnchorCreatedEvent = null;

    private static ARCloudAnchorManager instance = null;
    public static ARCloudAnchorManager Instance => instance;

    private void Awake()
    {


        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        cloudAnchorCreatedEvent = new AnchorCreatedEvent();
        cloudAnchorCreatedEvent.AddListener((t)=> ARPlacementManager.Instance.ReCreatePlacement(t));
    }

    private Pose GetCameraPose()
    {
        return new Pose(arCamera.transform.position,arCamera.transform.rotation);
    }

    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchor =arAnchor;
    }
    public void HostAnchor()
    {
        FeatureMapQuality quality = aRAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        cloudAnchor = aRAnchorManager.HostCloudAnchor(pendingHostAnchor, 1);

        if(cloudAnchor != null)
        {
            anchorHostInProgress = true;
        }
    }

    public void Resolve()
    {
        cloudAnchor = aRAnchorManager.ResolveCloudAnchorId(anchorIDToResolve);

        if (cloudAnchor != null)
        {
            anchorResolveInProgress = true;
        }
    }

    private void CheckHostingProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        if(cloudAnchorState == CloudAnchorState.Success)
        {
            anchorHostInProgress = false;
            anchorIDToResolve = cloudAnchor.cloudAnchorId;
        }
        else if(cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            anchorHostInProgress = false;
        }
    }
    private void CheckResolveProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        if (cloudAnchorState == CloudAnchorState.Success)
        {
            anchorResolveInProgress = false;
            
        }
        else if (cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            anchorResolveInProgress = false;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(anchorHostInProgress)
        {
            CheckHostingProgress();
            return;
        }

        if(anchorResolveInProgress && safeToResolvePassed <=0)
        {
            safeToResolvePassed = resolveAnchorPassedTimeout;
            if(!string.IsNullOrEmpty(anchorIDToResolve))
            {
                CheckResolveProgress();
            }
        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }
    }
}
