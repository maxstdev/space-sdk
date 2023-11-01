namespace MaxstXR.Place
{
    public enum DIComponent
    {
        none = 0,
        place,
        minimap,
        navigation
    }

    public class DI : DIBase
    {
        public DI(DIScope scope, DIComponent component = DIComponent.none)
        {
            Scope = scope;
            ScopeName = component.ToString();
        }
    }
}
