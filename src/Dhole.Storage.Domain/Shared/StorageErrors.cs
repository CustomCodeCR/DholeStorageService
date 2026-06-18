using CustomCodeFramework.Core.Results;

namespace Dhole.Storage.Domain.Shared;

public static class StorageErrors
{
    public static readonly Error FileNotFound = new("Storage.FileNotFound", "File not found.");

    public static readonly Error ProviderNotFound = new(
        "Storage.ProviderNotFound",
        "Storage provider not found."
    );

    public static readonly Error ProviderInactive = new(
        "Storage.ProviderInactive",
        "Storage provider is inactive."
    );

    public static readonly Error VersionNotFound = new(
        "Storage.VersionNotFound",
        "File version not found."
    );

    public static readonly Error InvalidFile = new("Storage.InvalidFile", "File is invalid.");

    public static readonly Error InvalidReference = new(
        "Storage.InvalidReference",
        "File reference is invalid."
    );

    public static readonly Error FileDeleted = new("Storage.FileDeleted", "File is deleted.");

    public static readonly Error ProviderCodeAlreadyExists = new(
        "Storage.ProviderCodeAlreadyExists",
        "Storage provider code already exists."
    );
}
