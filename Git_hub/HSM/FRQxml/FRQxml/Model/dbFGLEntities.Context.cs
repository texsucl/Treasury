﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FRQxml.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dbGLEntities : DbContext
    {
        public dbGLEntities()
            : base("name=dbGLEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<SYS_PARA> SYS_PARA { get; set; }
        public virtual DbSet<SYS_SEQ> SYS_SEQ { get; set; }
        public virtual DbSet<FRT_XML_522657> FRT_XML_522657 { get; set; }
        public virtual DbSet<FRT_XML_R_622685> FRT_XML_R_622685 { get; set; }
        public virtual DbSet<FRT_XML_T_622685> FRT_XML_T_622685 { get; set; }
        public virtual DbSet<FRT_XML_T_622685_NEW> FRT_XML_T_622685_NEW { get; set; }
        public virtual DbSet<FRT_XML_R_622685_NEW> FRT_XML_R_622685_NEW { get; set; }
    }
}