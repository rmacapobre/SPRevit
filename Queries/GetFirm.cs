using System.Collections.Generic;

namespace Specpoint.Revit2026
{
    public class FirmMembership
    {
        public string firmId;
        public string firmName;
        public string firmRole;
        public string userId;
        public bool isActive;
        public string isPrimary;
        public string firmAccountType;
        public string firmAccountTypeAbbreviation;
        public string firmAccountTypeApplicationURL;
    }

    public class CurrentUserActiveFirm
    {
        public FirmMembership myActiveFirm;
    }

    public class Firm
    {
        public List<ProjectGroup> projectGroups;
        public List<ProfessionalDiscipline> professionalDisciplines;
    }

    public class GetFirm
    {
        public Firm firm;
    }
}
