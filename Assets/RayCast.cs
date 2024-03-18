//Jesús Daniel Rivas Ñuño A01655181
//Luis Humberto Romero Pérez A01752789
//Silvio Emmanuel Prieto Caro A01423341
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.Linq;

/*
rend.material.shader = 
¨*/

public class RayCast : MonoBehaviour
{
    private Camera mainCam;

    private Light point;
    private List<Vector3> ka, kd, ks;
    private Vector3 Ia, Id, Is;
    private List<float> alpha;
    private List<float> radius;
    private List<Vector3> center;

    private List<GameObject> spheres;

    private float near, fov, cameraAspect;

    private GameObject pointLight;
    private GameObject plane;

    private int fbw, fbh;

    private float AMT_SPHERES;

    private float h, w;
    private Vector3 Cam;
    private Vector3 Light;

    private Vector3 n,v,l,r;
    private Vector3 PoI;

    private Texture2D Texture;

    public Texture2D windowsBackground;

    int backgroundHeightPorcentage;
    int backgroundWidthPorcentage;


    // Start is called before the first frame update
    void Start()
    {
        Ia = new Vector3(0.7f, 0.7f, 0.7f);
        Id = new Vector3(0.8f, 0.8f, 1f);
        Is = new Vector3(1f, 1f, 1f);
        SetCameraProperties();
        SetLightProperties();
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = new Vector3(0, 5, -2);
        plane.transform.rotation = Quaternion.Euler(90, 0, 0);
        plane.transform.localScale = new Vector3(1, 1, 1);
        plane.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Texture");
    
        fbw = 640;
        fbh = 480;
        Texture = new Texture2D(fbw, fbh);
        backgroundHeightPorcentage = windowsBackground.height;
        backgroundWidthPorcentage = windowsBackground.width;

        h = 2.0f * near * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        w = h * Camera.main.aspect;

        AMT_SPHERES = 20;
        
        SpawnSpheres();

        DoRender(Texture);

        

        

    }

    void SetCameraProperties(){
        mainCam = Camera.main;
        mainCam.transform.position = new Vector3(0, 4, 5.5f);
        mainCam.transform.rotation = Quaternion.Euler(0, 0, 0);
        mainCam.fieldOfView = 65;
        mainCam.nearClipPlane = 1;
        mainCam.farClipPlane = 20;
        mainCam.transform.localScale = new Vector3(1, 1, 1);
        mainCam.clearFlags = CameraClearFlags.SolidColor;

        Cam = mainCam.transform.position;
        near = mainCam.nearClipPlane;
        fov = mainCam.fieldOfView;
        cameraAspect = mainCam.aspect;
    }

    void SetLightProperties(){
        pointLight = new GameObject("Point Light");
        pointLight.AddComponent<Light>();
        pointLight.transform.position = new Vector3(0, 7.5f, 3);
        pointLight.GetComponent<Light>().intensity = 10;
        pointLight.GetComponent<Light>().range = 10;
        pointLight.GetComponent<Light>().color = new Color(Id.x, Id.y, Id.z);
        Light = pointLight.transform.position;
    }

