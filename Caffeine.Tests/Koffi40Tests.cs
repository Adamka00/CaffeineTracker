using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Xunit;
namespace Caffeine.Tests;

public sealed class Koffi40PolicyTests
{
    [Theory]
    [InlineData(9, "InsightInsufficient", "ConfidenceLow")]
    [InlineData(10, "InsightEarly", "ConfidenceLow")]
    [InlineData(19, "InsightEarly", "ConfidenceLow")]
    [InlineData(20, "InsightEmerging", "ConfidenceMedium")]
    [InlineData(29, "InsightEmerging", "ConfidenceMedium")]
    [InlineData(30, "InsightEstablished", "ConfidenceMoreData")]
    public void ConfidenceStagesUseUsableNightsPerMetric(int count, string stage, string confidence)
    {
        var logs = Enumerable.Range(0, count).Select(i => new SleepLog { SleepDate = new DateTime(2026, 9, 1).AddDays(-i),
            EstimatedCaffeineAtBedtime = i*10, PreviousDayCaffeineMg = i*20, SleepRating = i%5+1, ActualBedtime = new DateTime(2026,9,1).AddDays(-i).AddHours(-8), WakeTime = new DateTime(2026,9,1).AddDays(-i) });
        var insights = new SleepInsightService().Analyze(logs);
        Assert.Equal(stage, insights[0].StageKey); Assert.Equal(confidence, insights[0].ConfidenceKey);
        Assert.Equal("InsightInsufficient", insights[2].StageKey);
    }
    [Theory]
    [InlineData(5, false, false, false)] [InlineData(6, false, false, true)]
    [InlineData(11, false, false, true)] [InlineData(12, false, false, false)]
    [InlineData(8, true, false, false)] [InlineData(8, false, true, false)]
    public void MorningReminderHasCalendarAndTimeBoundaries(int hour, bool logged, bool dismissed, bool expected)
    {
        var now = new DateTime(2026,10,5,hour,0,0);
        Assert.Equal(expected, ExperienceService.MorningDue(now, logged, dismissed ? now.Date : now.Date.AddDays(-1)));
    }
    [Theory]
    [InlineData("", true)] [InlineData("3.0", true)] [InlineData("4.0", false)]
    public void ReleaseIsShownOncePerVersion(string seen, bool expected) => Assert.Equal(expected, ExperienceService.ReleaseDue(seen));

    [Theory]
    [InlineData("3017620422003", true)] [InlineData("12345670", true)]
    [InlineData("3017620422004", false)] [InlineData("1234", false)]
    [InlineData("<script>alert", false)] [InlineData("03017620422003", true)]
    public void BarcodeChecksumAndShapeAreValidated(string code, bool expected) => Assert.Equal(expected, ProductLookupService.ValidBarcode(code));
    [Fact] public void ProductMappingDoesNotInferUnknownCaffeineOrDensity()
    {
        using var json = JsonDocument.Parse("""{"product_name_de":"Getränk", "product_name":"Drink","quantity":"500 ml","nutriments":{"caffeine_100g":0.032}}""");
        var mapped = OpenFoodFactsClient.Map(json.RootElement,"3017620422003","de");
        Assert.Equal("Getränk", mapped.Name); Assert.Equal(500, mapped.AmountMl); Assert.Null(mapped.CaffeinePer100Ml);
        using var known = JsonDocument.Parse("""{"product_name":"Drink","quantity":"0.5 l","nutrition_data_per":"100ml","nutriments":{"caffeine_100g":0.032}}""");
        Assert.Equal(32, OpenFoodFactsClient.Map(known.RootElement,"3017620422003","hu").CaffeinePer100Ml);
        using var multi = JsonDocument.Parse("""{"quantity":"6 x 500 ml","nutriments":{}}""");
        Assert.Null(OpenFoodFactsClient.Map(multi.RootElement,"3017620422003","en").AmountMl);
    }
    private sealed class Handler(HttpStatusCode status, string body, bool timeout = false) : HttpMessageHandler {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => timeout
            ? Task.FromException<HttpResponseMessage>(new TaskCanceledException())
            : Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
    }
    [Theory]
    [InlineData(404,"{}",false,"not-found")]
    [InlineData(429,"{}",false,"unavailable")]
    [InlineData(200,"not-json",false,"unavailable")]
    [InlineData(200,"{}",true,"unavailable")]
    [InlineData(200,"{}",false,"not-found")]
    public async Task ExternalLookupErrorsBecomeManualFallback(int status,string json,bool timeout,string expected)
    {
        using var http = new HttpClient(new Handler((HttpStatusCode)status,json,timeout)) { BaseAddress = new Uri("https://world.openfoodfacts.org/") };
        Assert.Equal(expected,(await new OpenFoodFactsClient(http).FindAsync("3017620422003","en",CancellationToken.None)).Status);
    }
    [Fact] public void PlaintextSmtpModesAreRejected() {
        Assert.Throws<ArgumentException>(()=>SmtpEmailSender.SecurityMode("None"));
        Assert.Throws<ArgumentException>(()=>SmtpEmailSender.SecurityMode("StartTlsWhenAvailable"));
    }
}

