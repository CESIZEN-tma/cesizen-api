using api.CZ.Core.Extensions;
using DotNetEnv;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder =  WebApplication.CreateBuilder(args);

Env.Load();


// Building WebApp with all the dependencies and middlewares needed
await builder.BuildSolution();