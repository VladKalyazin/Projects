
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
    
public partial class Comment
{

    public int CommentID { get; set; }

    public Nullable<int> ProjectID { get; set; }

    public Nullable<int> UserID { get; set; }

    public string CommentText { get; set; }



    public virtual Project Project { get; set; }

    public virtual User User { get; set; }

}

}
