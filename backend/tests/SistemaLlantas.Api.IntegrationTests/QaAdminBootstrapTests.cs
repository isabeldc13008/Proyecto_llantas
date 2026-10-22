using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.IntegrationTests;

public class QaAdminBootstrapTests
{
    private static IConfiguration Config(string auth, string enabled) => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
        ["Authentication"]=auth, ["Authentication:QaAdminBootstrap:Enabled"]=enabled }).Build();
    [Theory]
    [InlineData("QA","true","true",true)]
    [InlineData("Development","true","true",false)]
    [InlineData("Production","true","true",false)]
    [InlineData("QA","false","true",false)]
    [InlineData("QA","true","false",false)]
    public void RequiresAllGates(string env,string auth,string enabled,bool expected) => Assert.Equal(expected,QaAdminBootstrap.IsEnabled(env,Config(auth,enabled)));
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public async Task OutsideQaDoesNotResolveConnection(string env) => await QaAdminBootstrap.RunAsync(env,Config("true","true"));
    [Theory]
    [InlineData("GDLLSQLDLLO")]
    [InlineData("master")]
    [InlineData("Production")]
    public void RejectsOtherDatabase(string db) => Assert.Throws<InvalidOperationException>(() => QaAdminBootstrap.ValidateTarget($"Server=srvsqlgdllprbs.database.windows.net;Database={db};Encrypt=True", "srvsqlgdllprbs.database.windows.net"));
    [Fact] public void AcceptsOnlyPinnedSecureTarget()
    {
        QaAdminBootstrap.ValidateTarget("Server=srvsqlgdllprbs.database.windows.net;Database=GDLLSQLPRBS;Encrypt=True", "srvsqlgdllprbs.database.windows.net");
        foreach (var suffix in new[]{";TrustServerCertificate=True", ";Failover Partner=other", ";Encrypt=False"})
            Assert.Throws<InvalidOperationException>(()=>QaAdminBootstrap.ValidateTarget("Server=srvsqlgdllprbs.database.windows.net;Database=GDLLSQLPRBS;Encrypt=True"+suffix,"srvsqlgdllprbs.database.windows.net"));
        Assert.Throws<InvalidOperationException>(()=>QaAdminBootstrap.ValidateTarget("Server=other;Database=GDLLSQLPRBS;Encrypt=True","srvsqlgdllprbs.database.windows.net"));
    }
    private const string QaHost = "srvsqlgdllprbs.database.windows.net";
    public static IEnumerable<object[]> AllowedTargets()
    {
        var forms = new[] { QaHost, "tcp:" + QaHost, QaHost + ",1433", "tcp:" + QaHost + ",1433" };
        foreach (var source in forms)
            foreach (var expected in forms)
                yield return new object[] { source, expected };
        yield return new object[] { "TCP:" + QaHost.ToUpperInvariant() + ",1433", QaHost };
    }
    [Theory, MemberData(nameof(AllowedTargets))]
    public void AllowsOnlyDocumentedEndpointForms(string source, string expected) =>
        QaAdminBootstrap.ValidateTarget($"Server={source};Database=GDLLSQLPRBS;Encrypt=True;TrustServerCertificate=False", expected);

    [Theory]
    [InlineData("other.database.windows.net")]
    [InlineData("srvsqlgdllprbs.database.windows.net.evil")]
    [InlineData("srvsqlgdllprbs.database.windows.net,1444")]
    [InlineData("srvsqlgdllprbs.database.windows.net,01433")]
    [InlineData("srvsqlgdllprbs.database.windows.net:1433")]
    [InlineData("srvsqlgdllprbs.database.windows.net\\SQLEXPRESS")]
    [InlineData("np:srvsqlgdllprbs.database.windows.net")]
    [InlineData("tcp:tcp:srvsqlgdllprbs.database.windows.net")]
    [InlineData("srvsqlgdllprbs.database.windows.net,1433,1433")]
    [InlineData("srvsqlgdllprbs.database.windows.net.")]
    [InlineData("localhost")]
    [InlineData("")]
    public void RejectsInvalidSourceAndExpectedEvenWhenTheyMatch(string target)
    {
        Assert.Throws<InvalidOperationException>(() => QaAdminBootstrap.ValidateTarget($"Server={target};Database=GDLLSQLPRBS;Encrypt=True", QaHost));
        Assert.Throws<InvalidOperationException>(() => QaAdminBootstrap.ValidateTarget($"Server={QaHost};Database=GDLLSQLPRBS;Encrypt=True", target));
        Assert.Throws<InvalidOperationException>(() => QaAdminBootstrap.ValidateTarget($"Server={target};Database=GDLLSQLPRBS;Encrypt=True", target));
    }

    [Fact] public async Task CreatesOnlyUserWithVerifiableHashAndRequiredFlags()
    {
        var s=new Store(); var password=Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        Assert.True(await QaAdminBootstrap.CreateIfMissing(s,password));
        var u=Assert.Single(s.Inserted); Assert.Equal("admin.qa",u.Username); Assert.True(u.Activo); Assert.True(u.DebeCambiarClave);
        Assert.Equal(s.Role!.Value.Id,u.RolId); Assert.Null(u.CentroId);
        Assert.Equal(PasswordVerificationResult.Success,new PasswordHasher<UsuarioSistema>().VerifyHashedPassword(u,u.PasswordHash,password));
        Assert.Equal(PasswordVerificationResult.Failed,new PasswordHasher<UsuarioSistema>().VerifyHashedPassword(u,u.PasswordHash,password+"!"));
        Assert.False(await QaAdminBootstrap.CreateIfMissing(s,null)); Assert.Single(s.Inserted);
    }
    [Fact] public async Task ExistingIdentityIsUntouchedEvenWithoutPasswordOrRole()
    {
        var s=new Store{Exists=true,Role=null}; Assert.False(await QaAdminBootstrap.CreateIfMissing(s,null)); Assert.Empty(s.Inserted); Assert.Equal(0,s.RoleReads);
    }
    [Theory] [InlineData(0)] [InlineData(1)] [InlineData(2)]
    public async Task RejectsMissingInactiveOrIncompleteRole(int mode)
    {
        var s=new Store(); s.Role=mode==0?null:(Guid.NewGuid(),mode!=1, mode==2?[]:QaAdminBootstrap.ExpectedPermissions.ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>QaAdminBootstrap.CreateIfMissing(s,new string('x',20))); Assert.Empty(s.Inserted);
    }
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("short")]
    public async Task MissingOrShortSecretNeverInserts(string? password)
    {
        var s=new Store(); await Assert.ThrowsAsync<InvalidOperationException>(()=>QaAdminBootstrap.CreateIfMissing(s,password)); Assert.Empty(s.Inserted);
    }
    private sealed class Store:QaAdminBootstrap.IStore
    {
        public bool Exists; public int RoleReads;
        public (Guid Id,bool Active,string[] Permissions)? Role=(Guid.NewGuid(),true,QaAdminBootstrap.ExpectedPermissions.ToArray());
        public List<UsuarioSistema> Inserted=new();
        public Task<bool> UserExists(CancellationToken ct)=>Task.FromResult(Exists);
        public Task<(Guid,bool,string[])?> ReadRole(CancellationToken ct){RoleReads++;return Task.FromResult(Role);}
        public Task Insert(UsuarioSistema user,CancellationToken ct){Inserted.Add(user);Exists=true;return Task.CompletedTask;}
    }
}
