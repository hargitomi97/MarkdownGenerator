# [IDataSetLoader](./IDataSetLoader.md)

Namespace: [SigStat]() > [Common](./../README.md) > [Loaders](./README.md)

Assembly: SigStat.Common.dll

## Summary
Exposes a function to enable loading collections of `SigStat.Common.Signer`s.  Base abstract class: `SigStat.Common.Loaders.DataSetLoader`.

## Methods

| Return | Name | Summary | 
| --- | --- | --- | 
| [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1)\<[Signer](./../Signer.md)> | EnumerateSigners (  ) | Enumerates all signers of the database | 
| [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.IEnumerable-1)\<[Signer](./../Signer.md)> | EnumerateSigners ( [`Predicate`](https://docs.microsoft.com/en-us/dotnet/api/System.Predicate-1)\<[`Signer`](./../Signer.md)> signerFilter ) | Enumerates all `SigStat.Common.Signer`s that match the ``. | 


