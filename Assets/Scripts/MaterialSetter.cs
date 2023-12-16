using UnityEngine;

//[RequireComponent(typeof(MeshRenderer))]
public class MaterialSetter : MonoBehaviour
{
   private MeshRenderer Renderer;

    private MeshRenderer meshRenderer
    {
        get
        {
            if(Renderer == null)
                Renderer = GetComponentInChildren<MeshRenderer>();
            return Renderer;
        }
    }

    public void SetSingleMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }
}
