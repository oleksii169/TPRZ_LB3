using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Utility;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderHeaderRepository> _mockOrderHeader;
        private readonly Mock<IOrderDetailRepository> _mockOrderDetail;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockOrderHeader = new Mock<IOrderHeaderRepository>();
            _mockOrderDetail = new Mock<IOrderDetailRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _mockUnitOfWork.Setup(u => u.OrderHeader).Returns(_mockOrderHeader.Object);
            _mockUnitOfWork.Setup(u => u.OrderDetail).Returns(_mockOrderDetail.Object);

            _controller = new OrderController(_mockUnitOfWork.Object);
        }

        [Fact]
        public void OrderDetails_ReturnsCorrectViewModel()
        {
            var header = new OrderHeader { Id = 1 };
            var details = new List<OrderDetail> { new OrderDetail { OrderHeaderId = 1 } };

            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), "ApplicationUser"))
                .Returns(header);
            _mockOrderDetail.Setup(r => r.GetAll("Product")).Returns(details);

            var result = _controller.OrderDetails(1);

            Assert.Equal(header, result.OrderHeader);
            Assert.Equal(details, result.OrderDetails);
        }

        [Fact]
        public void SetToInProcess_UpdatesStatus()
        {
            var vm = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };

            _controller.SetToInProcess(vm);

            _mockOrderHeader.Verify(r => r.UpdateStatus(1, OrderStatus.StatusInProcess, null), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void SetToShipped_UpdatesOrderFields()
        {
            var vm = new OrderVM
            {
                OrderHeader = new OrderHeader
                {
                    Id = 1,
                    Carrier = "DHL",
                    TrackingNumber = "123456"
                }
            };
            var dbOrder = new OrderHeader { Id = 1 };
            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), null)).Returns(dbOrder);

            _controller.SetToShipped(vm);

            Assert.Equal("DHL", dbOrder.Carrier);
            Assert.Equal("123456", dbOrder.TrackingNumber);
            Assert.Equal(OrderStatus.StatusShipped, dbOrder.OrderStatus);
            Assert.True(dbOrder.DateOfShipping <= DateTime.Now);

            _mockOrderHeader.Verify(r => r.Update(dbOrder), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void SetToCancelOrder_ApprovedPayment_RefundsAndCancels()
        {
            var vm = new OrderVM
            {
                OrderHeader = new OrderHeader
                {
                    Id = 1,
                    PaymentStatus = PaymentStatus.StatusApproved,
                    PaymentIntentId = "pi_123"
                }
            };
            var dbOrder = vm.OrderHeader;
            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), null)).Returns(dbOrder);

            _controller.SetToCancelOrder(vm);

            _mockOrderHeader.Verify(r => r.UpdateStatus(1, OrderStatus.StatusCancelled, null), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void SetToCancelOrder_UnapprovedPayment_JustCancels()
        {
            var vm = new OrderVM
            {
                OrderHeader = new OrderHeader
                {
                    Id = 2,
                    PaymentStatus = PaymentStatus.StatusPending
                }
            };
            var dbOrder = vm.OrderHeader;
            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), null)).Returns(dbOrder);

            _controller.SetToCancelOrder(vm);

            _mockOrderHeader.Verify(r => r.UpdateStatus(2, OrderStatus.StatusCancelled, null), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }
    }
}













//using Moq;
//using ShoppingCart.DataAccess.Repositories;
//using ShoppingCart.DataAccess.ViewModels;
//using ShoppingCart.Models;
//using ShoppingCart.Utility;
//using ShoppingCart.Web.Areas.Admin.Controllers;
//using Stripe;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using Xunit;

//namespace ShoppingCart.Tests
//{
//    public class OrderControllerTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<IOrderHeaderRepository> _mockOrderHeader;
//        private readonly Mock<IOrderDetailRepository> _mockOrderDetail;
//        private readonly OrderController _controller;

//        public OrderControllerTests()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockOrderHeader = new Mock<IOrderHeaderRepository>();
//            _mockOrderDetail = new Mock<IOrderDetailRepository>();

//            _mockUnitOfWork.Setup(u => u.OrderHeader).Returns(_mockOrderHeader.Object);
//            _mockUnitOfWork.Setup(u => u.OrderDetail).Returns(_mockOrderDetail.Object);

//            _controller = new OrderController(_mockUnitOfWork.Object);
//        }

//        [Fact]
//        public void OrderDetails_ReturnsCorrectViewModel()
//        {
//            var header = new OrderHeader { Id = 1 };
//            var details = new List<OrderDetail> { new OrderDetail { OrderHeaderId = 1 } };

//            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>(), "ApplicationUser")).Returns(header);
//            _mockOrderDetail.Setup(r => r.GetAll("Product")).Returns(details);

//            var result = _controller.OrderDetails(1);

//            Assert.Equal(header, result.OrderHeader);
//            Assert.Equal(details, result.OrderDetails);
//        }

//        [Fact]
//        public void SetToInProcess_UpdatesStatus()
//        {
//            var vm = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };

//            _controller.SetToInProcess(vm);

