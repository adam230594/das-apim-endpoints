using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Models;
using SFA.DAS.EmployerIncentives.Application.Queries;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Controllers.EligibleApprenticeshipSearch
{
    public class WhenGettingEligibleApprentices
    {
        [Test, MoqAutoData]
        public async Task Then_Gets_Eligible_Apprentices_From_Mediator(
            long accountId,
            long accountLegalEntityId,
            GetEligibleApprenticeshipsSearchResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Greedy]EligibleApprenticeshipSearchController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.Is<GetEligibleApprenticeshipsSearchQuery>(c=>
                                            c.AccountId.Equals(accountId) 
                                            && c.AccountLegalEntityId.Equals(accountLegalEntityId)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResult);

            var controllerResult = await controller.GetEligibleApprentices(accountId, accountLegalEntityId) as ObjectResult;

            Assert.IsNotNull(controllerResult);
            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var model = controllerResult.Value as EligibleApprenticeshipsResponse;
            Assert.IsNotNull(model);
            model.Apprentices.Should().BeEquivalentTo(mediatorResult.Apprentices, options=>options
                .Excluding(tc=>tc.StartDate)
            );
        }
        
        [Test, MoqAutoData]
        public async Task And_Throws_Exception_Then_Returns_Bad_Request(
            long accountId,
            long accountLegalEntityId,
            GetEligibleApprenticeshipsSearchResult mediatorResult,
            [Frozen] Mock<IMediator> mockMediator,
            [Greedy]EligibleApprenticeshipSearchController controller)
        {
            mockMediator
                .Setup(mediator => mediator.Send(
                    It.IsAny<GetEligibleApprenticeshipsSearchQuery>(),
                    It.IsAny<CancellationToken>()))
                .Throws<InvalidOperationException>();
            
            var controllerResult = await controller.GetEligibleApprentices(accountId, accountLegalEntityId) as StatusCodeResult;

            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}