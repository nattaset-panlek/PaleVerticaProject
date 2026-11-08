#!/usr/bin/env bash
# Run Unity editor methods in batch mode, one after another (Git Bash on Windows).
#
# Usage:
#   Tools/FigmaToUnity/unity_batch.sh <Method> [<Method> ...]
#   Tools/FigmaToUnity/unity_batch.sh tests
#
# Examples:
#   unity_batch.sh CoreSystem.Editor.UI.SkillManagePrefabBuilder.BuildBatch CoreSystem.Editor.UI.UISnapshotRenderer.RenderAllBatch
#   unity_batch.sh tests
#
# Unity must be closed (batch mode cannot open a project the editor has open).
# The script waits until it is closed. Logs go to Logs/Batch/<name>.log.

UNITY="${UNITY:-D:/Unity/6000.4.8f1/Editor/Unity.exe}"
PROJECT="$(cd "$(dirname "$0")/../.." && pwd)"
LOGS="$PROJECT/Logs/Batch"
mkdir -p "$LOGS"

unity_open() {
  tasklist //FI "IMAGENAME eq Unity.exe" 2>/dev/null | grep -q "Unity.exe" || [ -f "$PROJECT/Temp/UnityLockfile" ]
}

if unity_open; then
  echo "Unity is open. Ask the user to save and close it. Waiting..."
  while unity_open; do sleep 3; done
fi

status=0
for method in "$@"; do
  if [ "$method" = "tests" ]; then
    log="$LOGS/tests.log"
    "$UNITY" -batchmode -nographics -projectPath "$PROJECT" -runTests -testPlatform EditMode -testResults "$LOGS/results.xml" -logFile "$log"
    code=$?
    python -I - "$LOGS/results.xml" <<'EOF'
import sys, xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
print(f"tests: {root.get('passed')}/{root.get('total')} passed, {root.get('failed')} failed")
for case in root.iter("test-case"):
    if case.get("result") == "Failed":
        msg = case.find("failure/message")
        print("  FAIL", case.get("fullname"), (msg.text or "").strip()[:300] if msg is not None else "")
EOF
  else
    log="$LOGS/${method##*.}_$(echo "$method" | awk -F. '{print $(NF-1)}').log"
    "$UNITY" -batchmode -projectPath "$PROJECT" -executeMethod "$method" -logFile "$log"
    code=$?
    grep -E "error CS|Exception|issue\(s\)|^\[|^  " "$log" | grep -v "^\[Licensing\|^\[Package\|^\[Worker\|^\[ScriptCompilation\|^  [A-Z]:" | head -40
  fi
  echo "$method -> exit $code (log: $log)"
  [ $code -ne 0 ] && status=$code
done
exit $status
