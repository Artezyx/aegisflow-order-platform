using AegisFlow.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AegisFlow.Api.Migrations;

[DbContext(typeof(AegisDbContext))]
public sealed class AegisDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
        => InitialModel.Build(modelBuilder);
}
