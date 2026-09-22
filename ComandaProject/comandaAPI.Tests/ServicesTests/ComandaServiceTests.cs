using comandaAPI.Domain.Exception;
using comandaAPI.Models.DTOs.Request;
using comandaAPI.Models.DTOs.Response;
using comandaAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace comandaAPI.Tests.ServicesTests
{
    public class ComandaServiceTests
    {
        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task ProcessCommand_WhenApiGroqReturnHttpError_MustThrowExternalServiceException(HttpStatusCode statusCode)
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GROQ_API_KEY"]).Returns("chave-falsa");
            var loggerMock = new Mock<ILogger<ComandaService>>();
            var service = new ComandaService(httpClient, configMock.Object, loggerMock.Object);

            var request = new ComandaRequest
            {
                Text = "Um pastel e um caldo de cana"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ExternalServiceException>(() =>
                service.ProcessarComanda(request));
        }

        [Fact]
        public async Task ProcessCommand_WhenApiGroqReturnSuccess_MustReturnComandaResponse()
        {
            // Arrange
            var comandaObj = new
            {
                Nome = "Gabriel",
                Pedidos = new[]
                {
        new { Item = "Acai", Tamanho = "500ml", Acompanhamentos = Array.Empty<string>() },
        new { Item = "Gelinho", Tamanho = "Pequeno", Acompanhamentos = Array.Empty<string>() }
    },
                Valor = "R$ 15,00",
                FormaDePagamento = "Pix",
                Endereço = "Rua Você está lendo isso?, 123"
            };

            var jsonComandaInterno = System.Text.Json.JsonSerializer.Serialize(comandaObj);

            // Arrange the mock response from the GROQ API
            var groqResponseObj = new
            {
                choices = new[]
                {
        new
        {
            message = new
            {
                content = jsonComandaInterno
            }
        }
    }
            };

            var groqResponseJson = System.Text.Json.JsonSerializer.Serialize(groqResponseObj);

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(groqResponseJson, System.Text.Encoding.UTF8, "application/json")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["GROQ_API_KEY"]).Returns("chave-falsa");
            var loggerMock = new Mock<ILogger<ComandaService>>();
            var service = new ComandaService(httpClient, configMock.Object, loggerMock.Object);

            var request = new ComandaRequest
            {
                Text = "Um acai de 500ml e um gelinho pequeno"
            };

            // Act
            var response = await service.ProcessarComanda(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal("Gabriel", response.Nome);
            Assert.Equal(2, response.Pedidos.Count);
            Assert.Equal("Acai", response.Pedidos[0].Item);
        }
    }
}
