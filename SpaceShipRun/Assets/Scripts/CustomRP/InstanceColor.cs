using System;
using UnityEngine;


public class InstanceColor : MonoBehaviour
{
    [SerializeField] Color color = Color.white;
    private static MaterialPropertyBlock propertyBlock;
    
    private static int colorID = Shader.PropertyToID("_Color");
    
    private void OnValidate()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor(colorID, color);
        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }

    private void Awake()
    {
        OnValidate();
    }
}
