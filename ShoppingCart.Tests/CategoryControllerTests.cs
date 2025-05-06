using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(uow => uow.Category).Returns(_mockCategoryRepo.Object);
            _controller = new CategoryController(_mockUnitOfWork.Object);
        }

        [Fact]
        public void GetCategories_All_ReturnAllCategories()
        {
            _mockCategoryRepo.Setup(r => r.GetAll(null)).Returns(CategoryDataset.Categories);

            var result = _controller.Get();

            Assert.Equal(CategoryDataset.Categories, result.Categories);
        }

        [Fact]
        public void GetCategory_ById_ReturnSingleCategory()
        {
            var category = new Category { Id = 1, Name = "Test" };
            _mockCategoryRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), null))
                .Returns(category);

            var result = _controller.Get(1);

            Assert.Equal(category, result.Category);
        }

        [Fact]
        public void CreateUpdate_NewCategory_AddsAndSaves()
        {
            var category = new Category { Id = 0, Name = "New Category" };
            var vm = new CategoryVM { Category = category };

            _controller.ModelState.Clear(); // Valid model

            _controller.CreateUpdate(vm);

            _mockCategoryRepo.Verify(r => r.Add(category), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_ExistingCategory_UpdatesAndSaves()
        {
            var category = new Category { Id = 5, Name = "Updated" };
            var vm = new CategoryVM { Category = category };

            _controller.ModelState.Clear();

            _controller.CreateUpdate(vm);

            _mockCategoryRepo.Verify(r => r.Update(category), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_InvalidModel_ThrowsException()
        {
            var vm = new CategoryVM { Category = new Category() };
            _controller.ModelState.AddModelError("Name", "Required");

            Assert.Throws<Exception>(() => _controller.CreateUpdate(vm));
        }

        [Fact]
        public void DeleteData_ValidId_DeletesCategory()
        {
            var category = new Category { Id = 1 };
            _mockCategoryRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), null)).Returns(category);

            _controller.DeleteData(1);

            _mockCategoryRepo.Verify(r => r.Delete(category), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void DeleteData_InvalidId_ThrowsException()
        {
            _mockCategoryRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), null)).Returns((Category)null);

            Assert.Throws<Exception>(() => _controller.DeleteData(100));
        }
    }
}





//using Moq;
//using ShoppingCart.DataAccess.Repositories;
//using ShoppingCart.DataAccess.ViewModels;
//using ShoppingCart.Models;
//using ShoppingCart.Tests.Datasets;
//using ShoppingCart.Web.Areas.Admin.Controllers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using Xunit;

//namespace ShoppingCart.Tests
//{
//    public class CategoryControllerTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
//        private readonly CategoryController _controller;

//        public CategoryControllerTests()
//        {
//            _mockCategoryRepo = new Mock<ICategoryRepository>();
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockUnitOfWork.Setup(uow => uow.Category).Returns(_mockCategoryRepo.Object);
//            _controller = new CategoryController(_mockUnitOfWork.Object);
//        }

//        [Fact]
//        public void Get_All_ReturnsAllCategories()
//        {
//            // Arrange
//            _mockCategoryRepo.Setup(r => r.GetAll(It.IsAny<string>())).Returns(CategoryDataset.Categories);

//            // Act
//            var result = _controller.Get();

//            // Assert
//            Assert.Equal(CategoryDataset.Categories, result.Categories);
//        }

//        [Fact]
//        public void Get_ById_ReturnsCorrectCategory()
//        {
//            var category = new Category { Id = 1, Name = "Test" };
//            _mockCategoryRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>())).Returns(category);

//            var result = _controller.Get(1);

//            Assert.Equal(category, result.Category);
//        }

//        [Fact]
//        public void CreateUpdate_NewCategory_CallsAdd()
//        {
//            var vm = new CategoryVM { Category = new Category { Id = 0, Name = "New" } };

//            _controller.CreateUpdate(vm);

//            _mockCategoryRepo.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void CreateUpdate_UpdateCategory_CallsUpdate()
//        {
//            var vm = new CategoryVM { Category = new Category { Id = 5, Name = "Update" } };

//            _controller.CreateUpdate(vm);

//            _mockCategoryRepo.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void CreateUpdate_InvalidModel_ThrowsException()
//        {
//            var vm = new CategoryVM { Category = new Category { Id = 0 } };
//            _controller.ModelState.AddModelError("Name", "Required");

//            Assert.Throws<Exception>(() => _controller.CreateUpdate(vm));
//        }

//        [Fact]
//        public void DeleteData_ValidId_CallsDelete()
//        {
//            var category = new Category { Id = 3 };
//            _mockCategoryRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>())).Returns(category);

//            _controller.DeleteData(3);

//            _mockCategoryRepo.Verify(r => r.Delete(category), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void DeleteData_NotFound_ThrowsException()
//        {
//            _mockCategoryRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>())).Returns((Category)null);

//            Assert.Throws<Exception>(() => _controller.DeleteData(999));
//        }
//    }
//}




//using Moq;
//using ShoppingCart.DataAccess.Repositories;
//using ShoppingCart.DataAccess.ViewModels;
//using ShoppingCart.Models;
//using ShoppingCart.Tests.Datasets;
//using ShoppingCart.Web.Areas.Admin.Controllers;
//using System.Linq.Expressions;
//using Xunit;

//namespace ShoppingCart.Tests
//{
//    public class CategoryControllerTests
//    {
//        [Fact]
//        public void GetCategories_All_ReturnAllCategories()
//        {
//            // Arrange
//            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();

//            repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
//                .Returns(() => CategoryDataset.Categories);
//            var mockUnitOfWork = new Mock<IUnitOfWork>();
//            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
//            var controller = new CategoryController(mockUnitOfWork.Object);

//            // Act
//            var result = controller.Get();

//            // Assert
//            Assert.Equal(CategoryDataset.Categories, result.Categories);
//        }
//    }
//}