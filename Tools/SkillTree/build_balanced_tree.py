import argparse
import collections
import json
import math
import os
import re

TREE_PATH = 'Assets/Resources/Data/SkillTree.json'
UNIQUE_PATH = 'Assets/Resources/Data/UniquePassive.json'
EDGE_CLEARANCE = 30.0
MIN_NODE_DISTANCE = 65.0
ORIGIN_RADIUS = 340.0
TAN_15 = math.tan(math.radians(15.0))

STAT_EXTRAS = {
    'Strength': [{'id': 9, 'value': 1.0}, {'id': 21, 'value': 2.0}],
    'Vitality': [{'id': 13, 'value': 1.0}, {'id': 3, 'value': 10.0}],
    'Agility': [{'id': 11, 'value': 1.0}, {'id': 131, 'value': 0.5}],
    'Intelligence': [{'id': 15, 'value': 1.0}, {'id': 23, 'value': 2.0}],
    'Dexterity': [{'id': 17, 'value': 1.0}],
    'Lucky': [{'id': 19, 'value': 1.0}],
}

ORIGIN_IDS = {'Mage': 0, 'Sorcerer': 1, 'Archer': 2, 'Gunner': 3, 'Inquisitor': 4, 'Warrior': 5, 'Enchanter': 6,
              'Assassin': 7, 'Alchemist': 8, 'Temmer': 9, 'Necomancer': 341, 'Priest': 342}

CLASSES = [
    ('Warrior', 'Strength', 'Vitality'),
    ('Inquisitor', 'Strength', 'Intelligence'),
    ('Enchanter', 'Intelligence', 'Agility'),
    ('Assassin', 'Agility', 'Lucky'),
    ('Gunner', 'Dexterity', 'Lucky'),
    ('Archer', 'Dexterity', 'Agility'),
    ('Mage', 'Intelligence', 'Dexterity'),
    ('Sorcerer', 'Intelligence', 'Lucky'),
    ('Necomancer', 'Intelligence', 'Vitality'),
    ('Alchemist', 'Vitality', 'Intelligence'),
    ('Priest', 'Intelligence', 'Vitality'),
    ('Temmer', 'Vitality', 'Dexterity'),
]

UNIQUE_SLOTS = {
    ('Warrior', 'C1'): 'Rock',
    ('Inquisitor', 'C1'): 'Thorn',
    ('Enchanter', 'C1'): 'Time Collector',
    ('Assassin', 'C1'): 'Blood Bleeding',
    ('Gunner', 'C1'): 'Execution',
    ('Archer', 'C1'): 'Double Attack',
    ('Mage', 'C1'): 'Double Spell',
    ('Sorcerer', 'C1'): 'Depression Chaos',
    ('Necomancer', 'C1'): 'Blood Mage',
    ('Alchemist', 'C1'): 'Eldritch Faith',
    ('Priest', 'C1'): 'Blessing of Magearna',
    ('Temmer', 'C1'): 'Vampire',
    ('Sorcerer', 'C2'): 'Mana Leech',
    ('Necomancer', 'C2'): 'Curse of Kanix',
}
NOT_FOR_TREE = {'Bleeding'}
CLASS_ALIASES = {'Necromancer': 'Necomancer'}
CONCEPTS_PATH = 'Docs/01-Design/Unique_Node_Concepts_TH.md'

CENTER = {'A': 480, 'B': 610, 'H1b': 720, 'C1': 850, 'H1t': 980, 'D': 1120, 'E': 1220, 'H2b': 1330, 'C2': 1480,
          'H2t': 1630, 'F': 1770, 'M': 2120, 'M2': 2290, 'N': 2420, 'H4b': 2550, 'C4': 2720, 'H4t': 2890}
