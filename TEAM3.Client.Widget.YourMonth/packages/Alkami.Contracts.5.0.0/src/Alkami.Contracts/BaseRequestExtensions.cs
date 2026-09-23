using System;
using System.Data;

namespace Alkami.Contracts
{
    public static class BaseRequestExtensions
    {
        public const string BankInstanceIdentifierColumnName = "BankInstanceIdentifier";

        public static void SetBankIdentifier(this BaseRequest request, DataRow tenant)
        {
            if (!request.BankIdentifier.HasValue)
            {
                var tenantBankUrlSignatures = tenant["BankUrlSignatures"]?.ToString();
                var tenantAdminUrlSignatures = tenant["BankAdminUrlSignatures"]?.ToString();
                var tenantBankIdentifiers = tenant["BankIdentifiers"]?.ToString();

                var bankInstanceIdentifier = tenant.Table.Columns[BankInstanceIdentifierColumnName] == null ? null : tenant[BankInstanceIdentifierColumnName].ToString();
                if (bankInstanceIdentifier != null)
                {
                    Guid parsedId;
                    if (Guid.TryParse(tenant[BankInstanceIdentifierColumnName].ToString(), out parsedId))
                        request.BankInstanceIdentifier = parsedId;
                }

                request.SetBankIdentifier(tenantBankUrlSignatures, tenantAdminUrlSignatures, tenantBankIdentifiers);
            }
        }

        public static void SetBankIdentifier(this BaseRequest request, string tenantBankUrlSignatures, string tenantAdminUrlSignatures, string tenantBankIdentifiers)
        {
            if (!request.BankIdentifier.HasValue)
            {
                var bankUrlSignatures = tenantBankUrlSignatures?.Split(',');
                var adminUrlSignatures = tenantAdminUrlSignatures?.Split(',');
                var bankIdentifiers = tenantBankIdentifiers?.Split(',');

                request.BankIdentifier = BankIdentifierBySignature(request.BankUri, bankUrlSignatures, bankIdentifiers)
                                         ?? BankIdentifierBySignature(request.BankUri, adminUrlSignatures, bankIdentifiers);
            }
        }

        internal static Guid? BankIdentifierBySignature(string uri, string[] signatures, string[] bankIdentifiers)
        {
            for (int i = 0; i < signatures.Length; i++)
            {
                if (string.Equals(signatures[i], uri, StringComparison.OrdinalIgnoreCase))
                {
                    Guid bankIdentifier;
                    if (bankIdentifiers.Length > i)
                    {
                        Guid.TryParse(bankIdentifiers[i], out bankIdentifier);
                    }
                    else
                    {
                        //BankIdentifiers does not match the number of BankUrlSignatures so we will just default to the first one.
                        Guid.TryParse(bankIdentifiers[0], out bankIdentifier);
                    }

                    return bankIdentifier;
                }
            }

            return null;
        }
    }
}