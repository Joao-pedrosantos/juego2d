using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;

    public Canvas gameCanvas;

    private void Awake()
    {
        gameCanvas = FindObjectOfType<Canvas>();
        CharacterEvents.characterDamaged.AddListener(CharacterTookDamage);
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged.AddListener(CharacterTookDamage);
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged.RemoveListener(CharacterTookDamage);
    }

    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform).GetComponent<TMP_Text>();

        tmpText.text = damageReceived.ToString();
    }
}
