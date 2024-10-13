using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Task_Management_system.Data;
using Task_Management_system.Data.Repositories;
using Task_Management_system.Data.UnitOfWork;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options=>
   options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<AppDbContext>();



builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddHttpClient<ApiService>();

// Configure JSON options
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints => endpoints.MapRazorPages());

app.Run();
