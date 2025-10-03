#pragma once

#include "CoreMinimal.h"
#include "Engine/DeveloperSettings.h"
#include "CSDeveloperSettings.generated.h"

UCLASS(Abstract)
class UCSDeveloperSettings : public UDeveloperSettings
{
	GENERATED_BODY()

public:
	// UDeveloperSettings interface
    virtual FName GetContainerName() const override;
    virtual FName GetCategoryName() const override;
    virtual FName GetSectionName() const override;
#if WITH_EDITOR
    virtual FText GetSectionText() const override;
    virtual FText GetSectionDescription() const override;
	virtual bool SupportsAutoRegistration() const override { return false; }
#endif
	// End of UDeveloperSettings interface

protected:
    UFUNCTION(BlueprintImplementableEvent, DisplayName = "Get Container Name", Category = "Managed Settings", meta = (ScriptName = "GetContainerName"))
    FName K2_GetContainerName() const;

    UFUNCTION(BlueprintImplementableEvent, DisplayName = "Get Category Name", Category = "Managed Settings", meta = (ScriptName = "GetCategoryName"))
    FName K2_GetCategoryName() const;

    UFUNCTION(BlueprintImplementableEvent, DisplayName = "Get Section Name", Category = "Managed Settings", meta = (ScriptName = "GetSectionName"))
    FName K2_GetSectionName() const;

    UFUNCTION(BlueprintImplementableEvent, DisplayName = "Get Section Text", Category = "Managed Settings", meta = (ScriptName = "GetSectionText"))
    FText K2_GetSectionText() const;

    UFUNCTION(BlueprintImplementableEvent, DisplayName = "Get Section Description", Category = "Managed Settings", meta = (ScriptName = "GetSectionDescription"))
    FText K2_GetSectionDescription() const;
};
