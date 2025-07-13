﻿#pragma once
#include "CSManagedCallbacksCache.h"
#include "CSManagedGCHandle.generated.h"

enum class GCHandleType : char
{
	Null,
	StrongHandle,
	WeakHandle,
	PinnedHandle,
};

struct FGCHandleIntPtr
{
	bool operator == (const FGCHandleIntPtr& Other) const
	{
		return IntPtr == Other.IntPtr;
	}

	bool operator != (const FGCHandleIntPtr& Other) const
	{
		return IntPtr != Other.IntPtr;
	}
	
	// Pointer to the managed object in C#
	uint8* IntPtr = nullptr;
};

static_assert(sizeof(FGCHandleIntPtr) == sizeof(void *));

struct FGCHandle
{
	FGCHandleIntPtr Handle;
	GCHandleType Type = GCHandleType::Null;

	static FGCHandle Null() { return FGCHandle(nullptr, GCHandleType::Null); }

	bool IsNull() const { return !Handle.IntPtr; }
	bool IsWeakPointer() const { return Type == GCHandleType::WeakHandle; }
	
	FGCHandleIntPtr GetHandle() const { return Handle; }
	uint8* GetPointer() const { return Handle.IntPtr; };
	
	void Dispose(FGCHandleIntPtr AssemblyHandle = FGCHandleIntPtr())
	{
		if (!Handle.IntPtr || Type == GCHandleType::Null)
		{
			return;
		}

		FCSManagedCallbacks::ManagedCallbacks.Dispose(Handle, AssemblyHandle);
	
		Handle.IntPtr = nullptr;
		Type = GCHandleType::Null;
	}

	void operator = (const FGCHandle& Other)
	{
		Handle = Other.Handle;
		Type = Other.Type;
	}
	
	operator void*() const
	{
		return Handle.IntPtr;
	}

	FGCHandle(){}
	FGCHandle(const FGCHandleIntPtr InHandle, const GCHandleType InType) : Handle(InHandle), Type(InType) {}

	FGCHandle(uint8* InHandle, const GCHandleType InType) : Type(InType)
	{
		Handle.IntPtr = InHandle;
	}

	FGCHandle(const FGCHandleIntPtr InHandle) : Handle(InHandle)
	{
		Type = GCHandleType::Null;
	}
};

struct FScopedGCHandle
{
    
    FGCHandleIntPtr Handle;

    explicit FScopedGCHandle(FGCHandleIntPtr InHandle) : Handle(InHandle) {}

    FScopedGCHandle(const FScopedGCHandle&) = delete;
    FScopedGCHandle(FScopedGCHandle&&) = delete;

    ~FScopedGCHandle()
    {
        if (Handle.IntPtr != nullptr) 
        {
            FCSManagedCallbacks::ManagedCallbacks.FreeHandle(Handle);
        }
    }
    
    FScopedGCHandle& operator=(const FScopedGCHandle&) = delete;
    FScopedGCHandle& operator=(FScopedGCHandle&&) = delete;
};

USTRUCT()
struct FSharedGCHandle
{
    GENERATED_BODY()

    FSharedGCHandle() = default;
    explicit FSharedGCHandle(FGCHandleIntPtr InHandle) : Handle(MakeShared<FScopedGCHandle>(InHandle)) {}

    FGCHandleIntPtr GetHandle() const
    {
        if (Handle == nullptr) 
        {
            return FGCHandleIntPtr();
        }
        
        return Handle->Handle;
    }
    
private:
    TSharedPtr<FScopedGCHandle> Handle;
};