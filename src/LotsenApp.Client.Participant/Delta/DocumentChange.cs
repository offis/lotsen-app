namespace LotsenApp.Client.Participant.Delta
{
    public interface IDocumentChange
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public string Name { get; set; }
        public IFieldChange[] Fields { get; set; }
        public IGroupChange[] Groups { get; set; }
    }
}