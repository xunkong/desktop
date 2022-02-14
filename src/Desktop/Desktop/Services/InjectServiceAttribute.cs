namespace Xunkong.Desktop.Services
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InjectServiceAttribute : Attribute
    {

        public bool IsSingleton { get; set; }

        public InjectServiceAttribute(bool isSingleton = false)
        {
            IsSingleton = isSingleton;
        }

    }
}
