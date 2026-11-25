"""
Compares the snapshot-input.snapshot.json files under two exported trees, as produced by snapshot.sh.

Usage: python3 -I diff-snapshots.py <base tree> <head tree>

For each snapshot, it lists the resources added, removed and changed, and the outputs that changed. For each change,
it shows a token-level diff of each changed property, as ARM expressions are often one very long line.

A changed resource is flagged "same literals" when both versions contain exactly the same quoted strings and function
names, just in a different order or nesting. That usually means a harmless reordering (for example of union()
arguments), but check the diff to be sure.
"""

import difflib
import glob
import json
import os
import re
import sys
from collections import Counter

SNAPSHOT_GLOB = "infrastructure/templates/**/snapshot-input.snapshot.json"
MAX_SNIPPET = 600


def load(path):
    with open(path) as f:
        snapshot = json.load(f)
    resources = {}
    for resource in snapshot.get("predictedResources", []):
        key = f"{resource.get('type')} {resource.get('name')}"
        while key in resources:
            key += " (duplicate)"
        resources[key] = resource
    return resources, snapshot.get("outputs", {})


def literals(value):
    """
    Counts the plain values, and the quoted strings and function names inside ARM expressions, so that two values
    which only differ in the order or nesting of the same things count the same.
    """
    counts = Counter()
    if isinstance(value, dict):
        for key, item in value.items():
            counts[f"key:{key}"] += 1
            counts += literals(item)
    elif isinstance(value, list):
        for item in value:
            counts += literals(item)
    elif isinstance(value, str) and value.startswith("[") and value.endswith("]"):
        counts += Counter(re.findall(r"'(?:[^']|'')*'|\b[a-zA-Z]+\(", value))
    else:
        counts[json.dumps(value)] += 1
    return counts


def tokens(text):
    return re.findall(r"[A-Za-z0-9_@.\-]+|'[^']*'|.", text)


def print_token_diff(before, after, indent):
    a, b = tokens(json.dumps(before, sort_keys=True)), tokens(json.dumps(after, sort_keys=True))
    matcher = difflib.SequenceMatcher(None, a, b, autojunk=False)
    for op, i1, i2, j1, j2 in matcher.get_opcodes():
        if op == "equal":
            continue
        context = "".join(a[max(0, i1 - 12) : i1])[-120:]
        print(f"{indent}...{context}")
        print(f"{indent}  - {''.join(a[i1:i2])[:MAX_SNIPPET]}")
        print(f"{indent}  + {''.join(b[j1:j2])[:MAX_SNIPPET]}")


def walk(before, after, path, indent):
    if isinstance(before, dict) and isinstance(after, dict):
        for key in sorted(set(before) | set(after)):
            walk(before.get(key), after.get(key), f"{path}.{key}", indent)
    elif before != after:
        print(f"{indent}{path or '(whole value)'}")
        print_token_diff(before, after, indent + "  ")


def main(base_tree, head_tree):
    base_files = sorted(glob.glob(os.path.join(base_tree, SNAPSHOT_GLOB), recursive=True))
    if not base_files:
        sys.exit(f"No snapshots found under {base_tree}")

    differences = 0
    for base_file in base_files:
        relative = os.path.relpath(base_file, base_tree)
        head_file = os.path.join(head_tree, relative)
        label = os.path.dirname(os.path.relpath(base_file, os.path.join(base_tree, "infrastructure/templates")))

        if not os.path.exists(head_file):
            print(f"##### {label}: no head snapshot (did it fail?)")
            differences += 1
            continue

        base, base_outputs = load(base_file)
        head, head_outputs = load(head_file)
        print(f"##### {label}: {len(base)} -> {len(head)} resources")

        for key in sorted(set(base) - set(head)):
            print(f"  REMOVED {key}")
            differences += 1
        for key in sorted(set(head) - set(base)):
            print(f"  ADDED   {key}")
            differences += 1
        for key in sorted(set(base) & set(head)):
            if base[key] != head[key]:
                hint = " (same literals)" if literals(base[key]) == literals(head[key]) else ""
                print(f"  CHANGED {key}{hint}")
                walk(base[key], head[key], "", "      ")
                differences += 1
        if base_outputs != head_outputs:
            hint = " (same literals)" if literals(base_outputs) == literals(head_outputs) else ""
            print(f"  OUTPUTS CHANGED{hint}")
            walk(base_outputs, head_outputs, "", "      ")
            differences += 1

    for head_file in sorted(glob.glob(os.path.join(head_tree, SNAPSHOT_GLOB), recursive=True)):
        if not os.path.exists(os.path.join(base_tree, os.path.relpath(head_file, head_tree))):
            print(f"##### {os.path.relpath(head_file, head_tree)}: no base snapshot (new, or did it fail?)")
            differences += 1

    print(f"\n{differences} difference(s).")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        sys.exit(__doc__)
    main(sys.argv[1], sys.argv[2])
