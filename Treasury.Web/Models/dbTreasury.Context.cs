﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Treasury.Web.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class dbTreasuryEntities : DbContext
    {
        public dbTreasuryEntities()
            : base("name=dbTreasuryEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<APLY_REC_HIS> APLY_REC_HIS { get; set; }
        public virtual DbSet<AUTH_APLY_REC> AUTH_APLY_REC { get; set; }
        public virtual DbSet<AUTH_APPR> AUTH_APPR { get; set; }
        public virtual DbSet<CODE_FUNC> CODE_FUNC { get; set; }
        public virtual DbSet<CODE_ROLE_FUNC> CODE_ROLE_FUNC { get; set; }
        public virtual DbSet<CODE_ROLE_FUNC_HIS> CODE_ROLE_FUNC_HIS { get; set; }
        public virtual DbSet<CODE_ROLE_ITEM> CODE_ROLE_ITEM { get; set; }
        public virtual DbSet<CODE_ROLE_ITEM_HIS> CODE_ROLE_ITEM_HIS { get; set; }
        public virtual DbSet<CODE_ROLE_TREA_ITEM> CODE_ROLE_TREA_ITEM { get; set; }
        public virtual DbSet<CODE_ROLE_TREA_ITEM_HIS> CODE_ROLE_TREA_ITEM_HIS { get; set; }
        public virtual DbSet<CODE_USER_HIS> CODE_USER_HIS { get; set; }
        public virtual DbSet<DEP_CHK_ITEM> DEP_CHK_ITEM { get; set; }
        public virtual DbSet<DEP_CHK_ITEM_HIS> DEP_CHK_ITEM_HIS { get; set; }
        public virtual DbSet<ITEM_CHARGE_UNIT> ITEM_CHARGE_UNIT { get; set; }
        public virtual DbSet<ITEM_OTHER> ITEM_OTHER { get; set; }
        public virtual DbSet<ITEM_REAL_ESTATE> ITEM_REAL_ESTATE { get; set; }
        public virtual DbSet<ITEM_SEAL> ITEM_SEAL { get; set; }
        public virtual DbSet<MAIL_CONTENT> MAIL_CONTENT { get; set; }
        public virtual DbSet<MAIL_CONTENT_HIS> MAIL_CONTENT_HIS { get; set; }
        public virtual DbSet<MAIL_RECEIVE> MAIL_RECEIVE { get; set; }
        public virtual DbSet<MAIL_RECEIVE_HIS> MAIL_RECEIVE_HIS { get; set; }
        public virtual DbSet<MAIL_TIME_HIS> MAIL_TIME_HIS { get; set; }
        public virtual DbSet<PIA_EXEC_TYPE> PIA_EXEC_TYPE { get; set; }
        public virtual DbSet<PIA_LOG_MAIN> PIA_LOG_MAIN { get; set; }
        public virtual DbSet<PIA_TRACK_TYPE> PIA_TRACK_TYPE { get; set; }
        public virtual DbSet<PIA_TRN_HIST> PIA_TRN_HIST { get; set; }
        public virtual DbSet<SYS_CODE> SYS_CODE { get; set; }
        public virtual DbSet<SYS_SEQ> SYS_SEQ { get; set; }
        public virtual DbSet<TREA_APLY_REC> TREA_APLY_REC { get; set; }
        public virtual DbSet<TREA_APLY_TEMP> TREA_APLY_TEMP { get; set; }
        public virtual DbSet<TREA_EQUIP> TREA_EQUIP { get; set; }
        public virtual DbSet<TREA_EQUIP_HIS> TREA_EQUIP_HIS { get; set; }
        public virtual DbSet<TREA_ITEM> TREA_ITEM { get; set; }
        public virtual DbSet<TREA_ITEM_HIS> TREA_ITEM_HIS { get; set; }
        public virtual DbSet<ITEM_CHARGE_UNIT_HIS> ITEM_CHARGE_UNIT_HIS { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<CODE_USER> CODE_USER { get; set; }
        public virtual DbSet<CODE_USER_ROLE> CODE_USER_ROLE { get; set; }
        public virtual DbSet<CODE_USER_ROLE_HIS> CODE_USER_ROLE_HIS { get; set; }
        public virtual DbSet<CODE_ROLE> CODE_ROLE { get; set; }
        public virtual DbSet<CODE_ROLE_HIS> CODE_ROLE_HIS { get; set; }
        public virtual DbSet<ITEM_BOOK> ITEM_BOOK { get; set; }
        public virtual DbSet<BLANK_NOTE_APLY> BLANK_NOTE_APLY { get; set; }
        public virtual DbSet<ITEM_BLANK_NOTE> ITEM_BLANK_NOTE { get; set; }
        public virtual DbSet<ITEM_CA> ITEM_CA { get; set; }
        public virtual DbSet<ITEM_DEP_ORDER_M> ITEM_DEP_ORDER_M { get; set; }
        public virtual DbSet<ITEM_DEP_RECEIVED> ITEM_DEP_RECEIVED { get; set; }
        public virtual DbSet<ITEM_REFUNDABLE_DEP> ITEM_REFUNDABLE_DEP { get; set; }
        public virtual DbSet<ITEM_STOCK> ITEM_STOCK { get; set; }
        public virtual DbSet<TREA_OPEN_REC> TREA_OPEN_REC { get; set; }
        public virtual DbSet<INVENTORY_CHG_APLY> INVENTORY_CHG_APLY { get; set; }
        public virtual DbSet<MAIL_TIME> MAIL_TIME { get; set; }
        public virtual DbSet<SYS_JOB_REC> SYS_JOB_REC { get; set; }
        public virtual DbSet<ITEM_IMPO> ITEM_IMPO { get; set; }
        public virtual DbSet<OTHER_ITEM_APLY> OTHER_ITEM_APLY { get; set; }
        public virtual DbSet<ITEM_DEP_ORDER_D> ITEM_DEP_ORDER_D { get; set; }
    }
}
