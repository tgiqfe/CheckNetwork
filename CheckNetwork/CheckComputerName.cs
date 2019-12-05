using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CheckNetwork
{
    class CheckComputerName
    {
        private string _currentCmputerName = null;

        public CheckComputerName()
        {
            this._currentCmputerName = Environment.MachineName;
        }

        /// <summary>
        /// ホスト名と一致チェック
        /// </summary>
        /// <param name="validComputerName"></param>
        /// <returns></returns>
        public bool IsMatch(string validComputerName)
        {
            return IsMatch(new string[1] { validComputerName });
        }

        /// <summary>
        /// ホスト名と一致チェック
        /// 複数の引数でチェック
        /// </summary>
        /// <param name="Comp"></param>
        /// <param name="validComputerNames"></param>
        /// <returns></returns>
        public bool IsMatch(string validComputerName, params string[] validComputerNames)
        {
            List<string> tempList = new List<string>(validComputerNames);
            tempList.Insert(0, validComputerName);
            return IsMatch(tempList.ToArray());
        }

        /// <summary>
        /// ホスト名と一致チェック
        /// 文字列配列あるいは「,」区切り文字列をチェック。
        /// </summary>
        /// <param name="validComputerNames"></param>
        /// <returns></returns>
        public bool IsMatch(string[] validComputerNames)
        {
            Regex reg_comma = new Regex(@",\s*");
            foreach (string validComputerName in validComputerNames)
            {
                if (string.IsNullOrEmpty(validComputerName)) { continue; }
                foreach (string computerName in reg_comma.Split(validComputerName))
                {
                    if ((computerName.Contains("*") && GetWildCardRegex(computerName).IsMatch(_currentCmputerName)) ||
                        (computerName.Equals(_currentCmputerName, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ワイルドカードを含む名前を比較する為のRegexを取得
        /// </summary>
        /// <param name="validComputerNames"></param>
        /// <returns></returns>
        private Regex GetWildCardRegex(string validComputerNames)
        {
            string patternString = Regex.Replace(validComputerNames, ".",
                x =>
                {
                    string y = x.Value;
                    if (y.Equals("?")) { return "."; }
                    else if (y.Equals("*")) { return ".*"; }
                    else { return Regex.Escape(y); }
                });
            if (!patternString.StartsWith("*")) { patternString = "^" + patternString; }
            if (!patternString.EndsWith("*")) { patternString = patternString + "$"; }

            return new Regex(patternString, RegexOptions.IgnoreCase);
        }
    }
}
