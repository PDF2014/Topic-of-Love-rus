using Topic_of_Love.Mian.CustomAssets.Custom.meta.orientations;

namespace Topic_of_Love.Mian.CustomAssets.Custom;

public class ExtendedToolTipData : TooltipData
{
    public LikeNeuronElement like_neuron;
    public Orientation orientation;

    public new void Dispose()
    {
        base.Dispose();
        orientation = null;
        like_neuron = null;
    }
}