using UnityEngine;
using Game.Character;

public class HealthBarController : MonoBehaviour
{
    private Status status;
    private Transform barRoot;
    private Transform fillTransform;
    private SpriteRenderer fillRenderer;
    private SpriteRenderer bgRenderer;

    private static Sprite whitePixelSprite;

    public void Init(Status targetStatus)
    {
        this.status = targetStatus;
        if (status != null)
        {
            status.OnHpChanged += OnHpChanged;
            CreateHealthBarVisuals();
            OnHpChanged(status.CurHp, status.MaxHp);
        }
    }

    private void CreateHealthBarVisuals()
    {
        // 创建一个简单的1x1白色纹理Sprite
        if (whitePixelSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            // 关键修复：Pivot设在中心 (0.5, 0.5)，这样缩放和旋转都是以中心为基准
            whitePixelSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        // 根节点（用来整体移动位置）
        GameObject rootObj = new GameObject("HealthBarRoot");
        barRoot = rootObj.transform;
        // 关键改动：不再作为子物体挂在角色下，而是放在世界空间中独立存在
        // 这样角色的旋转和缩放完全不会影响到血条
        barRoot.SetParent(null); 
        
        // 背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(barRoot, false);
        bgObj.transform.localPosition = Vector3.zero;
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = whitePixelSprite;
        bgRenderer.color = new Color(0, 0, 0, 0.7f); // 半透明黑
        
        // 关键修复：设置更高的排序层级，防止被地牢背景遮挡
        bgRenderer.sortingLayerName = "Default"; 
        bgRenderer.sortingOrder = 999; // 极大值
        
        bgObj.transform.localScale = new Vector3(1f, 0.15f, 1f); // 宽1，高0.15

        // 血条填充
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barRoot, false);
        // 背景宽1，Pivot在中心，所以左边缘在 -0.5
        fillObj.transform.localPosition = new Vector3(-0.5f, 0, 0); 
        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = whitePixelSprite;
        fillRenderer.color = Color.green;
        
        // 关键修复：填充层级更高
        fillRenderer.sortingLayerName = "Default";
        fillRenderer.sortingOrder = 1000; // 比背景高一层
        
        fillTransform = fillObj.transform;
        
        // 因为填充Pivot改成了中心(0.5)，所以要改一下Pivot模式或者调整位置
        // 为了方便，我们这里重新生成一个Pivot在左边缘(0, 0.5)的Sprite专门给Fill用
        Texture2D texFill = new Texture2D(1, 1);
        texFill.SetPixel(0, 0, Color.white);
        texFill.Apply();
        Sprite fillSprite = Sprite.Create(texFill, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        fillRenderer.sprite = fillSprite;

        fillTransform.localScale = new Vector3(1f, 0.15f, 1f);
    }

    private void LateUpdate()
    {
        // 检查目标对象(角色)是否还存在
        if (transform == null || status == null || !status.Alive)
        {
            if (barRoot != null) Destroy(barRoot.gameObject);
            return;
        }

        if (barRoot != null)
        {
            // 手动跟随：每帧把血条位置设置到角色头顶
            // 这种方式最稳，完全不受角色旋转、缩放、父子层级的影响
            Vector3 targetPos = transform.position + new Vector3(0, 0.8f, 0);
            barRoot.position = targetPos;
            barRoot.rotation = Quaternion.identity; // 永远保持水平
            barRoot.localScale = Vector3.one; // 永远保持原始大小
        }
    }

    private void OnDestroy()
    {
        if (status != null)
        {
            status.OnHpChanged -= OnHpChanged;
        }
        // 确保销毁时顺便把血条也删了
        if (barRoot != null)
        {
            Destroy(barRoot.gameObject);
        }
    }

    private void OnHpChanged(int cur, int max)
    {
        if (fillTransform == null) return;

        float pct = Mathf.Clamp01((float)cur / max);
        fillTransform.localScale = new Vector3(pct, 0.15f, 1f);

        if (pct > 0.6f)
        {
            fillRenderer.color = Color.green;
        }
        else if (pct > 0.3f)
        {
            fillRenderer.color = Color.yellow;
        }
        else
        {
            fillRenderer.color = Color.red;
        }
    }
}
