// "Unreal Pokémon" created by Retro & Chill.

#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "WidgetTreeExtensions.generated.h"

class UWidgetTree;
class UWidget;
/**
 * 
 */
UCLASS(meta = (InternalType))
class UNREALSHARPCORE_API UWidgetTreeExtensions : public UBlueprintFunctionLibrary
{
    GENERATED_BODY()

public:
    UFUNCTION(meta = (ScriptMethod, DynamicOutputParam = ReturnValue, DeterminesOutputType = WidgetClass))
    static UWidget* ConstructWidget(UWidgetTree* WidgetTree, TSubclassOf<UWidget> WidgetClass, FName WidgetName);
};
