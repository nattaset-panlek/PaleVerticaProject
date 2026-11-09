import json
import math
import os
import matplotlib.pyplot as plt

def round_coord(v):
    return round(float(v), 1)

def get_stat_extras(stat_name):
    if stat_name == "Strength":
        return [{"id": 9, "value": 1.0}, {"id": 21, "value": 2.0}]
    elif stat_name == "Vitality":
        return [{"id": 13, "value": 1.0}, {"id": 3, "value": 10.0}]
    elif stat_name == "Agility":
        return [{"id": 11, "value": 1.0}, {"id": 131, "value": 0.5}]
    elif stat_name == "Intelligence":
        return [{"id": 15, "value": 1.0}, {"id": 23, "value": 2.0}]
    elif stat_name == "Dexterity":
        return [{"id": 17, "value": 1.0}]
    elif stat_name == "Lucky":
        return [{"id": 19, "value": 1.0}]
    else:
        raise ValueError(f"Unknown stat: {stat_name}")

def main():
    source_path = 'Backup/SkillTree/SkillTree_2026-11-09_Original.json'
    dest_path = 'Assets/Resources/Data/SkillTree.json'
    with open(source_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    nodes = [dict(n) for n in data['nodes']]
    nodes_by_id = {n['id']: n for n in nodes}

    # 1. Rename the 23 generic Keystones
    renames = {
        10: "Double Spell",
        13: "Time Collector",
        17: "Sprint",
        18: "Perfect Parry",
        20: "Rock",
        22: "Force Nova",
        23: "Eldritch Faith",
        27: "Vampire",
        32: "Mana Leech",
        58: "Mind Control",
        63: "Fire Bolt",
        93: "Tackle",
        106: "Blood Mage",
        107: "Blessing of Magearna",
        118: "Shadow Dash",
        119: "Double Attack",
        149: "Chronos Field",
        206: "Lightning Strike",
        207: "Thorn",
        322: "Blink",
        323: "Blink Strike",
        333: "Blood Bleeding",
        340: "Time Dilation"
    }

    for nid, new_label in renames.items():
        nodes_by_id[nid]['label'] = new_label
        nodes_by_id[nid]['iconPath'] = "SkillIcon/Icon_KeySkill.png"
        nodes_by_id[nid]['shape'] = "rect"

    # Track existing undirected edge pairs
    existing_undirected = set()
    for e in data['edges']:
        u, v = e['from'], e['to']
        existing_undirected.add(tuple(sorted((u, v))))

    new_undirected_edges = set()

    def add_undirected_edge(u, v):
        if u == v:
            return
        pair = tuple(sorted((u, v)))
        if pair not in existing_undirected:
            new_undirected_edges.add(pair)

    # 2. Heal existing dead ends / leaves
    heal_pairs = [
        (186, 3),   (191, 8),   (183, 184), (190, 189),
        (174, 179), (174, 231), (175, 178), (175, 231),
        (106, 101), (106, 102), (107, 94),  (107, 96),
        (63, 65),   (63, 66),   (206, 200), (206, 201),
        (207, 194), (207, 195), (93, 88),   (93, 92),
        (118, 79),  (118, 83),  (119, 71),  (119, 75),
        (256, 39),  (256, 75),  (257, 51),  (257, 83),
        (32, 33),   (32, 31),   (58, 57),   (58, 59),
        (334, 335), (334, 54),  (157, 264), (157, 153),
        (149, 151), (149, 152), (260, 212), (260, 259),
        (261, 213), (261, 258), (137, 138), (137, 136),
        (141, 142), (141, 139), (265, 241), (265, 157),
        (263, 243), (263, 277), (333, 332), (333, 335),
        (232, 216), (232, 217), (233, 214), (233, 215),
        (340, 326), (340, 329), (138, 159), (142, 158),
        (322, 319), (322, 316), (323, 313), (323, 310),
        (162, 159), (162, 165), (163, 158), (163, 164),
        (332, 241), (156, 153), (156, 155),
        (158, 160), (159, 161), (136, 222), (139, 223),
        (164, 29),  (165, 11),  (143, 120), (143, 12),
        (144, 121), (144, 28),
        (10, 222),  (10, 223),  (13, 274),  (13, 276),
        (17, 300),  (17, 299),  (18, 286),  (18, 287),
        (22, 290),  (22, 291),  (23, 297),  (23, 294),
        (27, 271),  (27, 272),  (148, 147)
    ]

    for u, v in heal_pairs:
        add_undirected_edge(u, v)

    # 3. Class definitions for Outer Constellations
    classes = [
        {
            "sector": 0, "name": "Archer", "angle": 0.0,
            "keystone": "Wind Slash",
            "prim": "Dexterity", "sec": "Agility",
            "spokes": [15, 269, 267]
        },
        {
            "sector": 1, "name": "Gunner", "angle": 30.0,
            "keystone": "Execution",
            "prim": "Dexterity", "sec": "Lucky",
            "spokes": [285, 300, 299]
        },
        {
            "sector": 2, "name": "Inquisitor", "angle": 60.0,
            "keystone": "Lightning Blessing",
            "prim": "Strength", "sec": "Dexterity",
            "spokes": [289, 288, 18]
        },
        {
            "sector": 3, "name": "Warrior", "angle": 90.0,
            "keystone": "Earthshatter",
            "prim": "Strength", "sec": "Vitality",
            "spokes": [20, 220, 221]
        },
        {
            "sector": 4, "name": "Enchanter", "angle": 120.0,
            "keystone": "Aura of Flames",
            "prim": "Strength", "sec": "Intelligence",
            "spokes": [292, 293, 22]
        },
        {
            "sector": 5, "name": "Alchemist", "angle": 150.0,
            "keystone": "Fire Pila",
            "prim": "Intelligence", "sec": "Dexterity",
            "spokes": [284, 297, 294]
        },
        {
            "sector": 6, "name": "Assassin", "angle": 180.0,
            "keystone": "Cherry Blossom Execution",
            "prim": "Agility", "sec": "Lucky",
            "spokes": [25, 268, 266]
        },
        {
            "sector": 7, "name": "Necromancer", "angle": 210.0,
            "keystone": "Curse of Kanix",
            "prim": "Intelligence", "sec": "Vitality",
            "spokes": [270, 283, 282]
        },
        {
            "sector": 8, "name": "Temmer", "angle": 240.0,
            "keystone": "Summon Valkyrie",
            "prim": "Agility", "sec": "Vitality",
            "spokes": [28, 29, 144]
        },
        {
            "sector": 9, "name": "Mage", "angle": 270.0,
            "keystone": "Hellfire Blast",
            "prim": "Intelligence", "sec": "Agility",
            "spokes": [147, 148]
        },
        {
            "sector": 10, "name": "Sorcerer", "angle": 300.0,
            "keystone": "Chain Lightning",
            "prim": "Intelligence", "sec": "Dexterity",
            "spokes": [12, 11, 143]
        },
        {
            "sector": 11, "name": "Priest", "angle": 330.0,
            "keystone": "Divine Blessing",
            "prim": "Intelligence", "sec": "Vitality",
            "spokes": [275, 278, 279]
        }
    ]

    next_node_id = max(n['id'] for n in nodes) + 1
    constellation_nodes = []

    for c in classes:
        ang_deg = c['angle']
        theta = math.radians(ang_deg)
        d_theta = math.radians(3.2)

        # Keystone at r = 2100
        key_id = next_node_id; next_node_id += 1
        key_x = round_coord(2100.0 * math.cos(theta))
        key_y = round_coord(2100.0 * math.sin(theta))
        key_node = {
            "id": key_id,
            "label": c['keystone'],
            "kind": "active",
            "role": "normal",
            "x": key_x,
            "y": key_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "rect",
            "iconPath": "SkillIcon/Icon_KeySkill.png",
            "extras": []
        }
        nodes.append(key_node)

        # Ingress at r = 1950
        ing_id = next_node_id; next_node_id += 1
        ing_x = round_coord(1950.0 * math.cos(theta))
        ing_y = round_coord(1950.0 * math.sin(theta))
        ing_node = {
            "id": ing_id,
            "label": f"Attribute {c['prim']}",
            "kind": "active",
            "role": "normal",
            "x": ing_x,
            "y": ing_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "circle",
            "iconPath": "SkillIcon/Icon_Attribute.png",
            "extras": get_stat_extras(c['prim'])
        }
        nodes.append(ing_node)

        # Apex (Egress) at r = 2280
        apex_id = next_node_id; next_node_id += 1
        apex_x = round_coord(2280.0 * math.cos(theta))
        apex_y = round_coord(2280.0 * math.sin(theta))
        apex_node = {
            "id": apex_id,
            "label": f"Attribute {c['sec']}",
            "kind": "active",
            "role": "normal",
            "x": apex_x,
            "y": apex_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "circle",
            "iconPath": "SkillIcon/Icon_Attribute.png",
            "extras": get_stat_extras(c['sec'])
        }
        nodes.append(apex_node)

        # Inner Left Shoulder at r = 2025, theta - d_theta
        il_id = next_node_id; next_node_id += 1
        il_x = round_coord(2025.0 * math.cos(theta - d_theta))
        il_y = round_coord(2025.0 * math.sin(theta - d_theta))
        il_node = {
            "id": il_id,
            "label": f"Attribute {c['prim']}",
            "kind": "active",
            "role": "normal",
            "x": il_x,
            "y": il_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "circle",
            "iconPath": "SkillIcon/Icon_Attribute.png",
            "extras": get_stat_extras(c['prim'])
        }
        nodes.append(il_node)

        # Inner Right Shoulder at r = 2025, theta + d_theta
        ir_id = next_node_id; next_node_id += 1
        ir_x = round_coord(2025.0 * math.cos(theta + d_theta))
        ir_y = round_coord(2025.0 * math.sin(theta + d_theta))
        ir_node = {
            "id": ir_id,
            "label": f"Attribute {c['sec']}",
            "kind": "active",
            "role": "normal",
            "x": ir_x,
            "y": ir_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "circle",
            "iconPath": "SkillIcon/Icon_Attribute.png",
            "extras": get_stat_extras(c['sec'])
        }
        nodes.append(ir_node)

        # Outer Left Shoulder at r = 2175, theta - d_theta
        ol_id = next_node_id; next_node_id += 1
        ol_x = round_coord(2175.0 * math.cos(theta - d_theta))
        ol_y = round_coord(2175.0 * math.sin(theta - d_theta))
        ol_node = {
            "id": ol_id,
            "label": f"Attribute {c['prim']}",
            "kind": "active",
            "role": "normal",
            "x": ol_x,
            "y": ol_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "circle",
            "iconPath": "SkillIcon/Icon_Attribute.png",
            "extras": get_stat_extras(c['prim'])
        }
        nodes.append(ol_node)

        # Outer Right Shoulder at r = 2175, theta + d_theta
        or_id = next_node_id; next_node_id += 1
        or_x = round_coord(2175.0 * math.cos(theta + d_theta))
        or_y = round_coord(2175.0 * math.sin(theta + d_theta))
        or_node = {
            "id": or_id,
            "label": f"Attribute {c['sec']}",
            "kind": "active",
            "role": "normal",
            "x": or_x,
            "y": or_y,
            "isActive": False,
            "linkStyle": "curve",
            "shape": "circle",
            "iconPath": "SkillIcon/Icon_Attribute.png",
            "extras": get_stat_extras(c['sec'])
        }
        nodes.append(or_node)

        c_dict = {
            "keystone": key_id,
            "ingress": ing_id,
            "apex": apex_id,
            "inner_left": il_id,
            "inner_right": ir_id,
            "outer_left": ol_id,
            "outer_right": or_id
        }
        constellation_nodes.append(c_dict)

        # Constellation internal edges
        add_undirected_edge(ing_id, il_id)
        add_undirected_edge(ing_id, ir_id)
        add_undirected_edge(ing_id, key_id)
        add_undirected_edge(key_id, il_id)
        add_undirected_edge(key_id, ir_id)
        add_undirected_edge(il_id, ol_id)
        add_undirected_edge(ir_id, or_id)
        add_undirected_edge(key_id, ol_id)
        add_undirected_edge(key_id, or_id)
        add_undirected_edge(key_id, apex_id)
        add_undirected_edge(ol_id, apex_id)
        add_undirected_edge(or_id, apex_id)

        # Spokes to perimeter
        for sp in c['spokes']:
            add_undirected_edge(ing_id, sp)
            if len(c['spokes']) >= 2:
                add_undirected_edge(il_id, c['spokes'][0])
                add_undirected_edge(ir_id, c['spokes'][1])

    # 4. Outer Celestial Ring
    for i in range(12):
        c1 = classes[i]
        c2 = classes[(i + 1) % 12]
        apex1_id = constellation_nodes[i]['apex']
        apex2_id = constellation_nodes[(i + 1) % 12]['apex']

        theta1 = c1['angle']
        gw_angles = [theta1 + 7.5, theta1 + 15.0, theta1 + 22.5]
        gw_radii  = [2290.0, 2300.0, 2290.0]
        gw_stats  = [c1['sec'], c1['prim'], c2['prim']]

        gw_ids = []
        for g_ang, g_rad, g_st in zip(gw_angles, gw_radii, gw_stats):
            gid = next_node_id; next_node_id += 1
            rad_theta = math.radians(g_ang)
            gx = round_coord(g_rad * math.cos(rad_theta))
            gy = round_coord(g_rad * math.sin(rad_theta))
            g_node = {
                "id": gid,
                "label": f"Attribute {g_st}",
                "kind": "active",
                "role": "normal",
                "x": gx,
                "y": gy,
                "isActive": False,
                "linkStyle": "curve",
                "shape": "circle",
                "iconPath": "SkillIcon/Icon_Attribute.png",
                "extras": get_stat_extras(g_st)
            }
            nodes.append(g_node)
            gw_ids.append(gid)

        # Connect ring sequence: Apex1 -> GW0 -> GW1 -> GW2 -> Apex2
        add_undirected_edge(apex1_id, gw_ids[0])
        add_undirected_edge(gw_ids[0], gw_ids[1])
        add_undirected_edge(gw_ids[1], gw_ids[2])
        add_undirected_edge(gw_ids[2], apex2_id)

        # Bracing from outer shoulders
        add_undirected_edge(constellation_nodes[i]['outer_right'], gw_ids[0])
        add_undirected_edge(constellation_nodes[(i + 1) % 12]['outer_left'], gw_ids[2])

    # Re-index all nodes
    old_to_new = {}
    for idx, n in enumerate(nodes):
        old_to_new[n['id']] = idx
        n['id'] = idx

    # Build final edges
    final_edges = []
    edge_id = 0
    
    all_undirected = set()
    for e in data['edges']:
        u = old_to_new[e['from']]
        v = old_to_new[e['to']]
        all_undirected.add(tuple(sorted((u, v))))

    for u_old, v_old in new_undirected_edges:
        u = old_to_new[u_old]
        v = old_to_new[v_old]
        all_undirected.add(tuple(sorted((u, v))))

    for u, v in sorted(all_undirected):
        final_edges.append({
            "id": edge_id,
            "from": u,
            "to": v,
            "style": "curve"
        })
        edge_id += 1
        final_edges.append({
            "id": edge_id,
            "from": v,
            "to": u,
            "style": "curve"
        })
        edge_id += 1

    output_data = {
        "nodes": nodes,
        "edges": final_edges
    }
    with open(dest_path, 'w', encoding='utf-8') as f:
        json.dump(output_data, f, indent=2, ensure_ascii=False)
    print(f"Successfully generated expanded tree at {dest_path}")
    print(f"Total Nodes: {len(nodes)}, Total Edges: {len(final_edges)}")

if __name__ == '__main__':
    main()
