using UnityEngine;

/// <summary>
/// 피해 시스템 및 데미지 표시
/// 데미지 플로팅 텍스트와 효과를 담당합니다
/// </summary>
public class DamageDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject _damageTextPrefab;

    [SerializeField]
    private float _floatingSpeed = 2f;

    [SerializeField]
    private float _displayDuration = 1f;

    /// <summary>
    /// 데미지 표시
    /// </summary>
    public void ShowDamage(int damage, Vector3 position, bool isCritical = false)
    {
        if (_damageTextPrefab == null)
            return;

        GameObject damageText = Instantiate(_damageTextPrefab, position, Quaternion.identity);
        TextMesh textMesh = damageText.GetComponent<TextMesh>();

        if (textMesh != null)
        {
            // 치명타면 색상 변경
            if (isCritical)
            {
                textMesh.text = $"<color=yellow>{damage}!</color>";
            }
            else
            {
                textMesh.text = damage.ToString();
            }
        }

        // 위로 떠오르며 사라지는 애니메이션
        StartCoroutine(FloatingTextCoroutine(damageText.transform, _displayDuration));
    }

    /// <summary>
    /// 데미지 텍스트 플로팅 애니메이션
    /// </summary>
    private System.Collections.IEnumerator FloatingTextCoroutine(Transform textTransform, float duration)
    {
        Vector3 startPosition = textTransform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // 위로 이동
            Vector3 newPosition = startPosition + Vector3.up * _floatingSpeed * elapsedTime;
            textTransform.position = newPosition;

            // 알파값 감소 (페이드 아웃)
            if (textTransform.GetComponent<TextMesh>() != null)
            {
                Color color = textTransform.GetComponent<TextMesh>().color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                textTransform.GetComponent<TextMesh>().color = color;
            }

            yield return null;
        }

        Destroy(textTransform.gameObject);
    }
}