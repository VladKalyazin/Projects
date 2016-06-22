
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace DatabaseEntities
{

using System;
    using System.Collections.Generic;
    
public partial class Defect
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Defect()
    {

        this.DefectAttachments = new HashSet<DefectAttachment>();

    }


    public int DefectID { get; set; }

    public string Subject { get; set; }

    public Nullable<int> ProjectID { get; set; }

    public Nullable<int> AssigneeUserID { get; set; }

    public Nullable<int> DefectPriorityID { get; set; }

    public Nullable<int> DefectStatusID { get; set; }

    public Nullable<System.DateTime> CreationDate { get; set; }

    public Nullable<System.DateTime> ModificationDate { get; set; }

    public string Description { get; set; }



    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<DefectAttachment> DefectAttachments { get; set; }

    public virtual DefectPriority DefectPriority { get; set; }

    public virtual Project Project { get; set; }

    public virtual DefectStatus DefectStatus { get; set; }

    public virtual User User { get; set; }

}

}
