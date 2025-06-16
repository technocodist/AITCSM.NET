using AITCSM.NET.Implementations;

await Task.WhenAll([CH01DOM01.DefaultSimulate(), CH01DOM02.DefaultSimulate(), CH01FF03.DefaultSimulate()]);
await Task.WhenAll([CH01DOM01.DefaultPlot(), CH01DOM02.DefaultPlot(), CH01FF03.DefaultPlot()]);