PAIRED = {
    'H1l': (785, 113), 'H1u': (915, 113), 'W1': (1020, 170), 'W2': (1170, 215), 'W3': (1320, 270), 'W4': (1480, 300),
    'W5': (1640, 320), 'H2l': (1405, 130), 'H2u': (1555, 130), 'W6': (1800, 370), 'W7': (1960, 430), 'T': (1890, 125),
    'T3b': (1990, 250), 'T3il': (2055, 137), 'T3iu': (2185, 137), 'T3ol': (2055, 363), 'T3ou': (2185, 363),
    'T3t': (2250, 250), 'C3': (2120, 250), 'R1': (2400, 300), 'R2': (2560, 340), 'R3': (2560, 510),
    'H4l': (2635, 147), 'H4u': (2805, 147),
}
BOUNDARY = {'G1': 850, 'G2': 1640, 'G3': 2120, 'G4': 2560}
CENTER_LINKS = [('A', 'B'), ('B', 'H1b'), ('H1b', 'C1'), ('H1t', 'D'), ('D', 'E'), ('E', 'H2b'), ('H2b', 'C2'),
                ('H2t', 'F'), ('M2', 'N'), ('N', 'H4b'), ('H4b', 'C4')]
FORK_LINKS = [('H1b', 'H1l'), ('H1t', 'H1u'), ('H2b', 'H2l'), ('H2t', 'H2u'), ('F', 'T'), ('M', 'T3il'),
              ('M2', 'T3iu'), ('H4b', 'H4l'), ('H4t', 'H4u')]
SIDE_LINKS = [('H1l', 'H1u'), ('H2l', 'H2u'), ('H4l', 'H4u'), ('H1u', 'W1'), ('W1', 'W2'), ('W2', 'W3'), ('W3', 'W4'),
              ('W4', 'W5'), ('H2l', 'W3'), ('H2u', 'W5'), ('W5', 'W6'), ('W6', 'W7'), ('W7', 'T3ol'), ('T', 'T3b'),
              ('T3b', 'T3il'), ('T3il', 'T3iu'), ('T3iu', 'T3t'), ('T3t', 'T3ou'), ('T3ou', 'T3ol'), ('T3ol', 'T3b'),
              ('T3b', 'C3'), ('T3t', 'R1'), ('R1', 'R2'), ('R2', 'H4l'), ('R2', 'R3')]
BOUNDARY_LINKS = [('H1u', 'G1'), ('W5', 'G2'), ('T3ou', 'G3'), ('R3', 'G4')]
SECOND_STAT_KEYS = ('W', 'R', 'T3')
SLOT_KEYS = ('C1', 'C2', 'C3', 'C4')

SILHOUETTE = {0: 0.88, 30: 1.06, 60: 0.84, 90: 1.24, 120: 0.84, 150: 1.06,
              180: 0.88, 210: 1.18, 240: 0.90, 270: 1.26, 300: 0.90, 330: 1.18}
WARP_START = 600.0
WARP_RANGE = 2300.0

DIAMOND = (1.22, 0.80)
WIDE = (0.82, 1.24)
TALL = (1.30, 0.92)
PLAIN = (1.0, 1.0)
CLUSTER_STYLES = {
    0: {'H1': PLAIN, 'H2': WIDE, 'T3': PLAIN, 'H4': DIAMOND},
    1: {'H1': DIAMOND, 'H2': PLAIN, 'T3': WIDE, 'H4': PLAIN},
    2: {'H1': PLAIN, 'H2': DIAMOND, 'T3': PLAIN, 'H4': WIDE},
    3: {'H1': WIDE, 'H2': TALL, 'T3': DIAMOND, 'H4': TALL},
    4: {'H1': PLAIN, 'H2': WIDE, 'T3': TALL, 'H4': PLAIN},
    5: {'H1': DIAMOND, 'H2': PLAIN, 'T3': WIDE, 'H4': DIAMOND},
    6: {'H1': WIDE, 'H2': DIAMOND, 'T3': PLAIN, 'H4': TALL},
}
STYLE_BY_ANGLE = {90: 3, 270: 6, 0: 0, 180: 0, 30: 1, 150: 1, 60: 2, 120: 2, 210: 4, 330: 4, 240: 5, 300: 5}
CLUSTER_CENTERS = {'H1': (850, 0), 'H2': (1480, 0), 'H4': (2720, 0), 'T3': (2120, 250)}
CLUSTER_KEYS = {'H1l', 'H1u', 'H1b', 'H1t', 'H2l', 'H2u', 'H2b', 'H2t', 'H4l', 'H4u', 'H4b', 'H4t',
                'T3b', 'T3il', 'T3iu', 'T3ol', 'T3ou', 'T3t'}


