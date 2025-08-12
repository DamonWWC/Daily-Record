namespace WPFDeveloper.Models
{
    public class NavigationItem
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string ViewType { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Group { get; set; }
    }
}


