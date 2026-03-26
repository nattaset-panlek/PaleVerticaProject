import os

path = r'F:\GameProject\Realm of Hero\My project\Assets\Script\Editor\ProjectMobilityAssistant.cs'

with open(path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
skip = False
for line in lines:
    if 'private enum ToolTab' in line:
        new_lines.append(line)
        new_lines.append('    {\n')
        new_lines.append('        Dashboard,\n')
        new_lines.append('        Attributes,\n')
        new_lines.append('        Items,\n')
        new_lines.append('        Skills,\n')
        new_lines.append('        SupportSkills,\n')
        new_lines.append('        Characters,\n')
        new_lines.append('        IdManager,\n')
        new_lines.append('        Sets\n')
        new_lines.append('    }\n')
        skip = True
    elif skip and '}' in line:
        skip = False
    elif not skip:
        if '_tabNames = {' in line:
            new_lines.append('    private string[] _tabNames = { "Dashboard", "Attributes", "Items", "Skills", "Support Skills", "Characters", "ID Manager", "Sets" };\n')
        elif 'case ToolTab.ItemSetManagement:' in line:
            new_lines.append('            case ToolTab.Sets: DrawItemSetEditor(); break;\n')
        else:
            new_lines.append(line)

with open(path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)
