using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Диктофон: каждое нажатие E включает следующую запись.
// Если звука записи ещё нет — показываются только субтитры.
// Вешается на диктофон (у него нужен Collider).
public class Dictaphone : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class Record
    {
        [Tooltip("Голос / звук записи (можно пусто — тогда только текст)")]
        public AudioClip clip;
        [Tooltip("Субтитры. Можно несколько строк — каждая покажется по очереди")]
        [TextArea(2, 5)] public string[] lines;
        [Tooltip("Сколько секунд показывать каждую строку, если нет звука")]
        public float secondsPerLine = 4f;
    }

    public Record[] records;
    public AudioSource audioSource;

    [Tooltip("Что произойдёт после последней записи")]
    public UnityEvent onAllPlayed;

    int next;
    bool playing;

    public string GetPrompt()
    {
        if (playing) return "";
        if (records == null || next >= records.Length) return "";
        return $"Включить запись {next + 1} / {records.Length}";
    }

    public void Interact(PlayerInteraction player)
    {
        if (playing || records == null || next >= records.Length) return;
        StartCoroutine(Play(records[next]));
        next++;
    }

    IEnumerator Play(Record r)
    {
        playing = true;
        int n = r.lines != null ? r.lines.Length : 0;
        float total = (r.clip != null) ? r.clip.length : r.secondsPerLine * Mathf.Max(1, n);
        if (audioSource != null && r.clip != null) audioSource.PlayOneShot(r.clip);

        float perLine = n > 0 ? total / n : total;
        for (int i = 0; i < n; i++)
        {
            if (GameManager.Instance != null) GameManager.Instance.ShowMessage(r.lines[i], perLine);
            yield return new WaitForSeconds(perLine);
        }
        if (n == 0) yield return new WaitForSeconds(total);

        playing = false;
        if (next >= records.Length) onAllPlayed.Invoke();
    }
}
