
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using FGL.Web.ViewModels;


namespace FGL.Web.Daos
{
    public class XAC279350738548Dao
    {
        public void insert(List<ItemWanpieModel> wanpieList, SqlConnection conn, SqlTransaction transaction)
        {

            try
            {

                string sql = @"INSERT INTO XAC279350738548
                   (
 APLY_NO
,FLAG
,CORP_NO
,VOUCHER_NO
,VOUCHER_DATE
,START_DATE
,ENTRY_TIME
,A_SYS_TYPE
,A_ACTNUM_LASTSIX
,A_INSUR_POLICY_ITEM
,A_INSUR_POLICY_OBJECT_NO
,A_INSUR_POLICY_OBJECT_NAME
,A_INSUR_POLICY_MAJOR_CATEGORIES
,A_INSUR_POLICY_SUBDIVISIONS
,A_INSUR_POLICY_PERSON_GROUP
,A_CONTRACT_TYPE
,A_INSUR_POLICY_TRADITION
,A_INSUR_POLICY_BUSINESS_OBJECT
,A_INSUR_POLICY_MAIN_OR_RIDER
,A_INSUR_POLICY_OBJECT_TYPE_1
,A_INSUR_POLICY_OBJECT_TYPE_2
,A_INSUR_POLICY_MAIN_INSUR_TYPE
,A_INSUR_POLICY_DANGER_TYPE
,A_INSUR_POLICY_IS_PAR
,DETAIL_REMARK
,END_DATE
,FIELD_CHA_1
,FIELD_CHA_2
,FIELD_CHA_3
,FIELD_CHA_4
,FIELD_CHA_5
,A_INSUR_POLICY_INDEX
,A_INSUR_POLICY_STRATEGY_OBJECT
,A_INSUR_POLICY_RAPID_REPORT_CATEGORY_1
,A_INVESTMENT_INSURANCE_CONTRACT
,A_YM1_CODE
,A_EXAMINE_SPECIAL_PAYMENT
) VALUES (
 @APLY_NO
,@FLAG
,@CORP_NO
,@VOUCHER_NO
,@VOUCHER_DATE
,@START_DATE
,@ENTRY_TIME
,@A_SYS_TYPE
,@A_ACTNUM_LASTSIX
,@A_INSUR_POLICY_ITEM
,@A_INSUR_POLICY_OBJECT_NO
,@A_INSUR_POLICY_OBJECT_NAME
,@A_INSUR_POLICY_MAJOR_CATEGORIES
,@A_INSUR_POLICY_SUBDIVISIONS
,@A_INSUR_POLICY_PERSON_GROUP
,@A_CONTRACT_TYPE
,@A_INSUR_POLICY_TRADITION
,@A_INSUR_POLICY_BUSINESS_OBJECT
,@A_INSUR_POLICY_MAIN_OR_RIDER
,@A_INSUR_POLICY_OBJECT_TYPE_1
,@A_INSUR_POLICY_OBJECT_TYPE_2
,@A_INSUR_POLICY_MAIN_INSUR_TYPE
,@A_INSUR_POLICY_DANGER_TYPE
,@A_INSUR_POLICY_IS_PAR
,@DETAIL_REMARK
,@END_DATE
,@FIELD_CHA_1
,@FIELD_CHA_2
,@FIELD_CHA_3
,@FIELD_CHA_4
,@FIELD_CHA_5
,@A_INSUR_POLICY_INDEX
,@A_INSUR_POLICY_STRATEGY_OBJECT
,@A_INSUR_POLICY_RAPID_REPORT_CATEGORY_1
,@A_INVESTMENT_INSURANCE_CONTRACT
,@A_YM1_CODE
,@A_EXAMINE_SPECIAL_PAYMENT
)";

                SqlCommand cmd = conn.CreateCommand();

                cmd.Connection = conn;
                cmd.Transaction = transaction;

                cmd.CommandText = sql;

                foreach (ItemWanpieModel d in wanpieList)
                {
                    cmd.Parameters.Clear();


                    cmd.Parameters.AddWithValue("@APLY_NO", System.Data.SqlDbType.VarChar).Value = (Object)d.aplyNo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FLAG", System.Data.SqlDbType.VarChar).Value = (Object)d.flag ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@CORP_NO", System.Data.SqlDbType.VarChar).Value = (Object)d.corpNo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@VOUCHER_NO", System.Data.SqlDbType.VarChar).Value = (Object)d.voucherNo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@VOUCHER_DATE", System.Data.SqlDbType.DateTime).Value = (Object)d.voucherDate ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@START_DATE", System.Data.SqlDbType.DateTime).Value = (Object)d.startDate ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@ENTRY_TIME", DateTime.Now);
                    cmd.Parameters.AddWithValue("@A_SYS_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)d.aSysType ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_ACTNUM_LASTSIX", System.Data.SqlDbType.VarChar).Value = (Object)d.aActnumLastsix ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_ITEM", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyItem ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_OBJECT_NO", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyObjectNo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_OBJECT_NAME", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyObjectName ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_MAJOR_CATEGORIES", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyMajorCategories ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_SUBDIVISIONS", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicySubdivisions ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_PERSON_GROUP", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyPersonGroup ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_CONTRACT_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)d.aContractType ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_TRADITION", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyTradition ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_BUSINESS_OBJECT", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyBusinessObject ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_MAIN_OR_RIDER", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyMainOrRider ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_OBJECT_TYPE_1", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyObjectType1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_OBJECT_TYPE_2", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyObjectType2 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_MAIN_INSUR_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyMainInsurType ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_DANGER_TYPE", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyDangerType ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_IS_PAR", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyIsPar ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@DETAIL_REMARK", System.Data.SqlDbType.VarChar).Value = (Object)d.detailRemark ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@END_DATE", System.Data.SqlDbType.DateTime).Value = (Object)d.endDate ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FIELD_CHA_1", System.Data.SqlDbType.VarChar).Value = (Object)d.fieldCha1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FIELD_CHA_2", System.Data.SqlDbType.VarChar).Value = (Object)d.fieldCha2 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FIELD_CHA_3", System.Data.SqlDbType.VarChar).Value = (Object)d.fieldCha3 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FIELD_CHA_4", System.Data.SqlDbType.VarChar).Value = (Object)d.fieldCha4 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FIELD_CHA_5", System.Data.SqlDbType.VarChar).Value = (Object)d.fieldCha5 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_INDEX", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyIndex ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_STRATEGY_OBJECT", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyStrategyObject ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INSUR_POLICY_RAPID_REPORT_CATEGORY_1", System.Data.SqlDbType.VarChar).Value = (Object)d.aInsurPolicyRapidReportCategory1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_INVESTMENT_INSURANCE_CONTRACT", System.Data.SqlDbType.VarChar).Value = (Object)d.aInvestmentInsuranceContract ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_YM1_CODE", System.Data.SqlDbType.VarChar).Value = (Object)d.aYmiCode ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@A_EXAMINE_SPECIAL_PAYMENT", System.Data.SqlDbType.VarChar).Value = (Object)d.aExamineSpecialPayment ?? DBNull.Value;

                    int cnt = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {

                throw e;
            }

        }
    }
}