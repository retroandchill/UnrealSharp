#include "CSDeveloperSettings.h"

FName UCSDeveloperSettings::GetContainerName() const
{
    if (const FName ConfigName = K2_GetContainerName(); ConfigName != NAME_None)
    {
        return ConfigName;   
    }
    
    return Super::GetContainerName();
}

FName UCSDeveloperSettings::GetCategoryName() const
{
    if (const FName ManagedResult = K2_GetCategoryName(); ManagedResult != NAME_None)
    {
        return ManagedResult;  
    }
    
    return Super::GetCategoryName();
}

FName UCSDeveloperSettings::GetSectionName() const
{
    if (const FName ManagedResult = K2_GetSectionName(); ManagedResult != NAME_None)
    {
        return ManagedResult; 
    }
    
    return Super::GetSectionName();
}

#if WITH_EDITOR
FText UCSDeveloperSettings::GetSectionText() const
{
    if (const FText SectionText = K2_GetSectionText(); !SectionText.IsEmpty())
    {
        return SectionText;
    }

    return Super::GetSectionText();
}

FText UCSDeveloperSettings::GetSectionDescription() const
{
    if (const FText SectionDescription = K2_GetSectionDescription(); !SectionDescription.IsEmpty())
    {
        return SectionDescription;
    }
    
    return Super::GetSectionDescription();
}
#endif