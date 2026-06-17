namespace FloatSoda.Elements;

public class ComponentElement : Element
{
    
}


public class StatefulElement : ComponentElement
{
    public void MarkNeedsBuild()
    {
        throw new NotImplementedException();
    }
}

public class StatelessElement : ComponentElement
{
    
}