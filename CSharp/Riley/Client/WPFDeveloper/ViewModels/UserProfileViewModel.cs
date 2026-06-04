
using WPFDeveloper.Attributes;

namespace WPFDeveloper.ViewModels
{
    public partial class UserProfileViewModel
    {
        // 标记需要自动生成属性的字段
        [AutoNotify]
        private string _firstName = string.Empty;

        [AutoNotify("LastName")]
        private string _lastName = string.Empty;

        [AutoNotify(AlsoNotifyFor = new[] { nameof(FullName) })]
        private int _age;

        // 普通属性（不会被自动生成）
        public string FullName => $"{FirstName} {LastName}";

        // 其他业务逻辑
        public bool IsAdult => Age >= 18;
    }
}
