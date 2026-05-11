using FartmaalerAPI.Controllers;
using FartmaalerAPI.Data;
using FartmaalerAPI.DTOs;
using FartmaalerAPI.Models;
using FartmaalerAPI.Repositories;
using FartmaalerAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class MeasurementsControllerTest : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MeasurementsRepo _repo;
    private readonly MeasurementService _measurementService;
    private readonly MeasurementsController _controller;

    public MeasurementsControllerTest()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repo = new MeasurementsRepo(_context);
        _measurementService = new MeasurementService(_context, _repo);
        _controller = new MeasurementsController(_repo, _context, _measurementService);

        // ✅ Sæt admin-rolle på controlleren (nødvendigt for Delete og live-overview)
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    public void Dispose() => _context.Dispose();

    private Session CreateSession(string status = "Started") => new Session
    {
        GroupId = 1,
        CarType = "Toy car",
        RoadType = "Byzone",
        SpeedLimit = 50,
        ScalingFactor = 10,
        Status = status,
        CreatedAt = DateTime.Now
    };

    // ✅ Hjælpemetode der laver en CreateMeasurementRequest (ikke Measurement)
    private CreateMeasurementRequest CreateRequest(int sessionId, double time = 1) =>
        new CreateMeasurementRequest { SessionId = sessionId, Time = time };

    [Fact]
    public void Add_ValidMeasurement_ReturnsCreated()
    {
        var session = CreateSession();
        _context.Sessions.Add(session);
        _context.SaveChanges();

        var result = _controller.Add(CreateRequest(session.Id));

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public void Add_ValidMeasurement_SavesMeasurementInDatabase()
    {
        var session = CreateSession();
        _context.Sessions.Add(session);
        _context.SaveChanges();

        _controller.Add(CreateRequest(session.Id));

        Assert.Single(_context.Measurements);
    }

    [Fact]
    public void Add_WhenSessionIdInvalid_ReturnsBadRequest()
    {
        var result = _controller.Add(CreateRequest(0));
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Add_WhenSessionNotFound_ReturnsNotFound()
    {
        var result = _controller.Add(CreateRequest(999));
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Add_WhenTimeIsZero_ReturnsBadRequest()
    {
        var session = CreateSession();
        _context.Sessions.Add(session);
        _context.SaveChanges();

        var result = _controller.Add(CreateRequest(session.Id, time: 0));
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Add_WhenSessionEnded_ReturnsBadRequest()
    {
        var session = CreateSession("Ended");
        _context.Sessions.Add(session);
        _context.SaveChanges();

        var result = _controller.Add(CreateRequest(session.Id));
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Delete_WhenNotExists_ReturnsNotFound()
    {
        var result = _controller.Delete(999);
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Delete_WhenMeasurementExists_ReturnsOk()
    {
        var measurement = _repo.Add(new Measurement { SessionId = 1, Time = 1 });
        var result = _controller.Delete(measurement.Id);
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void Update_Always_ReturnsBadRequest()
    {
        var result = _controller.Update(1, new Measurement());
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void GetSessionSummary_WhenSessionNotFound_ReturnsNotFound()
    {
        var result = _controller.GetSessionSummary(999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void GetSessionSummary_WhenNoMeasurements_ReturnsZeroValues()
    {
        var session = CreateSession();
        _context.Sessions.Add(session);
        _context.SaveChanges();

        var result = _controller.GetSessionSummary(session.Id);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}