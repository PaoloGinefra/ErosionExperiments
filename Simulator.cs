using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator
{

    //0-1 no change in direction - follows strictly the negative gradient
    public float p_inertia = 0.5f;

    //The amount of sediment a drop can carry
    public float p_capacity = 5;

    //The deposition speed 0-1 
    public float p_deposition = 0.3f;

    //The erosion speed 0-1
    public float p_erosion = 0.2f;

    //How fast a drop evaporates 0-1
    public float p_evaporation = 0.05f;

    //Erosion Radius
    public float p_radius = 4;
    public float p_minSlope = 0.01f;
    public float p_gravity = 1f;
    public float p_maxStep = 100;
    public Simulator(float p_inertia = 0.5f, float p_capacity = 5, float p_deposition = 0.3f,
                    float p_erosion = 0.2f, float p_evaporation = 0.05f, float p_radius = 4,
                    float p_minSlope = 0.01f, float p_gravity = 1f, float p_maxStep = 100)
    {
        this.p_inertia = p_inertia; this.p_capacity = p_capacity; this.p_deposition = p_deposition;
        this.p_erosion = p_erosion; this.p_evaporation = p_erosion; this.p_radius = p_radius;
        this.p_minSlope = p_minSlope; this.p_gravity = p_gravity; this.p_maxStep = p_maxStep;
    }

    public float heightAt(List<float> map, int gridSize, Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        float u = pos.x - x;
        float v = pos.y - y;

        int i00 = x + gridSize * y;
        int i10 = i00 + 1;
        int i01 = i00 + gridSize;
        int i11 = i00 + 1 + gridSize;

        float r1 = Mathf.Lerp(map[i00], map[i10], u);
        float r2 = Mathf.Lerp(map[i01], map[i11], u);

        return Mathf.Lerp(r1, r2, v);
    }

    public Vector2 gradientAt(List<float> map, int gridSize, Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        float u = pos.x - x;
        float v = pos.y - y;

        int i00 = x + gridSize * y;
        int i10 = i00 + 1;
        int i01 = i00 + gridSize;
        int i11 = i00 + 1 + gridSize;

        return new Vector2(
            (1 - v) * (map[i10] - map[i00]) + v * (map[i11] - map[i01]),
            (1 - u) * (map[i01] - map[i00]) + u * (map[i11] - map[i10])
        ).normalized;
    }

    void updateDir(List<float> map, int gridSize, Drop drop)
    {
        Vector2 g = this.gradientAt(map, gridSize, drop.pos);
        drop.dir = Vector2.Lerp(drop.dir, g, this.p_inertia).normalized;
    }

    //Updates the drop's postion and returns the height difference of the step
    void updateDrop(Drop drop, List<float> map, int gridSize)
    {
        //Updates the drop's direction
        this.updateDir(map, gridSize, drop);

        //Updates the drop's position
        Vector2 posOld = drop.pos;
        float hOld = this.heightAt(map, gridSize, posOld);

        drop.pos += drop.dir;

        if (!this.isValid(drop, gridSize))
            return;

        float hNew = this.heightAt(map, gridSize, drop.pos);

        float hDif = hNew - hOld;

        //EROSION/DEPOSITION
        //if it moved uphill it fills the pit as well as it can
        if (hDif > 0)
        {
            float depositAmount;

            if (drop.sediment <= hDif)
            {
                depositAmount = drop.sediment;
            }
            else
            {
                depositAmount = hDif;
            }

            this.DepositAt(map, gridSize, posOld, depositAmount);
            drop.sediment -= depositAmount;
        }
        //if it moved downhill
        else
        {
            //computes the new carry capacity
            float c = Mathf.Max(-hDif, this.p_minSlope) * drop.vel * drop.water * this.p_capacity;

            if (drop.sediment > c)
            {
                float depositAmount = (drop.sediment - c) * this.p_deposition;
                this.DepositAt(map, gridSize, drop.pos, depositAmount);
                drop.sediment -= depositAmount;
            }
            else
            {
                float erosionAmount = Mathf.Min((c - drop.sediment) * this.p_erosion, -hDif);
                this.ErodeAt(map, gridSize, posOld, erosionAmount);
                drop.sediment += erosionAmount;
            }
        }

        //Updates velocity
        drop.vel = Mathf.Sqrt(drop.vel * drop.vel + hDif * this.p_gravity);

        //Updates water
        drop.water = drop.water * (1 - p_evaporation);
    }

    public void ErodeAt(List<float> map, int gridSize, Vector2 pos, float amount)
    {
        //Debug.Log("Erode: " + amount.ToString());
        Dictionary<int, float> weights = new Dictionary<int, float>();

        //Compute all the weights
        int minY = Mathf.Max(0, Mathf.CeilToInt(pos.y - this.p_radius));
        int maxY = Mathf.Min(gridSize, Mathf.FloorToInt(pos.y + this.p_radius) + 1);

        int minX = Mathf.Max(0, Mathf.CeilToInt(pos.x - this.p_radius));
        int maxX = Mathf.Min(gridSize, Mathf.FloorToInt(pos.x + this.p_radius) + 1);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                float dist = Vector2.Distance(pos, new Vector2(x, y));

                if (dist <= this.p_radius)
                {
                    weights.Add(x + y * gridSize, this.p_radius - dist);
                }
            }
        }

        //Find the overall sum of the weights
        float weightSum = 0;
        foreach (var (index, weight) in weights)
        {
            weightSum += weight;
        }

        //Here an erosion factor could be implemented
        foreach (var (index, weight) in weights)
        {
            map[index] -= amount * weight;
        }
    }

    public void DepositAt(List<float> map, int gridSize, Vector2 pos, float amount)
    {
        //Debug.Log("Deposit: " + amount.ToString());
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        float u = pos.x - x;
        float v = pos.y - y;

        int i00 = x + gridSize * y;
        int i10 = i00 + 1;
        int i01 = i00 + gridSize;
        int i11 = i00 + 1 + gridSize;

        //Interpolates on the Y axis
        float amDown = (1 - v) * amount;
        float amUp = v * amount;

        //Interpolates on the x axis and apply
        map[i00] += (1 - u) * amDown;
        map[i10] += u * amDown;

        map[i01] += (1 - u) * amUp;
        map[i11] += u * amUp;
    }

    public bool isValid(Drop drop, int gridSize)
    {
        return drop.pos.x >= 0 && drop.pos.x <= gridSize - 1 && drop.pos.y >= 0 && drop.pos.y <= gridSize - 1;
    }

    public void simulateDrop(List<float> map, int gridSize)
    {
        Drop drop = new Drop(gridSize);
        for (int step = 0; step < this.p_maxStep && this.isValid(drop, gridSize); step++)
        {
            this.updateDrop(drop, map, gridSize);
        }
    }
}
