namespace Topic_of_Love.Mian.CustomAssets.Custom;

public class ExtendedToolTipData : TooltipData
{
    public LikeNeuronElement like_neuron;

    public new void Dispose()
    {
        base.Dispose();
        like_neuron = null;
    }
}