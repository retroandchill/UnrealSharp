// "Unreal Pokémon" created by Retro & Chill.


#include "WidgetTreeExtensions.h"
#include "Blueprint/WidgetTree.h"

UWidget * UWidgetTreeExtensions::ConstructWidget(UWidgetTree *WidgetTree, const TSubclassOf<UWidget> WidgetClass,
                                                 const FName WidgetName)
{
    return WidgetTree->ConstructWidget(WidgetClass, WidgetName);
}