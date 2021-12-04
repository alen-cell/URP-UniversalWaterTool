using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraRayCast: MonoBehaviour
{
   public static float drawRadius = 0.05f;
    public static bool isRayCast = false;
    public static Vector4 currentPos;

    Camera m_camera;


    // Start is called before the first frame update
    void Start()
    {
        m_camera = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {

        isRayCast = false;

        if (Input.GetMouseButton(0))
        {
        
            
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            
            if (Physics.Raycast(ray, out hit))
            {
               
                
                isRayCast = true;
                currentPos = new Vector4(hit.textureCoord.x, hit.textureCoord.y, drawRadius);
                Shader.SetGlobalVector("_HitPoint", currentPos);
            
                 
             
            }
          
            
        }
     
        


    }
}
