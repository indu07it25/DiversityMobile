using Ninject.Selection.Heuristics;

namespace DiversityPhone
{
    public class CustomInjectionHeuristic : IInjectionHeuristic
    {
        public bool ShouldInject(System.Reflection.MemberInfo member)
        {
            return member.IsDefined(
            typeof(InjectAttribute),
            true);

        }

        public Ninject.INinjectSettings Settings
        {
            get;
            set;
        }

        public void Dispose()
        {

        }
    }
}
