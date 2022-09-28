using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StudyItBot.Services;
using Xunit;
using Xunit.Abstractions;

namespace StudyItBotTests.Services;

public class SwotServiceTest
{
    private readonly ITestOutputHelper _output;

    public SwotServiceTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void GetDomainPartsTest()
    {
        var parts = SwotService.GetDomainParts("valid@test.de");

        Assert.Equal(2, parts.Count);
        Assert.Equal("de", parts[0]);
        Assert.Equal("test", parts[1]);

        parts = SwotService.GetDomainParts("valid@abc.test.de");

        Assert.Equal(3, parts.Count);
        Assert.Equal("de", parts[0]);
        Assert.Equal("test", parts[1]);
        Assert.Equal("abc", parts[2]);
    }

    [Fact]
    public void CheckSetTest()
    {
        var parts = SwotService.GetDomainParts("valid@abc.test.de");
        Assert.True(SwotService.CheckSet(new HashSet<string>(new[] { "de" }), parts));
        Assert.True(SwotService.CheckSet(new HashSet<string>(new[] { "test.de" }), parts));
        Assert.True(SwotService.CheckSet(new HashSet<string>(new[] { "abc.test.de" }), parts));

        Assert.False(SwotService.CheckSet(new HashSet<string>(new[] { "abc.test.ge" }), parts));
        Assert.False(SwotService.CheckSet(new HashSet<string>(new[] { "abc.test2.de" }), parts));
        Assert.False(SwotService.CheckSet(new HashSet<string>(new[] { "fail.test.de" }), parts));
        Assert.False(SwotService.CheckSet(new HashSet<string>(new[] { "-abc.test.de" }), parts));
    }

    [Fact]
    public async Task GetSwotListAsyncTest()
    {
        var service = new SwotService();
        var watch = new Stopwatch();

        watch.Start();
        var list = await service.GetSwotListAsync();
        watch.Stop();

        _output.WriteLine($"(First) Elapsed ms: {watch.ElapsedMilliseconds}");
        _output.WriteLine($"(First) Elapsed ticks: {watch.Elapsed}");

        Assert.False(list.IsEmpty);
        Assert.Contains("h-ka.de", list);
        _output.WriteLine($"Length: {list.Count}");

        watch.Reset();

        watch.Start();
        var list2 = await service.GetSwotListAsync();
        watch.Stop();

        _output.WriteLine($"(Second) Elapsed ms: {watch.ElapsedMilliseconds}");
        _output.WriteLine($"(Second) Elapsed ticks: {watch.Elapsed}");

        Assert.False(list2.IsEmpty);
        Assert.Contains("h-ka.de", list2);
        Assert.InRange(watch.ElapsedMilliseconds, 0, 2);
    }

    [Fact]
    public async Task GetStopListAsyncTest()
    {
        var service = new SwotService();
        var stopList = await service.GetStopListAsync();
        Assert.True(stopList.Count < (await service.GetSwotListAsync()).Count);
        Assert.False(stopList.First().StartsWith("-"));
        _output.WriteLine($"Length: {stopList.Count}");
    }

    [Fact]
    public async Task VerifyUniversityEmailAsyncTest()
    {
        var service = new SwotService();
        Assert.True(await service.VerifyUniversityEmailAsync("test@h-ka.de"));
        Assert.True(await service.VerifyUniversityEmailAsync("test@abc.h-ka.de"));
        Assert.True(await service.VerifyUniversityEmailAsync("test@gef.abc.h-ka.de"));

        Assert.False(await service.VerifyUniversityEmailAsync("test@example.com"));
        Assert.False(await service.VerifyUniversityEmailAsync($"test@{(await service.GetStopListAsync()).First()}"));
        Assert.False(
            await service.VerifyUniversityEmailAsync($"test@abc.{(await service.GetStopListAsync()).First()}")
        );
        Assert.False(await service.VerifyUniversityEmailAsync("test.de"));
        Assert.False(await service.VerifyUniversityEmailAsync("h-ka.de"));
    }
}