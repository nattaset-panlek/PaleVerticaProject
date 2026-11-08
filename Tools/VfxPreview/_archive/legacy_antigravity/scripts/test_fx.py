import subprocess
import json

cli = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"

def check_effects(query):
    p = subprocess.run([cli, "exec", "effect.list"], capture_output=True, text=True, encoding="utf-8")
    data = json.loads(p.stdout)
    matches = [e for e in data if query.lower() in e["name"].lower() or query.lower() in e["id"].lower()]
    for m in matches:
        print(f"ID: {m['id']} | Name: {m['name']} | Category: {m.get('category')}")
        print("Params:", [p['id'] for p in m.get('params', [])])

print("--- POLAR ---")
check_effects("polar")
print("--- TURBULENT ---")
check_effects("turbulent")
print("--- FRACTAL ---")
check_effects("fractal")
print("--- GLOW ---")
check_effects("glow")
