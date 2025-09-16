using UnrealSharp.Core;
using UnrealSharp.UnrealSharpCore;

namespace UnrealSharp.UMG;

public partial class UWidgetTree
{
    public T ConstructWidget<T>(TSubclassOf<T> widgetClass, FName widgetName = default) where T : UWidget
    {
        return UWidgetTreeExtensions.ConstructWidget(this, widgetClass, widgetName);
    }

    public T ConstructWidget<T>(FName widgetName = default) where T : UWidget
    {
        return UWidgetTreeExtensions.ConstructWidget<T>(this, widgetName);
    }
}