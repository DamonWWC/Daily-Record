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
        
        // 新增页面信息属性
        public string PageTitle { get; set; } = string.Empty;
        public string? PageDescription { get; set; }
        public string? PageIcon { get; set; }
        public string? PageCategory { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string? ToolTip { get; set; }
    }
}


