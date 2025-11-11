using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeObject : ActionObjectBase
{
    [SerializeField] private Material Material;
    [SerializeField] private Color ChangeColor = Color.red;
    [SerializeField] private Color DefaultColor = Color.white;
    [SerializeField] private ActionObjectFinder ActionObjectFinder;


    public override void Action()
    {
        if (GetActionable() == false)
        {
            return;
        }

        Debug.Log("ColorChangeObject Action " + gameObject.name);
    }


    void Start()
    {
        if (ActionObjectFinder == null)
        {
            Debug.LogError("ActionObjectFinder‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ " + gameObject.name);
        }

        if (Material == null)
        {
            Material = GetComponent<Renderer>().material;
        }
        Material.color = DefaultColor;
    }


    protected override void DoFixedUpdate()
    {
        if (ActionObjectFinder == null)
        {
            return;
        }

        GameObject actionObject = ActionObjectFinder.GetActionObjectInCenterView<ColorChangeObject>();
        bool isFound = false;

        if (actionObject == gameObject)
        {
            Material.color = ChangeColor;
            SetActionable(true);
            isFound = true;
        }

        if (isFound == false)
        {
            Material.color = DefaultColor;
            SetActionable(false);
        }
    }
}
