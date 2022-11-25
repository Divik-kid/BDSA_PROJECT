using Microsoft.EntityFrameworkCore;
using GitInsight;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//builder.Services.AddSingleton<GitInsight.Data.WeatherForecastService>();
builder.Services.AddScoped<GitInsightController>();
builder.Services.AddScoped<GitInsightContext>();
//--------------Real database setup---------------------
//Naviger til GitInsight folder og kør disse to commands i terminal.
//Husk at udskift database, username og password med dit eget

//$CONNECTION_STRING="Host=localhost;Database=GitDB;Username=postgres;Password=bianca3";
//dotnet user-secrets set "ConnectionStrings:GitIn" "$CONNECTION_STRING"

var configuration = new ConfigurationBuilder().AddUserSecrets<GitInsightController>().Build();
var connectionString = configuration.GetConnectionString("GitIn");

//hvis ovenstående ikke fungerer, brug denne i stedet. Husk at udskifte info og udkommentere ovenstående
//var connectionString = @"Host=localhost;Database=GitDB;Username=postgres;Password=bianca3";

GitInsightContextFactory factory = new GitInsightContextFactory();
        GitInsightContext context = factory.CreateDbContext(args);
        //context.Database.EnsureDeleted(); //to delete database for tests
        var repoRep = new GitInsightController(context);
        //Console.WriteLine(context.Database.EnsureDeleted()); //to delete database for tests
        //var repoRep = new RepoCheckRepository(context);
        Console.WriteLine(context.Database.EnsureCreated());


builder.Services.AddDbContext<GitInsightContext>(options =>
     options.UseNpgsql(connectionString)); //options.UseSqlServer(connectionString));
     //lortet funker kun med Npgsql server, dunno why but it works

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// JsonConvert.



//-------------------------------------

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GetInsight", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GetInsight v1"));
} 
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapRazorPages();
app.MapControllers();

app.Run();