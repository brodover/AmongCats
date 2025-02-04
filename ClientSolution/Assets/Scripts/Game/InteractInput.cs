using UnityEngine;

public class InteractInput : MonoBehaviour
{
    private const float _RANGE = 2f;

    public void OnInteractClick()
    {
        Interactable newTarget = null;
        var targetList = FindObjectsByType<Interactable>(FindObjectsSortMode.None);

        foreach (var target in targetList)
        {
            var distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance > _RANGE) { continue; }
            
            if (gameObject.tag == "Human")
            {
                target.CleanUp();
            }
            else if (gameObject.tag == "Cat")
            {
                target.MessUp();
            }

            break;
        }
    }
}
