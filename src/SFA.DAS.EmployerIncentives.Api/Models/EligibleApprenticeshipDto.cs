﻿namespace SFA.DAS.EmployerIncentives.Api.Models
{
    public class EligibleApprenticeshipDto
    {
        public long ApprenticeshipId { get; set; }
        public long Uln { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CourseName { get; set; }
    }
}