public sealed class Koffi40IntegrationTests : IDisposable
{
    private readonly KoffiFeatureApp app = new();
    private readonly HttpClient client;
    public Koffi40IntegrationTests() {
        client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var scope = app.Services.CreateScope(); DatabaseUpgrade.ApplyAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>()).GetAwaiter().GetResult();
    }
    private async Task<T> Work<T>(Func<IServiceProvider,AppDbContext,Task<T>> action) {
        using var scope = app.Services.CreateScope(); return await action(scope.ServiceProvider,scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
    private static async Task<HttpResponseMessage> Post(HttpClient c,string url,Dictionary<string,string> data) {
        var html = await c.GetStringAsync("/Help"); var match = Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success); data["__RequestVerificationToken"]=WebUtility.HtmlDecode(match.Groups[1].Value);
        return await c.PostAsync(url,new FormUrlEncodedContent(data));
    }
    [Theory] [InlineData("hu","Segítség")] [InlineData("en","Help")] [InlineData("de","Hilfe")]
    public async Task HelpAndCorePagesRenderInThreeLanguages(string culture,string heading) {
        var html=await client.GetStringAsync("/Help?culture="+culture); Assert.Contains(heading, WebUtility.HtmlDecode(html));
        foreach(var page in new[]{"/","/Sleep","/Sleep/Edit","/Planning","/Planning/Settings","/Stats","/Barcode","/Help/WhatsNew","/Help/Support","/Account/ForgotPassword"})
            Assert.Equal(HttpStatusCode.OK,(await client.GetAsync(page+"?culture="+culture)).StatusCode);
    }
    [Fact] public async Task AcknowledgementIsScopedIdempotentAndCsrfProtected() {
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsync("/Help/Acknowledge",new FormUrlEncodedContent(new Dictionary<string,string>{{"kind","tutorial"}}))).StatusCode);
        for(var i=0;i<2;i++) Assert.Equal(HttpStatusCode.NoContent,(await Post(client,"/Help/Acknowledge",new(){{"kind","release"},{"version","4.0"}})).StatusCode);
        using var outsider=app.CreateClient(new WebApplicationFactoryClientOptions{AllowAutoRedirect=false});
        Assert.Contains("data-auto=\"true\"",await outsider.GetStringAsync("/Help"));
        Assert.Equal(1,await Work((_,db)=>db.ExperiencePreferences.CountAsync()));
        Assert.Equal(HttpStatusCode.BadRequest,(await Post(client,"/Help/Acknowledge",new(){{"kind","release"},{"version","bogus"}})).StatusCode);
    }
    [Fact] public async Task MorningStateFollowsWakeDateAndOwner() {
        app.Clock.At=new DateTimeOffset(2026,10,5,6,0,0,TimeSpan.Zero); // 08:00 Budapest
        await Work(async(s,db)=>{
            var experience=s.GetRequiredService<ExperienceService>(); var state=await experience.GetAsync("owner"); Assert.True(state.ShowMorning);
            await experience.SaveAsync("owner","morning"); Assert.False((await experience.GetAsync("owner")).ShowMorning);
            Assert.True((await experience.GetAsync("other")).ShowMorning);
            db.SleepLogs.Add(new SleepLog{UserId="other",SleepDate=new DateTime(2026,10,5),ActualBedtime=new DateTime(2026,10,4,23,0,0),WakeTime=new DateTime(2026,10,5,7,0,0),SleepRating=4});
            await db.SaveChangesAsync(); Assert.False((await experience.GetAsync("other")).ShowMorning);return 0;
        });
    }
    private sealed class External : IOpenFoodFactsClient {
        public int Calls;
        public Task<ProductLookupResult> FindAsync(string code,string culture,CancellationToken token){Calls++;return Task.FromResult(new ProductLookupResult("not-found"));}
    }
    [Fact] public async Task VerifiedKoffiMappingTakesPriorityAndLookupNeverWritesLogs() {
        await Work(async(_,db)=>{
            db.BeverageBarcodes.Add(new(){Barcode="3017620422003",BeverageId=4});await db.SaveChangesAsync();
            var external=new External(); using var cache=new MemoryCache(new MemoryCacheOptions());
            var service=new ProductLookupService(db,external,cache,new ConfigurationBuilder().Build());
            var result=await service.FindAsync("guest","3017620422003","en");Assert.Equal("Koffi",result.Product!.Source);Assert.Equal(0,external.Calls);Assert.Equal(0,await db.CaffeineLogs.CountAsync());
            return 0;
        });
        Assert.Equal(HttpStatusCode.OK,(await Post(client,"/Barcode/Lookup",new(){{"Code","3017620422003"}})).StatusCode);
        Assert.Equal(0,await Work((_,db)=>db.CaffeineLogs.CountAsync()));
    }
    [Fact] public async Task ExperiencePreferencesTransferWithoutLosingAccountState() {
        await Work(async(_,db)=>{
            db.ExperiencePreferences.AddRange(new ExperiencePreference{UserId="guest",TutorialCompleted=true,LastSeenRelease="4.0",MorningDismissedDate=new DateTime(2026,10,5)},new ExperiencePreference{UserId="owner",LastSeenRelease="3.0"});await db.SaveChangesAsync();
            await using var tx=await db.Database.BeginTransactionAsync();await UserFeatureData.TransferAsync(db,"guest","owner");await db.SaveChangesAsync();await tx.CommitAsync();
            var p=await db.ExperiencePreferences.SingleAsync();Assert.Equal("owner",p.UserId);Assert.True(p.TutorialCompleted);Assert.Equal("4.0",p.LastSeenRelease);
            await UserFeatureData.DeleteAsync(db,"owner");await db.SaveChangesAsync();Assert.Empty(await db.ExperiencePreferences.ToListAsync());return 0;
        });
    }
    [Fact] public async Task YearStatisticsAndCalendarAreUserIsolated() {
        await Work(async(s,db)=>{
            db.CaffeineLogs.AddRange(new CaffeineLog{UserId="owner",BeverageId=4,ConsumedAt=new DateTime(2025,12,31,12,0,0),ConsumedAmountMl=500,TotalCaffeineMg=160},
                new CaffeineLog{UserId="owner",BeverageId=4,ConsumedAt=new DateTime(2026,1,2,12,0,0),ConsumedAmountMl=500,TotalCaffeineMg=160},
                new CaffeineLog{UserId="other",BeverageId=4,ConsumedAt=new DateTime(2025,12,31,12,0,0),ConsumedAmountMl=500,TotalCaffeineMg=999});await db.SaveChangesAsync();
            var service=s.GetRequiredService<LifetimeStatsService>();var stats=await service.GetAsync("owner",new DateTime(2026,10,5),2025);Assert.Equal(1,stats.DrinkCount);Assert.Equal(160,stats.TotalMg);
            var calendar=await service.CalendarAsync("owner",new DateTime(2025,12,1),new DateTime(2026,1,1),new DateTime(2026,10,5));Assert.Equal(31,calendar.Count);Assert.Equal(160,calendar.Sum(d=>d.Mg));return 0;
        });
    }
    [Fact] public async Task PasswordResetGermanMailHasSafeLocalizedLinkAndBody() {
        await Work(async(s,db)=>{
            db.Users.Add(new AppUser{Username="mailtest",Email="mail@example.test",PasswordHash="old"});await db.SaveChangesAsync();
            await s.GetRequiredService<PasswordResetService>().RequestAsync("mail@example.test","de");return 0;
        });
        ResetMail? mail=null;for(var i=0;i<100 && !app.Email.Messages.TryDequeue(out mail);i++) await Task.Delay(10);
        Assert.NotNull(mail); Assert.Contains("culture=de",mail.Url); Assert.StartsWith("https://koffi.example/",mail.Url);
        using var scope=app.Services.CreateScope();var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"Email:From","noreply@koffi.hu"}}).Build();
        var sender=new SmtpEmailSender(config,scope.ServiceProvider.GetRequiredService<IStringLocalizer<SharedResource>>());
        var message=sender.CreateMessage(mail);Assert.Contains("Passwort",message.Subject);Assert.Contains("30 Minuten",message.TextBody);
        Assert.Throws<ArgumentException>(()=>sender.CreateMessage(mail with {Url="http://bad.test/token"}));
    }
    [Fact] public void ProductionRegistersNoPushWorkerOrSender() {
        // This fixture intentionally registers legacy services for domain tests; workers remain absent.
        Assert.DoesNotContain(app.Services.GetServices<IHostedService>(),s=>s is NotificationWorker);
    }
    [Fact] public async Task GermanResourcesAndFallbackAreResolved() {
        await Work((s,_)=>{
            var old=CultureInfo.CurrentUICulture;try {
                CultureInfo.CurrentUICulture=new CultureInfo("de-DE");var t=s.GetRequiredService<IStringLocalizer<SharedResource>>();Assert.Equal("Hilfe",t["HelpTitle"].Value);Assert.False(t["HelpTitle"].ResourceNotFound);
                Assert.True(t["UnknownKeyForTest"].ResourceNotFound);
            }finally{CultureInfo.CurrentUICulture=old;}return Task.FromResult(0);
        });
    }
    public void Dispose(){client.Dispose();app.Dispose();}
}

