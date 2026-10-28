using System;
using System.Collections.Generic;

namespace Domain.Entity_Najm;

public partial class PublicationsRegisterBook
{
    public int Id { get; set; }

    public int? AssessorId { get; set; }

    public string Bookname { get; set; } = null!;

    public int Booktype { get; set; }

    public int? Pages { get; set; }

    public int? Words { get; set; }

    public string? Others { get; set; }

    public DateOnly? AuthorSendDate { get; set; }

    public DateOnly? AuthorReciveDate { get; set; }

    public DateOnly? HeadmasterSendDate { get; set; }

    public DateOnly? HeadmasterReciveDate { get; set; }

    public bool? IsOwner { get; set; }

    public DateOnly? ReciveBookDate { get; set; }

    public string? CostDescription { get; set; }

    public virtual PublicationsRegisterAssessor? Assessor { get; set; }

    public virtual ICollection<PublicationCost> PublicationCosts { get; set; } = new List<PublicationCost>();

    public virtual ICollection<PublicationFactorItem> PublicationFactorItems { get; set; } = new List<PublicationFactorItem>();

    public virtual ICollection<PublicationIndexing> PublicationIndexings { get; set; } = new List<PublicationIndexing>();

    public virtual ICollection<PublicationStore> PublicationStores { get; set; } = new List<PublicationStore>();

    public virtual ICollection<PublicationVerification> PublicationVerifications { get; set; } = new List<PublicationVerification>();

    public virtual ICollection<PublicationsBookEdit> PublicationsBookEdits { get; set; } = new List<PublicationsBookEdit>();

    public virtual ICollection<PublicationsDesign> PublicationsDesigns { get; set; } = new List<PublicationsDesign>();

    public virtual ICollection<PublicationsPaging> PublicationsPagings { get; set; } = new List<PublicationsPaging>();

    public virtual ICollection<PublicationsPermission> PublicationsPermissions { get; set; } = new List<PublicationsPermission>();

    public virtual ICollection<PublicationsPrinting> PublicationsPrintings { get; set; } = new List<PublicationsPrinting>();

    public virtual ICollection<PublicationsRepository> PublicationsRepositories { get; set; } = new List<PublicationsRepository>();
}
