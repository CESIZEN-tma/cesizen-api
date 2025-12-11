using api.CZ.Core.Extensions;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();


// Building WebApp with all the dependencies and middlewares needed
builder.BuildSolution();