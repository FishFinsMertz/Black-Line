using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RotatePipe : MonoBehaviour, ISaveable
{
    [SerializeField] private string saveID;
    [SerializeField] private Transform pipeVisual;
    [SerializeField] private List<RotationState> rotations = new List<RotationState>();
    [SerializeField] private int startIndex = 0;
    [SerializeField] private float rotationDuration = 0.5f;

    [Header("Temperature Bounds")]
    [SerializeField] private float lowTemperature = 50f;
    [SerializeField] private float highTemperature = 100f;

    [Header("Heat Sources (Tilemaps)")]
    [SerializeField] private List<TileMapThermal> sourceTilemaps = new List<TileMapThermal>();

    [Header("Destination Tilemaps")]
    [SerializeField] private List<TileMapThermal> destinationTilemaps = new List<TileMapThermal>();

    private int currentIndex = 0;
    private bool isRotating = false;
    private ThermalObject pipeThermal;
    private bool isActive = false;
    private float currentTemperature;
    private List<SprayPipe> currentlyActiveSprayPipes = new List<SprayPipe>();

    [System.Serializable]
    public class RotationState
    {
        public float angle;
        public bool sourceConnected = false;
        public bool destinationActive = false;
        public List<SprayPipe> activeSprayPipes = new List<SprayPipe>();
    }

    private void Start()
    {
        currentTemperature = lowTemperature;
        SaveManager.Instance?.Register(this);
        pipeThermal = pipeVisual.GetComponent<ThermalObject>();
        currentIndex = Mathf.Clamp(startIndex, 0, rotations.Count - 1);
        ApplyStateInstant(currentIndex);
        Invoke(nameof(UpdateActiveState), 0.1f);
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(this);
    }

    private void Update()
    {
        if (isRotating) return;

        bool currentlyActive = IsSupplied();
        float targetTemp = ComputeTargetTemperature();

        if (currentlyActive != isActive || !Mathf.Approximately(targetTemp, currentTemperature))
        {
            ApplyState(currentIndex);
        }
        else
        {
            UpdateTemperature();
        }
    }

    private float ComputeTargetTemperature()
    {
        if (!isActive || !rotations[currentIndex].sourceConnected)
            return lowTemperature;

        if (sourceTilemaps.Count > 0)
        {
            float totalTemp = 0f;
            int validCount = 0;
            foreach (var tm in sourceTilemaps)
            {
                if (tm != null)
                {
                    totalTemp += tm.GetCurrentTemperature();
                    validCount++;
                }
            }
            if (validCount > 0)
            {
                float avg = totalTemp / validCount;
                return Mathf.Clamp(avg, lowTemperature, highTemperature);
            }
        }
        else
        {
            return highTemperature;
        }
        return lowTemperature;
    }

    private void UpdateTemperature()
    {
        if (pipeThermal == null) return;
        float target = ComputeTargetTemperature();
        currentTemperature = target;
        pipeThermal.SetBaseTemperature(target);
    }

    public void RotateNext()
    {
        if (isRotating) return;
        if (rotations.Count <= 1) return;

        int nextIndex = (currentIndex + 1) % rotations.Count;
        StartCoroutine(RotateToState(nextIndex));
    }

    private System.Collections.IEnumerator RotateToState(int targetIndex)
    {
        isRotating = true;

        foreach (var spray in currentlyActiveSprayPipes)
            if (spray != null) spray.Deactivate();
        currentlyActiveSprayPipes.Clear();

        RotationState currentState = rotations[currentIndex];
        RotationState targetState = rotations[targetIndex];

        bool targetIsActive = IsSupplied() && targetState.sourceConnected;
        float targetTemp = lowTemperature;
        if (targetIsActive)
        {
            if (sourceTilemaps.Count > 0)
            {
                float totalTemp = 0f;
                int validCount = 0;
                foreach (var tm in sourceTilemaps)
                {
                    if (tm != null)
                    {
                        totalTemp += tm.GetCurrentTemperature();
                        validCount++;
                    }
                }
                if (validCount > 0)
                    targetTemp = Mathf.Clamp(totalTemp / validCount, lowTemperature, highTemperature);
            }
            else
            {
                targetTemp = highTemperature;
            }
        }

        // Immediate cooling if temperature drops
        if (targetTemp < currentTemperature)
        {
            if (pipeThermal != null)
                pipeThermal.SetBaseTemperature(targetTemp);
            currentTemperature = targetTemp;

            if (targetState.destinationActive)
            {
                foreach (var tm in destinationTilemaps)
                    if (tm != null) tm.SetTemperature(targetTemp);
            }
            else
            {
                foreach (var tm in destinationTilemaps)
                    if (tm != null) tm.SetTemperature(lowTemperature);
            }
        }

        // Rotate visual
        float elapsed = 0f;
        Quaternion startRot = pipeVisual.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetState.angle);

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotationDuration;
            pipeVisual.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        pipeVisual.localRotation = endRot;

        ApplyState(targetIndex);
        isRotating = false;
    }

    private void ApplyState(int index)
    {
        currentIndex = index;
        RotationState state = rotations[index];

        isActive = IsSupplied() && state.sourceConnected;

        // Update temperature
        float target = ComputeTargetTemperature();
        currentTemperature = target;
        if (pipeThermal != null)
            pipeThermal.SetBaseTemperature(target);

        // Destination tilemaps (still controlled by destinationActive)
        if (state.destinationActive && isActive)
        {
            foreach (var tm in destinationTilemaps)
                if (tm != null) tm.SetTemperature(currentTemperature);
        }
        else
        {
            foreach (var tm in destinationTilemaps)
                if (tm != null) tm.SetTemperature(lowTemperature);
        }

        currentlyActiveSprayPipes.Clear();
        if (isActive)
        {
            foreach (var spray in state.activeSprayPipes)
            {
                if (spray != null)
                {
                    spray.Activate();
                    currentlyActiveSprayPipes.Add(spray);
                }
            }
        }
        else
        {
            foreach (var spray in state.activeSprayPipes)
                if (spray != null) spray.Deactivate();
        }
    }

    private void ApplyStateInstant(int index)
    {
        currentIndex = index;
        RotationState state = rotations[index];

        pipeVisual.localRotation = Quaternion.Euler(0, 0, state.angle);
        isActive = IsSupplied() && state.sourceConnected;
        float target = ComputeTargetTemperature();
        currentTemperature = target;
        if (pipeThermal != null)
            pipeThermal.SetBaseTemperature(target);

        if (state.destinationActive && isActive)
        {
            foreach (var tm in destinationTilemaps)
                if (tm != null) tm.SetTemperature(currentTemperature);
        }
        else
        {
            foreach (var tm in destinationTilemaps)
                if (tm != null) tm.SetTemperature(lowTemperature);
        }

        currentlyActiveSprayPipes.Clear();
        if (isActive)
        {
            foreach (var spray in state.activeSprayPipes)
            {
                if (spray != null)
                {
                    spray.Activate();
                    currentlyActiveSprayPipes.Add(spray);
                }
            }
        }
        else
        {
            foreach (var spray in state.activeSprayPipes)
                if (spray != null) spray.Deactivate();
        }
    }

    private bool IsSupplied()
    {
        if (sourceTilemaps.Count == 0)
            return true;

        foreach (var tm in sourceTilemaps)
            if (tm != null && tm.GetCurrentTemperature() > 50f)
                return true;

        return false;
    }

    private void UpdateActiveState()
    {
        if (!isRotating)
            ApplyState(currentIndex);
    }

    public void RefreshState()
    {
        if (!isRotating)
            ApplyState(currentIndex);
    }

    public float GetCurrentTemperature() => currentTemperature;
    public float GetCurrentAngle() => rotations[currentIndex].angle;

    // --- ISaveable ---
    public void Save(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        if (rotations.Count <= 1) return;

        string state = rotations[currentIndex].angle.ToString();
        data.componentStates.RemoveAll(c => c.id == saveID);
        data.componentStates.Add(new ComponentState { id = saveID, state = state });
    }

    public void Load(GameData data)
    {
        if (string.IsNullOrEmpty(saveID)) return;
        ComponentState cs = data.componentStates.FirstOrDefault(c => c.id == saveID);
        if (cs != null && float.TryParse(cs.state, out float loadedAngle))
        {
            int targetIndex = rotations.FindIndex(r => Mathf.Approximately(r.angle, loadedAngle));
            if (targetIndex >= 0 && targetIndex != currentIndex)
            {
                pipeVisual.localRotation = Quaternion.Euler(0, 0, rotations[targetIndex].angle);
                ApplyState(targetIndex);
            }
        }
    }

    public void SetRotation(float angle)
    {
        int targetIndex = rotations.FindIndex(r => Mathf.Approximately(r.angle, angle));
        if (targetIndex >= 0 && targetIndex != currentIndex)
        {
            pipeVisual.localRotation = Quaternion.Euler(0, 0, rotations[targetIndex].angle);
            ApplyState(targetIndex);
        }
    }
}