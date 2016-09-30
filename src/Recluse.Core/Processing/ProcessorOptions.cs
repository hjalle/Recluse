namespace Recluse.Core.Processing
{
    public class ProcessorOptions
    {
        public int MaxConcurrency { get; set; } = 10;
        public int LimitBeforeNewFetch { get; set; } = 200;
        public int MaxAmountToFetch { get; set; } = 200;
    }
}
