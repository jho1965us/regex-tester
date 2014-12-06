using System.Windows.Documents;
using Sharomank.RegexTester.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Sharomank.RegexTester.Strategies
{
    /// <summary>
    /// Author: Roman Kurbangaliyev (sharomank)
    /// </summary>
    public class DefaultStrategy : IRegexProcessStrategy
    {
        public bool Match(BackgroundWorker worker, RegexTesterPageViewModel viewModel, RegexProcessContext context)
        {
            if (!RegexIsMatch(context))
            {
                return OperationIsComplete(worker);
            }

            var matches = context.MatchRegex.Matches(context.InputText);
            viewModel.Matches = new MatchesViewModel(context.MatchRegex, matches, context.InputText);

            int count = 0;
            var isSimpleMatch = string.IsNullOrEmpty(context.ReplaceRegexPattern);

            Dictionary<String, String> groups = GetMatchGroups(context, isSimpleMatch);

            foreach (Match item in matches)
            {
                if (worker.CancellationPending)
                    return false;

                if (item.Success)
                {
                    if (isSimpleMatch)
                    {
                        viewModel.AppendOutputText(item.Value);
                    }
                    else
                    {
                        string result = Replace(worker, context, item, groups);
                        if (result == null)
                            return false;
                        viewModel.AppendOutputText(result);
                    }
                    count++;
                }
            }
            viewModel.Count = count;
            return OperationIsComplete(worker);
        }

        public bool Split(BackgroundWorker worker, RegexTesterPageViewModel viewModel, RegexProcessContext context)
        {
            if (!RegexIsMatch(context))
            {
                return OperationIsComplete(worker);
            }

            int count = 0;
            var matches = context.MatchRegex.Split(context.InputText);
            viewModel.Matches = null;
            foreach (var str in matches)
            {
                if (worker.CancellationPending)
                    return false;

                if (!string.IsNullOrEmpty(str))
                {
                    viewModel.AppendOutputText(str);
                    count++;
                }
            }
            viewModel.Count = count;
            return OperationIsComplete(worker);
        }

        public bool Replace(BackgroundWorker worker, RegexTesterPageViewModel viewModel, RegexProcessContext context)
        {
            if (!RegexIsMatch(context))
            {
                return OperationIsComplete(worker);
            }

            Dictionary<String, String> groups = GetMatchGroups(context, false);

            var matches = new List<MatchAndReplace>();
            string result;
            if (!context.MatchRegex.RightToLeft)
            {
                var replaceIndex = 0;
                var lastMatchEnd = 0;
                result = context.MatchRegex.Replace(context.InputText, delegate(Match m)
                {
                    var value = Replace(worker, context, m, groups) ?? "";
                    replaceIndex += m.Index - lastMatchEnd;
                    matches.Add(new MatchAndReplace(m, replaceIndex, value));
                    replaceIndex += value.Length;
                    lastMatchEnd = m.Index + m.Length;
                    return value;
                });
            }
            else
            {
                var replaceIndex = 0;
                var lastMatchEnd = context.InputText.Length;
                result = context.MatchRegex.Replace(context.InputText, delegate(Match m)
                {
                    var value = Replace(worker, context, m, groups) ?? "";
                    replaceIndex -= lastMatchEnd - (m.Index + m.Length) + value.Length;
                    matches.Add(new MatchAndReplace(m, replaceIndex, value));
                    lastMatchEnd = m.Index;
                    return value;
                });
                foreach (var m in matches)
                {
                    m.ReplaceIndex += result.Length;
                }
            }
            viewModel.Matches = new MatchesViewModel(context.MatchRegex, matches, context.InputText);
            viewModel.Count = matches.Count;
            viewModel.AppendOutputText(result);
            return OperationIsComplete(worker);
        }

        private static string Replace(BackgroundWorker worker, RegexProcessContext context, Match m, Dictionary<string, string> groups)
        {
            string value = "";
            switch (context.ReplaceMode)
            {
                case ReplaceMode.Original:
                    value = context.ReplaceRegexPattern;
                    foreach (var group in groups)
                    {
                        if (worker.CancellationPending)
                            return null;

                        value = value.Replace(group.Value, m.Groups[group.Key].Value);
                    }
                    break;
                case ReplaceMode.ReplaceSubstitutionPattern:
                    value = m.Result(context.ReplaceRegexPattern);
                    break;
            }
            return value;
        }

        private bool RegexIsMatch(RegexProcessContext context)
        {
            return context.MatchRegex.IsMatch(context.InputText);
        }

        private static Dictionary<String, String> GetMatchGroups(RegexProcessContext context, bool isSimpleMatch)
        {
            Dictionary<String, String> groups = new Dictionary<String, String>();

            if (isSimpleMatch)
                return groups;

            foreach (var groupName in context.MatchRegex.GetGroupNames())
            {
                string group = string.Format("$[{0}]", groupName);
                if (context.ReplaceRegexPattern.Contains(group))
                {
                    groups.Add(groupName, group);
                }
            }
            return groups;
        }

        private bool OperationIsComplete(BackgroundWorker worker)
        {
            if (worker.CancellationPending)
                return false;
            return true;
        }
    }
}
