using CustomCodeFramework.Core.Results;

namespace Dhole.Storage.Domain.Shared;

public static class StorageErrors
{
    public static readonly Error FileNotFound = new(
        "Storage.FileNotFound",
        "El archivo no fue encontrado."
    );

    public static readonly Error ProviderNotFound = new(
        "Storage.ProviderNotFound",
        "El proveedor de almacenamiento no fue encontrado."
    );

    public static readonly Error ProviderInactive = new(
        "Storage.ProviderInactive",
        "El proveedor de almacenamiento está inactivo."
    );

    public static readonly Error VersionNotFound = new(
        "Storage.VersionNotFound",
        "La versión del archivo no fue encontrada."
    );

    public static readonly Error InvalidFile = new(
        "Storage.InvalidFile",
        "El archivo no es válido."
    );

    public static readonly Error InvalidReference = new(
        "Storage.InvalidReference",
        "La referencia del archivo no es válida."
    );

    public static readonly Error FileDeleted = new(
        "Storage.FileDeleted",
        "El archivo fue eliminado."
    );

    public static readonly Error ProviderCodeAlreadyExists = new(
        "Storage.ProviderCodeAlreadyExists",
        "Ya existe un proveedor de almacenamiento con ese código."
    );
}
