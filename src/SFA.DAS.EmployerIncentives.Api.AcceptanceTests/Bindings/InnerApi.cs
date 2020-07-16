﻿using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "innerApi")]
    public class InnerApi
    {
        private readonly TestContext _context;

        public InnerApi(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 1)]
        public void InitialiseInnerApi()
        {
            _context.InnerApi = new MockApi();
        }
    }
}
