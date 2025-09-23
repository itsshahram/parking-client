using System.ComponentModel;

namespace Parking.App.Models.Dto.User
{
    public class UserListItemModel : INotifyPropertyChanged
    {
        private bool _isActive;

        public Guid Id { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Fullname { get; set; }
        public DateTime RegisterDate { get; set; }
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? Avatar { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string? Role { get; set; }
        public string? RoleFaName { get; set; }
        public string? RoleEnName { get; set; }
        public Guid? RoleId { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
