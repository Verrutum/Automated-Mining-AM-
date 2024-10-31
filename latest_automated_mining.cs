List<IMyPistonBase> retractablePistons = new List<IMyPistonBase>();
List<IMyPistonBase> horizontalPistons = new List<IMyPistonBase>();
List<IMyPistonBase> verticalPistons = new List<IMyPistonBase>();

enum State
{
    START,
    IDLE,
    WORKING,
    RESET_HORIZONTAL,
    MOVE_VERTICAL,
    JOB_COMPLETED,
}

State currentState = State.START;
int currentPistonIndex = 0;
int cycleCount = 0;
int verticalCycleCount = 0;

// Constants
const float stepDistance = 3.35f;
const int totalCycles = 3;

public Program()
{
    InitializePistons();
    SetPistonsToStartingPosition();
    Echo($"Initial State: {currentState}");
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    //TODO: Need to hook this information up to a LCD Screen.
}

public void Main(string argument, UpdateType updateSource)
{

}

void startMiningJob()
{
    Echo($"Current State: {currentState}");
    if (!IsAnyPistonMoving())
    {
        switch (currentState)
        {
            case State.START:
                SetPistonsToStartingPosition();
                currentState = State.IDLE;
                break;

            case State.IDLE:
            // TODO Rewrite the IDLE state
                if (cycleCount < totalCycles)
                {
                    currentState = State.WORKING;
                }
                else
                {
                    currentState = State.MOVE_VERTICAL;
                }
                break;

            case State.WORKING:
                if (currentPistonIndex < retractablePistons.Count)
                {
                    AdjustPiston(retractablePistons[currentPistonIndex], stepDistance, extend: false);
                }
                else if (currentPistonIndex < retractablePistons.Count + horizontalPistons.Count)
                {
                    AdjustPiston(horizontalPistons[currentPistonIndex - retractablePistons.Count], stepDistance, extend: true);
                }

                // TODO: Do we need cycles if we're moving 1 piston at a time each time.
                cycleCount++;

                if (cycleCount >= totalCycles)
                {
                    currentPistonIndex++;
                    cycleCount = 0;
                }


                if (currentPistonIndex > retractablePistons.Count + horizontalPistons.Count)
                {
                    currentPistonIndex = 0;
                    currentState = State.RESET_HORIZONTAL;
                }
                break;

            case State.RESET_HORIZONTAL:
                ResetHorizontalPistonsToStart();
                currentState = State.MOVE_VERTICAL;
                cycleCount = 0;
                currentPistonIndex = 0;
                break;

            case State.MOVE_VERTICAL:
                if (verticalCycleCount < totalCycles)
                {
                    MoveVerticalPistons();
                    verticalCycleCount++;
                    currentState = State.IDLE;
                }
                else
                {
                    currentState = State.JOB_COMPLETED;
                }
                break;

            case State.JOB_COMPLETED:
                SetPistonsToStartingPosition();
                Echo("JOB COMPLETED");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                break;
        }
    }

}

void InitializePistons()
{
    var allPistons = new List<IMyPistonBase>();
    GridTerminalSystem.GetBlocksOfType(allPistons);

    foreach (var piston in allPistons)
    {
        if (piston.CustomName.StartsWith("Piston PH"))
        {
            retractablePistons.Add(piston);
        }
        else if (piston.CustomName.StartsWith("Piston H"))
        {
            horizontalPistons.Add(piston);
        }
        else if (piston.CustomName.StartsWith("Piston V"))
        {
            verticalPistons.Add(piston);
        }
    }

    Echo($"Initialized Pistons: Retractable: {retractablePistons.Count}, Horizontal: {horizontalPistons.Count}, Vertical: {verticalPistons.Count}");
}

void SetPistonsToStartingPosition()
{
    // Reset retractable pistons
    foreach (var piston in retractablePistons)
    {
        SetPistonPosition(piston, 10f, 10f, extend: true);
    }
    // Reset horizontal pistons
    foreach (var piston in horizontalPistons)
    {
        SetPistonPosition(piston, 0, 0, extend: false);
    }
    // Reset vertical pistons
    foreach (var piston in verticalPistons)
    {
        SetPistonPosition(piston, 0, 0, extend: false);
    }
}

void ResetHorizontalPistonsToStart()
{
    foreach (var piston in retractablePistons)
    {
        SetPistonPosition(piston, 10f, 10f, extend: true);
    }
    foreach (var piston in horizontalPistons)
    {
        SetPistonPosition(piston, 0, 0, extend: false);
    }
}

void SetPistonPosition(IMyPistonBase piston, float minLimit, float maxLimit, bool extend)
{
    if (piston != null)
    {
        piston.MinLimit = minLimit;
        piston.MaxLimit = maxLimit;

        if (extend)
        {
            piston.Extend();
        }
        else
        {
            piston.Retract();
        }
    }
}

bool IsAnyPistonMoving()
{
    foreach (var piston in retractablePistons)
    {
        if (piston.Status.ToString() == "Retracting" || piston.Status.ToString() == "Extending")
        {
            return true;
        }
    }
    foreach (var piston in horizontalPistons)
    {
        if (piston.Status.ToString() == "Retracting" || piston.Status.ToString() == "Extending")
        {
            return true;
        }
    }
    foreach (var piston in verticalPistons)
    {
        if (piston.Status.ToString() == "Retracting" || piston.Status.ToString() == "Extending")
        {
            return true;
        }
    }
    return false;
}

void AdjustPiston(IMyPistonBase piston, float amount, bool extend)
{
    if (piston != null)
    {
        if (extend)
        {
            piston.MaxLimit += amount;
            piston.Extend();
        }
        else
        {
            piston.MinLimit -= amount;
            piston.Retract();
        }
    }
}

void MoveVerticalPistons()
{
    foreach (var piston in verticalPistons)
    {
        AdjustPiston(piston, stepDistance, extend: true);
    }
}
