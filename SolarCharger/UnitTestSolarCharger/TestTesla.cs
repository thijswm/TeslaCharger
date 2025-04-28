using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SolarCharger.EF;
using SolarCharger.Services;

namespace UnitTestSolarCharger
{
    [TestClass]
    public sealed class TestTesla
    {
        private Mock<ILogger<Tesla>> _mockLogger;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;

        private HttpClient GetMockedHttpClient()
        {
            //var handlerMock = new Mock<HttpMessageHandler>();

            //handlerMock
            //    .Protected()
            //    .Setup<Task<HttpResponseMessage>>(
            //        "SendAsync",
            //        ItExpr.IsAny<HttpRequestMessage>(),
            //        ItExpr.IsAny<CancellationToken>()
            //    )
            //    .ReturnsAsync(responseMessage);

            //var httpClientHandler = new HttpClientHandler
            //{
            //    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            //};

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            return httpClient;
        }

        //private ChargeContext CreateInMemoryChargeContext()
        //{
        //    var options = new DbContextOptionsBuilder<ChargeContext>()
        //        //.UseInMemoryDatabase(databaseName: "TestDatabase")
        //        .Options;

        //    return new ChargeContext(options, new Mock<ILogger<ChargeContext>>().Object);
        //}



        [TestInitialize]
        public void TestInit()
        {
            _mockLogger = new Mock<ILogger<Tesla>>(MockBehavior.Loose);
            //_mockChargeContext = new Mock<ChargeContext>(MockBehavior.Strict);
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        }

        //private Tesla GetTesla()
        //{
        //    //var httpClient = GetMockedHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        //    var httpClient = GetMockedHttpClient();
        //    return new Tesla(_mockLogger.Object, _mockChargeContext.Object, httpClient);
        //}

        //[TestMethod]
        //public async Task Start()
        //{
        //    var tesla = GetTesla();
        //    await tesla.StartAsync();
        //}
    }
}
