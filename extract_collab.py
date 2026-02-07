import re
import csv

# HTMLファイルを読み込む
with open('https___wikiwiki.jp_nijisanji__E3_82_B3_E3_83_A9_E3_83_9C_E4_B8_80_E8_A6_A7_E8_A1_A8.htm', 'r', encoding='utf-8') as f:
    content = f.read()

results = []

# tbody以降のコンテンツを取得
tbody_idx = content.find('start-tag">tbody')
if tbody_idx == -1:
    print("テーブルが見つかりません")
    exit()

table_content = content[tbody_idx:]

# 各行を tr タグで分割
row_pattern = re.compile(r'start-tag">tr</span>&gt;</span>(.*?)end-tag">tr</span>&gt;', re.DOTALL)
rows = row_pattern.findall(table_content)

print(f'テーブル行数: {len(rows)}')

for row in rows:
    # コラボ名を抽出
    # パターン: width:182px;</a>"&gt;</span><span>コラボ名</span>
    collab_pattern = re.compile(r'width:182px;</a>"&gt;</span><span>([^<]+)</span>')
    collab_match = collab_pattern.search(row)
    
    if not collab_match:
        continue
    
    collab_name = collab_match.group(1).strip()
    
    # ヘッダー行をスキップ
    if collab_name in ['コラボ名', '人数', '詳細', 'メンバー', '']:
        continue
    
    # 脚注マークを除去
    collab_name = re.sub(r'\*\d+$', '', collab_name).strip()
    
    if not collab_name:
        continue
    
    # メンバー列（width:765px）の部分だけを抽出
    # パターン: width:765px;</a>"&gt;</span>...end-tag">td</span>&gt;
    member_cell_pattern = re.compile(
        r'width:765px;</a>"&gt;</span>(.*?)end-tag">td</span>&gt;',
        re.DOTALL
    )
    member_cell_match = member_cell_pattern.search(row)
    
    if not member_cell_match:
        continue
    
    member_cell = member_cell_match.group(1)
    
    # メンバーセルからのみメンバーを抽出
    # title属性ではなく表示テキストを使用（例: title="ましろ" だが表示は "ましろ爻"）
    # パターン: rel-wiki-page</a>"&gt;</span><span>表示テキスト</span><span>&lt;/
    member_pattern = re.compile(
        r'rel-wiki-page</a>"&gt;</span><span>([^<]+)</span><span>&lt;/<span class="end-tag">a</span>&gt;'
    )
    members = member_pattern.findall(member_cell)
    
    # コラボ名自体や特殊なタイトルを除外
    excluded = {collab_name, 'FrontPage', ''}
    members = [m for m in members 
               if m not in excluded 
               and not m.startswith('編集') 
               and 'http' not in m
               and '/' not in m]  # ページパス（にじさんじ語録集/... など）を除外
    
    # 重複を除去しつつ順序を保持
    seen = set()
    unique_members = []
    for m in members:
        if m not in seen:
            seen.add(m)
            unique_members.append(m)
    
    if collab_name and unique_members:
        results.append({
            'collab_name': collab_name,
            'members': ', '.join(unique_members)
        })

# CSVに出力
with open('collab_members.csv', 'w', encoding='utf-8-sig', newline='') as f:
    writer = csv.DictWriter(f, fieldnames=['collab_name', 'members'])
    writer.writeheader()
    writer.writerows(results)

print(f'抽出完了: {len(results)} 件のコラボを保存しました')
print('出力ファイル: collab_members.csv')

# 最初の10件を表示
print('\n最初の10件:')
for i, r in enumerate(results[:10]):
    members_display = r["members"][:80] + '...' if len(r["members"]) > 80 else r["members"]
    print(f'{i+1}. {r["collab_name"]}: {members_display}')
