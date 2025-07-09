using UnityEngine;
using UnityEngine.UI;

namespace Topic_of_Love.Mian.CustomAssets.Custom;

public class LikeNerveImpulseElement : MonoBehaviour
{
  public Image image;
  public const float SPEED_MIN = 1f;
  public const float SPEED_MAX = 3f;
  public const float SCALE_MIN = 0.6f;
  public const float SCALE_MAX = 1f;
  public float _speed_current;
  public float _move_timer;
  public LikeNeuronElement presynaptic_neuron;
  public LikeNeuronElement postsynaptic_neuron;
  public Color _color_back = Toolbox.makeColor("#26A8A8", 0.5f);
  public Color _color_front = Toolbox.makeColor("#3AFFFF", 0.7f);
  public int wave;

  public void energize(
    LikeNeuronElement pPresynapticNeuron,
    LikeNeuronElement pPostsynapticNeuron,
    int pWave)
  {
    this.transform.localPosition = pPresynapticNeuron.transform.localPosition;
    this.presynaptic_neuron = pPresynapticNeuron;
    this.postsynaptic_neuron = pPostsynapticNeuron;
    this._move_timer = 0.0f;
    this._speed_current = Randy.randomFloat(1f, 3f);
    this.wave = pWave;
  }

  public ImpulseReachResult moveTowardsNextNeuron()
  {
    if ((Object) this.postsynaptic_neuron == (Object) null)
      return ImpulseReachResult.Done;
    this._move_timer += this._speed_current * Time.deltaTime;
    this._move_timer = Mathf.Clamp01(this._move_timer);
    this.transform.localPosition = Vector3.Lerp(this.presynaptic_neuron.transform.localPosition, this.postsynaptic_neuron.transform.localPosition, this._move_timer);
    this.updateImpulseColor();
    if ((double) this._move_timer < 1.0)
      return ImpulseReachResult.Move;
    this.presynaptic_neuron = this.postsynaptic_neuron;
    this.postsynaptic_neuron = this.GetNextTargetNeuron();
    this._move_timer = 0.0f;
    --this.wave;
    return this.wave > 0 ? ImpulseReachResult.Split : ImpulseReachResult.Done;
  }

  public LikeNeuronElement GetNextTargetNeuron()
  {
    return this.presynaptic_neuron.connected_neurons.Count == 0 ? (NeuronElement) null : this.presynaptic_neuron.connected_neurons.GetRandom<NeuronElement>();
  }

  public void updateImpulseColor()
  {
    float t = Mathf.Lerp(this.presynaptic_neuron.render_depth, this.postsynaptic_neuron.render_depth, this._move_timer);
    Color color = Color.Lerp(this._color_back, this._color_front, t);
    if (this.image.color != color)
      this.image.color = color;
    float num = Mathf.Lerp(0.6f, 1f, t);
    if ((double) this.transform.localScale.x == (double) num)
      return;
    this.transform.localScale = new Vector3(num, num, num);
  }
}