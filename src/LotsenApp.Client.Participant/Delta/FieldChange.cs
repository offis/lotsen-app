namespace LotsenApp.Client.Participant.Delta
{
    public interface IFieldChange
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public int? UseDisplay { get; set; }
    }
}