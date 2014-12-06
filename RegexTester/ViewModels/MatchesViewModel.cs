using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sharomank.RegexTester.Common
{
    public class MatchesViewModel
    {
        private readonly Regex _regex;
        private readonly string _inputText;
        private ObservableCollection<MatchViewModel> _matchViewModels;

        public MatchesViewModel(Regex regex, MatchCollection matches, string inputText)
        {
            _regex = regex;
            _matchViewModels = CaptureViewModel.NewCollection(matches.Cast<Match>().Select(Selector));
            _inputText = inputText;
        }

        public MatchesViewModel(Regex regex, IEnumerable<MatchAndReplace> matches, string inputText)
        {
            _regex = regex;
            _matchViewModels = CaptureViewModel.NewCollection(matches.Select(Selector));
            _inputText = inputText;
        }

        public ObservableCollection<MatchViewModel> Matches
        {
            get { return _matchViewModels; }
        }

        private MatchViewModel Selector(Match m, int i)
        {
            return new MatchViewModel(m, i, this);
        }

        private MatchViewModel Selector(MatchAndReplace matchAndReplace, int i)
        {
            return new MatchViewModel(matchAndReplace, i, this);
        }

        public Regex Regex
        {
            get { return _regex; }
        }

        public string InputText
        {
            get { return _inputText; }
        }
    }

    public class MatchAndReplace
    {
        private Match _match;
        private int _replaceIndex;
        private readonly string _replaceValue;

        public MatchAndReplace(Match match, int replaceIndex, string replaceValue)
        {
            _match = match;
            _replaceIndex = replaceIndex;
            _replaceValue = replaceValue;
        }

        public Match Match
        {
            get { return _match; }
        }

        public int ReplaceIndex
        {
            get { return _replaceIndex; }
            set { _replaceIndex = value; }
        }

        public string ReplaceValue
        {
            get { return _replaceValue; }
        }
    }

    public class MatchViewModel : GroupViewModel
    {
        protected Match _match;
        private readonly MatchAndReplace _matchAndReplace;
        private readonly int _i;
        private readonly MatchesViewModel _matchesViewModel;
        private ObservableCollection<GroupsItemViewModel> _groupsItemViewModels;

        public MatchViewModel(Match match, int i, MatchesViewModel matchesViewModel)
            : base(match)
        {
            _match = match;
            _i = i;
            _matchesViewModel = matchesViewModel;
        }

        public MatchViewModel(MatchAndReplace matchAndReplace, int i, MatchesViewModel matchesViewModel)
            : base(matchAndReplace.Match)
        {
            _match = matchAndReplace.Match;
            _matchAndReplace = matchAndReplace;
            _i = i;
            _matchesViewModel = matchesViewModel;
        }

        public ObservableCollection<GroupsItemViewModel> Groups
        {
            get
            {
                if (_groupsItemViewModels == null)
                {
                    _groupsItemViewModels = NewCollection(_match.Groups.Cast<Group>().Select(Selector).Where(Predicate));
                }
                return _groupsItemViewModels;
            }
        }

        private bool Predicate(GroupsItemViewModel gi)
        {
            return gi.Group.Success;
        }

        private GroupsItemViewModel Selector(Group g, int i)
        {
            return new GroupsItemViewModel(g, i, _matchesViewModel, this);
        }

        public string VisibleName
        {
            get { return string.Format("[{0}]", _i); }
        }

        protected override string InputText
        {
            get { return _matchesViewModel.InputText; }
        }

        public MatchAndReplace MatchAndReplace
        {
            get { return _matchAndReplace; }
        }
    }

    public class GroupsItemViewModel : GroupViewModel
    {
        private readonly int _i;
        private readonly MatchesViewModel _matchesViewModel;
        private readonly MatchViewModel _matchViewModel;

        public GroupsItemViewModel(Group group, int i, MatchesViewModel matchesViewModel, MatchViewModel matchViewModel)
            : base(group)
        {
            _i = i;
            _matchesViewModel = matchesViewModel;
            _matchViewModel = matchViewModel;
        }

        public string VisibleName
        {
            get
            {
                var groupNameFromNumber = _matchesViewModel.Regex.GroupNameFromNumber(_i);
                if (groupNameFromNumber == _i.ToString())
                {
                    return string.Format("[{0}]", groupNameFromNumber);
                }
                else
                {
                    return string.Format(@"[""{0}""]", groupNameFromNumber);
                }
            }
        }

        protected override string InputText
        {
            get { return _matchesViewModel.InputText; }
        }
    }

    public abstract class GroupViewModel : CaptureViewModel
    {
        private readonly Group _group;
        private ObservableCollection<CapturesItemViewModel> _capturesItemViewModels;

        protected GroupViewModel(Group group)
            : base(group)
        {
            _group = group;
        }

        public ObservableCollection<CapturesItemViewModel> Captures
        {
            get
            {
                if (_capturesItemViewModels == null)
                {
                    _capturesItemViewModels = NewCollection(Group.Captures.Cast<Capture>().Select(Selector));
                }
                return _capturesItemViewModels;
            }
        }

        private CapturesItemViewModel Selector(Capture g, int i)
        {
            return new CapturesItemViewModel(g, i, this);
        }

        public Group Group
        {
            get { return _group; }
        }
    }

    public class CapturesItemViewModel : CaptureViewModel
    {
        private readonly int _i;
        private readonly GroupViewModel _groupViewModel;

        public CapturesItemViewModel(Capture capture, int i, GroupViewModel groupViewModel) : base(capture)
        {
            _i = i;
            _groupViewModel = groupViewModel;
        }

        public string VisibleName
        {
            get { return string.Format("[{0}]", _i); }
        }

        protected override string InputText
        {
            get { return _groupViewModel.TextValue; }
        }
    }

    public abstract class CaptureViewModel
    {
        private readonly Capture _capture;
        private int _textLine = -1;
        private int _textLinePos;

        protected CaptureViewModel(Capture capture)
        {
            _capture = capture;
        }

        protected abstract string InputText { get; }

        public int TextIndex
        {
            get { return _capture.Index; }
        }

        public int TextLength
        {
            get { return _capture.Length; }
        }

        public int TextLine
        {
            get
            {
                GetLineAndPos();
                return _textLine;
            }
        }

        private void GetLineAndPos()
        {
            if (_textLine == -1)
            {
                RegexTesterPage.Lines(InputText, TextIndex, out _textLine, out _textLinePos);
            }
        }

        public int TextLinePos
        {
            get
            {
                GetLineAndPos();
                return _textLinePos;
            }
        }

        public string TextValue
        {
            get
            {
                var builder = new StringBuilder(_capture.Length);
                var value = _capture.Value;
                for (int index = 0; index < value.Length; index++)
                {
                    var add = index < 20 || index >= value.Length - 20 || value.Length <= 45;
                    var elipsis = index == 20 && !add;
                    var chr = value[index];
                    if (index + 1 < value.Length &&
                        char.IsSurrogatePair(chr, value[index + 1]))
                    {
                        if (add)
                            builder.AppendFormat(@"\U{0:X4}{1:X4}", (int) chr, (int)value[index + 1]);
                        index++;
                    }
                    else if (add)
                    {
                        switch (chr)
                        {
                            case '\a':
                                builder.Append(@"\a");
                                break;
                            case '\b':
                                builder.Append(@"\b");
                                break;
                            case '\f':
                                builder.Append(@"\f");
                                break;
                            case '\n':
                                builder.Append(@"\n");
                                break;
                            case '\r':
                                builder.Append(@"\r");
                                break;
                            case '\t':
                                builder.Append(@"\t");
                                break;
                            case '\v':
                                builder.Append(@"\v");
                                break;
                            case '\\':
                                builder.Append(@"\\");
                                break;
                            default:
                                if (chr < ' ')
                                    builder.AppendFormat(@"\x{0:X2}", (int) chr);
                                else if (chr > '\xFF')
                                    builder.AppendFormat(@"\u{0:X4}", (int)chr); // todo strictly these should be red if High or Low surrogate pair char (i.e. unpaired)
                                else if (chr >= '\x7F')
                                    builder.AppendFormat(@"\x{0:X2}", (int) chr);
                                else
                                    builder.Append(chr);
                                break;
                        }
                    }
                    else if (elipsis)
                    {
                        builder.Append(" ... "); // todo strictly these should be gray to disting from normal chars
                    }
                }
                return builder.ToString();
            }
        }

        internal static ObservableCollection<T> NewCollection<T>(IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }
    }

}
