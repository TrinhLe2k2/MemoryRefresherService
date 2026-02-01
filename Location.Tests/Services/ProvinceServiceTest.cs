using Location.Models;
using Location.Repositories.Interfaces;
using Location.Services.Implements;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Tests.Services
{
    public class ProvinceServiceTest
    {
        private readonly Mock<IProvinceRepository> _provinceRepositoryMock;
        public ProvinceServiceTest()
        {
            _provinceRepositoryMock = new Mock<IProvinceRepository>();
        }

        [Fact(DisplayName = "CreateProvince - Return dbResult 0 when repository less than one")]
        public async Task CreateProvince_ReturnDbResult_WhenRepositoryLessThanOne()
        {
            // Arrange
            var createModel = new CreateProvince { Name = "Test", Code = "TST", User = "unit-test" };
            _provinceRepositoryMock.Setup(e => e.CreateProvince(createModel)).ReturnsAsync(0);

            // Act
            var dbResult = await _provinceRepositoryMock.Object.CreateProvince(createModel);

            // Assert
            Assert.Equal(0, dbResult);

            _provinceRepositoryMock.Verify(e => e.CreateProvince(createModel), Times.Once);
        }
    }
}
