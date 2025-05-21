using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GCTL.Data.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GCTL.Service.Common
{
    public class CommonService : ICommonService
    {
        private readonly IConfiguration configuration;
        private const string connectionStringName = "ApplicationDbConnection";
        public CommonService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void FindMaxNo(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding)
        {
            string query = "Select isnull(MAX(convert(int," + strFldName + "))+1,0) as MaxNo from " + strTableName + "";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                strMaxNo = result.ToString();
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
            else
            {
                strMaxNo = "1";
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
        }
        public void FindAccTwoDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue)
        {
            try
            {
                string Query = "Select isnull(max(right(" + strFldName + ",2)),0)+1 as MaxNo from " + strTableName + " where " + WhereColumn + "='" + WhereValue + "'";
                int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), Query);
                if (result != 0)
                {
                    strMaxNo = result.ToString();
                    strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
                }
                else
                {
                    strMaxNo = "1";
                    strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
                }
            }
            catch (System.Exception ex)
            {
                string a = ex.Message.ToString();

            }
            finally
            {


            }
        }
        public string NextCode(string fieldName, string table, int length)
        {
            string nextCode = string.Empty;
            string query = "Select isnull(MAX(convert(int," + fieldName + "))+1,0) as MaxNo from " + table + "";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                nextCode = result.ToString();
            }
            else
            {
                nextCode = "1";

            }

            return nextCode.PadLeft(length, '0'); ;
        }
        public void FindMaxGCTL(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue)
        {
            string query = "Select isnull(max(right(" + strFldName + ",6)),0)+1 as MaxNo from " + strTableName + " where left(right(" + WhereColumn + ",8),2)='" + WhereValue + "'";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                strMaxNo = result.ToString();
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
            else
            {
                strMaxNo = "1";
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
        }

        public void FindMaxNoAuto(ref string strMaxNo, string strFldName, string strTableName)
        {
            string query = "Select isnull(MAX(convert(int," + strFldName + "))+1,0) as MaxNo from " + strTableName + "";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                strMaxNo = result.ToString();
            }
            else
            {
                strMaxNo = "1";
            }
        }

        public void FindAccThreeDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue)
        {
            string query = "Select isnull(max(right(" + strFldName + ",3)),0)+1 as MaxNo from " + strTableName + " where " + WhereColumn + "='" + WhereValue + "'";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                strMaxNo = result.ToString();
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
            else
            {
                strMaxNo = "1";
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
        }

        public void FindAccFourDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue)
        {
            string query = "Select isnull(max(right(" + strFldName + ",4)),0)+1 as MaxNo from " + strTableName + " where " + WhereColumn + "='" + WhereValue + "'";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                strMaxNo = result.ToString();
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
            else
            {
                strMaxNo = "1";
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
        }
        public void FindAccFiveDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue)
        {
            string query = "Select isnull(max(right(" + strFldName + ",5)),0)+1 as MaxNo from " + strTableName + " where " + WhereColumn + "='" + WhereValue + "'";
            int result = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (result != 0)
            {
                strMaxNo = result.ToString();
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
            else
            {
                strMaxNo = "1";
                strMaxNo = strMaxNo.PadLeft(intLenWithPadding, '0');
            }
        }

        public string GenerateCode(string columnName, string tableName, string prefix = "", int length = 8)
        {
            string query = $"Select CONCAT('{prefix}',FORMAT(ISNULL(Max(CAST(RIGHT({columnName},{length}) as int)),0)+1,'00000000')) MaxCode from {tableName}";
            return QueryExtensionsHelpers.QuerySingle<string>(configuration.GetConnectionString(connectionStringName), query);
        }


        public string GenerateNextCode(string field, string table, int length = 3, string prefix = "")
        {
            string result = string.Empty;
            string query = $"Select isnull(max(right({field},{length})),0)+1 as MaxNo from " + table;
            int nextNumber = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            if (nextNumber != 0)
            {
                result = nextNumber.ToString();
                result = result.PadLeft(length, '0');
            }
            else
            {
                result = "1";
                result = result.PadLeft(length, '0');
            }

            if (!string.IsNullOrWhiteSpace(prefix))
                result = $"{prefix}{result}";

            return result;
        }

       
        public string GenerateNextNumber(string field, string table, int length = 2, string prefix = "")
        {
            string query = $"SELECT COALESCE(CAST(MAX(RIGHT(COALESCE({field}, ''), CHARINDEX('_', REVERSE(COALESCE({field}, '')) + '_') - 1)) AS INT), 0) From {table}";
            int number = QueryExtensionsHelpers.QuerySingle<int>(configuration.GetConnectionString(connectionStringName), query);
            string result;
            if (number > 0)
            {
                number++;
            }
            else
            {
                number = 1;
            }

            result = number.ToString().PadLeft(length, '0');

            if (!string.IsNullOrWhiteSpace(prefix))
                result = $"{prefix}{result}";

            return result;
        }
        public void FindVoucherNo(ref string strMaxNo, string VoucherType_Code)
        {
            string Query = "SELECT CONCAT((Select Voucher_TypeName from Acc_VoucherType"
           + " where VoucherType_Code = '" + VoucherType_Code + "'),'_',RIGHT(CONVERT(VARCHAR(8), GETDATE(), 1), 2),'_',"
           + " FORMAT(CONVERT(INT, ISNULL(MAX(CAST(RIGHT(VoucherNo, 6) as int)), 0) + 1), '000000')) MaxNo"
           + " From Acc_VoucherEntry Where LEFT(RIGHT(VoucherNo, 9), 2) = RIGHT(CONVERT(VARCHAR(8), GETDATE(), 1), 2) AND VoucherType_Code = '" + VoucherType_Code + "'";
            string result = QueryExtensionsHelpers.QuerySingle<string>(configuration.GetConnectionString(connectionStringName), Query);
            if (result != "")
            {
                strMaxNo = result.ToString();
            }

            else
            {
                strMaxNo = "";
            }
            
        }
        public void MaxNoWithYearAndTwoDight(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string StartTwoDight)
        {
            string Query = "SELECT CONCAT('"+ StartTwoDight + "','_',RIGHT(CONVERT(VARCHAR(8), GETDATE(), 1), 2),'_',"
           + " FORMAT(CONVERT(INT, ISNULL(MAX(CAST(RIGHT("+ strFldName + ", 6) as int)), 0) + 1), '000000')) MaxNo"
           + " From "+ strTableName + " Where LEFT(RIGHT(" + strFldName + ", 9), 2) = RIGHT(CONVERT(VARCHAR(8), GETDATE(), 1), 2)";
            string result = QueryExtensionsHelpers.QuerySingle<string>(configuration.GetConnectionString(connectionStringName), Query);
            if (result != "")
            {
                strMaxNo = result.ToString();
            }

            else
            {
                strMaxNo = "";
            }

        }

    }
}
