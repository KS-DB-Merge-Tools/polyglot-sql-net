using System.Text.Json;
using PolyglotSql;
using PolyglotSql.Models;

namespace PolyglotSql.Tests;

// Pure serialization tests - no native library required
public class BuildRequestTests
{
    private const string PlanJson = "{\"base\":{\"kind\":\"select\",\"expressions\":[{\"kind\":\"sql\",\"sql\":\"x\"}]},\"operations\":[]}";

    private static string Serialize(BuildRequest request)
        => JsonSerializer.Serialize(request, PolyglotJsonContext.Default.BuildRequest);

    [Fact]
    public void TestDefaultAstOutput()
    {
        var request = new BuildRequest { Plan = BuilderPlan.FromJson(PlanJson) };

        Assert.Equal("{\"version\":1,\"read_dialect\":\"generic\",\"plan\":" + PlanJson + ",\"output\":{\"kind\":\"ast\"}}", Serialize(request));
    }

    [Fact]
    public void TestSqlOutput()
    {
        var request = new BuildRequest
        {
            ReadDialect = Dialect.PostgreSQL,
            Plan = BuilderPlan.FromJson(PlanJson),
            Output = new BuilderOutput.Sql { Dialect = Dialect.TSQL },
        };

        Assert.Equal("{\"version\":1,\"read_dialect\":\"postgresql\",\"plan\":" + PlanJson + ",\"output\":{\"kind\":\"sql\",\"dialect\":\"tsql\"}}", Serialize(request));
    }

    [Fact]
    public void TestRoundTrip()
    {
        string json = "{\"version\":1,\"read_dialect\":\"duckdb\",\"plan\":" + PlanJson + ",\"output\":{\"kind\":\"sql\",\"dialect\":\"mysql\"}}";
        var request = JsonSerializer.Deserialize(json, PolyglotJsonContext.Default.BuildRequest)!;

        Assert.Equal(1, request.Version);
        Assert.Equal(Dialect.DuckDB, request.ReadDialect);
        Assert.Equal(PlanJson, request.Plan.ToJsonString());
        var output = Assert.IsType<BuilderOutput.Sql>(request.Output);
        Assert.Equal(Dialect.MySQL, output.Dialect);
        Assert.Equal(json, Serialize(request));
    }

    [Fact]
    public void TestBuilderPlanPreservesJson()
    {
        var plan = BuilderPlan.FromJson(PlanJson);

        Assert.Equal(PlanJson, plan.ToJsonString());
        Assert.Equal(PlanJson, JsonSerializer.Serialize(plan, PolyglotJsonContext.Default.BuilderPlan));
    }

    [Fact]
    public void TestBuilderPlanInvalidJson()
    {
        Assert.ThrowsAny<JsonException>(() => BuilderPlan.FromJson("{not json"));
    }
}
