using UnityEngine;

public class ReplayCharacter : MonoBehaviour
{
    PlayerController controller;
    ReplayInputProvider replayInput;

    public void Initialize(ReplayData data)
    {
        controller = GetComponent<PlayerController>();
        replayInput = new ReplayInputProvider(data);

        controller.SetInput(replayInput);
    }

    void Update()
    {
        replayInput.Update(Time.deltaTime);
    }
}
