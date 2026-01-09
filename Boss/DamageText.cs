using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    private Text textComponent;
    private float lifetime = 1f;
    private float elapsedTime = 0f;
    private Vector3 moveDirection = new Vector3(0, 2f, 0);
    private float moveSpeed = 1f;
    
    private void Start()
    {
        textComponent = GetComponent<Text>();
        if (textComponent == null)
        {
            textComponent = gameObject.AddComponent<Text>();
            SetupDefaultText();
        }
    }
    
    private void Update()
    {
        if (textComponent == null) return;
        
        // 移动文本
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // 计算透明度
        elapsedTime += Time.deltaTime;
        float alpha = 1f - (elapsedTime / lifetime);
        
        // 设置透明度
        Color color = textComponent.color;
        color.a = alpha;
        textComponent.color = color;
        
        // 销毁文本
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetDamage(int damage)
    {
        if (textComponent != null)
        {
            textComponent.text = damage.ToString();
        }
    }
    
    private void SetupDefaultText()
    {
        // 设置默认文本样式
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 24;
        textComponent.color = Color.red;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.raycastTarget = false;
        
        // 添加Canvas Renderer
        if (GetComponent<CanvasRenderer>() == null)
        {
            gameObject.AddComponent<CanvasRenderer>();
        }
    }
}