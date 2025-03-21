using UnityEngine;

public class PuckViewModel
{
    public Vector2 position;
    public float size;
    public int growthLevel;
    
    private PuckModel puckModel;
    
    public void SetModel(PuckModel model)
    {
        puckModel = model;
    }

    public void UpdateDisplayData()
    {
        if (puckModel != null)
        {
            position = puckModel.Position;
            size = puckModel.Size;
            growthLevel = puckModel.GrowthLevel;
        }
    }
}