//            _mockOrderHeader.Verify(r => r.UpdateStatus(1, OrderStatus.StatusInProcess), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void SetToShipped_UpdatesOrderWithTracking()
//        {
//            var order = new OrderHeader { Id = 1 };
//            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>())).Returns(order);

//            var vm = new OrderVM
//            {
//                OrderHeader = new OrderHeader
//                {
//                    Id = 1,
//                    Carrier = "UPS",
//                    TrackingNumber = "123"
//                }
//            };

//            _controller.SetToShipped(vm);

//            Assert.Equal("UPS", order.Carrier);
//            Assert.Equal("123", order.TrackingNumber);
//            Assert.Equal(OrderStatus.StatusShipped, order.OrderStatus);
//            Assert.True(order.DateOfShipping <= DateTime.Now);

//            _mockOrderHeader.Verify(r => r.Update(order), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void SetToCancelOrder_RefundApprovedOrder()
//        {
//            var order = new OrderHeader
//            {
//                Id = 1,
//                PaymentStatus = PaymentStatus.StatusApproved,
//                PaymentIntentId = "pi_123"
//            };

//            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>())).Returns(order);

//            var vm = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };

//            _controller.SetToCancelOrder(vm);

//            _mockOrderHeader.Verify(r => r.UpdateStatus(1, OrderStatus.StatusCancelled), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void SetToCancelOrder_RefundNotApproved()
//        {
//            var order = new OrderHeader
//            {
//                Id = 1,
//                PaymentStatus = PaymentStatus.StatusPending
//            };

//            _mockOrderHeader.Setup(r => r.GetT(It.IsAny<Expression<Func<OrderHeader, bool>>>())).Returns(order);

//            var vm = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };

//            _controller.SetToCancelOrder(vm);

//            _mockOrderHeader.Verify(r => r.UpdateStatus(1, OrderStatus.StatusCancelled), Times.Once);
//            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }
//    }
//}










//using Moq;
//using ShoppingCart.DataAccess.Repositories;
//using ShoppingCart.Models;
//using ShoppingCart.Web.Areas.Admin.Controllers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using Xunit;
//using Microsoft.AspNetCore.Mvc;
//using ShoppingCart.DataAccess.ViewModels;
//using ShoppingCart.Utility;

//namespace ShoppingCart.Tests
//{
//    public class OrderControllerTests
//    {
//        // Метод для швидкого налаштування моків
//        private (Mock<IRepository<OrderHeader>>, Mock<IRepository<OrderDetails>>, Mock<IUnitOfWork>, OrderController) SetupMocks()
//        {
//            var orderHeaderRepositoryMock = new Mock<IRepository<OrderHeader>>();
//            var orderDetailsRepositoryMock = new Mock<IRepository<OrderDetails>>();
//            var mockUnitOfWork = new Mock<IUnitOfWork>();

//            mockUnitOfWork.Setup(uow => uow.OrderHeader).Returns(orderHeaderRepositoryMock.Object);
//            mockUnitOfWork.Setup(uow => uow.OrderDetails).Returns(orderDetailsRepositoryMock.Object);

//            var controller = new OrderController(mockUnitOfWork.Object);
//            return (orderHeaderRepositoryMock, orderDetailsRepositoryMock, mockUnitOfWork, controller);
//        }

//        [Fact]
//        public void Get_ReturnsAllOrderHeaders()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, _, controller) = SetupMocks();
//            var orderHeaders = new List<OrderHeader>
//            {
//                new OrderHeader { Id = 1, Name = "User1", OrderStatus = SD.StatusPending },
//                new OrderHeader { Id = 2, Name = "User2", OrderStatus = SD.StatusApproved }
//            };

//            orderHeaderRepo.Setup(r => r.GetAll(It.IsAny<string>()))
//                .Returns(orderHeaders);

//            // Act
//            var result = controller.Get();

//            // Assert
//            Assert.Equal(orderHeaders, result);
//        }

//        [Fact]
//        public void GetOrder_ExistingId_ReturnsOrderVM()
//        {
//            // Arrange
//            var (orderHeaderRepo, orderDetailsRepo, _, controller) = SetupMocks();
//            int orderId = 1;

//            var orderHeader = new OrderHeader
//            {
//                Id = orderId,
//                Name = "Test User",
//                OrderStatus = SD.StatusPending,
//                OrderDate = DateTime.Now,
//                PhoneNumber = "1234567890",
//                StreetAddress = "123 Test St",
//                City = "Test City",
//                State = "TS",
//                PostalCode = "12345"
//            };

//            var orderDetails = new List<OrderDetails>
//            {
//                new OrderDetails { Id = 1, OrderHeaderId = orderId, ProductId = 1, Count = 2, Price = 19.99 },
//                new OrderDetails { Id = 2, OrderHeaderId = orderId, ProductId = 2, Count = 1, Price = 29.99 }
//            };

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns(orderHeader);

//            orderDetailsRepo.Setup(r => r.GetAll(
//                    It.IsAny<Expression<Func<OrderDetails, bool>>>(),
//                    It.IsAny<string>()))
//                .Returns(orderDetails);