def silhouette_scale(angle):
    angle %= 360.0
    low = int(angle // 30) * 30 % 360
    high = (low + 30) % 360
    blend = (1.0 - math.cos(math.radians((angle - low) * 6.0))) * 0.5
    return SILHOUETTE[low] + (SILHOUETTE[high] - SILHOUETTE[low]) * blend


def warp(x, y):
    radius = math.hypot(x, y)
    angle = math.degrees(math.atan2(y, x))
    amount = max(0.0, min(1.0, (radius - WARP_START) / WARP_RANGE))
    factor = 1.0 + (silhouette_scale(angle) - 1.0) * amount
    return round(x * factor, 1), round(y * factor, 1)


def to_world(sector, u, v):
    angle = math.radians(90.0 + sector * 30.0)
    return warp(u * math.cos(angle) - v * math.sin(angle), u * math.sin(angle) + v * math.cos(angle))


def styled(sector, key, u, v):
    if key not in CLUSTER_KEYS:
        return u, v
    group = key[:2]
    center_u, center_v = CLUSTER_CENTERS[group]
    tall, wide = CLUSTER_STYLES[STYLE_BY_ANGLE[(90 + sector * 30) % 360]][group]
    return center_u + (u - center_u) * tall, center_v + (v - center_v) * wide


def shared_stat(sector):
    _, primary, secondary = CLASSES[sector]
    _, next_primary, next_secondary = CLASSES[(sector + 1) % 12]
    return secondary if secondary in (next_primary, next_secondary) else primary


def slots():
    result = {}
    for sector in range(12):
        for key, u in CENTER.items():
            result[(sector, key)] = to_world(sector, styled(sector, key, u, 0.0)[0], 0.0)
        for key, (u, v) in PAIRED.items():
            styled_u, styled_v = styled(sector, key, u, v)
            result[(sector, key + '-')] = to_world(sector, styled_u, -styled_v)
            result[(sector, key + '+')] = to_world(sector, styled_u, styled_v)
        for key, u in BOUNDARY.items():
            result[(sector, key)] = to_world(sector, u, u * TAN_15)
    return result


def links():
    result = []
    for s in range(12):
        result.append((('origin', s), (s, 'A')))
        result += [((s, a), (s, b)) for a, b in CENTER_LINKS]
        for a, b in FORK_LINKS:
            result += [((s, a), (s, b + '-')), ((s, a), (s, b + '+'))]
        for a, b in SIDE_LINKS:
            result += [((s, a + '-'), (s, b + '-')), ((s, a + '+'), (s, b + '+'))]
        for a, boundary in BOUNDARY_LINKS:
            result.append(((s, a + '+'), (s, boundary)))
            result.append(((s, boundary), ((s + 1) % 12, a + '-')))
    return result


def stat_for(slot):
    sector, key = slot
    _, primary, secondary = CLASSES[sector]
    if key in BOUNDARY:
        return shared_stat(sector)
    return secondary if key.startswith(SECOND_STAT_KEYS) else primary


def concept_slots():
    sides = {'ซ้าย': '-', 'ขวา': '+'}
    with open(CONCEPTS_PATH, encoding='utf-8') as handle:
        rows = re.findall(r'^\| ([A-Za-z]+) \| (C[234])(?: (\S+))? \| ([^|]+?) \|', handle.read(), re.M)
    return {(CLASS_ALIASES.get(name, name), slot + sides.get(side, '')): label for name, slot, side, label in rows}


def build():
    with open(UNIQUE_PATH, encoding='utf-8-sig') as handle:
        unique_ids = {e['Name']: e['Id'] for e in json.load(handle)['UniquePassives']}
    all_slots = {**UNIQUE_SLOTS, **concept_slots()}
    positions = slots()
    count = 12 + len(positions)
    free_ids = iter(i for i in range(count) if i not in ORIGIN_IDS.values())
    nodes, ids = [], {}

    def base_node(node_id, x, y):
        return {'id': node_id, 'kind': 'active', 'role': 'normal', 'x': x, 'y': y, 'isActive': False, 'linkStyle': 'curve'}

    for sector, (name, _, _) in enumerate(CLASSES):
        x, y = to_world(sector, ORIGIN_RADIUS, 0.0)
        node = base_node(ORIGIN_IDS[name], x, y)
        node.update(label=name, role='base', shape='poly', polySides=6, iconPath='SkillIcon/Icon_Skill.png', extras=[])
        nodes.append(node)
        ids[('origin', sector)] = node['id']

    for slot, (x, y) in positions.items():
        node = base_node(next(free_ids), x, y)
        ids[slot] = node['id']
        key = slot[1].rstrip('+-')
        unique = all_slots.get((CLASSES[slot[0]][0], slot[1])) if key in SLOT_KEYS else None
        if unique:
            node.update(label=unique, shape='rect', iconPath='SkillIcon/Icon_KeySkill.png', extras=[],
                        uniquePassiveId=unique_ids[unique])
        else:
            stat = stat_for(slot)
            node.update(label=f'Attribute {stat}', shape='circle', iconPath='SkillIcon/Icon_Attribute.png',
                        extras=[dict(e) for e in STAT_EXTRAS[stat]])
        nodes.append(node)

    edges = []
    for a, b in links():
        for u, v in ((ids[a], ids[b]), (ids[b], ids[a])):
            edges.append({'id': len(edges), 'from': u, 'to': v, 'style': 'curve'})
    nodes.sort(key=lambda n: n['id'])
    return {'nodes': nodes, 'edges': edges}


def segments_cross(a, b, c, d):
    def side(p, q, r):
        return (q[0] - p[0]) * (r[1] - p[1]) - (q[1] - p[1]) * (r[0] - p[0])
    return side(a, b, c) * side(a, b, d) < 0 and side(c, d, a) * side(c, d, b) < 0


def point_segment_distance(p, a, b):
    dx, dy = b[0] - a[0], b[1] - a[1]
    t = max(0.0, min(1.0, ((p[0] - a[0]) * dx + (p[1] - a[1]) * dy) / (dx * dx + dy * dy)))
    return math.dist(p, (a[0] + t * dx, a[1] + t * dy))


def hops_from(origin, adjacency, roles):
    distance = {origin: 0}
    queue = collections.deque([origin])
    while queue:
        current = queue.popleft()
        for other in adjacency[current]:
            if other not in distance and roles[other] != 'base':
                distance[other] = distance[current] + 1
                queue.append(other)
    return distance


def check(tree):
    nodes = tree['nodes']
    points = {n['id']: (n['x'], n['y']) for n in nodes}
    roles = {n['id']: n['role'] for n in nodes}
    pairs = sorted({tuple(sorted((e['from'], e['to']))) for e in tree['edges']})
    adjacency = {n['id']: set() for n in nodes}
    for u, v in pairs:
        adjacency[u].add(v)
        adjacency[v].add(u)
    crossings = sum(1 for i, (a, b) in enumerate(pairs) for c, d in pairs[i + 1:]
                    if len({a, b, c, d}) == 4 and segments_cross(points[a], points[b], points[c], points[d]))
    ids = list(points)
    nearest = min(math.dist(points[a], points[b]) for i, a in enumerate(ids) for b in ids[i + 1:])
    grazing = [(u, v, w) for u, v in pairs for w in ids if w not in (u, v)
               and point_segment_distance(points[w], points[u], points[v]) < EDGE_CLEARANCE]
    big = {n['id']: n['label'] for n in nodes if n['shape'] == 'rect'}
    classes = {}
    for node in nodes:
        if node['role'] != 'base':
            continue
        distance = hops_from(node['id'], adjacency, roles)
        classes[node['label']] = (len(distance), sum(1 for h in distance.values() if h <= 15),
                                  min(h for i, h in distance.items() if i in big))
    with open(UNIQUE_PATH, encoding='utf-8-sig') as handle:
        uniques = {e['Name'] for e in json.load(handle)['UniquePassives']}
    stats = collections.Counter(n['label'].split(' ', 1)[1] for n in nodes if n['label'].startswith('Attribute '))
    return {
        'nodes': len(nodes), 'links': len(pairs), 'crossings': crossings, 'nearest': round(nearest, 1),
        'edges_through_nodes': grazing, 'class_reach_within15_firstUnique': sorted(set(classes.values())),
        'classes': len(classes), 'dead_ends': sum(1 for n in adjacency.values() if len(n) <= 1),
        'big_not_unique': sorted(set(big.values()) - uniques),
        'unique_without_node': sorted(uniques - set(big.values()) - NOT_FOR_TREE),
        'stats': dict(sorted(stats.items())),
    }


def render(tree, path):
    import matplotlib
    matplotlib.use('Agg')
    import matplotlib.pyplot as plt
    points = {n['id']: (n['x'], -n['y']) for n in tree['nodes']}
    fig, ax = plt.subplots(figsize=(15, 15), dpi=110)
    for u, v in {tuple(sorted((e['from'], e['to']))) for e in tree['edges']}:
        ax.plot(*zip(points[u], points[v]), color='#808080', lw=0.7, zorder=1)
    colors = {'Strength': '#d9534f', 'Vitality': '#e08a2e', 'Agility': '#4cae4c', 'Intelligence': '#4a7fd6',
              'Dexterity': '#22b8b8', 'Lucky': '#c9b03a'}
    for node in tree['nodes']:
        x, y = points[node['id']]
        if node['shape'] == 'rect':
            ax.scatter([x], [y], s=170, marker='s', color='#111111', zorder=3)
            ax.annotate(node['label'], (x, y), fontsize=7, ha='center', va='bottom', xytext=(0, 8), textcoords='offset points')
        elif node['shape'] == 'poly':
            ax.scatter([x], [y], s=240, marker='h', color='#7a3fb0', zorder=3)
            ax.annotate(node['label'][:4], (x, y), fontsize=6, ha='center', va='center', color='white', zorder=4)
        else:
            ax.scatter([x], [y], s=24, color=colors[node['label'].split(' ', 1)[1]], zorder=2)
    ax.set_aspect('equal')
    ax.axis('off')
    fig.savefig(path, bbox_inches='tight')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--check-only', action='store_true')
    parser.add_argument('--render')
    args = parser.parse_args()
    if args.check_only:
        with open(TREE_PATH, encoding='utf-8-sig') as handle:
            tree = json.load(handle)
    else:
        tree = build()
        report = check(tree)
        if report['crossings'] or report['edges_through_nodes'] or report['nearest'] < MIN_NODE_DISTANCE \
                or len(report['class_reach_within15_firstUnique']) != 1:
            raise SystemExit(f'not written: {report}')
        with open(TREE_PATH, 'w', encoding='utf-8', newline='\n') as handle:
            json.dump(tree, handle, indent=2, ensure_ascii=False)
    print(check(tree))
    if args.render:
        render(tree, args.render)


if __name__ == '__main__':
    os.chdir(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
    main()
