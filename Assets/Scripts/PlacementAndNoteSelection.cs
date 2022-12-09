using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;


[RequireComponent(typeof(ARRaycastManager))]
public class PlacementAndNoteSelection : MonoBehaviour
{
    public GameObject[] noteSelections;
    GameObject note;
    public GameObject indicator;
    Pose placementPost;
    GameObject selectedObject;
    ARRaycastManager _aRRaycastManager;
    ARPlaneManager _arPlaneManager;
    Vector3 center;
    //bool doubleTap = false;
    bool placementPoseValid = false;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    static List<ARRaycastHit> placementHits = new List<ARRaycastHit>();
    public GameObject EditText;
    public TextMeshProUGUI NoteText;
    int maxSize = 512;

    private void Awake()
    {
        _aRRaycastManager = GetComponent<ARRaycastManager>();
        _arPlaneManager = GetComponent<ARPlaneManager>();
        note = noteSelections[0];
        NoteText.text = "Current Selection: Basic Note";
    }
    /*    void Start()
        {
        }
    */

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    void UpdatePlacementPose() {
        //find a valid plane to put our indicator on -- doing this here is 
        //better than doing it in a bunch of places! 
        center = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        _aRRaycastManager.Raycast(center, hits, TrackableType.Planes);
        if(hits.Count > 0) {
            placementPoseValid = true;
            placementPost = hits[0].pose;
        }
    }

    void UpdatePlacementIndicator() {
        //will place indicator if valid or not, depends on if a plane is found
        if (placementPoseValid)
        {
            indicator.SetActive(true);
            indicator.transform.SetPositionAndRotation(placementPost.position, placementPost.rotation);
        }
        else {
            indicator.SetActive(false);
        }
    }

    //Retrieved from https://forum.unity.com/threads/ar-foundation-align-ar-object-pararell-to-floor.699614/ 
    //when looking into how to orient the objects appropriately
    private void GetWallPlacement(ARRaycastHit _planeHit, out Quaternion orientation, out Quaternion zUp)
    {
        TrackableId planeHit_ID = _planeHit.trackableId;
        ARPlane planeHit = _arPlaneManager.GetPlane(planeHit_ID);
        Vector3 planeNormal = planeHit.normal;
        orientation = Quaternion.FromToRotation(Vector3.up, planeNormal);
        Vector3 forward = _planeHit.pose.position - (_planeHit.pose.position + Vector3.down);
        zUp = Quaternion.LookRotation(forward, planeNormal);
    }


    // Called when the Place button is pressed
    public void PlaceNote() 
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        //by this point, we dont actually need to raycast again -- we already do in update
        //and update our hits accordingly 
        //at this point, a plane was hit -- we can check if something was there or not first 

        var hitPose = hits[0].pose;
        Quaternion orientation = Quaternion.identity;
        Quaternion zUp = Quaternion.identity;

        GetWallPlacement(hits[0], out orientation, out zUp);
        if (Physics.Raycast(ray, out hit)) //already a note here, no point
        {
            return;
        }
        // shoutout to https://assetstore.unity.com/packages/tools/integration/native-gallery-for-android-ios-112630
        // for simplifying this process a bunch! 
        if (note.tag == "IN")
        {
            selectedObject = Instantiate(note, hitPose.position, orientation);
            //selectedObject.transform.rotation = zUp;
            GameObject imageChild = selectedObject.transform.GetChild(0).gameObject;
            //this is an async callback to get the image! undo the permission get if needed 
            /*NativeGallery.Permission p =*/
            NativeGallery.GetImageFromGallery((path) =>
            {
                if (path != null)
                {
                    Texture2D tex = NativeGallery.LoadImageAtPath(path, maxSize);
                    if (tex == null)
                    {
                        return;
                    }
                    Material mat = imageChild.GetComponent<Renderer>().material;
                    mat.mainTexture = tex;
                }
            });
        }
        else
        {
            selectedObject = Instantiate(note, hitPose.position, orientation);
            //selectedObject.transform.rotation = zUp;
        }
        // this line works -- we can get the inputfield and replace 
        // EditText.GetComponent<TMP_InputField>().text = "I put it down" + hitPose.position;
/*        if (_aRRaycastManager.Raycast(center, hits, TrackableType.Planes)) {

        }*/
    }

    // Basic stuff to switch the type of note being selected including color of pressed button 
    public void SwitchToBasic() { 
        note = noteSelections[0];
        NoteText.text = "Current Selection: Basic Note";
    }
    public void SwitchToTimer() { 
        note = noteSelections[1];
        NoteText.text = "Current Selection: Timer Note";
    }
    public void SwitchToImage() { 
        note = noteSelections[2];
        NoteText.text = "Current Selection: Image Note";
    }
    
    //Note Editing handling 
    public void EditNote() {
        Ray ray = Camera.main.ScreenPointToRay(center);
        RaycastHit hit;

        var hitCheck = Physics.Raycast(ray, out hit);
        if (!hitCheck) //no note found under the indicator! 
        {
            //EditText.GetComponent<TMP_InputField>().text = "early return (no hit)";
            return;
        }else if(hit.transform.gameObject == null){
            //EditText.GetComponent<TMP_InputField>().text = "early return (null obj)";
            return;
        }
        selectedObject = hit.transform.gameObject;
        TextMeshProUGUI textbox;
        TMP_InputField editbox;
        switch (selectedObject.tag)
        { //Actions change based on type of note
            case "BN":
                textbox = selectedObject.GetComponentInChildren<TextMeshProUGUI>();
                textbox.text = EditText.GetComponent<TMP_InputField>().text;
                EditText.GetComponent<TMP_InputField>().text = "";
                break;
            case "TN":
                editbox = EditText.GetComponent<TMP_InputField>();
                if (editbox.text == "") {
                    //some clown didnt put any #s in there!
                    return;
                }
                textbox = selectedObject.GetComponentInChildren<TextMeshProUGUI>();
                var counter = selectedObject.GetComponent<CountDown>();
                if (counter == null) {
                    //something has seriously gone wrong if this happens
                    return;
                }
                counter.ParseInput(editbox.text);
                editbox.text = "";
                break;
            case "IN":
            case "IN_QUAD":
                editbox = EditText.GetComponent<TMP_InputField>();
                GameObject imageObj;
                //ignore the naming conventions lmao 
                if (selectedObject.tag == "IN")
                    imageObj = selectedObject.transform.GetChild(0).gameObject;
                else
                    imageObj = selectedObject;
                //this is an async callback to get the image! undo the permission get if needed 
                /*NativeGallery.Permission p =*/ 
                NativeGallery.GetImageFromGallery((path) =>
                {
                    if (path != null)
                    {
                        Texture2D tex = NativeGallery.LoadImageAtPath(path, maxSize);
                        if (tex == null)
                        {
                            editbox.text = "Couldnt load texture from " + path;
                            return;
                        }
                        Material mat = imageObj.GetComponent<Renderer>().material;
                        mat.mainTexture = tex;
                    }
                });
                break;
        }
    }

    //simple deletion script, called when you hit the "delete note" button
    public void DeleteNote() {
        
        Ray ray = Camera.main.ScreenPointToRay(center);
        RaycastHit hit;

        var hitCheck = Physics.Raycast(ray, out hit);
        if (!hitCheck) //no note found under the indicator! 
        {
            //EditText.GetComponent<TMP_InputField>().text = "early return (no hit)";
            return;
        }
        else if (hit.transform.gameObject == null)
        {
            //EditText.GetComponent<TMP_InputField>().text = "early return (null obj)";
            return;
        }
        Destroy(hit.transform.gameObject);
    }
}
