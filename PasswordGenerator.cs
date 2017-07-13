using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Entropy
{
    public class PasswordGenerator
    {
        // GoManPTCAccountCreator.Controller.Account.Generator
// Token: 0x060001D1 RID: 465 RVA: 0x0000C394 File Offset: 0x0000A594
        private static string RandomPassword(int length)
        {
            return RandomString(length);
        }
        
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*!@#$%^&*!@#$%" +
                                 "^&*!@#$%^&*!@#$%^&*";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

    }
}