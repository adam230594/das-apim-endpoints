using System.Collections.Generic;
using SFA.DAS.FindApprenticeshipTraining.InnerApi.Responses;

namespace SFA.DAS.FindApprenticeshipTraining.Application.TrainingCourses.Queries.GetTrainingCourseProviders
{
    public class GetTrainingCourseProvidersResult 
    {
        public GetStandardsListItem Course { get ; set ; }
        public IEnumerable<GetProvidersListItem> Providers { get ; set ; }
        public int Total { get ; set ; }
    }
}