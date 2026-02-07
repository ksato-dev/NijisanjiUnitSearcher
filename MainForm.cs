using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NijisanjiUnitSearcher
{
    public class MainForm : Form
    {
        private List<string> allMembers = new List<string>();
        private List<(string CollabName, List<string> Members)> collabs = new List<(string, List<string>)>();

        private TextBox txtSearch = null!;
        private ListBox lstCandidates = null!;
        private Button btnAdd = null!;
        private ListBox lstSelected = null!;
        private Button btnRemove = null!;
        private Button btnClear = null!;
        private ListView lvResults = null!;
        private Label lblStatus = null!;
        private TextBox txtDetail = null!;
        private ToolTip toolTip = null!;

        public MainForm()
        {
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "にじさんじユニット検索";
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Yu Gothic UI", 10);

            // === 左パネル（メンバー選択） ===
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                Padding = new Padding(10)
            };

            // 検索ボックス
            var lblSearch = new Label
            {
                Text = "メンバー検索:",
                Location = new Point(10, 10),
                AutoSize = true
            };

            txtSearch = new TextBox
            {
                Location = new Point(10, 35),
                Width = 200,
                PlaceholderText = "名前を入力..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            txtSearch.KeyDown += TxtSearch_KeyDown;

            btnAdd = new Button
            {
                Text = "追加",
                Location = new Point(220, 33),
                Width = 60,
                Height = 26
            };
            btnAdd.Click += BtnAdd_Click;

            // 候補リスト
            var lblCandidates = new Label
            {
                Text = "候補:",
                Location = new Point(10, 70),
                AutoSize = true
            };

            lstCandidates = new ListBox
            {
                Location = new Point(10, 95),
                Size = new Size(270, 200)
            };
            lstCandidates.DoubleClick += LstCandidates_DoubleClick;
            lstCandidates.KeyDown += LstCandidates_KeyDown;

            // 選択済みメンバー
            var lblSelected = new Label
            {
                Text = "選択済みメンバー:",
                Location = new Point(10, 305),
                AutoSize = true
            };

            lstSelected = new ListBox
            {
                Location = new Point(10, 330),
                Size = new Size(270, 200)
            };
            lstSelected.KeyDown += LstSelected_KeyDown;

            // ボタン
            btnRemove = new Button
            {
                Text = "削除",
                Location = new Point(10, 540),
                Width = 80,
                Height = 30
            };
            btnRemove.Click += BtnRemove_Click;

            btnClear = new Button
            {
                Text = "クリア",
                Location = new Point(100, 540),
                Width = 80,
                Height = 30
            };
            btnClear.Click += BtnClear_Click;

            pnlLeft.Controls.AddRange(new Control[] { 
                lblSearch, txtSearch, btnAdd,
                lblCandidates, lstCandidates,
                lblSelected, lstSelected,
                btnRemove, btnClear
            });

            // === 右パネル（検索結果） ===
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblResults = new Label
            {
                Text = "検索結果（選択メンバー全員を含むコラボ）:",
                Location = new Point(10, 10),
                AutoSize = true
            };

            lvResults = new ListView
            {
                Location = new Point(10, 35),
                Size = new Size(600, 420),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lvResults.Columns.Add("コラボ名", 200);
            lvResults.Columns.Add("メンバー", 380);
            lvResults.SelectedIndexChanged += LvResults_SelectedIndexChanged;
            lvResults.MouseMove += LvResults_MouseMove;

            // ツールチップ
            toolTip = new ToolTip
            {
                AutoPopDelay = 10000,
                InitialDelay = 300,
                ReshowDelay = 100
            };

            // 詳細表示エリア
            var lblDetail = new Label
            {
                Text = "メンバー詳細（項目を選択）:",
                Location = new Point(10, 465),
                AutoSize = true
            };

            txtDetail = new TextBox
            {
                Location = new Point(10, 490),
                Size = new Size(600, 70),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White
            };

            lblStatus = new Label
            {
                Location = new Point(10, 570),
                AutoSize = true,
                Text = ""
            };

            pnlRight.Controls.AddRange(new Control[] { lblResults, lvResults, lblDetail, txtDetail, lblStatus });

            // フォームにパネルを追加
            this.Controls.Add(pnlRight);
            this.Controls.Add(pnlLeft);

            // リサイズ対応
            this.Resize += (s, e) => AdjustLayout();
        }

        private void AdjustLayout()
        {
            int leftPanelWidth = 300;
            int bottomMargin = 100;

            // 左パネルのレイアウト調整
            int availableHeight = this.ClientSize.Height - bottomMargin;
            int candidateHeight = (availableHeight - 150) / 2;
            int selectedTop = 95 + candidateHeight + 40;

            lstCandidates.Height = candidateHeight;
            
            lstSelected.Top = lstCandidates.Bottom + 40;
            lstSelected.Height = candidateHeight;

            // ラベル位置調整
            foreach (Control c in this.Controls)
            {
                if (c is Panel pnl && pnl.Dock == DockStyle.Left)
                {
                    foreach (Control ctrl in pnl.Controls)
                    {
                        if (ctrl is Label lbl && lbl.Text == "選択済みメンバー:")
                        {
                            lbl.Top = lstCandidates.Bottom + 15;
                        }
                    }
                    break;
                }
            }

            lstSelected.Top = lstCandidates.Bottom + 40;
            btnRemove.Top = lstSelected.Bottom + 10;
            btnClear.Top = lstSelected.Bottom + 10;

            // 右パネルのレイアウト調整
            int rightWidth = this.ClientSize.Width - leftPanelWidth - 30;
            lvResults.Width = rightWidth;
            lvResults.Height = this.ClientSize.Height - 230;
            
            // 詳細エリアの位置調整
            foreach (Control c in this.Controls)
            {
                if (c is Panel pnl && pnl.Dock == DockStyle.Fill)
                {
                    foreach (Control ctrl in pnl.Controls)
                    {
                        if (ctrl is Label lbl && lbl.Text.StartsWith("メンバー詳細"))
                        {
                            lbl.Top = lvResults.Bottom + 10;
                        }
                    }
                    break;
                }
            }
            
            txtDetail.Top = lvResults.Bottom + 35;
            txtDetail.Width = rightWidth;
            lblStatus.Top = txtDetail.Bottom + 10;

            if (lvResults.Columns.Count >= 2)
            {
                lvResults.Columns[1].Width = lvResults.Width - lvResults.Columns[0].Width - 25;
            }
        }

        private void LoadData()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string? membersPath = FindFile("all_members.csv", baseDir);
            string? collabsPath = FindFile("collab_members.csv", baseDir);

            if (membersPath == null || collabsPath == null)
            {
                MessageBox.Show(
                    "CSVファイルが見つかりません。\nall_members.csv と collab_members.csv を実行ファイルと同じディレクトリに配置してください。",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            LoadMembers(membersPath);
            LoadCollabs(collabsPath);

            // 初期表示：全メンバーを候補に
            lstCandidates.Items.AddRange(allMembers.ToArray());

            lblStatus.Text = $"メンバー: {allMembers.Count}人 / コラボ: {collabs.Count}件";
        }

        private string? FindFile(string fileName, string baseDir)
        {
            string path = Path.Combine(baseDir, fileName);
            if (File.Exists(path)) return path;

            DirectoryInfo? dir = new DirectoryInfo(baseDir);
            while (dir != null)
            {
                path = Path.Combine(dir.FullName, fileName);
                if (File.Exists(path)) return path;
                dir = dir.Parent;
            }

            return null;
        }

        private void LoadMembers(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            allMembers = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        }

        private void LoadCollabs(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var (collabName, membersStr) = ParseCsvLine(line);
                if (collabName == null) continue;

                var members = membersStr
                    .Split(',')
                    .Select(m => m.Trim())
                    .Where(m => !string.IsNullOrEmpty(m))
                    .ToList();

                collabs.Add((collabName, members));
            }
        }

        private (string? CollabName, string Members) ParseCsvLine(string line)
        {
            int firstComma = -1;
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    firstComma = i;
                    break;
                }
            }

            if (firstComma == -1) return (null, "");

            string collabName = line.Substring(0, firstComma);
            string members = line.Substring(firstComma + 1).Trim().Trim('"');

            return (collabName, members);
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            lstCandidates.Items.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                lstCandidates.Items.AddRange(allMembers.ToArray());
            }
            else
            {
                var filtered = allMembers
                    .Where(m => m.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                lstCandidates.Items.AddRange(filtered);
            }

            // 候補が1件のみなら自動選択
            if (lstCandidates.Items.Count == 1)
            {
                lstCandidates.SelectedIndex = 0;
            }
        }

        private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && lstCandidates.Items.Count > 0)
            {
                lstCandidates.Focus();
                lstCandidates.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                AddSelectedMember();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void LstCandidates_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddSelectedMember();
                e.Handled = true;
            }
        }

        private void LstCandidates_DoubleClick(object? sender, EventArgs e)
        {
            AddSelectedMember();
        }

        private void LstSelected_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedMember();
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            AddSelectedMember();
        }

        private void AddSelectedMember()
        {
            string? memberToAdd = null;

            // 候補リストで選択されていればそれを使用
            if (lstCandidates.SelectedItem != null)
            {
                memberToAdd = lstCandidates.SelectedItem.ToString();
            }
            // 候補が1件のみならそれを使用
            else if (lstCandidates.Items.Count == 1)
            {
                memberToAdd = lstCandidates.Items[0].ToString();
            }
            // 検索テキストから部分一致で探す
            else if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var match = allMembers.FirstOrDefault(m => 
                    m.Equals(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    match = allMembers.FirstOrDefault(m => 
                        m.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase));
                }
                memberToAdd = match;
            }

            if (string.IsNullOrEmpty(memberToAdd))
            {
                MessageBox.Show("メンバーを選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!lstSelected.Items.Contains(memberToAdd))
            {
                lstSelected.Items.Add(memberToAdd);
            }

            // 検索テキストをクリア
            txtSearch.Clear();
            txtSearch.Focus();

            // 自動検索実行
            SearchCollabs();
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            RemoveSelectedMember();
        }

        private void RemoveSelectedMember()
        {
            if (lstSelected.SelectedIndex >= 0)
            {
                lstSelected.Items.RemoveAt(lstSelected.SelectedIndex);
                SearchCollabs();
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            lstSelected.Items.Clear();
            lvResults.Items.Clear();
            txtSearch.Clear();
            txtSearch.Focus();
            lblStatus.Text = $"メンバー: {allMembers.Count}人 / コラボ: {collabs.Count}件";
        }

        private void SearchCollabs()
        {
            if (lstSelected.Items.Count == 0)
            {
                lvResults.Items.Clear();
                lblStatus.Text = $"メンバー: {allMembers.Count}人 / コラボ: {collabs.Count}件";
                return;
            }

            var selectedMembers = lstSelected.Items.Cast<string>().ToList();

            var matchingCollabs = collabs
                .Where(c => selectedMembers.All(m => c.Members.Any(cm => cm.Equals(m, StringComparison.OrdinalIgnoreCase))))
                .OrderBy(c => c.CollabName)
                .ToList();

            lvResults.Items.Clear();

            foreach (var collab in matchingCollabs)
            {
                var item = new ListViewItem(collab.CollabName);
                item.SubItems.Add(string.Join(", ", collab.Members));
                lvResults.Items.Add(item);
            }

            lblStatus.Text = $"検索結果: {matchingCollabs.Count}件 （選択: {string.Join(", ", selectedMembers)}）";
        }

        private void LvResults_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count > 0)
            {
                var item = lvResults.SelectedItems[0];
                string collabName = item.Text;
                string members = item.SubItems[1].Text;
                txtDetail.Text = $"【{collabName}】\r\n{members}";
            }
            else
            {
                txtDetail.Text = "";
            }
        }

        private int lastHoveredIndex = -1;

        private void LvResults_MouseMove(object? sender, MouseEventArgs e)
        {
            var hit = lvResults.HitTest(e.Location);
            if (hit.Item != null)
            {
                int index = hit.Item.Index;
                if (index != lastHoveredIndex)
                {
                    lastHoveredIndex = index;
                    string members = hit.Item.SubItems[1].Text;
                    toolTip.SetToolTip(lvResults, $"【{hit.Item.Text}】\n{members}");
                }
            }
            else
            {
                if (lastHoveredIndex != -1)
                {
                    lastHoveredIndex = -1;
                    toolTip.SetToolTip(lvResults, "");
                }
            }
        }
    }
}
