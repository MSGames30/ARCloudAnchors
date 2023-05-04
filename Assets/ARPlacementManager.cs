using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Pour avoir acces aux classes de AR foundation
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

//Pour avoir acces au syst�me d'input am�lior�
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

//On force la pr�sence du component Raycast Manager et AR Plane manager sur l'objet est plac� sur le script
[RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
public class ARPlacementManager : MonoBehaviour
{
    //R�f�rence a l'objet que l'on veut faire apparaitre
    [SerializeField]
    private GameObject prefab;

    //Reference a l'AR Raycast manager
    private ARRaycastManager aRRaycastManager;

    //Reference au AR Plane manager
    private ARPlaneManager aRPlaneManager;

    //Liste pour stocker les r�sultats du raycast
    private List<ARRaycastHit> rayHits = new List<ARRaycastHit>();
    GameObject placedObject = null;

    private static ARPlacementManager instance = null;
    public static ARPlacementManager Instance => instance;

    private void Awake()
    {
        //On r�cup�re les r�f�rences a l'AR Plane manager et au AR Raycast manager
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
        //On active le support de l'input tactile am�lior�
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();

        //On �coute l'�v�nement onFinger down de l'input manager, la fonction d�finie plus bas FingerDown() est appel�e 
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        //On d�sactive le support de l'input tactile am�lior�
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();

        //On arr�te d'�couter l'�v�nement onFinger down de l'input manager
        EnhancedTouch.Touch.onFingerDown -= FingerDown;
    }

    //Fonction appel�e lorsqu'on touche l'�cran
    private void FingerDown(EnhancedTouch.Finger finger)
    {
        //On regarde si on touche l'�cran avec un seul doigt
        if (finger.index != 0) return;

        if (placedObject != null) return;

        //On tire un rayon depuis l'endroit ou on a touch� l'�cran pour voir si il y a un plane dessous
        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, rayHits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in rayHits)
            {
                //Pour chaque plan trouv� on instancie l'objet d�sir� a la position de la colision avec le rayon
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
