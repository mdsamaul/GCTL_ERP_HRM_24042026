namespace GCTL.Service.Common
{
    public interface ICommonService
    {
        void FindAccFiveDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue);
        void FindAccFourDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue);
        void FindAccThreeDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue);
        void FindAccTwoDigit(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue);
        void FindMaxGCTL(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding, string WhereColumn, string WhereValue);
        void FindMaxNo(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding);
        void FindMaxNoAuto(ref string strMaxNo, string strFldName, string strTableName);
        string NextCode(string fieldName, string table, int length);
        string GenerateCode(string columnName, string tableName, string prefix = "", int length = 8);
        string GenerateNextCode(string field, string table, int length = 3, string prefix = "");
        public string GenerateNextNumber(string field, string table, int length = 2, string prefix = "");

        void FindVoucherNo(ref string strMaxNo, string VoucherType_Code);
        void MaxNoWithYearAndTwoDight(ref string strMaxNo, string strFldName, string strTableName, int intLenWithPadding,string StartTwoDight );
    }
}