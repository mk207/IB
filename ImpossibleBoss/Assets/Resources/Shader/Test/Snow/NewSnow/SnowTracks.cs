using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowTracks : MonoBehaviour
{
    public Shader _drawShader;
    private Material _drawMaterial;
    private Material myMaterial;
    public GameObject _terrain;
    public Transform[] _wheel;
    private RenderTexture _splatmap;
    RaycastHit _groundHit;
    int _layerMask;

    [Range(0, 2)]
    public float _brushSize;
    [Range(0, 1)]
    public float _brushStrength;
    // Use this for initialization
    void Start()
    {
        _layerMask = LayerMask.GetMask("GROUND");
        _drawMaterial = new Material(_drawShader);

        myMaterial = _terrain.GetComponent<MeshRenderer>().material;
        myMaterial.SetTexture("_Splat", _splatmap = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat));
        

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _wheel.Length; i++)
        {
            if (Physics.Raycast(_wheel[i].position, -Vector3.up, out _groundHit, 1f, _layerMask))
            {
                _drawMaterial.SetVector("_Coordinate", new Vector4(_groundHit.textureCoord.x, _groundHit.textureCoord.y, 0, 0));
                _drawMaterial.SetFloat("_Strength", _brushStrength);
                _drawMaterial.SetFloat("_Size", _brushSize);
                RenderTexture temp = RenderTexture.GetTemporary(_splatmap.width, _splatmap.height, 0, RenderTextureFormat.ARGBFloat);
                Graphics.Blit(_splatmap, temp);
                Graphics.Blit(temp, _splatmap, _drawMaterial);
                RenderTexture.ReleaseTemporary(temp);
            }
        }

    }
}
