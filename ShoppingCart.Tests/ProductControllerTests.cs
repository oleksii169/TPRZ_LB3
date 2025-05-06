using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;
using System;

namespace ShoppingCart.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockProductRepo = new Mock<IProductRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            _mockUnitOfWork.Setup(u => u.Product).Returns(_mockProductRepo.Object);
            _mockUnitOfWork.Setup(u => u.Category).Returns(_mockCategoryRepo.Object);

            _controller = new ProductController(_mockUnitOfWork.Object, _mockWebHostEnvironment.Object);
        }

        [Fact]
        public void Get_IdIsNull_ReturnsEmptyProductVM()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetAll(null)).Returns(new List<Category>
            {
                new Category { Id = 1, Name = "Cat1" },
                new Category { Id = 2, Name = "Cat2" }
            });

            // Act
            var result = _controller.Get(null);

            // Assert
            Assert.NotNull(result.Product);
            Assert.Equal(2, result.Categories.Count());
        }

        [Fact]
        public void Get_ProductFound_ReturnsProductVM()
        {
            // Arrange
            int productId = 1;
            var product = new Product { Id = productId, Name = "Test" };

            _mockCategoryRepo.Setup(r => r.GetAll(null)).Returns(new List<Category>());
            _mockProductRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Product, bool>>>(), null))
                .Returns(product);

            // Act
            var result = _controller.Get(productId);

            // Assert
            Assert.Equal(productId, result.Product.Id);
        }

        [Fact]
        public void Get_ProductNotFound_ThrowsException()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetAll(null)).Returns(new List<Category>());
            _mockProductRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Product, bool>>>(), null))
                .Returns((Product)null);

            // Act & Assert
            Assert.Throws<Exception>(() => _controller.Get(1));
        }

        [Fact]
        public void CreateUpdate_NewProduct_AddCalled()
        {
            // Arrange
            var product = new Product { Id = 0, Name = "New Product" };
            var vm = new ProductVM { Product = product };
            _controller.ModelState.Clear();

            // Act
            _controller.CreateUpdate(vm);

            // Assert
            _mockProductRepo.Verify(r => r.Add(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_ExistingProduct_UpdateCalled()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Existing Product" };
            var vm = new ProductVM { Product = product };
            _controller.ModelState.Clear();

            // Act
            _controller.CreateUpdate(vm);

            // Assert
            _mockProductRepo.Verify(r => r.Update(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_InvalidModel_ThrowsException()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Invalid model");
            var vm = new ProductVM();

            // Act & Assert
            Assert.Throws<Exception>(() => _controller.CreateUpdate(vm));
        }

        [Fact]
        public void Delete_ProductExists_ReturnsSuccess()
        {
            // Arrange
            var product = new Product { Id = 1 };
            _mockProductRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Product, bool>>>(), null))
                .Returns(product);

            // Act
            var result = _controller.Delete(1) as JsonResult;

            // Assert
            Assert.True((bool)result.Value.GetType().GetProperty("success").GetValue(result.Value));
        }

        [Fact]
        public void Delete_ProductNotFound_ReturnsFailure()
        {
            // Arrange
            _mockProductRepo.Setup(r => r.GetT(It.IsAny<Expression<Func<Product, bool>>>(), null))
                .Returns((Product)null);

            // Act
            var result = _controller.Delete(1) as JsonResult;

            // Assert
            Assert.False((bool)result.Value.GetType().GetProperty("success").GetValue(result.Value));
        }
    }
}
