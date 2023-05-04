using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Pour avoir acces aux classes de AR foundation
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

//Pour avoir acces au systéme d'input amélioré
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

//On force la présence du component Raycast Manager et AR Plane manager sur l'objet est placé sur le script
[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ARPlacementManager : MonoBehaviour
{
    //Référence a l'objet que l'on veut faire apparaitre
    [SerializeField]
    private GameObject prefab;

    //Reference a l'AR Raycast manager
    private ARRaycastManager aRRaycastManager;

    //Reference au AR Plane manager
    private ARPlaneManager aRPlaneManager;

    //Liste pour stocker les résultats du raycast
    private List<ARRaycastHit> rayHits = new List<ARRaycastHit>();
    GameObject placedObject = null;

    private static ARPlacementManager instance = null;
    public static ARPlacementManager Instance => instance;

    private void Awake()
    {
        //On récupére les références a l'AR Plane manager et au AR Raycast manager
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();

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
    }

    private void OnEnable()
    {
        //On active le support de l'input tactile amélioré
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();

        //On écoute l'événement onFinger down de l'input manager, la fonction définie plus bas FingerDown() est appelée 
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        //On désactive le support de l'input tactile amélioré
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();

        //On arrête d'écouter l'événement onFinger down de l'input manager
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    //Fonction appelée lorsqu'on touche l'écran
    private void FingerDown(EnhancedTouch.Finger finger)
    {
        //On regarde si on touche l'écran avec un seul doigt
        if (finger.index != 0) return;

        if (placedObject != null) return;

        //On tire un rayon depuis l'endroit ou on a touché l'écran pour voir si il y a un plane dessous
        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, rayHits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in rayHits)
            {
                //Pour chaque plan trouvé on instancie l'objet désiré a la position de la colision avec le rayon
                Pose pose = hit.pose;
                placedObject = Instantiate(prefab, pose.position, pose.rotation);
            }
        }

    }
    public void ReCreatePlacement(Transform transform)
    {
        placedObject = Instantiate(prefab, transform.position, transform.rotation);
        placedObject.transform.parent = transform;
    }
}
