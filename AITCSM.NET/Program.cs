
using AITCSM.NET.Implementations.Simulation.CH01;

await DistributionOfMoney.DefaultSimulate();
await DistributionOfMoney.DefaultPlot();

await DistributionOfMoneyWithSaving.DefaultSimulate();
await DistributionOfMoneyWithSaving.DefaultPlot();

await FreeFall.DefaultSimulate();
await FreeFall.DefaultPlot();

await FreeFallWithAirResistance.DefaultSimulate();
await FreeFallWithAirResistance.DefaultPlot();