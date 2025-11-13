using UnityEngine;

public class ActionTest : MonoBehaviour
{
    [SerializeField] private ActionObjectFinder ActionObjectFinder;

    void Start()
    {
        if (ActionObjectFinder == null)
        {
            Debug.LogError("ActionObjectFinderがアタッチされていません " + gameObject.name);
        }
    }

    void Update()
    {
        if (ActionObjectFinder == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //List<GameObject> actionObject = _actionObjectFinder.GetActionObjectsInView<ColorChangeObject>();

            //foreach (var obj in actionObject)
            //{
            //    ColorChangeObject action = obj.GetComponent<ColorChangeObject>();
            //    if (action != null)
            //    {
            //        action.Action();
            //    }
            //}
        }
    }
}
