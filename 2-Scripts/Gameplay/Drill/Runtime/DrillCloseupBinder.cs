using UnityEngine;
using Zenject;

public class DrillCloseupBinder : MonoBehaviour
{
    [SerializeField] private CloseupInteractionV2 _drillCloseup;

    [Inject]
    private void Construct(DrillCloseupControllerService service)
    {
        service.RegisterCloseup(_drillCloseup);
    }
}
