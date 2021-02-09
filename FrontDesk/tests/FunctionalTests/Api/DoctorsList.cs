﻿using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Models.Doctor;
using FrontDesk.Api;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests.Api
{
  public class DoctorsList : IClassFixture<CustomWebApplicationFactory<Startup>>
  {
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _outputHelper;

    public DoctorsList(CustomWebApplicationFactory<Startup> factory, ITestOutputHelper outputHelper)
    {
      _client = factory.CreateClient();
      _outputHelper = outputHelper;
    }

    [Fact]
    public async Task Returns3Doctors()
    {
      var response = await _client.GetAsync("/api/doctors");
      response.EnsureSuccessStatusCode();
      var stringResponse = await response.Content.ReadAsStringAsync();
      _outputHelper.WriteLine(stringResponse);
      var result = JsonSerializer.Deserialize<ListDoctorResponse>(stringResponse,
        Constants.DefaultJsonOptions);

      Assert.Equal(3, result.Doctors.Count());
      Assert.Contains(result.Doctors, x => x.Name == "Dr. Smith");
    }
  }
}
