namespace LotsenApp.Client.Participant.Delta
{
    public interface IGroupChange
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public IGroupChange[] Children { get; set; }
        public IFieldChange[] Fields { get; set; }
    }
}