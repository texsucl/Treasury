﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FGL.Web.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dbFGLEntities : DbContext
    {
        public dbFGLEntities()
            : base("name=dbFGLEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<PIA_EXEC_TYPE> PIA_EXEC_TYPE { get; set; }
        public virtual DbSet<PIA_LOG_MAIN> PIA_LOG_MAIN { get; set; }
        public virtual DbSet<PIA_TRACK_TYPE> PIA_TRACK_TYPE { get; set; }
        public virtual DbSet<PIA_TRN_HIST> PIA_TRN_HIST { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<CODE_ROLE> CODE_ROLE { get; set; }
        public virtual DbSet<CODE_ROLE_HIS> CODE_ROLE_HIS { get; set; }
        public virtual DbSet<CODE_USER> CODE_USER { get; set; }
        public virtual DbSet<CODE_USER_HIS> CODE_USER_HIS { get; set; }
        public virtual DbSet<CODE_USER_ROLE> CODE_USER_ROLE { get; set; }
        public virtual DbSet<CODE_USER_ROLE_HIS> CODE_USER_ROLE_HIS { get; set; }
        public virtual DbSet<SYS_SEQ> SYS_SEQ { get; set; }
        public virtual DbSet<CODE_SYS_INFO> CODE_SYS_INFO { get; set; }
        public virtual DbSet<CODE_ROLE_FUNC> CODE_ROLE_FUNC { get; set; }
        public virtual DbSet<CODE_FUNC> CODE_FUNC { get; set; }
        public virtual DbSet<SYS_CODE> SYS_CODE { get; set; }
        public virtual DbSet<FGL_APLY_REC> FGL_APLY_REC { get; set; }
        public virtual DbSet<FGL_ITEM_ACCT> FGL_ITEM_ACCT { get; set; }
        public virtual DbSet<FGL_ITEM_ACCT_HIS> FGL_ITEM_ACCT_HIS { get; set; }
        public virtual DbSet<FGL_ITEM_CODE_TRAN> FGL_ITEM_CODE_TRAN { get; set; }
        public virtual DbSet<FGL_ITEM_CODE_TRAN_HIS> FGL_ITEM_CODE_TRAN_HIS { get; set; }
        public virtual DbSet<FGL_ITEM_SMPNUM> FGL_ITEM_SMPNUM { get; set; }
        public virtual DbSet<FGL_ITEM_SMPNUM_HIS> FGL_ITEM_SMPNUM_HIS { get; set; }
        public virtual DbSet<FGL_SMPA> FGL_SMPA { get; set; }
        public virtual DbSet<FGL_SMPA_HIS> FGL_SMPA_HIS { get; set; }
        public virtual DbSet<FGL_SMPB> FGL_SMPB { get; set; }
        public virtual DbSet<FGL_SMPB_HIS> FGL_SMPB_HIS { get; set; }
        public virtual DbSet<FGL_SMP_NUM_RULE> FGL_SMP_NUM_RULE { get; set; }
        public virtual DbSet<FGL_SMP_NUM_SEQ> FGL_SMP_NUM_SEQ { get; set; }
        public virtual DbSet<SYS_PARA> SYS_PARA { get; set; }
        public virtual DbSet<FGL_SCHEDULE_JOB> FGL_SCHEDULE_JOB { get; set; }
        public virtual DbSet<FGL_ITEM_INFO> FGL_ITEM_INFO { get; set; }
        public virtual DbSet<FGL_ITEM_INFO_HIS> FGL_ITEM_INFO_HIS { get; set; }
        public virtual DbSet<FGL_GITM_HIS> FGL_GITM_HIS { get; set; }
        public virtual DbSet<FGL_PAY_SUB_HIS> FGL_PAY_SUB_HIS { get; set; }
        public virtual DbSet<FGL_PAY_MAIN_HIS> FGL_PAY_MAIN_HIS { get; set; }
    }
}