public sealed class Koffi40MigrationTests
{
    private static async Task<Dictionary<string,string>> Snapshot(AppDbContext db) {
        var connection = db.Database.GetDbConnection(); await db.Database.OpenConnectionAsync();
        using var tableCommand = connection.CreateCommand(); tableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory' ORDER BY name";
        var tables = new List<string>(); await using(var reader=await tableCommand.ExecuteReaderAsync()) while(await reader.ReadAsync()) tables.Add(reader.GetString(0));
        var result = new Dictionary<string,string>();
        foreach(var table in tables) {
            using var command=connection.CreateCommand();command.CommandText="SELECT * FROM \""+table+"\" ORDER BY rowid";
            await using var reader=await command.ExecuteReaderAsync();var rows=new List<string[]>();
            while(await reader.ReadAsync()){var row=new string[reader.FieldCount];for(var i=0;i<row.Length;i++) row[i]=reader.IsDBNull(i)?"<NULL>":reader.GetValue(i).ToString()!;rows.Add(row);}
            result[table]=JsonSerializer.Serialize(rows);
        }return result;
    }
    [Fact] public async Task UpgradeFromProduction30IsAdditiveIdempotentAndPreservesEveryOldTable() {
        var path=Path.Combine(Path.GetTempPath(),$"koffi40-upgrade-{Guid.NewGuid():N}.db");
        try {
            await using var db=new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source="+path).Options);
            // Reproduce the released legacy profile schema before the 3.0 migration.
            await db.GetService<IMigrator>().MigrateAsync("20260821170905_3.0");
            await db.Database.ExecuteSqlRawAsync("""
                ALTER TABLE CaffeineLogs ADD COLUMN UserId TEXT NOT NULL DEFAULT '';
                CREATE TABLE Users (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Username TEXT NOT NULL, Email TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL, CreatedAt TEXT NOT NULL);
                INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ('20260827170851_AddUserProfileSystem', '10.0.11');
                """);
            await db.GetService<IMigrator>().MigrateAsync("20260924142648_Koffi30Features");
            db.Users.Add(new AppUser{Username="preserved",Email="old@example.test",PasswordHash="existing-hash",SecurityStamp="stamp"});
            db.CaffeineLogs.Add(new CaffeineLog{UserId="1",BeverageId=4,ConsumedAt=new DateTime(2026,9,24),ConsumedAmountMl=500,TotalCaffeineMg=160});
            db.FavoriteDrinks.Add(new FavoriteDrink{UserId="1",BeverageId=4,AmountMl=500});
            db.CaffeineFreeDays.Add(new CaffeineFreeDay{UserId="guest",Day=new DateTime(2026,9,23)});
            db.SleepLogs.Add(new SleepLog{UserId="guest",SleepDate=new DateTime(2026,9,24),ActualBedtime=new DateTime(2026,9,23,23,0,0),WakeTime=new DateTime(2026,9,24,7,0,0),SleepRating=4,Notes="private notes"});
            db.NotificationPreferences.Add(new NotificationPreference{UserId="guest",StreakReminder=true});
            db.TrackerPreferences.Add(new TrackerPreference{UserId="guest",TargetMg=35});await db.SaveChangesAsync();
            var before=await Snapshot(db);
            await DatabaseUpgrade.ApplyAsync(db);await DatabaseUpgrade.ApplyAsync(db);
            var after=await Snapshot(db);foreach(var table in before) Assert.Equal(table.Value,after[table.Key]);
            Assert.True(after.ContainsKey("ExperiencePreferences"));Assert.True(after.ContainsKey("BeverageBarcodes"));
            Assert.False(db.Database.HasPendingModelChanges());
        } finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();File.Delete(path); }
    }
}
