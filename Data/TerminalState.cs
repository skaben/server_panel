namespace Panel.Data
{
    public class TerminalState
    {
        public DateOnly Date { get; set; }
        public string Text { get; set; } = "Замок1 в комнате которая находится позади помещения";
        public bool? IsNotLocked { get; set; }
        public bool? Locked { get; set; }
        public bool Block { get; set; } = false;
        public string Line { get; set; } = "Зеленая полоска";
    }
}
