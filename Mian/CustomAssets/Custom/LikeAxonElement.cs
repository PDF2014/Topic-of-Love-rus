using UnityEngine;
using UnityEngine.UI;

namespace Topic_of_Love.Mian.CustomAssets.Custom;

// Code taken from the main game refactored for our purposes
public class LikeAxonElement : MonoBehaviour
{
    public Image image;
    public LikeNeuronElement neuron_1;
    public LikeNeuronElement neuron_2;
    public float mod_light = 1f;
    public bool axon_center;

    public void update()
    {
        this.mod_light -= Time.deltaTime * 2f;
        this.mod_light = Mathf.Max(0.0f, this.mod_light);
    }

    public void clear() => this.axon_center = false;
}