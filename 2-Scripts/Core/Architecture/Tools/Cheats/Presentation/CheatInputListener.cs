using UnityEngine;
using Zenject;

public class CheatInputListener : MonoBehaviour
{
    private ICheatService _cheats;

    [Inject]
    private void Construct(ICheatService cheats)
    {
        _cheats = cheats;
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F9))
        {
            _cheats.GoToBreakingPoint();
        }
#endif
    }
}
