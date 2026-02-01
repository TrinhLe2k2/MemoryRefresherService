using Location.Infrastructures.Elasticsearch;
using Location.Infrastructures.Redis;
using Location.Models;
using Location.Repositories.Interfaces;
using Location.Services.Implements;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Location.Tests.Services
{
    // Unit tests cho ProvinceService (file tên là LocationServiceTests.cs)
    public class LocationServiceTests
    {
        private readonly Mock<IProvinceRepository> _repoMock;
        private readonly Mock<IRedisCacheUsingMultiplexer> _cacheMock;
        private readonly Mock<IElasticsearchService<DetailProvince>> _esMock;
        private readonly Mock<ILogger<ProvinceService>> _loggerMock;
        private readonly ProvinceService _service;

        public LocationServiceTests()
        {
            // Khởi tạo mock và service dùng chung cho các test
            _repoMock = new Mock<IProvinceRepository>();
            _cacheMock = new Mock<IRedisCacheUsingMultiplexer>();
            _esMock = new Mock<IElasticsearchService<DetailProvince>>();
            _loggerMock = new Mock<ILogger<ProvinceService>>();

            _service = new ProvinceService(
                _repoMock.Object,
                _cacheMock.Object,
                _esMock.Object,
                _loggerMock.Object
            );
        }

        [Fact(DisplayName = "CreateProvince - returns id when repository succeeds")]
        public async Task CreateProvince_ReturnsId_WhenRepositorySucceeds()
        {
            // Arrange: repository trả về id > 0, repo.GetProvince trả về DetailProvince
            var createdId = 42;
            var createModel = new CreateProvince { Name = "Test", Code = "TST", User = "unit-test" };
            _repoMock.Setup(r => r.CreateProvince(createModel)).ReturnsAsync(createdId);

            var expectedDetail = new DetailProvince { Id = createdId, Name = "Test", Code = "TST" };
            // Khi service gọi GetProvince (nội bộ) sẽ gọi elasticsearch trước (trả null) rồi fallback repo.GetProvince
            _esMock.Setup(e => e.GetDocumentByIdAsync(createdId.ToString())).ReturnsAsync((DetailProvince?)null);
            _repoMock.Setup(r => r.GetProvince(createdId)).ReturnsAsync(expectedDetail);

            // Act
            var result = await _service.CreateProvince(createModel);

            // Assert
            Assert.Equal(23, result);
            _repoMock.Verify(r => r.CreateProvince(createModel), Times.Once);
            _repoMock.Verify(r => r.GetProvince(createdId), Times.Once);
        }

        [Fact(DisplayName = "CreateProvince - returns 0 when repository returns 0 and does not call GetProvince")]
        public async Task CreateProvince_ReturnsZero_WhenRepositoryReturnsZero()
        {
            // =========================
            // ARRANGE
            // =========================

            // Tạo model đầu vào cho hàm CreateProvince
            // Giả lập trường hợp dữ liệu hợp lệ nhưng repository không tạo được record
            // (ví dụ: insert thất bại, constraint, hoặc business rule nào đó)
            var createModel = new CreateProvince
            {
                Name = "Nope",
                Code = "NOP",
                User = "unit-test"
            };

            // Setup mock repository:
            // Khi service gọi repo.CreateProvince(createModel)
            // thì trả về 0 → biểu thị "không tạo được province"
            _repoMock
                .Setup(r => r.CreateProvince(createModel))
                .ReturnsAsync(0);

            // =========================
            // ACT
            // =========================

            // Gọi hàm cần test trong service
            // Vì repo trả về 0 nên service được kỳ vọng sẽ:
            // - không gọi GetProvince
            // - trả về 0 ngay lập tức

            var result = await _service.CreateProvince(createModel);

            // =========================
            // ASSERT
            // =========================

            // Kiểm tra giá trị trả về:
            // Service phải trả đúng 0 khi repository không tạo được dữ liệu
            Assert.Equal(0, result);

            // Verify rằng CreateProvince của repository
            // được gọi đúng 1 lần với đúng input
            _repoMock.Verify(
                r => r.CreateProvince(createModel),
                Times.Once
            );

            // Verify rằng GetProvince KHÔNG BAO GIỜ được gọi
            // Điều này đảm bảo service có "short-circuit logic":
            // Nếu dbResult < 1 thì không thực hiện các bước xử lý tiếp theo
            _repoMock.Verify(
                r => r.GetProvince(It.IsAny<int>()),
                Times.Never
            );
        }


        [Fact(DisplayName = "GetProvince - returns from Elasticsearch when found")]
        public async Task GetProvince_ReturnsFromElasticsearch_WhenFound()
        {
            // Arrange: Elasticsearch trả về DetailProvince => repository không được gọi
            var id = 11;
            var esDetail = new DetailProvince { Id = id, Name = "ES", Code = "ES" };
            _esMock.Setup(e => e.GetDocumentByIdAsync(id.ToString())).ReturnsAsync(esDetail);

            // Act
            var result = await _service.GetProvince(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(esDetail.Id, result!.Id);
            _esMock.Verify(e => e.GetDocumentByIdAsync(id.ToString()), Times.Once);
            _repoMock.Verify(r => r.GetProvince(It.IsAny<int>()), Times.Never);
        }

        [Fact(DisplayName = "GetProvinces - returns cached value when cache hit")]
        public async Task GetProvinces_ReturnsCached_WhenCacheHit()
        {
            // Arrange: Cache có dữ liệu => không gọi ES/DB
            var keyword = "key";
            var cached = new List<DetailProvince>
            {
                new DetailProvince { Id = 1, Name = "C1", Code = "C1" }
            };
            _cacheMock.Setup(c => c.GetAsync<IEnumerable<DetailProvince>>($"provinces:{keyword}"))
                      .ReturnsAsync((IEnumerable<DetailProvince>?)cached);

            // Act
            var result = await _service.GetProvinces(keyword);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _cacheMock.Verify(c => c.GetAsync<IEnumerable<DetailProvince>>($"provinces:{keyword}"), Times.Once);
            _esMock.Verify(e => e.GetAllDocumentsAsync(It.IsAny<string?>()), Times.Never);
            _repoMock.Verify(r => r.GetProvinces(It.IsAny<string?>()), Times.Never);
        }
    }
}