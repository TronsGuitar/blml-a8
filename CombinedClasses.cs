using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.PowerPacks.Printing.Compatibility.VB6;
using static Microsoft.VisualBasic.Constants;
using static Microsoft.VisualBasic.FileSystem;
using static Microsoft.VisualBasic.Interaction;
using static modConfig;
using static modConvert;
using static modProjectFiles;
using static modRefScan;
using static modSupportFiles;
using static modUtils;
using static VBConstants;
using static VBExtension;
using static Microsoft.VisualBasic.Strings;
using static modOrigConvert;
using System.Text;
using static modConvertForm;
using static modQuickConvert;
using static modTextFiles;
using static modUsingEverything;
using static Microsoft.VisualBasic.Conversion;
using static modConvertUtils;
using static modVB6ToCS;
using static modDirStack;
using static modShell;
using System.Runtime.InteropServices;
using static Microsoft.VisualBasic.Information;
using static modRegEx;
using static modSubTracking;
using static Microsoft.VisualBasic.DateAndTime;
using static Microsoft.VisualBasic.VBMath;
using static modControlProperties;
using static modQuickLint;
using VB2CS.Forms;
using static modGit;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

    public class ScreenMetrics
    {
        public int Width => (int)System.Windows.SystemParameters.PrimaryScreenWidth;
        public int Height => (int)System.Windows.SystemParameters.PrimaryScreenHeight;
        public FrameworkElement ActiveControl;
    }
    public static ScreenMetrics Screen { get => new ScreenMetrics(); }

    public static int itemData(this ComboBox c, int I) { try { return (((ComboboxItem)c.Items[I]).Value); } catch (Exception e) { return 0; } }
    public static int AddItem(this ComboBox c, string C) { return c.Items.Add(new ComboboxItem(C)); }
    public static int AddItem(this ComboBox c, string C, int D) { return c.Items.Add(new ComboboxItem(C, D)); }
    public static int AddItem(this ComboBox c, string C, bool Select) { ComboboxItem x = new ComboboxItem(C); int res = c.Items.Add(x); if (Select) c.SelectedItem = x; return res; }
    public static int AddItem(this ComboBox c, string C, int D, bool Select) { ComboboxItem x = new ComboboxItem(C, D); int res = c.Items.Add(x); if (Select) c.SelectedItem = x; return res; }
    public static String List(this ComboBox c, int Index) { return Index < c.Items.Count ? c.Items[Index].ToString() : null; }
    public static string SetItemText(this ComboBox c, int Index, string Text) { return ((ComboboxItem)c.Items[Index]).Text = Text; }
    public static int SelectedValue(this ComboBox c) { return ((ComboboxItem)c.SelectedItem).Value; }
    public static string SelectedText(this ComboBox c) { return c.SelectedItem == null ? "" : ((ComboboxItem)c.SelectedItem).Text; }
    public static bool SelectText(this ComboBox c, string S) { for (int i = 0; i < c.Items.Count; i++) if (i.ToString() == S) { c.SelectedIndex = i; return true; } return false; }
    public static void RemoveItem(this ComboBox c, int Index) { c.Items.RemoveAt(Index); }
    public static void Clear(this ComboBox c) { c.Items.Clear(); }

    public static int itemData(this ListBox c, int I) { try { return ((int)((ComboboxItem)c.Items[I]).Value); } catch (Exception e) { return 0; } }
    public static bool SelectText(this ListBox c, string S) { for (int i = 0; i < c.Items.Count; i++) if (i.ToString() == S) { c.SelectedIndex = i; return true; } return false; }
    public static int SelectItem(this ListBox c, int I, bool isSelected)
    {
        if (c.SelectionMode == SelectionMode.Multiple)
        { if (isSelected) c.SelectedItems.Add(c.Items[I]); else c.SelectedItems.Remove(c.Items[I]); }
        else { if (isSelected) c.SelectedItem = c.Items[I]; else { if (c.SelectedItem == c.Items[I]) c.SelectedItem = null; } }
        return I;
    }
    public static bool Selected(this ListBox c, int I) { return c.SelectedItems.Contains(c.Items[I]); }
    public static bool Selected(this ListBox c, int I, bool Value)
    {
        if (c.SelectionMode == SelectionMode.Single)
        {
            c.SelectedItem = c.Items[I];
            return true;
        }
        else
        {
            if (Value) c.SelectedItems.Add(c.Items[I]); else c.SelectedItems.Remove(c.Items[I]);
            return c.Selected(I);
        }
    }

    public static int IndexOf(this ListBox c, string s)
    {
        foreach (var l in c.Items) if (Strings.Trim(((ComboboxItem)l).ToString()) == s) return c.Items.IndexOf(l);
        return -1;
    }

    public static bool Contains(this ListBox c, string s) { return c.IndexOf(s) != -1; }

    public static string SelectedText(this ListBox c) { return c.SelectedItem == null ? "" : ((ComboboxItem)c.SelectedItem).ToString(); }
    public static int AddItem(this ListBox c, string C) { return c.Items.Add(new ComboboxItem(C)); }
    public static int AddItem(this ListBox c, string C, int D) { return c.Items.Add(new ComboboxItem(C, D)); }
    public static int AddItem(this ListBox c, string C, bool Selected) { int x = c.Items.Add(new ComboboxItem(C)); return SelectItem(c, x, Selected); }
    public static int AddItem(this ListBox c, string C, int D, bool Selected) { int x = c.Items.Add(new ComboboxItem(C, D)); return SelectItem(c, x, Selected); }
    public static void RemoveItem(this ListBox c, int Index) { c.Items.RemoveAt(Index); }
    //public static string List(this ListBox c, int Index) { return modNumbers.InRange(0, Index, c.Items.Count) ? c.Items[Index].ToString() : ""; }

    public static bool getSelected(this ListBox c, int I) { return c.SelectedItems.Contains(c.Items[I]); }
    public static void setSelected(this ListBox c, int I, bool V)
    {
        if (c.SelectionMode == SelectionMode.Multiple)
        {
            if (V) c.SelectedItems.Add(c.Items[I]);
            else c.SelectedItems.Remove(c.Items[I]);
        }
        else
            c.SelectedItem = c.Items[I];
    }
    public static bool Clear(this ListBox c) { c.Items.Clear(); return true; }

    public static TreeViewItem TreeViewAddItem(TreeView t, string Value, string Key = null, TreeViewItem parent = null)
    {
        TreeViewItem tvi;
        if (parent == null)
        {
            int x = t.Items.Add(new TreeViewItemObject(Value, Key));
            tvi = t.Item(x);
        }
        else
        {
            int x = parent.Items.Add(new TreeViewItemObject(Value, Key));
            tvi = t.Item(x);
        }

        return tvi;

    }
    public static TreeViewItem AddItem(this TreeView t, string Value) { return TreeViewAddItem(t, Value); }
    public static TreeViewItem AddItem(this TreeView t, string Value, string Key) { return TreeViewAddItem(t, Value, Key); }
    public static TreeViewItem AddItem(this TreeView t, string Value, string Key, TreeViewItem parent) { return TreeViewAddItem(t, Value, Key, parent); }
    public static TreeViewItemObject Item(this TreeView t, int x) { return (TreeViewItemObject)t.Items.GetItemAt(x); }
    public static TreeViewItemObject Item(this TreeView t, string x) { foreach (var l in t.Items) if (((TreeViewItemObject)l).getKey() == x) return (TreeViewItemObject)l; return null; }
    public static void setItemColor(this TreeView t, int Item, Brush backColor = null, Brush foreColor = null)
    {
        {
            var actualItem = t.Item(Item);
            if (actualItem != null)
            {
                if (backColor != null) actualItem.Background = backColor;
                if (foreColor != null) actualItem.Foreground = foreColor;
            }
        }
    }
    public static void SetSelectedIndex(this TreeView t, int Index) { ((TreeViewItem)t.Items.GetItemAt(Index)).IsSelected = true; }
    public static void Clear(this TreeView t, string Value, String Key = null) { t.Items.Clear(); }

    public static DateTime getDateTime(this DatePicker DP) { return DP.SelectedDate ?? DP.DisplayDate; }
    public static string getDateString(this DatePicker DP) { return (DP.SelectedDate ?? DP.DisplayDate).ToShortDateString(); }
    public static string getTimeString(this DatePicker DP) { return (DP.SelectedDate ?? DP.DisplayDate).ToShortTimeString(); }

    public static BitmapImage PackageImage(string s, bool placeholder = true)
    {
        if (Strings.Left(s, 1) != "/") s = "/Resources/Images/" + s;
        s = "pack://application:,,," + s;
        try { return new BitmapImage(new Uri(@s)); }
        catch (Exception e)
        {
            if (!placeholder) return null;
            string d = "/Resources/Images/none.bmp";
            return new BitmapImage(new Uri(d, UriKind.Relative));
        }
    }

    public class ComboboxItem
    {
        public ComboboxItem(string vText) { Text = vText; }
        public ComboboxItem(string vText, int vValue) { Text = vText; Value = vValue; }
        public string Text { get; set; }
        public int Value { get; set; }
        public override string ToString() { return Text; }
    }

    public class TreeViewItemObject : TreeViewItem
    {
        string Text;
        string Key;

        public TreeViewItemObject(string Text = "", string Key = "")
        {
            this.Text = Text;
            Header = Text;
            this.Key = Key;
        }

        public new string ToString() { return Text; }
        public string getValue() { return Key; }
        public void setValue(string s) { Key = s; }
        public string getKey() { return Key; }
        public void setKey(string s) { Key = s; }

        public TreeViewItem getContainer(TreeView tv) { return tv.ItemContainerGenerator.ContainerFromItem(this) as TreeViewItem; }
    }

    public class PropIndexer<I, V>
    {
        public delegate void setProperty(I idx, V value);
        public delegate V getProperty(I idx);

        public event getProperty getter;
        public event setProperty setter;

        public PropIndexer(getProperty g, setProperty s) { getter = g; setter = s; }
        public PropIndexer(getProperty g) { getter = g; setter = setPropertyNoop; }
        public PropIndexer() { getter = getPropertyNoop; setter = setPropertyNoop; }

        public void setPropertyNoop(I idx, V value) { }
        public V getPropertyNoop(I idx) { return default(V); }

        public V this[I idx]
        {
            get => getter.Invoke(idx);
            set => setter.Invoke(idx, value);
        }
    }
    public class PropIndexer2<I, J, V>
    {
        public delegate void setProperty(I idx, J idx2, V value);
        public delegate V getProperty(I idx, J idx2);

        public event getProperty getter;
        public event setProperty setter;

        public PropIndexer2(getProperty g, setProperty s) { getter = g; setter = s; }
        public PropIndexer2(getProperty g) { getter = g; setter = setPropertyNoop; }
        public PropIndexer2() { getter = getPropertyNoop; setter = setPropertyNoop; }

        public void setPropertyNoop(I idx, J idx2, V value) { }
        public V getPropertyNoop(I idx, J idx2) { return default(V); }

        public V this[I idx, J idx2]
        {
            get => getter.Invoke(idx, idx2);
            set => setter.Invoke(idx, idx2, value);
        }
    }

    public static dynamic CreateObject(string IdName)
    {
        Type ObjectType = Type.GetTypeFromProgID(IdName);
        dynamic ObjectInst = Activator.CreateInstance(ObjectType);
        return ObjectInst;
    }
    public class Timer
    {
        private System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
        public Action Action;
        private void dispatcherTimer_Tick(object sender, EventArgs e) { if (Action != null) Action.Invoke(); }

        public Timer(Action e = null, int vInterval = 1000, bool vEnabled = false)
        {
            tmr.Tick += dispatcherTimer_Tick;
            Action = e;
            Interval = vInterval;
            Enabled = vEnabled;
        }

        public System.Windows.Threading.DispatcherTimer timer { get => tmr; }

        public bool IsEnabled
        {
            get => tmr.IsEnabled;
            set { tmr.IsEnabled = value; if (value) tmr.Start(); else tmr.Stop(); }
        }
        public bool Enabled { get => IsEnabled; set => IsEnabled = value; }
        public Timer Discard() { Enabled = false; return null; }

        public int Interval { get => (int)tmr.Interval.TotalMilliseconds; set => tmr.Interval = new TimeSpan(0, 0, 0, 0, value); }
        public int IntervalSeconds { get => (int)tmr.Interval.TotalSeconds; set => tmr.Interval = new TimeSpan(0, 0, 0, value); }
        public dynamic Tag { get; set; }

        public TimeSpan getInterval() { return tmr.Interval; }
        public void setInterval(TimeSpan value) { tmr.Interval = value; }

        public void startTimer(int MilliSeconds) { Enabled = false; Interval = MilliSeconds; Enabled = true; }
        public void startTimerSeconds(int Seconds) { Enabled = false; Interval = Seconds; Enabled = true; }
        public void startTimer(int MilliSeconds, dynamic setTag) { Tag = setTag; startTimer(MilliSeconds); }
        public void startTimerSeconds(int Seconds, dynamic setTag) { Tag = setTag; startTimerSeconds(Seconds); }
        public void stopTimer() { Enabled = false; }
    }

    public static List<T> controlArray<T>(this Window Frm, string name)
    {
        List<T> res = new List<T>();
        Panel G = (Panel)Frm.Content;
        foreach (var C in G.Children)
            if (((FrameworkElement)C).Name.StartsWith(name + "_")) res.Add((T)C);
        return res;
    }
    public static List<FrameworkElement> controlArray(this Window Frm, string name)
    {
        List<FrameworkElement> res = new List<FrameworkElement>();
        Panel G = (Panel)Frm.Content;
        foreach (var C in G.Children)
            if (((FrameworkElement)C).Name.StartsWith(name + "_")) res.Add((FrameworkElement)C);
        return res;
    }
    public static int controlIndex(String name) { try { return ValI(Strings.Mid(name, name.LastIndexOf('_') + 1)); } catch (Exception e) { } return -1; }
    public static int controlIndex(this Control C) { try { return ValI(Strings.Mid(C.Name, C.Name.LastIndexOf('_') + 1)); } catch (Exception e) { } return -1; }
    public static FrameworkElement getControlByIndex(this Window Frm, string Name, int Idx)
    { foreach (var C in Frm.Controls(true)) if (C.Name == Name + "_" + Idx) return C; return null; }
    public static FrameworkElement loadControlByIndex(this Window Frm, Type type, string Name, int Idx = -1)
    {
        FrameworkElement X = Frm.getControlByIndex(Name, Idx);
        if (X != null) return X;
        FrameworkElement C = (FrameworkElement)Activator.CreateInstance(type);
        C.Name = Name + "_" + Idx;
        List<FrameworkElement> els = controlArray(Frm, Name);
        Panel G;
        FrameworkElement el0 = getControlByIndex(Frm, Name, 0);
        if (els.Count > 0) G = els[0].Parent as Panel;
        else if (el0 != null) G = el0.Parent as Panel;
        else G = Frm.Content as Panel;
        G.Children.Add(C);
        return C;
    }
    public static void unloadControlByIndex(this Window Frm, string Name, int Idx = -1)
    {
        FrameworkElement X = Frm.getControlByIndex(Name, Idx);
        if (X != null)
        {
            Panel G = (Panel)Frm.Content;
            G.Children.Remove(X);
        }
    }
    public static void unloadControls(this Window Frm, string Name, int baseIndex = -1)
    {
        Panel G = (Panel)Frm.Content;
        foreach (var C in Frm.Controls())
        {
            string N = ((FrameworkElement)C).Name;
            if (N.StartsWith(Name + "_"))
            {
                if (controlIndex(N) == baseIndex) continue;
                G.Children.Remove(C);
            }
        }
    }
    public static int controlUBound(this Window Frm, string Name)
    {
        int Max = -1;
        foreach (var C in Frm.Controls(true))
        {
            string N = ((FrameworkElement)C).Name;
            if (N.StartsWith(Name + "_"))
            {
                int K = ValI(Strings.Mid(N, N.LastIndexOf('_') + 2));
                if (K > Max) Max = K;
            }
        }
        return Max;
    }

    public static List<FrameworkElement> Controls(this Window w, bool recurse = true)
    {
        Panel g = (Panel)w.Content;
        UIElementCollection children = g.Children;
        List<FrameworkElement> cts = new List<FrameworkElement>();
        foreach (var e in children)
        {
            cts.Add((FrameworkElement)e);
            if (recurse && e is GroupBox)
                foreach (var f in ((GroupBox)e).Controls(recurse)) cts.Add((FrameworkElement)f);
        }
        return cts;
    }
    public static List<FrameworkElement> Controls(this GroupBox w, bool recurse = true)
    {
        Panel g = (Panel)w.Content;
        UIElementCollection children = g.Children;
        List<FrameworkElement> cts = new List<FrameworkElement>();
        foreach (var e in children)
        {
            cts.Add((FrameworkElement)e);
            if (recurse && e is GroupBox)
                foreach (var f in ((GroupBox)e).Controls(recurse)) cts.Add((FrameworkElement)f);
        }
        return cts;
    }

    public static List<string> ControlNames(this Window w, bool recurse = true)
    {
        List<string> res = new List<string>();
        foreach (var c in w.Controls(recurse)) res.Add(c.Name);
        return res;
    }
    public static List<FrameworkElement> Controls(this Window w, Type T)
    {
        List<FrameworkElement> lst = w.Controls(), res = new List<FrameworkElement>();
        foreach (var l in lst) if (l.GetType() == T) res.Add(l);
        return res;
    }
    public static FrameworkElement ControlOf(this Window w, Type T, int n = 0)
    {
        List<FrameworkElement> lst = w.Controls(T);
        if (lst.Count == 0) return null;
        return lst[n < 0 ? 0 : n > lst.Count - 1 ? lst.Count - 1 : n];
    }
    public static FrameworkElement ControlOf(this Panel w, Type T, int n = 0)
    {
        List<FrameworkElement> lst = new List<FrameworkElement>();
        foreach (var l in w.getControls(true)) if (l.GetType() == T) lst.Add(l);
        if (lst.Count == 0) return null;
        return lst[n < 0 ? 0 : n > lst.Count - 1 ? lst.Count - 1 : n];
    }


    public static IEnumerable<FrameworkElement> getControls(this Visual parent, bool recurse = true)
    {
        List<FrameworkElement> res = new List<FrameworkElement>();
        foreach (var el in parent.GetChildren(recurse))
            res.Add((FrameworkElement)el);
        return res;
    }
    public static IEnumerable<Visual> GetChildren(this Visual parent, bool recurse = true)
    {
        if (parent != null)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                // Retrieve child visual at specified index value.
                var child = VisualTreeHelper.GetChild(parent, i) as Visual;

                if (child != null)
                {
                    yield return child;

                    if (recurse)
                    {
                        foreach (var grandChild in child.GetChildren(true))
                        {
                            yield return grandChild;
                        }
                    }
                }
            }
        }
    }

    //public class KeyedTreeViewItem
    //{
    //    public ObservableCollection<KeyedTreeViewItem> Items { get; set; }
    //    public string Key;
    //    public string Name;
    //    public KeyedTreeViewItem Parent;
    //    private void setup(KeyedTreeViewItem parent, string vKey, string vName)
    //    {
    //        Parent = parent;
    //        Items = new ObservableCollection<KeyedTreeViewItem>();
    //        Key = vKey;
    //        Name = vName;
    //    }

    //    public KeyedTreeViewItem(string vKey, string vName) : base()
    //    { setup(null, vKey, vName); }

    //    private KeyedTreeViewItem(KeyedTreeViewItem parent, string vKey, string vName) : base()
    //    { setup(parent, vKey, vName); }

    //    public void Add(string vKey, string vName)
    //    { Items.Add(new KeyedTreeViewItem(this, vKey, vName)); }

    //    public new string ToString() { return Name; }
    //}
    //public static KeyedTreeViewItem SelectedItemKeyed(this TreeView T)
    //{ return (KeyedTreeViewItem)T.SelectedItem; }

    //public static KeyedTreeViewItem getItemByKey(this TreeView T, string key)
    //{
    //    foreach (KeyedTreeViewItem I in T.Items)
    //        if (I.Key == key) return I;
    //    return null;
    //}


    public static T GetVisualChild<T>(Visual parent) where T : Visual
    {
        T child = default(T);
        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null) child = GetVisualChild<T>(v);
            if (child != null) break;
        }
        return child;
    }

    public static DataGridRow GetSelectedRow(this DataGrid grid)
    { return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem); }
    public static DataGridRow GetRow(this DataGrid grid, int index)
    {
        DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
        if (row == null)
        {
            // May be virtualized, bring into view and try again.
            grid.UpdateLayout();
            try { grid.ScrollIntoView(grid.Items[index]); } catch (Exception e) { }
            row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
        }
        return row;
    }

    public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
    {
        if (row != null)
        {
            DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

            if (presenter == null)
            {
                grid.ScrollIntoView(row, grid.Columns[column]);
                presenter = GetVisualChild<DataGridCellsPresenter>(row);
            }

            if (presenter == null) return null;
            DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
            return cell;
        }
        return null;
    }

    public static DataGridCell GetCell(this DataGrid grid, int row, int column)
    {
        DataGridRow rowContainer = grid.GetRow(row);
        return grid.GetCell(rowContainer, column);
    }

    public class CommandBase : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Func<bool> mCanExecute = null;
        private Action<object> mExecute = null;

        public CommandBase(Action<object> vExecute, Func<bool> fCanExecute = null) { mCanExecute = fCanExecute; mExecute = vExecute; }
        public bool CanExecute(object parameter) { return mCanExecute == null ? true : mCanExecute.Invoke(); }
        public void Execute(object parameter) { mExecute.Invoke(parameter); }
    }

    public static dynamic VBSwitch(params dynamic[] vals)
    {
        for (int i = 0; i < vals.Length; i += 2)
        {
            if (i == vals.Length - 1) return vals[i]; // odd number, return as default.
            if (CBool(vals[i])) return vals[i + 1];
        }
        return null;
    }

    // Concatenating an enum yields the NAME of the enum.  This method provides the underlying value (which could be int, long, byte, etc...)
    public static dynamic Value(this Enum item) => Convert.ChangeType(item, item.GetTypeCode());
    public static T Value<T>(this Enum item) => (T)item.Value();
}
    class Main
    {
    }
}
    public class RandomType
    {
        public int J;
        public string W;
        public string X; // TODO: (NOT SUPPORTED) Fixed Length String not supported: (5)
    }
    private static string ResolveSources(string FileName)
    {
        string _ResolveSources = "";
        if (FileName == "") FileName = "prj.vbp";
        if (FileName == "forms")
        {
            _ResolveSources = VBPForms("");
        }
        else if (FileName == "modules")
        {
            _ResolveSources = VBPModules();
        }
        else if (FileName == "classes")
        {
            _ResolveSources = VBPClasses();
        }
        else if (FileName == "usercontrols")
        {
            _ResolveSources = VBPUserControls();
        }
        else
        {
            if (InStr(FileName, "\\") == 0) FileName = AppContext.BaseDirectory + "\\" + FileName;
            _ResolveSources = (Right(FileName, 4) == ".vbp" ? VBPCode(FileName) : FileName);
        }
        return _ResolveSources;
    }
    public static string Convert(string FileName = "")
    {
        string _Convert = "";
        string FileList = "";
        FileList = ResolveSources(FileName);
        _Convert = QuickConvertFiles(FileList);
        return _Convert;
    }
    public static string QuickConvertFiles(string List)
    {
        string _QuickConvertFiles = "";
        int lintDotsPerRow = 50;
        dynamic L = null;
        int X = 0;
        DateTime StartTime = DateTime.MinValue;
        StartTime = DateTime.Now;
        foreach (var iterL in new List<string>(Split(List, vbCrLf)))
        {
            L = iterL;
            string Result = "";
            Result = QuickConvertFile(L);
            if (Result != "")
            {
                string S = "";
                Console.WriteLine(vbCrLf + "Done (" + DateDiff("s", StartTime, DateTime.Now) + "s).  To re-run for failing file, hit enter on the line below:");
                S = "LINT FAILED: " + L + vbCrLf + Result + vbCrLf + "?Lint(\"" + L + "\")";
                _QuickConvertFiles = S;
                return _QuickConvertFiles;
            }
            else
            {
                Console.Write(Switch(Right(L, 3) == "frm", "o", Right(L, 3) == "cls", "x", Right(L, 3) == "ctl", "+", true, "."));
            }
            X = X + 1;
            if (X >= lintDotsPerRow) { X = 0; Console.WriteLine(); }
            DoEvents();
        }
        Console.WriteLine(vbCrLf + "Done (" + DateDiff("s", StartTime, DateTime.Now) + "s).");
        _QuickConvertFiles = "";
        return _QuickConvertFiles;
    }
    public static CodeType CodeFileType(string File)
    {
        CodeType _CodeFileType = CodeType.CODE_MODULE;
        switch (Right(LCase(File), 4))
        {
            case ".bas":
                _CodeFileType = CodeType.CODE_MODULE;
                break;
            case ".frm":
                _CodeFileType = CodeType.CODE_FORM;
                break;
            case ".cls":
                _CodeFileType = CodeType.CODE_CLASS;
                break;
            case ".ctl":
                _CodeFileType = CodeType.CODE_CONTROL;
                break;
            default:
                _CodeFileType = CodeType.CODE_MODULE;
                break;
        }
        return _CodeFileType;
    }
    public static string QuickConvertFile(string File)
    {
        string _QuickConvertFile = "";
        ModuleArrays = "";
        if (InStr(File, "\\") == 0) File = AppContext.BaseDirectory + "\\" + File;
        string fName = "";
        string Contents = "";
        string GivenName = "";
        string CheckName = "";
        fName = Mid(File, InStrRev(File, "\\") + 1);
        CheckName = Replace(Replace(Replace(fName, ".bas", ""), ".cls", ""), ".frm", "");
        ErrorPrefix = Right(Space(18) + fName, 18) + " ";
        Contents = ReadEntireFile(File);
        GivenName = GetModuleName(Contents);
        if (LCase(CheckName) != LCase(GivenName))
        {
            _QuickConvertFile = "Module name [" + GivenName + "] must match file name [" + fName + "].  Rename module or class to match the other";
            return _QuickConvertFile;
        }
        _QuickConvertFile = ConvertContents(Contents, CodeFileType(File));
        return _QuickConvertFile;
    }
    public static string GetModuleName(string Contents)
    {
        string _GetModuleName = "";
        _GetModuleName = RegExNMatch(Contents, "Attribute VB_Name = \"([^\"]+)\"", 0);
        _GetModuleName = Replace(Replace(_GetModuleName, "Attribute VB_Name = ", ""), "\"", "");
        return _GetModuleName;
    }
    public static string I(int N)
    {
        string _I = "";
        if (N <= 0) _I = ""; else _I = Space(N);
        return _I;
    }
    public static string ConvertContents(string Contents, CodeType vCodeType, bool SubSegment = false)
    {
        string _ConvertContents = "";
        List<string> Lines = new List<string>();
        dynamic ActualLine = null;
        string LL = "";
        string L = "";
        // On Error GoTo LintError
        if (!SubSegment)
        {
            ModuleName = GetModuleName(Contents);
            ModuleFunctions = GetModuleFunctions(Contents);
        }
        Lines = new List<string>(Split(Replace(Contents, vbCr, ""), vbLf));
        bool InAttributes = false;
        bool InBody = false;
        InBody = SubSegment;
        string MultiLineOrig = "";
        string MultiLine = "";
        bool IsMultiLine = false;
        int LineN = 0;
        int Indent = 0;
        string NewContents = "";
        bool SelectHasCase = false;
        Indent = 0;
        NewContents = "";
        // NewContents = UsingEverything & vbCrLf2
        // NewContents = NewContents & __S1 & ModuleName & __S2 & vbCrLf
        foreach (var iterActualLine in Lines)
        {
            ActualLine = iterActualLine;
            LL = ActualLine;
            // If MaxErrors > 0 And ErrorCount >= MaxErrors Then Exit For
            IsMultiLine = false;
            if (Right(LL, 2) == " _")
            {
                string Portion = "";
                Portion = Left(LL, Len(LL) - 2);
                MultiLineOrig = MultiLineOrig + LL + vbCrLf;
                if (MultiLine != "") Portion = " " + Trim(Portion);
                MultiLine = MultiLine + Portion;
                LineN = LineN + 1;
                goto NextLineWithoutRecord;
            }
            else if (MultiLine != "")
            {
                MultiLineOrig = MultiLineOrig + LL;
                LL = MultiLine + " " + Trim(LL);
                MultiLine = "";
                IsMultiLine = true;
            }
            else
            {
                MultiLineOrig = "";
            }
            L = CleanLine(LL);
            if (!InBody)
            {
                bool IsAttribute = false;
                IsAttribute = StartsWith(LTrim(L), "Attribute ");
                if (!InAttributes && IsAttribute)
                {
                    InAttributes = true;
                    goto NextLineWithoutRecord;
                }
                else if (InAttributes && !IsAttribute)
                {
                    InAttributes = false;
                    InBody = true;
                    LineN = 0;
                }
                else
                {
                    goto NextLineWithoutRecord;
                }
            }
            LineN = LineN + 1;
            // If LineN >= 357 Then Stop
            bool UnindentedAlready = false;
            if (RegExTest(L, "^[ ]*(Else|ElseIf .* Then)$"))
            {
                Indent = Indent - Idnt;
                UnindentedAlready = true;
            }
            else if (RegExTest(L, "^[ ]*End Select$"))
            {
                Indent = Indent - Idnt - Idnt;
            }
            else if (RegExTest(L, "^[ ]*(End (If|Function|Sub|Property|Enum|Type)|Next( .*)?|Wend|Loop|Loop (While .*|Until .*)|ElseIf .*)$"))
            {
                Indent = Indent - Idnt;
                UnindentedAlready = true;
                CurrentEnumName = "";
                CurrentTypeName = "";
            }
            else
            {
                UnindentedAlready = false;
            }
            string NewLine = "";
            NewLine = "";
            if (InProperty)
            { // we process properties out of band to keep getters and setters together
                if (InStr(L, "End Property") > 0) InProperty = false;
                goto NextLineWithoutRecord;
            }
            if (CurrentTypeName != "")
            { // if we are in a type or an enum, the entire line is parsed as such
                NewLine = NewLine + ConvertTypeLine(L, vCodeType);
            }
            else if (CurrentEnumName != "")
            {
                NewLine = NewLine + ConvertEnumLine(L);
            }
            else if (RegExTest(L, "^[ ]*If "))
            { // The __S2 control structure, when single-line, lacks the __S3 to signal a close.
                NewLine = NewLine + ConvertIf(L);
                if (InStr(L, " Then ") == 0) Indent = Indent + Idnt;
            }
            else if (RegExTest(L, "^[ ]*ElseIf .*$"))
            {
                NewLine = NewLine + ConvertIf(L);
                if (InStr(L, " Then ") == 0) Indent = Indent + Idnt;
            }
            else
            {
                List<string> Statements = new List<string>();
                int SSI = 0;
                string St = "";
                Statements = new List<string>(Split(Trim(L), ": "));
                for (SSI = 0; SSI <= Statements.Count; SSI += 1)
                {
                    St = Statements[SSI];
                    if (RegExTest(St, "^[ ]*ElseIf .*$"))
                    {
                        NewLine = NewLine + ConvertIf(St);
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*Else$"))
                    {
                        NewLine = NewLine + "} else {";
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*End Function"))
                    {
                        NewLine = NewLine + "return " + CurrentFunctionReturnValue + ";" + vbCrLf + "}";
                        CurrentFunctionName = "";
                        CurrentFunctionReturnValue = "";
                        CurrentFunctionArrays = "";
                        if (!UnindentedAlready) Indent = Indent - Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*End Select$"))
                    {
                        NewLine = NewLine + "break;" + vbCrLf;
                        NewLine = NewLine + "}";
                        if (!UnindentedAlready) Indent = Indent - Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*End (If|Sub|Enum|Type)$"))
                    {
                        CurrentTypeName = "";
                        CurrentEnumName = "";
                        NewLine = NewLine + "}";
                        if (!UnindentedAlready) Indent = Indent - Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*For Each"))
                    {
                        Indent = Indent + Idnt;
                        NewLine = ConvertForEach(St);
                    }
                    else if (RegExTest(St, "^[ ]*For "))
                    {
                        Indent = Indent + Idnt;
                        NewLine = ConvertFor(St);
                    }
                    else if (RegExTest(St, "^[ ]*Next\\b"))
                    {
                        NewLine = NewLine + "}";
                        if (!UnindentedAlready) Indent = Indent - Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*While "))
                    {
                        NewLine = NewLine + ConvertWhile(St);
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*Wend"))
                    {
                        NewLine = NewLine + "}";
                        if (!UnindentedAlready) Indent = Indent - Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*Do (While|Until)"))
                    {
                        NewLine = NewLine + ConvertWhile(St);
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*Loop$"))
                    {
                        NewLine = NewLine + "}";
                    }
                    else if (RegExTest(St, "^[ ]*Do$"))
                    {
                        NewLine = NewLine + "do {";
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*(Loop While |Loop Until )"))
                    {
                        NewLine = NewLine + ConvertWhile(St);
                    }
                    else if (RegExTest(St, "^[ ]*Select Case "))
                    {
                        NewLine = NewLine + ConvertSwitch(St);
                        Indent = Indent + Idnt + Idnt;
                        SelectHasCase = false;
                    }
                    else if (RegExTest(St, "^[ ]*Case "))
                    {
                        NewLine = NewLine + ConvertSwitchCase(St, SelectHasCase);
                        SelectHasCase = true;
                    }
                    else if (RegExTest(St, "^[ ]*(Private |Public )?Declare (Function |Sub )"))
                    {
                        NewLine = NewLine + ConvertDeclare(St); // External Api
                    }
                    else if (RegExTest(St, "^((Private|Public|Friend) )?Function "))
                    {
                        CurrentFunctionArgs = "";
                        Indent = Indent + Idnt;
                        NewLine = NewLine + ConvertSignature(St, vCodeType);
                    }
                    else if (RegExTest(St, "^((Private|Public|Friend) )?Sub "))
                    {
                        CurrentFunctionArgs = "";
                        Indent = Indent + Idnt;
                        NewLine = NewLine + ConvertSignature(St, vCodeType);
                    }
                    else if (RegExTest(St, "^((Private|Public|Friend) )?Property (Get|Let|Set) "))
                    {
                        CurrentFunctionArgs = "";
                        NewLine = NewLine + ConvertProperty(St, Contents, vCodeType);
                        InProperty = !EndsWith(L, "End Property");
                        if (InProperty)
                        {
                            Indent = Indent + Idnt;
                        }
                        else
                        {
                            goto NextLine;
                        }
                    }
                    else if (RegExTest(St, "^[ ]*(Public |Private )?Enum "))
                    {
                        NewLine = NewLine + ConvertEnum(St);
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*(Public |Private )?Type "))
                    {
                        NewLine = NewLine + ConvertType(St);
                        Indent = Indent + Idnt;
                    }
                    else if (RegExTest(St, "^[ ]*(Dim|Private|Public|Const|Global|Static) "))
                    {
                        NewLine = NewLine + ConvertDeclaration(St, CurrentFunctionName == "" ? DeclarationType.DECL_GLOBAL : DeclarationType.DECL_LOCAL, vCodeType);
                    }
                    else
                    {
                        NewLine = NewLine + ConvertStatement(St);
                    }
                NextStatement:;
                }
            }
        NextLine:;
            // If IsMultiLine Then Stop
            // If InStr(LL, __S1) > 0 Then Stop
            // If InStr(LL, __S1) > 0 Then Stop
            // If Indent < 0 Then Stop
            NewLine = Decorate(NewLine);
            if (Trim(NewLine) != "")
            {
                NewContents = NewContents + I(Indent) + NewLine + vbCrLf;
            }
        NextLineWithoutRecord:;
        }
        // If AutoFix <> __S1 Then WriteFile AutoFix, Left(NewContents, Len(NewContents) - 2), True
        // NewContents = NewContents & __S1 & vbCrLf
        _ConvertContents = NewContents;
        return _ConvertContents;
    LintError:;
        Console.WriteLine("Error in quick convert [" + Err().Number + "]: " + Err().Description);
        _ConvertContents = "Error in quick convert [" + Err().Number + "]: " + Err().Description;
        return _ConvertContents;
    }
    private static string ReadEntireFile(string tFileName)
    {
        string _ReadEntireFile = "";
        // TODO: (NOT SUPPORTED): On Error Resume Next
        dynamic mFSO = null;
        mFSO = CreateObject("Scripting.FileSystemObject");
        _ReadEntireFile = mFSO.OpenTextFile(tFileName, 1).ReadAll;
        if (FileLen(tFileName) / 10 != Len(_ReadEntireFile) / 10)
        {
            MsgBox("ReadEntireFile was short: " + FileLen(tFileName) + " vs " + Len(_ReadEntireFile));
        }
        return _ReadEntireFile;
    }
    // de string and decomment a given line (before conversion)
    public static string CleanLine(string Line)
    {
        string _CleanLine = "";
        int X = 0;
        int Y = 0;
        string Token = "";
        string Value = "";
        LineStrings.Clear();
        LineStringsCount = 0;
        LineComment = "";
        while (true)
        {
            X = InStr(Line, Q);
            if (X == 0) break;
            Y = InStr(X + 1, Line, Q);
            while (Mid(Line, Y + 1, 1) == Q)
            {
                Y = InStr(Y + 2, Line, Q);
            }
            if (Y == 0) break;
            LineStringsCount = LineStringsCount + 1;
            // TODO: (NOT SUPPORTED): ReDim Preserve LineStrings(1 To LineStringsCount)
            Value = ConvertStringLiteral(Mid(Line, X, Y - X + 1));
            LineStrings[LineStringsCount] = Value;
            Token = STRING_TOKEN_PREFIX + LineStringsCount;
            Line = Left(Line, X - 1) + Token + Mid(Line, Y + 1);
        }
        X = InStr(Line, A);
        if (X > 0)
        {
            LineComment = Trim(Mid(Line, X + 1));
            Line = RTrim(Left(Line, X - 1));
        }
        _CleanLine = Line;
        return _CleanLine;
    }
    // re-string and re-comment a given line (after conversion)
    public static string Decorate(string Line)
    {
        string _Decorate = "";
        int I = 0;
        for (I = LineStringsCount; I <= -1; I += 1)
        {
            Line = Replace(Line, "__S" + I, LineStrings[I]);
        }
        if (LineComment != "") Line = Line + " // " + LineComment;
        _Decorate = Line;
        return _Decorate;
    }
    public static string ConvertStringLiteral(string L)
    {
        string _ConvertStringLiteral = "";
        L = Replace(L, "\\", "\\\\");
        L = "\"" + Replace(Mid(L, 2, Len(L) - 2), "\"\"", "\\\"") + "\"";
        _ConvertStringLiteral = L;
        return _ConvertStringLiteral;
    }
    public static bool StartsWith(string L, string Find)
    {
        bool _StartsWith = false;
        _StartsWith = Left(L, Len(Find)) == Find;
        return _StartsWith;
    }
    public static bool EndsWith(string L, string Find)
    {
        bool _EndsWith = false;
        _EndsWith = Right(L, Len(Find)) == Find;
        return _EndsWith;
    }
    public static string StripLeft(string L, string Find)
    {
        string _StripLeft = "";
        if (StartsWith(L, Find)) _StripLeft = Mid(L, Len(Find) + 1); else _StripLeft = L;
        return _StripLeft;
    }
    public static bool RecordLeft(ref string L, string Find)
    {
        bool _RecordLeft = false;
        _RecordLeft = StartsWith(L, Find);
        if (_RecordLeft) L = Mid(L, Len(Find) + 1);
        return _RecordLeft;
    }
    public static string RemoveUntil(ref string L, string Find, bool RemoveFind = false)
    {
        string _RemoveUntil = "";
        int IX = 0;
        IX = InStr(L, Find);
        if (IX <= 0) return _RemoveUntil;
        _RemoveUntil = Left(L, IX - 1);
        L = Mid(L, IIf(RemoveFind, IX + Len(Find), IX));
        return _RemoveUntil;
    }
    private static string GetModuleFunctions(string Contents)
    {
        string _GetModuleFunctions = "";
        string Pattern = "(Private (Function|Sub) [^(]+\\()";
        int N = 0;
        int I = 0;
        string S = "";
        N = RegExCount(Contents, Pattern);
        _GetModuleFunctions = "";
        for (I = 0; I <= N - 1; I += 1)
        {
            S = RegExNMatch(Contents, Pattern, I);
            S = Replace(S, "Private ", "");
            S = Replace(S, "Sub ", "");
            S = Replace(S, "Function ", "");
            S = Replace(S, "(", "");
            _GetModuleFunctions = _GetModuleFunctions + "[" + S + "]";
        }
        return _GetModuleFunctions;
    }
    private static bool IsLocalFuncRef(string F)
    {
        bool _IsLocalFuncRef = false;
        _IsLocalFuncRef = InStr(ModuleFunctions, "[" + Trim(F) + "]") != 0;
        return _IsLocalFuncRef;
    }
    private static int SearchLeft(int Start, string Src, string Find, bool NotIn = false, bool Reverse = false)
    {
        int _SearchLeft = 0;
        int Bg = 0;
        int Ed = 0;
        int St = 0;
        int I = 0;
        string C = "";
        bool Found = false;
        if (!Reverse)
        {
            Bg = (Start == 0 ? 1 : Start);
            Ed = Len(Src);
            St = 1;
        }
        else
        {
            Bg = (Start == 0 ? Len(Src) : Start);
            Ed = 1;
            St = -1;
        }
        for (I = Bg; I <= St; I += Ed)
        {
            C = Mid(Src, I, 1);
            Found = InStr(Find, C) > 0;
            if (!NotIn && Found || NotIn && !Found)
            {
                _SearchLeft = I;
                return _SearchLeft;
            }
        }
        _SearchLeft = 0;
        return _SearchLeft;
    }
    // '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    // '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    public static string ConvertIf(string L)
    {
        string _ConvertIf = "";
        int ixThen = 0;
        string Expression = "";
        bool WithThen = false;
        bool WithElse = false;
        bool MultiStatement = false;
        L = Trim(L);
        ixThen = InStr(L, " Then");
        WithThen = InStr(L, " Then ") > 0;
        WithElse = InStr(L, " Else ") > 0;
        Expression = Trim(Left(L, ixThen - 1));
        Expression = StripLeft(Expression, "If ");
        Expression = StripLeft(Expression, "ElseIf ");
        _ConvertIf = !IsInStr(L, "ElseIf") ? "if" : "} else if";
        _ConvertIf = _ConvertIf + "(" + ConvertExpression(Expression) + ")";
        if (!WithThen)
        {
            _ConvertIf = _ConvertIf + " {";
        }
        else
        {
            string cThen = "";
            string cElse = "";
            cThen = Trim(Mid(L, ixThen + 5));
            int ixElse = 0;
            ixElse = InStr(cThen, " Else ");
            if (ixElse > 0)
            {
                cElse = Mid(cThen, ixElse + 6);
                cThen = Left(cThen, ixElse - 1);
            }
            else
            {
                cElse = "";
            }
            // Inline Then
            dynamic St = null;
            MultiStatement = InStr(cThen, ": ") > 0;
            if (MultiStatement)
            {
                _ConvertIf = _ConvertIf + " { ";
                foreach (var iterSt in new List<string>(Split(cThen, ": ")))
                {
                    St = iterSt;
                    _ConvertIf = _ConvertIf + ConvertStatement(St) + " ";
                }
                _ConvertIf = _ConvertIf + "}";
            }
            else
            {
                _ConvertIf = _ConvertIf + ConvertStatement(cThen);
            }
            // Inline Then ... Else
            if (ixElse > 0)
            {
                MultiStatement = InStr(cElse, ":") > 0;
                if (MultiStatement)
                {
                    _ConvertIf = _ConvertIf + " else { ";
                    foreach (var iterSt in new List<string>(Split(cElse, ":")))
                    {
                        St = iterSt;
                        _ConvertIf = _ConvertIf + ConvertStatement(Trim(St));
                    }
                    _ConvertIf = _ConvertIf + " }";
                }
                else
                {
                    _ConvertIf = _ConvertIf + " else " + ConvertStatement(cElse);
                }
            }
        }
        return _ConvertIf;
    }
    public static string ConvertSwitch(string L)
    {
        string _ConvertSwitch = "";
        _ConvertSwitch = "switch(" + ConvertExpression(Trim(Replace(L, "Select Case ", ""))) + ") {";
        return _ConvertSwitch;
    }
    public static string ConvertSwitchCase(string L, bool SelectHasCase)
    {
        string _ConvertSwitchCase = "";
        dynamic V = null;
        _ConvertSwitchCase = "";
        if (SelectHasCase) _ConvertSwitchCase = _ConvertSwitchCase + "break;" + vbCrLf;
        if (Trim(L) == "Case Else")
        {
            _ConvertSwitchCase = _ConvertSwitchCase + "default: ";
        }
        else
        {
            RecordLeft(ref L, "Case ");
            if (Right(L, 1) == ":") L = Left(L, Len(L) - 1);
            foreach (var iterV in new List<string>(Split(L, ", ")))
            {
                V = iterV;
                V = Trim(V);
                if (InStr(V, " To ") > 0)
                {
                    _ConvertSwitchCase = _ConvertSwitchCase + "default: /* TODO: Cannot Convert Ranged Case: " + L + " */";
                }
                else if (StartsWith(V, "Is "))
                {
                    _ConvertSwitchCase = _ConvertSwitchCase + "default: /* TODO: Cannot Convert Expression Case: " + L + " */";
                }
                else
                {
                    _ConvertSwitchCase = _ConvertSwitchCase + "case " + ConvertExpression(V) + ": ";
                }
            }
        }
        return _ConvertSwitchCase;
    }
    public static string ConvertWhile(string L)
    {
        string _ConvertWhile = "";
        string Exp = "";
        bool Closing = false;
        bool Invert = false;
        L = LTrim(L);
        if (RecordLeft(ref L, "Do While "))
        {
            Exp = L;
        }
        else if (RecordLeft(ref L, "Do Until "))
        {
            Exp = L;
            Invert = true;
        }
        else if (RecordLeft(ref L, "While "))
        {
            Exp = L;
        }
        else if (RecordLeft(ref L, "Loop While "))
        {
            Exp = L;
            Closing = true;
        }
        else if (RecordLeft(ref L, "Loop Until "))
        {
            Exp = L;
            Closing = true;
            Invert = true;
        }
        _ConvertWhile = "";
        if (Closing) _ConvertWhile = _ConvertWhile + "} ";
        _ConvertWhile = _ConvertWhile + "while(";
        if (Invert) _ConvertWhile = _ConvertWhile + "!(";
        _ConvertWhile = _ConvertWhile + ConvertExpression(Exp);
        if (Invert) _ConvertWhile = _ConvertWhile + ")";
        _ConvertWhile = _ConvertWhile + ")";
        if (!Closing) _ConvertWhile = _ConvertWhile + " {"; else _ConvertWhile = _ConvertWhile + ";";
        return _ConvertWhile;
    }
    public static string ConvertFor(string L)
    {
        string _ConvertFor = "";
        string Var = "";
        string ForFrom = "";
        string ForTo = "";
        string ForStep = "";
        bool ForReverse = false;
        string ForCheck = "";
        L = Trim(L);
        RecordLeft(ref L, "For ");
        Var = RemoveUntil(ref L, " = ", true);
        ForFrom = RemoveUntil(ref L, " To ", true);
        ForTo = L;
        ForStep = RemoveUntil(ref ForTo, " Step ", true);
        if (ForStep == "") ForStep = "1";
        ForStep = ConvertExpression(ForStep);
        ForReverse = InStr(ForStep, "-") > 0;
        if (ForReverse) ForCheck = " >= "; else ForCheck = " <= ";
        _ConvertFor = "";
        _ConvertFor = _ConvertFor + "for (";
        _ConvertFor = _ConvertFor + ExpandToken(Var) + " = " + ConvertExpression(ForFrom) + "; ";
        _ConvertFor = _ConvertFor + ExpandToken(Var) + ForCheck + ConvertExpression(ForTo) + "; ";
        _ConvertFor = _ConvertFor + ExpandToken(Var) + " += " + ForStep;
        _ConvertFor = _ConvertFor + ") {";
        return _ConvertFor;
    }
    public static string ConvertForEach(string L)
    {
        string _ConvertForEach = "";
        string Var = "";
        string ForSource = "";
        L = Trim(L);
        RecordLeft(ref L, "For ");
        RecordLeft(ref L, "Each ");
        Var = RemoveUntil(ref L, " In ", true);
        ForSource = L;
        _ConvertForEach = _ConvertForEach + "foreach (var iter" + Var + " in " + ConvertExpression(ForSource) + ") {" + vbCrLf + Var + " = iter" + Var + ";";
        return _ConvertForEach;
    }
    public static string ConvertType(string L)
    {
        string _ConvertType = "";
        bool isPrivate = false;
        bool isPublic = false;
        isPublic = RecordLeft(ref L, "Public ");
        isPrivate = RecordLeft(ref L, "Private ");
        RecordLeft(ref L, "Type ");
        CurrentTypeName = L;
        _ConvertType = "";
        if (!isPrivate) _ConvertType = _ConvertType + "public ";
        _ConvertType = _ConvertType + "class "; // `struct ` is available, but leads to non-conforming behavior when indexing in lists...
        _ConvertType = _ConvertType + L;
        _ConvertType = _ConvertType + "{ ";
        return _ConvertType;
    }
    public static string ConvertTypeLine(string L, CodeType vCodeType)
    {
        string _ConvertTypeLine = "";
        _ConvertTypeLine = ConvertDeclaration(L, DeclarationType.DECL_TYPE, vCodeType);
        return _ConvertTypeLine;
    }
    public static string ConvertEnum(string L)
    {
        string _ConvertEnum = "";
        bool isPrivate = false;
        bool isPublic = false;
        isPublic = RecordLeft(ref L, "Public ");
        isPrivate = RecordLeft(ref L, "Private ");
        RecordLeft(ref L, "Enum ");
        CurrentEnumName = L;
        _ConvertEnum = "";
        if (!isPrivate) _ConvertEnum = _ConvertEnum + "public ";
        _ConvertEnum = _ConvertEnum + "enum ";
        _ConvertEnum = _ConvertEnum + L;
        _ConvertEnum = _ConvertEnum + "{ ";
        return _ConvertEnum;
    }
    public static string ConvertEnumLine(string L)
    {
        string _ConvertEnumLine = "";
        string Name = "";
        string Value = "";
        List<string> Parts = new List<string>();
        Parts = new List<string>(Split(L, " = "));
        Name = Trim(Parts[0]);
        if (Parts.Count >= 1) Value = Trim(Parts[1]); else Value = "";
        _ConvertEnumLine = "";
        if (Right(CurrentEnumName, 1) == "+") _ConvertEnumLine = _ConvertEnumLine + ", ";
        _ConvertEnumLine = _ConvertEnumLine + Name;
        if (Value != "") _ConvertEnumLine = _ConvertEnumLine + " = " + ConvertExpression(Value);
        CurrentEnumName = CurrentEnumName + "+"; // convenience
        return _ConvertEnumLine;
    }
    public static string ConvertProperty(string L, string FullContents, CodeType vCodeType)
    {
        string _ConvertProperty = "";
        string Name = "";
        int IX = 0;
        bool isPrivate = false;
        string ReturnType = "";
        string Discard = "";
        string PropertyType = "";
        string GetContents = "";
        string SetContents = "";
        IX = InStr(L, "(");
        Name = Left(L, IX - 1);
        RecordLeft(ref L, "Public ");
        isPrivate = RecordLeft(ref L, "Private ");
        RecordLeft(ref L, "Property ");
        RecordLeft(ref L, "Get ");
        RecordLeft(ref L, "Let ");
        RecordLeft(ref L, "Set ");
        IX = InStr(L, "(");
        Name = Left(L, IX - 1);
        if (InStr(ModuleProperties, Name) > 0) return _ConvertProperty;
        CurrentFunctionName = Name;
        CurrentFunctionReturnValue = "_" + Name;
        ModuleProperties = ModuleProperties + "[" + Name + "]";
        GetContents = FindPropertyBody(FullContents, "Get", Name, ref ReturnType);
        if (GetContents != "") GetContents = ConvertContents(GetContents, vCodeType, true);
        if (ReturnType == "") ReturnType = "Variant";
        SetContents = FindPropertyBody(FullContents, "Let", Name, ref Discard);
        if (SetContents == "") SetContents = FindPropertyBody(FullContents, "Set", Name, ref Discard);
        if (SetContents != "") SetContents = ConvertContents(SetContents, vCodeType, true);
        PropertyType = ConvertArgType(Name, ReturnType);
        _ConvertProperty = "";
        _ConvertProperty = _ConvertProperty + IIf(isPrivate, "private ", "public ");
        _ConvertProperty = _ConvertProperty + IIf(vCodeType == CodeType.CODE_MODULE, "static ", "");
        _ConvertProperty = _ConvertProperty + PropertyType + " " + Name + "{ " + vbCrLf;
        if (GetContents != "")
        {
            _ConvertProperty = _ConvertProperty + "get {" + vbCrLf;
            _ConvertProperty = _ConvertProperty + PropertyType + " " + CurrentFunctionReturnValue + " = default(" + PropertyType + ");" + vbCrLf;
            _ConvertProperty = _ConvertProperty + GetContents;
            _ConvertProperty = _ConvertProperty + "return " + CurrentFunctionReturnValue + ";" + vbCrLf;
            _ConvertProperty = _ConvertProperty + "}" + vbCrLf;
        }
        if (SetContents != "")
        {
            _ConvertProperty = _ConvertProperty + "set {" + vbCrLf;
            _ConvertProperty = _ConvertProperty + SetContents;
            _ConvertProperty = _ConvertProperty + "}" + vbCrLf;
        }
        _ConvertProperty = _ConvertProperty + "}" + vbCrLf;
        return _ConvertProperty;
    }
    public static string FindPropertyBody(string FullContents, string Typ, string Name, ref string ReturnType)
    {
        string _FindPropertyBody = "";
        int X = 0;
        X = InStr(FullContents, "Property " + Typ + " " + Name);
        if (X == 0) return _FindPropertyBody;
        _FindPropertyBody = Mid(FullContents, X);
        X = RegExNPos(_FindPropertyBody, "\\bEnd Property\\b", 0);
        _FindPropertyBody = Trim(Left(_FindPropertyBody, X - 1));
        RecordLeft(ref _FindPropertyBody, "Property " + Typ + " " + Name);
        RecordLeft(ref _FindPropertyBody, "(");
        X = 1;
        while (X > 0)
        {
            if (Left(_FindPropertyBody, 1) == "(") X = X + 1;
            if (Left(_FindPropertyBody, 1) == ")") X = X - 1;
            _FindPropertyBody = Mid(_FindPropertyBody, 2);
        }
        _FindPropertyBody = Trim(_FindPropertyBody);
        if (StartsWith(_FindPropertyBody, "As "))
        {
            _FindPropertyBody = Mid(_FindPropertyBody, 4);
            X = SearchLeft(1, _FindPropertyBody, ": " + vbCrLf, false, false);
            ReturnType = Left(_FindPropertyBody, X - 1);
            _FindPropertyBody = Mid(_FindPropertyBody, X);
        }
        while (StartsWith(_FindPropertyBody, vbCrLf)) { _FindPropertyBody = Mid(_FindPropertyBody, 3); }
        while (Right(_FindPropertyBody, 2) == vbCrLf) { _FindPropertyBody = Left(_FindPropertyBody, Len(_FindPropertyBody) - 2); }
        if (StartsWith(_FindPropertyBody, ":")) _FindPropertyBody = Trim(Mid(_FindPropertyBody, 2));
        if (Right(_FindPropertyBody, 1) == ":") _FindPropertyBody = Trim(Left(_FindPropertyBody, Len(_FindPropertyBody) - 1));
        return _FindPropertyBody;
    }
    public static string ConvertDeclaration(string L, DeclarationType declType, CodeType vCodeType)
    {
        string _ConvertDeclaration = "";
        bool IsDim = false;
        bool isPrivate = false;
        bool isPublic = false;
        bool IsConst = false;
        bool isGlobal = false;
        bool isStatic = false;
        bool IsOptional = false;
        bool IsByVal = false;
        bool IsByRef = false;
        bool IsParamArray = false;
        bool IsWithEvents = false;
        bool IsEvent = false;
        int FixedLength = 0;
        bool IsNewable = false;
        L = Trim(L);
        if (L == "") return _ConvertDeclaration;
        IsDim = RecordLeft(ref L, "Dim ");
        isPrivate = RecordLeft(ref L, "Private ");
        isPublic = RecordLeft(ref L, "Public ");
        isGlobal = RecordLeft(ref L, "Global ");
        IsConst = RecordLeft(ref L, "Const ");
        isStatic = RecordLeft(ref L, "Static ");
        // If IsInStr(L, __S1) Then Stop
        if (isStatic && declType == DeclarationType.DECL_LOCAL) LineComment = LineComment + " TODO: (NOT SUPPORTED) C# Does not support static local variables.";
        dynamic Item = null;
        string LL = "";
        foreach (var iterItem in new List<string>(Split(L, ", ")))
        {
            Item = iterItem;
            int IX = 0;
            string ArgName = "";
            string ArgType = "";
            string ArgDefault = "";
            bool IsArray = false;
            bool IsReferencableType = false;
            string ArgTargetType = "";
            bool StandardEvent = false;
            if (_ConvertDeclaration != "" && declType != DeclarationType.DECL_SIGNATURE && declType != DeclarationType.DECL_EXTERN) _ConvertDeclaration = _ConvertDeclaration + vbCrLf;
            LL = Item;
            IsEvent = RecordLeft(ref LL, "Event ");
            IsWithEvents = RecordLeft(ref LL, "WithEvents ");
            IsOptional = RecordLeft(ref LL, "Optional ");
            IsByVal = RecordLeft(ref LL, "ByVal ");
            IsByRef = RecordLeft(ref LL, "ByRef ");
            IsParamArray = RecordLeft(ref LL, "ParamArray ");
            IX = InStr(LL, " = ");
            if (IX > 0)
            {
                ArgDefault = Trim(Mid(LL, IX + 3));
                LL = Left(LL, IX - 1);
            }
            else
            {
                ArgDefault = "";
            }
            IX = InStr(LL, " As ");
            if (IX > 0)
            {
                ArgType = Trim(Mid(LL, IX + 4));
                LL = Left(LL, IX - 1);
            }
            else
            {
                ArgType = "";
            }
            if (StartsWith(ArgType, "New "))
            {
                IsNewable = true;
                RecordLeft(ref ArgType, "New ");
                LineComment = LineComment + "TODO: (NOT SUPPORTED) Dimmable 'New' not supported on variable declaration.  Instantiated only on declaration.  Please ensure usages";
            }
            if (InStr(ArgType, " * ") > 0)
            {
                FixedLength = ValI(Trim(Mid(ArgType, InStr(ArgType, " * ") + 3)));
                ArgType = RemoveUntil(ref ArgType, " * ");
                LineComment = LineComment + "TODO: (NOT SUPPORTED) Fixed Length String not supported: " + ArgName + "(" + FixedLength + ")";
            }
            ArgTargetType = ConvertArgType(ArgName, ArgType);
            ArgName = LL;
            if (Right(ArgName, 2) == "()")
            {
                IsArray = true;
                ArgName = Left(ArgName, Len(ArgName) - 2);
            }
            else if (RegExTest(ArgName, "^[a-zA-Z_][a-zA-Z_0-9]*\\(.* To .*\\)$"))
            {
                IsArray = true;
                LineComment = LineComment + " TODO: (NOT SUPPORTED) Array ranges not supported: " + ArgName;
                ArgName = RemoveUntil(ref ArgName, "(");
            }
            else
            {
                IsArray = false;
            }
            IsReferencableType = ArgTargetType == "Recordset" || ArgTargetType == "Collection";
            ArgTargetType = ConvertArgType(ArgName, ArgType);
            StandardEvent = IsStandardEvent(ArgName, ArgType);
            switch (((declType)))
            {
                case DeclarationType.DECL_GLOBAL:  // global
                    if (isPublic || IsDim)
                    {
                        _ConvertDeclaration = _ConvertDeclaration + "public ";
                        if (vCodeType == CodeType.CODE_MODULE && !IsConst) _ConvertDeclaration = _ConvertDeclaration + "static ";
                    }
                    else
                    {
                        _ConvertDeclaration = _ConvertDeclaration + "public " + IIf(!IsConst, "static ", "");
                    }
                    if (IsConst) _ConvertDeclaration = _ConvertDeclaration + "const ";
                    _ConvertDeclaration = _ConvertDeclaration + IIf(IsArray, "List<" + ArgTargetType + ">", ArgTargetType) + " ";
                    _ConvertDeclaration = _ConvertDeclaration + ArgName;
                    if (ArgDefault != "")
                    {
                        _ConvertDeclaration = _ConvertDeclaration + " = " + ConvertExpression(ArgDefault);
                    }
                    else
                    {
                        _ConvertDeclaration = _ConvertDeclaration + " = " + ArgTypeDefault(ArgTargetType, IsArray, IsNewable); // VB6 always initializes variables on declaration
                    }
                    _ConvertDeclaration = _ConvertDeclaration + ";";
                    if (IsArray) ModuleArrays = ModuleArrays + "[" + ArgName + "]";
                    break;
                case DeclarationType.DECL_LOCAL:  // function contents
                    _ConvertDeclaration = _ConvertDeclaration + IIf(IsArray, "List<" + ArgTargetType + ">", ArgTargetType) + " ";
                    _ConvertDeclaration = _ConvertDeclaration + ArgName;
                    if (ArgDefault != "")
                    {
                        _ConvertDeclaration = _ConvertDeclaration + " = " + ConvertExpression(ArgDefault);
                    }
                    else
                    {
                        _ConvertDeclaration = _ConvertDeclaration + " = " + ArgTypeDefault(ArgTargetType, IsArray, IsNewable); // VB6 always initializes variables on declaration
                    }
                    _ConvertDeclaration = _ConvertDeclaration + ";";
                    if (IsArray || IsReferencableType) CurrentFunctionArrays = CurrentFunctionArrays + "[" + ArgName + "]";
                    CurrentFunctionArgs = CurrentFunctionArgs + "[" + ArgName + "]";
                    break;
                case DeclarationType.DECL_SIGNATURE:  // sig def
                    if (_ConvertDeclaration != "") _ConvertDeclaration = _ConvertDeclaration + ", ";
                    if (IsByRef || !IsByVal) _ConvertDeclaration = _ConvertDeclaration + "ref ";
                    _ConvertDeclaration = _ConvertDeclaration + IIf(IsArray, "List<" + ArgTargetType + ">", ArgTargetType) + " ";
                    _ConvertDeclaration = _ConvertDeclaration + ArgName;
                    if (ArgDefault != "") _ConvertDeclaration = _ConvertDeclaration + " = " + ConvertExpression(ArgDefault); // default on method sig means optional param
                    if (IsArray || IsReferencableType) CurrentFunctionArrays = CurrentFunctionArrays + "[" + ArgName + "]";
                    CurrentFunctionArgs = CurrentFunctionArgs + "[" + ArgName + "]";
                    break;
                case DeclarationType.DECL_TYPE:
                    _ConvertDeclaration = _ConvertDeclaration + "public " + ArgTargetType + " " + ArgName + ";";
                    break;
                case DeclarationType.DECL_ENUM:
                    break;
                case DeclarationType.DECL_EXTERN:
                    if (_ConvertDeclaration != "") _ConvertDeclaration = _ConvertDeclaration + ", ";
                    if (IsByRef || !IsByVal) _ConvertDeclaration = _ConvertDeclaration + "ref ";
                    _ConvertDeclaration = _ConvertDeclaration + IIf(IsArray, "List<" + ArgTargetType + ">", ArgTargetType) + " ";
                    _ConvertDeclaration = _ConvertDeclaration + ArgName;
                    break;
            }
            // If IsParamArray Then Stop
            if (ArgType == "" && !IsEvent && !StandardEvent)
            {
            }
            if (declType == DeclarationType.DECL_SIGNATURE)
            {
                if (IsParamArray)
                {
                }
                else
                {
                    if (!IsByVal && !IsByRef && !StandardEvent)
                    {
                    }
                }
                if (IsOptional && IsByRef)
                {
                }
                if (IsOptional && ArgDefault == "")
                {
                }
            }
        }
        return _ConvertDeclaration;
    }
    // Function IsStandardEvent(ByVal ArgName As String, ByVal ArgType As String) As Boolean
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // If ArgName = __S1 And ArgType = __S2 Then IsStandardEvent = True: Exit Function
    // IsStandardEvent = False
    // End Function
    public static string ConvertArgType(string Name, string Typ)
    {
        string _ConvertArgType = "";
        switch (Typ)
        {
            case "Long":
            case "Integer":
            case "Int32":
            case "Short":
                _ConvertArgType = "int";
                break;
            case "Currency":
                _ConvertArgType = "decimal";
                break;
            case "Date":
                _ConvertArgType = "DateTime";
                break;
            case "Double":
            case "Float":
            case "Single":
                _ConvertArgType = "decimal";
                break;
            case "String":
                _ConvertArgType = "string";
                break;
            case "Boolean":
                _ConvertArgType = "bool";
                break;
            case "Variant":
            case "Object":
                _ConvertArgType = "dynamic";
                break;
            default:
                _ConvertArgType = Typ;
                break;
        }
        return _ConvertArgType;
    }
    public static string ArgTypeDefault(string ArgType, bool asArray = false, bool IsNewable = false)
    {
        string _ArgTypeDefault = "";
        if (!asArray)
        {
            switch (LCase(ArgType))
            {
                case "string":
                    _ArgTypeDefault = "\"\"";
                    break;
                case "long":
                case "int":
                case "integer":
                case "short":
                case "byte":
                case "decimal":
                case "float":
                case "double":
                case "currency":
                    _ArgTypeDefault = "0";
                    break;
                case "boolean":
                case "bool":
                    _ArgTypeDefault = "false";
                    break;
                case "vbtristate":
                    _ArgTypeDefault = "vbUseDefault";
                    break;
                case "datetime":
                case "date":
                    _ArgTypeDefault = "DateTime.MinValue";
                    break;
                default:
                    _ArgTypeDefault = (IsNewable ? "new " + ArgType + "()" : "null");
                    break;
            }
        }
        else
        {
            _ArgTypeDefault = "new List<" + ArgType + ">()";
        }
        return _ArgTypeDefault;
    }
    public static string ConvertSignature(string LL, CodeType vCodeType = CodeType.CODE_FORM)
    {
        string _ConvertSignature = "";
        string L = "";
        bool WithReturn = false;
        bool isPublic = false;
        bool isPrivate = false;
        bool IsFriend = false;
        bool IsPropertyGet = false;
        bool IsPropertyLet = false;
        bool IsPropertySet = false;
        bool IsFunction = false;
        bool IsSub = false;
        L = LL;
        isPrivate = RecordLeft(ref L, "Private ");
        isPublic = RecordLeft(ref L, "Public ");
        IsFriend = RecordLeft(ref L, "Friend ");
        IsSub = RecordLeft(ref L, "Sub ");
        IsFunction = RecordLeft(ref L, "Function ");
        IsPropertyGet = RecordLeft(ref L, "Property Get ");
        IsPropertyLet = RecordLeft(ref L, "Property let ");
        IsPropertySet = RecordLeft(ref L, "Property set ");
        WithReturn = IsFunction || IsPropertyGet;
        int IX = 0;
        int Ix2 = 0;
        string Name = "";
        string Args = "";
        string Ret = "";
        string RetTargetType = "";
        bool IsArray = false;
        IX = InStr(L, "(");
        if (IX == 0) return _ConvertSignature;
        Name = Left(L, IX - 1);
        if (RegExTest(L, "\\) As .*\\(\\)$"))
        {
            Ix2 = InStrRev(L, ")", Len(L) - 2);
        }
        else
        {
            Ix2 = InStrRev(L, ")");
        }
        Args = Mid(L, IX + 1, Ix2 - IX - 1);
        Ret = Mid(L, Ix2 + 1);
        Ret = Replace(Ret, " As ", "");
        IsArray = Right(Ret, 2) == "()";
        if (IsArray) Ret = Left(Ret, Len(Ret) - 2);
        RetTargetType = ConvertArgType(Name, Ret);
        if (IsArray) RetTargetType = "List<" + RetTargetType + ">";
        CurrentFunctionName = Name;
        CurrentFunctionReturnValue = (WithReturn ? "_" + CurrentFunctionName : "");
        _ConvertSignature = "";
        if (isPublic) _ConvertSignature = _ConvertSignature + "public ";
        if (isPrivate) _ConvertSignature = _ConvertSignature + "private ";
        if (vCodeType == CodeType.CODE_MODULE) _ConvertSignature = _ConvertSignature + "static ";
        _ConvertSignature = _ConvertSignature + IIf(Ret == "", "void ", RetTargetType + " ");
        _ConvertSignature = _ConvertSignature + Name + "(" + ConvertDeclaration(Args, DeclarationType.DECL_SIGNATURE, vCodeType) + ") {";
        if (WithReturn)
        {
            _ConvertSignature = _ConvertSignature + vbCrLf + RetTargetType + " " + CurrentFunctionReturnValue + " = " + ArgTypeDefault(RetTargetType) + ";";
        }
        if (IsEvent(Name)) _ConvertSignature = EventStub(Name) + _ConvertSignature;
        return _ConvertSignature;
    }
    public static string ConvertDeclare(string L)
    {
        string _ConvertDeclare = "";
        // Private Declare Function CreateFile Lib __S1 Alias __S2 (ByVal lpFileName As String, ByVal dwDesiredAccess As Long, ByVal dwShareMode As Long, ByVal lpSecurityAttributes As Long, ByVal dwCreationDisposition As Long, ByVal dwFlagsAndAttributes As Long, ByVal hTemplateFile As Long) As Long
        // [DllImport(__S1)]
        // public static extern int MessageBox(int h, string m, string c, int type);
        bool isPrivate = false;
        bool isPublic = false;
        bool IsFunction = false;
        bool IsSub = false;
        int X = 0;
        string Name = "";
        string cLib = "";
        string cAlias = "";
        string Args = "";
        string Ret = "";
        L = Trim(L);
        isPrivate = RecordLeft(ref L, "Private ");
        isPublic = RecordLeft(ref L, "Public ");
        L = StripLeft(L, "Declare ");
        IsFunction = RecordLeft(ref L, "Function ");
        IsSub = RecordLeft(ref L, "Sub ");
        X = InStr(L, " ");
        Name = Left(L, X - 1);
        L = Mid(L, X + 1);
        if (RecordLeft(ref L, "Lib "))
        {
            X = InStr(L, " ");
            cLib = Left(L, X - 1);
            // If Left(cLib, 1) = __S1 Then cLib = Mid(cLib, 2, Len(cLib) - 2)
            L = Mid(L, X + 1);
        }
        if (RecordLeft(ref L, "Alias "))
        {
            X = InStr(L, " ");
            cAlias = Left(L, X - 1);
            // If Left(cAlias, 1) = __S1 Then cAlias = Mid(cAlias, 2, Len(cAlias) - 2)
            L = Mid(L, X + 1);
        }
        X = InStrRev(L, ")");
        Ret = Trim(Mid(L, X + 1));
        Ret = Replace(Ret, "As ", "");
        Args = Mid(L, 2, X - 2);
        _ConvertDeclare = "";
        _ConvertDeclare = _ConvertDeclare + "[DllImport(" + cLib + ")]" + vbCrLf;
        _ConvertDeclare = _ConvertDeclare + IIf(isPrivate, "private ", "public ") + "static extern ";
        _ConvertDeclare = _ConvertDeclare + IIf(Ret == "", "void", ConvertArgType("return", Ret)) + " ";
        _ConvertDeclare = _ConvertDeclare + Name + "(";
        _ConvertDeclare = _ConvertDeclare + ConvertDeclaration(Args, DeclarationType.DECL_EXTERN, CodeType.CODE_MODULE);
        _ConvertDeclare = _ConvertDeclare + ");";
        return _ConvertDeclare;
    }
    public static string ConvertFileOpen(string L)
    {
        string _ConvertFileOpen = "";
        // Open pathname For mode [ Access access ] [ lock ] As [ # ] filenumber [ Len = reclength ]
        string vPath = "";
        string vMode = "";
        string vAccess = "";
        bool vLock = false;
        string vNumber = "";
        string vLen = "";
        L = Trim(L);
        RecordLeft(ref L, "Open ");
        vPath = RemoveUntil(ref L, " ", true);
        RecordLeft(ref L, "For ");
        vMode = RemoveUntil(ref L, " ", true);
        if (RecordLeft(ref L, "Access ")) vAccess = RemoveUntil(ref L, " ", true);
        vLock = RecordLeft(ref L, "Lock ");
        RecordLeft(ref L, "As #");
        vNumber = L;
        // If RecordLeft(L, __S1) Then vLen = L
        _ConvertFileOpen = "VBOpenFile(" + vPath + ", \"" + vMode + "\", " + vNumber + ")";
        return _ConvertFileOpen;
    }
    public static List<string> SplitByComma(string L)
    {
        List<string> _SplitByComma = null;
        List<string> Results = new List<string>();
        int ResultCount = 0;
        int N = 0;
        int I = 0;
        string C = "";
        int Depth = 0;
        string Part = "";
        N = Len(L);
        for (I = 1; I <= N; I += 1)
        {
            C = Mid(L, I, 1);
            if (C == "(")
            {
                Depth = Depth + 1;
                Part = Part + C;
            }
            else if (Depth > 0 && C == ")")
            {
                Depth = Depth - 1;
                Part = Part + C;
            }
            else if (Depth == 0 && (C == "," || C == ")"))
            {
                ResultCount = ResultCount + 1;
                // TODO: (NOT SUPPORTED): ReDim Preserve Results(1 To ResultCount)
                Results[ResultCount] = Trim(Part);
                Part = "";
            }
            else
            {
                Part = Part + C;
            }
        }
        ResultCount = ResultCount + 1;
        // TODO: (NOT SUPPORTED): ReDim Preserve Results(1 To ResultCount)
        Results[ResultCount] = Trim(Part);
        _SplitByComma = Results;
        return _SplitByComma;
    }
    public static int FindNextOperator(string L)
    {
        int _FindNextOperator = 0;
        int N = 0;
        N = Len(L);
        for (_FindNextOperator = 1; _FindNextOperator <= N; _FindNextOperator += 1)
        {
            if (StartsWith(Mid(L, _FindNextOperator), " && ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " || ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " ^^ ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " - ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " + ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " * ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " / ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " < ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " > ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " >= ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " <= ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " != ")) return _FindNextOperator;
            if (StartsWith(Mid(L, _FindNextOperator), " == ")) return _FindNextOperator;
        }
        _FindNextOperator = 0;
        return _FindNextOperator;
    }
    public static string ConvertIIf(string L)
    {
        string _ConvertIIf = "";
        List<string> Parts = new List<string>();
        string Condition = "";
        string TruePart = "";
        string FalsePart = "";
        Parts = SplitByComma(Mid(Trim(L), 5, Len(L) - 5));
        Condition = Parts[1];
        TruePart = Parts[2];
        FalsePart = Parts[3];
        _ConvertIIf = "(" + ConvertExpression(Condition) + " ? " + ConvertExpression(TruePart) + " : " + ConvertExpression(FalsePart) + ")";
        return _ConvertIIf;
    }
    public static string ConvertStatement(string L)
    {
        string _ConvertStatement = "";
        bool NonCodeLine = false;
        L = Trim(L);
        if (StartsWith(L, "Set ")) L = Mid(L, 5);
        if (StartsWith(L, "Option "))
        {
            // ignore __S1 directives
            NonCodeLine = true;
        }
        else if (RegExTest(L, "^[ ]*Exit (Function|Sub|Property)$"))
        {
            _ConvertStatement = _ConvertStatement + "return";
            if (CurrentFunctionReturnValue != "") _ConvertStatement = _ConvertStatement + " " + CurrentFunctionReturnValue;
        }
        else if (RegExTest(L, "^[ ]*Exit (Do|Loop|For|While)$"))
        {
            _ConvertStatement = _ConvertStatement + "break";
        }
        else if (RegExTest(L, "^[ ]*[^ ]+ = "))
        {
            int IX = 0;
            string AssignmentTarget = "";
            string AssignmentValue = "";
            IX = InStr(L, " = ");
            AssignmentTarget = Trim(Left(L, IX - 1));
            if (InStr(AssignmentTarget, "(") > 0) AssignmentTarget = ConvertExpression(AssignmentTarget);
            if (IsControlRef(AssignmentTarget, ModuleName))
            {
                // If InStr(AssignmentTarget, __S1) > 0 Then Stop
                AssignmentTarget = modRefScan.FormControlRepl(AssignmentTarget, ModuleName);
            }
            if (AssignmentTarget == CurrentFunctionName) AssignmentTarget = CurrentFunctionReturnValue;
            AssignmentValue = Mid(L, IX + 3);
            _ConvertStatement = AssignmentTarget + " = " + ConvertExpression(AssignmentValue);
        }
        else if (RegExTest(L, "^[ ]*Unload "))
        {
            L = Trim(L);
            RecordLeft(ref L, "Unload ");
            _ConvertStatement = (L == "Me" ? "Unload()" : L + ".instance.Unload()");
        }
        else if (RegExTest(L, "^[ ]*With") || RegExTest(L, "^[ ]*End With"))
        {
            _ConvertStatement = "// TODO: (NOT SUPPORTED): " + L;
            NonCodeLine = true;
        }
        else if (RegExTest(L, "^[ ]*(On Error|Resume) "))
        {
            _ConvertStatement = "// TODO: (NOT SUPPORTED): " + L;
            NonCodeLine = true;
        }
        else if (RegExTest(L, "^[ ]*ReDim "))
        {
            _ConvertStatement = "// TODO: (NOT SUPPORTED): " + L;
            NonCodeLine = true;
        }
        else if (RegExTest(L, "^[ ]*Err.Clear"))
        {
            _ConvertStatement = "// TODO: (NOT SUPPORTED): " + L;
            NonCodeLine = true;
        }
        else if (RegExTest(L, "^[ ]*(([a-zA-Z_()0-9.]\\.)*)?[a-zA-Z_0-9.]+$"))
        { // Method call without parens or args (statement, not expression)
            _ConvertStatement = _ConvertStatement + L + "()";
        }
        else if (RegExTest(L, "^[ ]*Close #"))
        {
            _ConvertStatement = "VBCloseFile(" + Replace(Trim(L), "Close #", "") + ")";
            LineComment = LineComment + "TODO: (NOT SUPPORTED) VB File Access Suppressed.  Convert manually: " + L;
        }
        else if (RegExTest(L, "^[ ]*Open .* As #"))
        {
            _ConvertStatement = ConvertFileOpen(L);
            LineComment = LineComment + "TODO: (NOT SUPPORTED) VB File Access Suppressed.  Convert manually: " + L;
        }
        else if (RegExTest(L, "^[ ]*Print #"))
        {
            _ConvertStatement = "VBWriteFile(\"" + Replace(Trim(L), "\"", "\"\"") + "\")";
            LineComment = LineComment + "TODO: (NOT SUPPORTED) VB File Access Suppressed.  Convert manually: " + L;
        }
        else if (RegExTest(L, "^[ ]*(([a-zA-Z_()0-9.]\\.)*)?[a-zA-Z_0-9.]+ .*"))
        { // Method call without parens but with args (statement, not expression)
            string FunctionCall = "";
            string ArgList = "";
            dynamic ArgPart = null;
            int ArgN = 0;
            FunctionCall = RegExNMatch(L, "^[ ]*((([a-zA-Z_()0-9.]\\.)*)?[a-zA-Z_0-9.]+)", 0);
            ArgList = Trim(Mid(L, Len(FunctionCall) + 1));
            _ConvertStatement = ExpandFunctionCall(FunctionCall, ArgList);
        }
        else
        {
            _ConvertStatement = L;
        }
        if (!NonCodeLine) _ConvertStatement = _ConvertStatement + ";";
        return _ConvertStatement;
    }
    public static string ConvertExpression(string L)
    {
        string _ConvertExpression = "";
        L = Replace(L, " \\ ", " / ");
        L = Replace(L, " = ", " == ");
        L = Replace(L, " Mod ", " % ");
        L = Replace(L, " & ", " + ");
        L = Replace(L, " And ", " && ");
        L = Replace(L, " Or ", " || ");
        L = Replace(L, " Xor ", " ^^ ");
        L = Replace(L, " Is ", " == ");
        if (InStr(L, " Like ") > 0) LineComment = LineComment + "TODO: (NOT SUPPORTED) LIKE statement changed to ==: " + L;
        L = Replace(L, " Like ", " == ");
        L = Replace(L, " <> ", " != ");
        L = RegExReplace(L, "\\bNot\\b", "!");
        L = RegExReplace(L, "\\bFalse\\b", "false");
        L = RegExReplace(L, "\\bTrue\\b", "true");
        if (LMatch(LTrim(L), "New ")) L = "new " + Mid(LTrim(L), 5) + "()";
        if (StartsWith(L, "IIf("))
        {
            L = ConvertIIf(L);
        }
        else
        {
            L = ParseAndExpandExpression(L);
        }
        if (CurrentFunctionName != "") L = RegExReplace(L, "\\b" + CurrentFunctionName + "([^(a-zA-Z_])", CurrentFunctionReturnValue + "$1");
        _ConvertExpression = L;
        return _ConvertExpression;
    }
    public static string ParseAndExpandExpression(string Src)
    {
        string _ParseAndExpandExpression = "";
        string S = "";
        string Token = "";
        string T = "";
        int I = 0;
        int J = 0;
        int X = 0;
        int Y = 0;
        string C = "";
        string FunctionName = "";
        string FunctionArgs = "";
        Token = EXPRESSION_TOKEN_PREFIX + CLng(Rnd() * 1000000);
        S = RegExNMatch(Src, "\\([^()]+\\)", 0);
        if (S != "")
        {
            X = InStr(Src, S);
            Src = Replace(Src, S, Token, 1, 1);
            if (X > 1) C = Mid(Src, X - 1, 1); else C = "";
            if (X > 1 && C != "(" && C != ")" && C != " ")
            {
                Y = SearchLeft(X - 1, Src, "() ", false, true);
                FunctionName = Mid(Src, Y + 1, X - Y - 1);
                Src = Replace(Src, FunctionName + Token, Token, 1, 1);
                FunctionArgs = Mid(S, 2, Len(S) - 2);
                if (modRefScan.IsControlRef(FunctionName, ModuleName))
                {
                    _ParseAndExpandExpression = FunctionName + "[" + FunctionArgs + "]" + "." + ConvertControlProperty("", "", FormControlRefDeclType(FunctionName, ModuleName));
                    return _ParseAndExpandExpression;
                }
                FunctionName = ExpandToken(FunctionName, true);
                S = ExpandFunctionCall(FunctionName, FunctionArgs);
                _ParseAndExpandExpression = ParseAndExpandExpression(Src);
                _ParseAndExpandExpression = Replace(_ParseAndExpandExpression, Token, S);
                // Debug.Print __S1 & S
                return _ParseAndExpandExpression;
            }
            else
            { // not a function, but sub expression maybe math
                T = Mid(S, 2, Len(S) - 2);
                X = FindNextOperator(T);
                if (X == 0)
                {
                    _ParseAndExpandExpression = ExpandToken(T);
                }
                else
                {
                    Y = InStr(X + 2, T, " ");
                    S = ExpandToken(Left(T, X - 1)) + Mid(T, X, Y - X + 1) + ParseAndExpandExpression(Mid(T, Y + 1));
                }
                _ParseAndExpandExpression = ParseAndExpandExpression(Src);
                _ParseAndExpandExpression = Replace(_ParseAndExpandExpression, Token, "(" + S + ")");
                // Debug.Print __S1 & S
                return _ParseAndExpandExpression;
            }
        }
        // no subexpression.  Check for math
        X = FindNextOperator(Src);
        if (X == 0)
        {
            _ParseAndExpandExpression = ExpandToken(Src);
            // Debug.Print __S1 & S
            return _ParseAndExpandExpression;
        }
        else
        {
            Y = InStr(X + 2, Src, " ");
            _ParseAndExpandExpression = ParseAndExpandExpression(Left(Src, X - 1)) + Mid(Src, X, Y - X + 1) + ParseAndExpandExpression(Mid(Src, Y + 1));
            // Debug.Print __S1 & S
            return _ParseAndExpandExpression;
        }
        return _ParseAndExpandExpression;
    }
    public static string ExpandToken(string T, bool WillAddParens = false, bool AsLast = false)
    {
        string _ExpandToken = "";
        bool WithNot = false;
        WithNot = Left(T, 1) == "!";
        if (WithNot) T = Mid(T, 2);
        // If InStr(T, __S1) > 0 Then Stop
        // If InStr(T, __S1) > 0 Then Stop
        // If InStr(T, __S1) > 0 Then Stop
        // Debug.Print __S1 & T
        if (T == CurrentFunctionName)
        {
            T = CurrentFunctionReturnValue;
        }
        else if (T == "Rnd")
        {
            T = T + "()";
        }
        else if (T == "Me")
        {
            T = "this";
        }
        else if (T == "App.Path")
        {
            T = "AppContext.BaseDirectory";
        }
        else if (T == "Now")
        {
            T = "DateTime.Now";
        }
        else if (T == "Nothing")
        {
            T = "null";
        }
        else if (T == "Err.Number")
        {
            T = "Err().Number";
        }
        else if (T == "Err.Description")
        {
            T = "Err().Description";
        }
        else if (InStr(CurrentFunctionArgs, T) == 0 && !WillAddParens && (IsFuncRef(T) || IsLocalFuncRef(T)))
        {
            // Debug.Print __S1 & T
            T = T + "()";
        }
        else if (modRefScan.IsFormRef(T))
        {
            T = FormRefRepl(T);
        }
        else if (modRefScan.IsControlRef(T, ModuleName))
        {
            T = FormControlRepl(T, ModuleName);
        }
        else if (modRefScan.IsEnumRef(T))
        {
            T = modRefScan.EnumRefRepl(T);
        }
        else if (Left(T, 2) == "&H")
        { // hex number
            T = "0x" + Mid(T, 3);
            if (Right(T, 1) == "&") T = Left(T, Len(T) - 1);
        }
        else if (RegExTest(T, "^[0-9.-]+[%&@!#]?$"))
        { // plain number.  Maybe:  negative, decimals, or typed
            if (RegExTest(T, "^[0-9.-]+[%&@!#]$")) T = Left(T, Len(T) - 1);
            if (IsInStr(T, ".")) T = T + "m";
        }
        else if (IsInStr(T, "."))
        {
            List<string> Parts = new List<string>();
            int I = 0;
            string Part = "";
            bool IsLast = false;
            string TOut = "";
            // Debug.Print __S1 & T
            TOut = "";
            Parts = new List<string>(Split(T, "."));
            for (I = 0; I <= Parts.Count; I += 1)
            {
                Part = Parts[I];
                IsLast = I == Parts.Count;
                if (TOut != "") TOut = TOut + ".";
                TOut = TOut + ExpandToken(Part, WillAddParens, IsLast);
            }
            T = TOut;
        }
        _ExpandToken = (WithNot ? "!" : "");
        return _ExpandToken;
    }
    public static string ExpandFunctionCall(string FunctionName, string Args)
    {
        string _ExpandFunctionCall = "";
        if (InStr(ModuleArrays + CurrentFunctionArrays + FormControlArrays, "[" + FunctionName + "]") > 0)
        {
            _ExpandFunctionCall = FunctionName + "[" + ProcessFunctionArgs(Args) + "]";
        }
        else if (FunctionName == "LBound")
        {
            _ExpandFunctionCall = "0";
        }
        else if (FunctionName == "UBound")
        {
            _ExpandFunctionCall = Args + ".Count";
        }
        else if (FunctionName == "Split")
        {
            _ExpandFunctionCall = "new List<string>(" + FunctionName + "(" + ProcessFunctionArgs(Args) + ")" + ")";
        }
        else if (FunctionName == "Debug.Print")
        {
            _ExpandFunctionCall = "Console.WriteLine(" + ProcessFunctionArgs(Args) + ")";
        }
        else if (FunctionName == "Erase")
        {
            _ExpandFunctionCall = Args + ".Clear()";
        }
        else if (FunctionName == "GoTo")
        {
            _ExpandFunctionCall = "goto " + Args;
        }
        else if (FunctionName == "Array")
        {
            _ExpandFunctionCall = "new List<dynamic>() {" + ProcessFunctionArgs(Args) + "}";
        }
        else if (FunctionName == "Show")
        {
            _ExpandFunctionCall = (Args == "" ? "Show()" : "ShowDialog()");
        }
        else if (modRefScan.IsFormRef(FunctionName))
        {
            _ExpandFunctionCall = modRefScan.FormRefRepl(FunctionName) + "(" + ProcessFunctionArgs(Args, FunctionName) + ")";
        }
        else
        {
            _ExpandFunctionCall = FunctionName + "(" + ProcessFunctionArgs(Args, FunctionName) + ")";
        }
        _ExpandFunctionCall = RegExReplace(_ExpandFunctionCall, "\\.Show\\(.+\\)", ".ShowDialog()");
        return _ExpandFunctionCall;
    }
    public static string ProcessFunctionArgs(string Args, string FunctionName = "")
    {
        string _ProcessFunctionArgs = "";
        dynamic Arg = null;
        int I = 0;
        foreach (var iterArg in SplitByComma(Args))
        {
            Arg = iterArg;
            I = I + 1;
            if (_ProcessFunctionArgs != "") _ProcessFunctionArgs = _ProcessFunctionArgs + ", ";
            if (FunctionName != "")
            {
                if (modRefScan.IsFuncRef(FunctionName))
                {
                    if (I <= FuncRefDeclArgCnt(FunctionName) && modRefScan.FuncRefArgByRef(FunctionName, I))
                    {
                        // If IsInStr(Arg, STRING_TOKEN_PREFIX) Then Stop
                        _ProcessFunctionArgs = _ProcessFunctionArgs + "ref ";
                    }
                }
            }
            _ProcessFunctionArgs = _ProcessFunctionArgs + ConvertExpression(Arg);
        }
        return _ProcessFunctionArgs;
    }

}
    public class STARTUPINFO
    {
        public int Cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public int wShowWindow;
        public int cbReserved2;
        public int lpReserved2;
        public int hStdInput;
        public int hStdOutput;
        public int hStdError;
    }
    public class PROCESS_INFORMATION
    {
        public int hProcess;
        public int hThread;
        public int dwProcessId;
        public int dwThreadId;
    }
    [DllImport("kernel32")]
    private static extern void Sleep(int dwMilliseconds);
    [DllImport("kernel32")]
    private static extern int CreateProcessA(int lpApplicationName, string lpCommandLine, int lpProcessAttributes, int lpThreadAttributes, int bInheritHandles, int dwCreationFlags, int lpEnvironment, int lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);
    [DllImport("kernel32")]
    private static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);
    [DllImport("kernel32")]
    private static extern bool CloseHandle(ref int hObject);
    [DllImport("USER32")]
    private static extern int GetDesktopWindow();
    [DllImport("shell32")]
    private static extern int ShellExecute(int hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
    // Run a given command and return stdout as a string.
    public static string RunCmdToOutput(string Cmd, out string ErrStr, bool AsAdmin = false)
    {
        string _RunCmdToOutput = "";
        // TODO: (NOT SUPPORTED): On Error GoTo RunError
        string A = "";
        string B = "";
        string C = "";
        long tLen = 0;
        int Iter = 0;
        A = TempFile();
        B = TempFile();
        if (!AsAdmin)
        {
            ShellAndWait("cmd /c " + Cmd + " 1> " + A + " 2> " + B, enSW.enSW_HIDE);
        }
        else
        {
            C = TempFile(".bat");
            WriteFile(C, Cmd + " 1> " + A + " 2> " + B, true);
            RunFileAsAdmin(C, 0, enSW.enSW_HIDE.Value<int>());
        }
        Iter = 0;
        int MaxIter = 10;
        while (true)
        {
            tLen = FileLen(A);
            Sleep(800);
            if (Iter > MaxIter || FileLen(A) == tLen) break;
            Iter = Iter + 1;
        }
        _RunCmdToOutput = ReadEntireFileAndDelete(A);
        if (Iter > MaxIter) _RunCmdToOutput = _RunCmdToOutput + vbCrLf2 + "<<< OUTPUT TRUNCATED >>>";
        ErrStr = ReadEntireFileAndDelete(B);
        DeleteFileIfExists(C);
        return _RunCmdToOutput;
    RunError:;
        _RunCmdToOutput = "";
        ErrStr = "ShellOut.RunCmdToOutput: Command Execution Error - [" + Err().Number + "] " + Err().Description;
        return _RunCmdToOutput;
    }
    // to allow for Shell.
    // This routine shells out to another application and waits for it to exit.
    public static void ShellAndWait(string AppToRun, enSW SW = enSW.enSW_NORMAL)
    {
        PROCESS_INFORMATION NameOfProc = null;
        STARTUPINFO NameStart = null;
        int RC = 0;
        // TODO: (NOT SUPPORTED): On Error GoTo ErrorRoutineErr
        NameStart.Cb = Len(NameStart);
        if (SW == enSW.enSW_HIDE)
        {
            RC = CreateProcessA(0, AppToRun, 0, 0, CInt(SW), CREATE_NO_WINDOW, 0, 0, ref NameStart, ref NameOfProc);
        }
        else
        {
            RC = CreateProcessA(0, AppToRun, 0, 0, CInt(SW), NORMAL_PRIORITY_CLASS, 0, 0, ref NameStart, ref NameOfProc);
        }
        LastProcessID = NameOfProc.dwProcessId;
        RC = WaitForSingleObject(NameOfProc.hProcess, INFINITE);
        RC = CloseHandle(ref NameOfProc.hProcess) ? 1 : 0;
    ErrorRoutineResume:;
        return;
    ErrorRoutineErr:;
        MsgBox("AppShell.Form1.ShellAndWait [" + Err().Number + "]: " + Err().Description);
        // TODO: (NOT SUPPORTED): Resume Next
    }
    // Generic temporary file.  Clean up is your responsibility.  Various configs available.
    public static string TempFile(string UseFolder = "", string UsePrefix = "tmp_", string Extension = ".tmp", bool TestWrite = true)
    {
        string _TempFile = "";
        string FN = "";
        string Res = "";
        if (UseFolder != "" && !DirExists(UseFolder)) UseFolder = "";
        if (UseFolder == "") UseFolder = AppContext.BaseDirectory + DIRSEP;
        if (Right(UseFolder, 1) != DIRSEP) UseFolder = UseFolder + DIRSEP;
        FN = Replace(UsePrefix + CDbl(DateTime.Now) + "_" + AppDomain.GetCurrentThreadId() + "_" + Random(999999), ".", "_");
        while (FileExists(UseFolder + FN + ".tmp"))
        {
            FN = FN + Chr(Random(25) + Asc("a"));
        }
        _TempFile = UseFolder + FN + Extension;
        if (TestWrite)
        {
            // TODO: (NOT SUPPORTED): On Error GoTo TestWriteFailed
            WriteFile(_TempFile, "TEST", true, true);
            // TODO: (NOT SUPPORTED): On Error GoTo TestReadFailed
            Res = ReadFile(_TempFile);
            if (Res != "TEST") MsgBox("Test write to temp file " + _TempFile + " failed." + vbCrLf + "Result (Len=" + Len(Res) + "):" + vbCrLf + Res, vbCritical);
            // TODO: (NOT SUPPORTED): On Error GoTo TestClearFailed
            Kill(_TempFile);
        }
        return _TempFile;
    TestWriteFailed:;
        MsgBox("Failed to write temp file " + _TempFile + "." + vbCrLf + Err().Description, vbCritical);
        return _TempFile;
    TestReadFailed:;
        MsgBox("Failed to read temp file " + _TempFile + "." + vbCrLf + Err().Description, vbCritical);
        return _TempFile;
    TestClearFailed:;
        if (Err().Number == 53)
        {
            // TODO: (NOT SUPPORTED): Err.Clear
            // TODO: (NOT SUPPORTED): Resume Next
        }
        // BFH20160627
        // Jerry wanted this commented out.  Absolutely horrible idea.
        // If IsDevelopment Then
        MsgBox("Failed to clear temp file " + _TempFile + "." + vbCrLf + Err().Description, vbCritical);
        // End If
        return _TempFile;
        return _TempFile;
    }
    // run as admin
    public static void RunShellExecuteAdmin(string App, int nHwnd = 0, int WindowState = SW_SHOWNORMAL)
    {
        if (nHwnd == 0) nHwnd = GetDesktopWindow();
        LastProcessID = ShellExecute(nHwnd, "runas", App, vbNullString, vbNullString, WindowState);
        // ShellExecute nHwnd, __S1, App, Command & __S2, vbNullString, SW_SHOWNORMAL
    }
    // Run as admin 2
    public static bool RunFileAsAdmin(string App, int nHwnd = 0, int WindowState = SW_SHOWNORMAL)
    {
        bool _RunFileAsAdmin = false;
        // If Not IsWinXP Then
        RunShellExecuteAdmin(App, nHwnd, WindowState);
        // Else
        // ShellOut App
        // End If
        _RunFileAsAdmin = true;
        return _RunFileAsAdmin;
    }

}
    public class Variable
    {
        public string Name;
        public string asType;
        public string asArray;
        public bool Param;
        public bool RetVal;
        public bool Assigned;
        public bool Used;
        public bool AssignedBeforeUsed;
        public bool UsedBeforeAssigned;
    }
    public class Property
    {
        public string Name;
        public bool asPublic;
        public string asType;
        public bool asFunc;
        public string Getter;
        public string Setter;
        public string origArgName;
        public string funcArgs;
        public string origProto;
    }
    public static bool Lockout = false;
    public static List<Variable> Vars = new List<Variable>();
    public static List<Property> Props = new List<Property>();
    public static bool Analyze
    {
        get
        {
            bool _Analyze = default(bool);
            _Analyze = Lockout;
            return _Analyze;
        }
    }

    public static void SubBegin(bool setLockout = false)
    {
        if (!setLockout)
        {
            List<Variable> nVars = new List<Variable>();
            Vars = nVars;
        }
        Lockout = Lockout;
    }
    private static int SubParamIndex(string P)
    {
        int _SubParamIndex = 0;
        // TODO: (NOT SUPPORTED): On Error GoTo NoEntries
        for (_SubParamIndex = 0; _SubParamIndex <= Vars.Count; _SubParamIndex += 1)
        {
            if (Vars[_SubParamIndex].Name == P) return _SubParamIndex;
        }
    NoEntries:;
        _SubParamIndex = -1;
        return _SubParamIndex;
    }
    public static Variable SubParam(string P)
    {
        Variable _SubParam = null;
        // TODO: (NOT SUPPORTED): On Error Resume Next
        _SubParam = Vars[SubParamIndex(P)];
        return _SubParam;
    }
    public static void SubParamDecl(string P, string asType, string asArray, bool isParam, bool isReturn)
    {
        if (Lockout) return;
        Variable K = null;
        int N = 0;
        K.Name = P;
        K.Param = isParam;
        // TODO: (NOT SUPPORTED): On Error Resume Next
        N = 0;
        N = Vars.Count + 1;
        // TODO: (NOT SUPPORTED): ReDim Preserve Vars(N)
        Vars[N].Name = P;
        Vars[N].asType = asType;
        Vars[N].Param = isParam;
        Vars[N].RetVal = isReturn;
        Vars[N].asArray = asArray;
    }
    public static void SubParamAssign(string P)
    {
        if (Lockout) return;
        int K = 0;
        K = SubParamIndex(P);
        if (K >= 0)
        {
            Vars[K].Assigned = true;
            if (!Vars[K].Used) Vars[K].AssignedBeforeUsed = true;
        }
    }
    public static void SubParamUsed(string P)
    {
        if (Lockout) return;
        int K = 0;
        K = SubParamIndex(P);
        if (K >= 0)
        {
            Vars[K].Used = true;
            if (!Vars[K].Assigned) Vars[K].UsedBeforeAssigned = true;
        }
    }
    public static void SubParamUsedList(string S)
    {
        List<string> Sp = new List<string>();
        dynamic L = null;
        if (Lockout) return;
        Sp = new List<string>(Split(S, ","));
        foreach (var iterL in Sp)
        {
            L = iterL;
            if (L != "") SubParamUsed(L);
        }
    }
    public static void ClearProperties()
    {
        List<Property> nProps = new List<Property>();
        Props = nProps;
    }
    private static int PropIndex(string P)
    {
        int _PropIndex = 0;
        // TODO: (NOT SUPPORTED): On Error GoTo NoEntries
        for (_PropIndex = 0; _PropIndex <= Props.Count; _PropIndex += 1)
        {
            if (Props[_PropIndex].Name == P) return _PropIndex;
        }
    NoEntries:;
        _PropIndex = -1;
        return _PropIndex;
    }
    public static void AddProperty(string S)
    {
        int X = 0;
        Property PP = null;
        string Pro = "";
        string origProto = "";
        bool asPublic = false;
        bool asFunc = false;
        string GSL = "";
        string pName = "";
        string pArgs = "";
        string pArgName = "";
        string pType = "";
        Pro = SplitWord(S, 1, vbCr);
        origProto = Pro;
        S = nlTrim(Replace(S, Pro, ""));
        if (Right(S, 12) == "End Property") S = nlTrim(Left(S, Len(S) - 12));
        if (LMatch(Pro, "Public ")) { Pro = Mid(Pro, 8); asPublic = true; } // if one is public, both are...
        if (LMatch(Pro, "Private ")) Pro = Mid(Pro, 9);
        if (LMatch(Pro, "Friend ")) Pro = Mid(Pro, 8);
        if (LMatch(Pro, "Property ")) Pro = Mid(Pro, 10);
        if (LMatch(Pro, "Get ")) { Pro = Mid(Pro, 5); GSL = "get"; }
        if (LMatch(Pro, "Let ")) { Pro = Mid(Pro, 5); GSL = "let"; }
        if (LMatch(Pro, "Set ")) { Pro = Mid(Pro, 5); GSL = "set"; }
        pName = RegExNMatch(Pro, patToken);
        Pro = Mid(Pro, Len(pName) + 1);
        if (LMatch(Pro, "(")) Pro = Mid(Pro, 2);
        pArgs = nextBy(Pro, ")");
        if ((GSL == "get" && pArgs != "") || (GSL != "get" && InStr(pArgs, ",") > 0))
        {
            asFunc = true;
        }
        if (GSL == "set" || GSL == "let")
        {
            string fArg = "";
            fArg = Trim(SplitWord(pArgs, -1, ","));
            if (LMatch(fArg, "ByVal ")) fArg = Mid(fArg, 7);
            if (LMatch(fArg, "ByRef ")) fArg = Mid(fArg, 7);
            pArgName = SplitWord(fArg, 1);
            if (SplitWord(fArg, 2, " ") == "As") pType = SplitWord(fArg, 3, " "); else pType = "Variant";
        }
        Pro = Mid(Pro, Len(pArgs) + 1);
        if (LMatch(Pro, ")")) Pro = Trim(Mid(Pro, 2));
        if (LMatch(Pro, "As "))
        {
            Pro = Mid(Pro, 4);
            pType = Pro;
        }
        if (pType == "") pType = "Variant";
        X = PropIndex(pName);
        if (X == -1)
        {
            X = 0;
            // TODO: (NOT SUPPORTED): On Error Resume Next
            X = Props.Count + 1;
            // TODO: (NOT SUPPORTED): On Error GoTo 0
            // TODO: (NOT SUPPORTED): ReDim Preserve Props(X)
        }
        Props[X].Name = pName;
        Props[X].origProto = origProto;
        if (asPublic) Props[X].asPublic = true; // if one is public, both are...
        switch (GSL)
        {
            case "get":
                Props[X].Getter = ConvertSub(S,false , vbTriState.vbFalse);
                Props[X].asType = ConvertDataType(pType);
                Props[X].asFunc = asFunc;
                Props[X].funcArgs = pArgs;
                break;
            case "set":
            case "let":
                Props[X].Setter = ConvertSub(S, false , vbTriState.vbFalse);
                Props[X].origArgName = pArgName;
                if (pType != "") Props[X].asType = ConvertDataType(pType);
                if (asFunc) Props[X].asFunc = true;
                if (pArgs != "") Props[X].funcArgs = pArgs;
                break;
        }
    }
    public static string ReadOutProperties(bool asModule = false)
    {
        string _ReadOutProperties = "";
        // TODO: (NOT SUPPORTED): On Error Resume Next
        int I = 0;
        string R = "";
        Property P = null;
        string N = "";
        string M = "";
        string T = "";
        R = "";
        M = "";
        N = vbCrLf;
        I = -1;
        for (I = 0; I <= Props.Count; I += 1)
        {
            if (I == -1) goto NoItems;
            if (Props[I].Name != "" && !(Props[I].Getter == "" && Props[I].Setter == ""))
            {
                if (Props[I].asPublic) R = R + "public ";
                if (asModule) R = R + "static ";
                // If .Getter = __S1 Then R = R & __S2
                // If .Setter = __S1 Then R = R & __S2
                if (Props[I].asFunc)
                {
                    R = R + " // TODO: Arguments not allowed on properties: " + Props[I].funcArgs + vbCrLf;
                    R = R + " //       " + Props[I].origProto + vbCrLf;
                }
                R = R + M + Props[I].asType + " " + Props[I].Name;
                R = R + " {";
                if (Props[I].Getter != "")
                {
                    R = R + N + "  get {";
                    R = R + N + "    " + Props[I].asType + " " + Props[I].Name + ";";
                    T = Props[I].Getter;
                    T = Replace(T, "Exit(Property)", "return " + Props[I].Name + ";");
                    R = R + N + "    " + T;
                    R = R + N + "  return " + Props[I].Name + ";";
                    R = R + N + "  }";
                }
                if (Props[I].Setter != "")
                {
                    R = R + N + "  set {";
                    T = Props[I].Setter;
                    T = ReplaceToken(T, "value", "valueOrig");
                    T = Replace(T, Props[I].origArgName, "value");
                    T = Replace(T, "Exit Property", "return;");
                    R = R + N + "    " + T;
                    R = R + N + "  }";
                }
                R = R + N + "}";
                R = R + N;
            }
        }
    NoItems:;
        _ReadOutProperties = R;
        return _ReadOutProperties;
    }

}
    internal class Resources {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WinCDS.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
    }
}
