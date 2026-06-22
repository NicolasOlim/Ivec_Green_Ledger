namespace WpfIveco.DTO
{
    /// <summary>
    /// DTO para uma entrada de log persistida em JSON.
    /// </summary>
    public class LogEntryDto
    {
        public string Timestamp { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public int ThreadId { get; set; }
    }
}