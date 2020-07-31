using SFA.DAS.SharedOuterApi.Interfaces;

namespace SFA.DAS.EmployerIncentives.InnerApi.Requests
{
    public class PatchSignAgreementRequest : IPatchApiRequest
    {
        private readonly long _accountId;
        private readonly long _accountLegalEntityId;

        public PatchSignAgreementRequest(long accountId, long accountLegalEntityId)
        {
            _accountId = accountId;
            _accountLegalEntityId = accountLegalEntityId;
        }

        public string BaseUrl { get; set; }
        public string PatchUrl => $"{BaseUrl}accounts/{_accountId}/legalentities/{_accountLegalEntityId}";
        public object Data { get; set; }
    }

    public class SignAgreementRequest
    {
        public int AgreementVersion { get; set; }
    }
}