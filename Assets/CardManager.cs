using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Transform parent;
    internal int range;
    internal int damage;
    private TextMeshProUGUI damageText;
    private TextMeshProUGUI rangeText;

    public void Start()
    {
        parent = GameObject.Find("Canvas/ScrollView/Viewport/Hand").transform;
        transform.SetParent(parent);
    }
    internal void Initialize(int _range, int _damage)
    {
        range = _range;
        damage = _damage;
        damageText = transform.Find("DamageValue").GetComponent<TextMeshProUGUI>();
        rangeText = transform.Find("RangeValue").GetComponent<TextMeshProUGUI>();
        damageText.text = damage.ToString();
        rangeText.text = range.ToString();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.i.dragging == null)
        {
            GameManager.i.dragging = this;
            transform.SetParent(null);
            GameManager.i.HighlightCardRange();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameManager.i.dragging != null)
        {
            if (GameManager.i.spellTarget != null && GameManager.i.currentCardRange.Select(x=>x.currentNode).Contains(GameManager.i.spellTarget.currentNode))
            {
                StartCoroutine(GameManager.i.CastSpell());
            }
            else
            {
                transform.SetParent(parent);
                GameManager.i.FinishCastingSpell();
            }
        }
    }
}
