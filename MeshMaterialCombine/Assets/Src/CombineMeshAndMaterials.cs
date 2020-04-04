using UnityEngine;
using System.Collections;

public class CombineMeshAndMaterials : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {


        //CombineMeshAndMaterial();
        //CombineMeshOnly();
    }

    void CombineMeshOnly()
    {
        // MeshRenderer
        MeshRenderer[] mrChildren = GetComponentsInChildren<MeshRenderer>();
        print("mrChildren.Length = " + mrChildren.Length);
        Material[] materials = new Material[mrChildren.Length];

        for(int i = 0; i < mrChildren.Length; i++)
        {
            materials[i] = mrChildren[i].sharedMaterial;
        }
        // MeshFilter
        MeshFilter[] mfChildren = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[mfChildren.Length];

        MeshRenderer mrSelf = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mfSelf = gameObject.AddComponent<MeshFilter>();

        //合并mesh，除去自身和子物体所有的meshFilter组件
        for ( int i = 0; i< mfChildren.Length; i++)
        {
            combine[i].mesh = mfChildren[i].sharedMesh;
            // 矩阵（Matrix）自身空间坐标的点转换成世界空间坐标的点
            combine[i].transform = mfChildren[i].transform.localToWorldMatrix;
            mfChildren[i].gameObject.SetActive(false);
        }
        //重新生成mesh
        mfSelf.mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, false);
        transform.gameObject.SetActive(true);
        mrSelf.sharedMaterials = materials;
    }

    void CombineMeshWithTextCombine()
    {

    }

    void CombineMeshAndMaterial()
    {
        //MeshFilter
        MeshFilter[] mfChildren = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[mfChildren.Length];
        // MeshRenderer
        MeshRenderer[] mrChildren = GetComponentsInChildren<MeshRenderer>();
        Material[] materials = new Material[mrChildren.Length];

        MeshRenderer mrSelf = gameObject.AddComponent<MeshRenderer>();
        MeshFilter mfSelf = gameObject.AddComponent<MeshFilter>();

        // textures存放所有该合并的texture2D
        Texture2D[] textures = new Texture2D[mrChildren.Length];
        for (int i = 0; i < mrChildren.Length; i++)
        {
            materials[i] = mrChildren[i].sharedMaterial;
            //Texture2D tx = materials[i].GetTexture("_MainTex") as Texture2D;
            Texture2D tx = materials[i].mainTexture as Texture2D ;

            Texture2D tx2D = new Texture2D(tx.width, tx.height, TextureFormat.ARGB32, false);
            //Color[] The array of pixels in the texture that have been selected.
            tx2D.SetPixels(tx.GetPixels(0, 0, tx.width, tx.height));
            tx2D.Apply();
            textures[i] = tx2D;
        }
        // 创建新材质，并赋予给合并gameobj的MeshRenderer
        Material materialNew = new Material(materials[0].shader);
        materialNew.CopyPropertiesFromMaterial(materials[0]);
        mrSelf.sharedMaterial = materialNew; // 仅创建shader，还有待合并的texture
        // Texture2D大小可以计算
        Texture2D texture = new Texture2D(1024, 1024);
        materialNew.SetTexture("_MainTex", texture);
        Rect[] rects = texture.PackTextures(textures, 10, 1024);
        // 合并texture之后，刷新uv信息，然后合并mesh
        for (int i = 0; i < mfChildren.Length; i++)
        {
            if (mfChildren[i].transform == transform)
            {
                continue;
            }
            Rect rect = rects[i];

            Mesh meshCombine = mfChildren[i].mesh;
            Vector2[] uvs = new Vector2[meshCombine.uv.Length];
            //把网格的uv根据贴图的rect刷一遍
            Debug.LogError(uvs.Length);
            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j].x = rect.x + meshCombine.uv[j].x * rect.width;
                print("银赫 j =" + j+ " + uvs[j].x"+ uvs[j].x);
                uvs[j].y = rect.y + meshCombine.uv[j].y * rect.height;
                print("银赫 j =" + j + " + uvs[j].y" + uvs[j].y);
            }
            //Texture coordinates have to be modified or created externally from the Mesh.
            meshCombine.uv = uvs;
            combine[i].mesh = meshCombine;
            combine[i].transform = mfChildren[i].transform.localToWorldMatrix;
            mfChildren[i].gameObject.SetActive(false);
        }

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine, true, true);//合并网格
        mfSelf.mesh = newMesh;
    }

}