//            // Act
//            var result = controller.Get(orderId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(orderId, result.OrderHeader.Id);
//            Assert.Equal(orderDetails.Count, result.OrderDetails.Count());
//        }

//        [Fact]
//        public void GetOrder_NonExistingId_ReturnsNull()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, _, controller) = SetupMocks();
//            int orderId = 999;

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns((OrderHeader)null);

//            // Act
//            var result = controller.Get(orderId);

//            // Assert
//            Assert.Null(result);
//        }

//        [Fact]
//        public void UpdateOrderHeader_ValidOrderHeader_UpdatesAndReturnsOrderHeader()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            var orderHeader = new OrderHeader
//            {
//                Id = 1,
//                Name = "Updated User",
//                OrderStatus = SD.StatusInProcess
//            };

//            // Act
//            var result = controller.UpdateOrderHeader(orderHeader);

//            // Assert
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Once);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//            Assert.Equal(orderHeader, result);
//        }

//        [Fact]
//        public void StartProcessing_ExistingOrderId_UpdatesStatusAndReturnsTrue()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 1;
//            var orderHeader = new OrderHeader { Id = orderId, OrderStatus = SD.StatusPending };

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns(orderHeader);

//            // Act
//            var result = controller.StartProcessing(orderId);

//            // Assert
//            Assert.True(result);
//            Assert.Equal(SD.StatusInProcess, orderHeader.OrderStatus);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Once);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void StartProcessing_NonExistingOrderId_ReturnsFalse()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 999;

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns((OrderHeader)null);

//            // Act
//            var result = controller.StartProcessing(orderId);

//            // Assert
//            Assert.False(result);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Never);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Never);
//        }

//        [Fact]
//        public void ShipOrder_ExistingOrderWithTrackingNumber_UpdatesStatusAndReturnsTrue()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 1;
//            string trackingNumber = "TRACK123456";
//            string carrier = "UPS";

//            var orderHeader = new OrderHeader
//            {
//                Id = orderId,
//                OrderStatus = SD.StatusInProcess,
//                PaymentStatus = SD.PaymentStatusApproved
//            };

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns(orderHeader);

//            // Act
//            var result = controller.ShipOrder(orderId, trackingNumber, carrier);

//            // Assert
//            Assert.True(result);
//            Assert.Equal(SD.StatusShipped, orderHeader.OrderStatus);
//            Assert.Equal(trackingNumber, orderHeader.TrackingNumber);
//            Assert.Equal(carrier, orderHeader.Carrier);
//            Assert.NotNull(orderHeader.ShippingDate);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Once);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void ShipOrder_NonExistingOrderId_ReturnsFalse()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 999;
//            string trackingNumber = "TRACK123456";
//            string carrier = "UPS";

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns((OrderHeader)null);

//            // Act
//            var result = controller.ShipOrder(orderId, trackingNumber, carrier);

//            // Assert
//            Assert.False(result);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Never);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Never);
//        }

//        [Fact]
//        public void CancelOrder_ExistingPendingOrder_UpdatesStatusAndReturnsTrue()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 1;

//            var orderHeader = new OrderHeader
//            {
//                Id = orderId,
//                OrderStatus = SD.StatusPending,
//                PaymentStatus = SD.PaymentStatusPending
//            };

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns(orderHeader);

//            // Act
//            var result = controller.CancelOrder(orderId);

//            // Assert
//            Assert.True(result);
//            Assert.Equal(SD.StatusCancelled, orderHeader.OrderStatus);
//            Assert.Equal(SD.StatusCancelled, orderHeader.PaymentStatus);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Once);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void CancelOrder_ExistingApprovedOrder_UpdatesStatusAndReturnsTrue()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 1;

//            var orderHeader = new OrderHeader
//            {
//                Id = orderId,
//                OrderStatus = SD.StatusApproved,
//                PaymentStatus = SD.PaymentStatusApproved
//            };

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns(orderHeader);

//            // Act
//            var result = controller.CancelOrder(orderId);

//            // Assert
//            Assert.True(result);
//            Assert.Equal(SD.StatusCancelled, orderHeader.OrderStatus);
//            Assert.Equal(SD.StatusRefunded, orderHeader.PaymentStatus);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Once);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Once);
//        }

//        [Fact]
//        public void CancelOrder_NonExistingOrderId_ReturnsFalse()
//        {
//            // Arrange
//            var (orderHeaderRepo, _, mockUnitOfWork, controller) = SetupMocks();
//            int orderId = 999;

//            orderHeaderRepo.Setup(r => r.Get(h => h.Id == orderId, It.IsAny<string>()))
//                .Returns((OrderHeader)null);

//            // Act
//            var result = controller.CancelOrder(orderId);

//            // Assert
//            Assert.False(result);
//            orderHeaderRepo.Verify(r => r.Update(It.IsAny<OrderHeader>()), Times.Never);
//            mockUnitOfWork.Verify(u => u.Save(), Times.Never);
//        }
//    }
//}