import csv

# collab_members.csv からメンバーを読み込む
all_members = set()
with open('collab_members.csv', 'r', encoding='utf-8-sig') as f:
    reader = csv.DictReader(f)
    for row in reader:
        members = row['members'].split(', ')
        for m in members:
            m = m.strip()
            if m:
                all_members.add(m)

# ソートしてCSVに出力
sorted_members = sorted(all_members)
with open('all_members.csv', 'w', encoding='utf-8-sig', newline='') as f:
    writer = csv.writer(f)
    writer.writerow(['member_name'])
    for m in sorted_members:
        writer.writerow([m])

print(f'ユニークなメンバー数: {len(sorted_members)}')
print('出力ファイル: all_members.csv')
print('\n最初の10名:')
for m in sorted_members[:10]:
    print(f'  - {m}')