    void SpawnSpheres(){
        alpha = new List<float>();
        radius = new List<float>();
        center = new List<Vector3>();
        ka = new List<Vector3>();
        kd = new List<Vector3>();
        ks = new List<Vector3>();
        spheres = new List<GameObject>();

        for(int i = 0; i < AMT_SPHERES; i++){
            alpha.Add(Random.Range(500f, 600f));
            radius.Add(Random.Range(0.1f, 0.35f));
            center.Add(new Vector3(Random.Range(-2f, 2f), Random.Range(2f, 6f), Random.Range(8f, 10f)));
            spheres.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            spheres[i].transform.position = center[i];
            spheres[i].transform.localScale = new Vector3(radius[i], radius[i], radius[i]);

            kd.Add(new Vector3(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f)));
            ka.Add(new Vector3(kd[i].x / 5.0f, kd[i].y / 5.0f, kd[i].z / 5.0f));
            ks.Add(new Vector3(kd[i].x / 3.0f, kd[i].y / 3.0f, kd[i].z / 3.0f));

            spheres[i].GetComponent<Renderer>().material.shader = Shader.Find("Specular");
            spheres[i].GetComponent<Renderer>().material.SetColor("_SpecColor", new Color(ks[i].x, ks[i].y, ks[i].z, 1.0f));
            spheres[i].GetComponent<Renderer>().material.SetColor("_Color", new Color(kd[i].x, kd[i].y, kd[i].z, 1.0f));
            spheres[i].GetComponent<Renderer>().material.SetFloat("_Shininess", alpha[i]/1000);
            spheres[i].name = "SPHERE_" + i;
        }         
        
    }
    Vector3 GetNTL()
    {
        Vector3 CAM_CAM = Camera.main.transform.position; // given the position of the camera
        float near = Camera.main.nearClipPlane; // and the camera's near plane
        Vector3 center = CAM_CAM +  Camera.main.transform.forward * near; // multiply the near plane by the camera's forward vector , then add that to the position of the camera
        Vector3 ntl = center - (Camera.main.transform.right * w/2.0f) + ( Camera.main.transform.up * h/2); // 

        
        return ntl;
    }

    public Vector3 GetPixelCenter(int row, int column){
        Vector3 NTL = GetNTL();
        float pixelWidth = w / fbw;
        float pixelHeight = h / fbh;

        Vector3 horizontalOffset = mainCam.transform.right * pixelWidth * (column + 0.5f);
        Vector3 verticalOffset = mainCam.transform.up * pixelHeight * (row + 0.5f);

        Vector3 pixelCenter = NTL + horizontalOffset - verticalOffset;

        // GameObject test = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // test.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        // test.transform.position = pixelCenter;
        return pixelCenter;
    }

    Vector3 GetUnitVector(Vector3 pixelCenter){
        Vector3 v = pixelCenter - mainCam.transform.position;
        float unitV = v.magnitude;
        return new Vector3(v.x / unitV, v.y / unitV, v.z / unitV);
    }

    List<float> GetDistance(float sphereRadius, Vector3 sphereCenter, Vector3 U){
        Vector3 camToSC = mainCam.transform.position - sphereCenter;
        float delta = Mathf.Pow(Math3D.dot(U, camToSC), 2) - (Mathf.Pow((camToSC).magnitude, 2) - Mathf.Pow(sphereRadius, 2)); 
        if(delta < 0){
            return new List<float>();
        } else if(delta == 0) {
            return new List<float>() {-Math3D.dot(U, camToSC)};
        } else {
            return new List<float>() {(-Math3D.dot(U, camToSC) + Mathf.Sqrt(delta)), (-Math3D.dot(U, camToSC) - Mathf.Sqrt(delta))};
        }
    } 

    

    void DoRender(Texture2D texture){
        for (int row = 0; row < fbh; row++){
            for (int col = 0; col < fbw; col++){
                List<float> distances = new List<float>();
                List<int> sphereIDs = new List<int>();
                
                Vector3 U = GetUnitVector(GetPixelCenter(row, col));
                for (int k = 0; k < AMT_SPHERES; k++){
                    List<float> tempDistanes = GetDistance(radius[k], center[k], U);
                    if (tempDistanes.Count == 1){
                        distances.Add(tempDistanes[0]);
                        sphereIDs.Add(k);
                    } else if (tempDistanes.Count == 2){
                        distances.Add(Mathf.Min(tempDistanes[0], tempDistanes[1]));
                        sphereIDs.Add(k);
                    }


                    
                }

                    if (distances.Count == 0){
                        // ESCRIBIR
                        float bgCol = col * ((float)backgroundWidthPorcentage / fbw);
                        float bgRow = row * ((float)backgroundHeightPorcentage / fbh);


                        Color backgroundPixelColor = windowsBackground.GetPixel((int)bgCol, backgroundHeightPorcentage-(int)bgRow);
                        texture.SetPixel(col, fbh - row, backgroundPixelColor);
                    } else {
                        // CALCULOS 
                        float d = distances[0];
                        int w = sphereIDs[0];
                        Vector3 SphereCenter = center[w];
                        Vector3 PoI = mainCam.transform.position + U * d;
                        n = PoI - SphereCenter;
                        Vector3 pixelColor = GetColor(n, PoI, alpha[w], ka[w], kd[w], ks[w], Ia, Id, Is);
                        
                        Debug.Log("Color: " + pixelColor + " Coords: " + PoI + " HitDistance " + d);
                        // ESCRIBIR
                        texture.SetPixel(col, fbh - row, new Color(pixelColor.x, pixelColor.y, pixelColor.z, 255));
                    }

            }
        }
        
        plane.GetComponent<Renderer>().material.mainTexture = texture;
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/Textures/";
        if(!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "TexturaFinal" + ".png", bytes);
    }


    
    
    public Vector3 GetColor(Vector3 normal, Vector3 PoI, float alpha, Vector3 ka, Vector3 kd, Vector3 ks, Vector3 Ia, Vector3 Id, Vector3 Is)
    {
        Vector3 v = (Cam - PoI);
        Vector3 vu = (Cam - PoI).normalized;
        Vector3 lu = (Light - PoI).normalized;
        Vector3 l = (Light - PoI);

        Vector3 lp_normal = normal.normalized * (Math3D.dot(normal.normalized, l));
        Vector3 lo_normal = l - lp_normal;
        Vector3 r_normal = lp_normal - lo_normal;

        float DotNuLu = Math3D.dot(normal.normalized, lu.normalized);
        float DotVuRu = Math3D.dot(vu.normalized, r_normal.normalized);
        float DotVuRuAlpha = Mathf.Pow(DotVuRu, alpha);

    
        Vector3 I = new Vector3();
        Vector3 A = new Vector3();
        Vector3 D = new Vector3();
        Vector3 S = new Vector3();

        // AMBIENT CALCULATIONS
        A.x = ka.x * Ia.x;
        A.y = ka.y * Ia.y;
        A.z = ka.z * Ia.z;

        // DIFFUSE CALCULATIONS
        D.x = kd.x * Id.x * (DotNuLu);
        D.y = kd.y * Id.y * (DotNuLu);
        D.z = kd.z * Id.z * (DotNuLu);

        // SEPECUELAR CALCULATIONS
        S.x = ks.x * Is.x * (DotVuRuAlpha);
        S.y = ks.y * Is.y * (DotVuRuAlpha);
        S.z = ks.z * Is.z * (DotVuRuAlpha);

        //Debug.Log("Specular: "+ S);

        if(float.IsNaN(S.x)){
            S.x = 0;
            S.y = 0;
            S.z = 0;
        }


        I = A + D + S;

        return I;
    }
}