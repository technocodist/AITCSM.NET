using AITCSM.NET.Implementations;

await Task.WhenAll([
    CH01DOM01.DefaultSimulate(),
    CH01DOM02.DefaultSimulate()
    ]);