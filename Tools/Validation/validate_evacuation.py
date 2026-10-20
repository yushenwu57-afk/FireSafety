#!/usr/bin/env python3
"""Compile first-party Unity code, exercise evacuation rules and check scene bindings.
Run from the project root: python3 Tools/Validation/validate_evacuation.py
Uses the installed Unity 2022.3.62f3 Mono and cached UI assemblies; no downloads.
This does not enter Play Mode or validate visual layout.
"""
from pathlib import Path
import re
import subprocess
import tempfile

PROJECT = Path(__file__).resolve().parents[2]
UNITY = Path('/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents')
MCS = UNITY / 'MonoBleedingEdge/bin/mcs'
MONO = UNITY / 'MonoBleedingEdge/bin/mono'


def run(arguments):
    subprocess.run([str(arg) for arg in arguments], cwd=PROJECT, check=True)


with tempfile.TemporaryDirectory(prefix='fire-evacuation-') as temporary:
    output = Path(temporary)
    references = {path.name: path for path in (UNITY / 'Managed/UnityEngine').glob('*.dll')}
    references['netstandard.dll'] = UNITY / 'NetStandard/ref/2.1.0/netstandard.dll'
    for name in ['UnityEngine.UI.dll', 'Unity.TextMeshPro.dll']:
        references[name] = PROJECT / 'Library/ScriptAssemblies' / name
    base = [MCS, '-nostdlib', '-target:library'] + [f'-r:{path}' for path in references.values()]
    sources = list((PROJECT / 'Assets/Scripts').glob('*.cs')) + list((PROJECT / 'Assets/FireSafety/Scripts').rglob('*.cs'))
    runtime = output / 'FireRuntime.dll'
    run(base + [f'-out:{runtime}'] + sources)
    print(f'PASS: all {len(sources)} first-party runtime scripts compile.', flush=True)
    editor_refs = {path.name: path for path in (UNITY / 'Managed').glob('UnityEditor*.dll') if path.name not in references}
    run(base + [f'-r:{path}' for path in editor_refs.values()] + [f'-r:{runtime}', f'-out:{output / "FireEditor.dll"}'] + list((PROJECT / 'Assets/Editor').glob('*.cs')))
    print('PASS: existing editor scripts compile.', flush=True)
    scenario = PROJECT / 'Assets/FireSafety/Scripts'
    checks = output / 'EvacuationChecks.exe'
    run([MCS, f'-out:{checks}', scenario / 'Evaluation/PerformanceRank.cs', scenario / 'Evaluation/ResponseQuality.cs', scenario / 'Scenario/EvacuationSession.cs', scenario / 'Scenario/EvacuationRouteGate.cs', PROJECT / 'Tools/Validation/EvacuationSessionChecks.cs'])
    run([MONO, checks])

scene = (PROJECT / 'Assets/Scenes/Level_2.unity').read_text()
blocks = {match[1]: match[0] for match in re.finditer(r'--- !u!\d+ &(\d+)[^\n]*\n.*?(?=--- !u!|\Z)', scene, re.S)}
assert len(blocks) == len(re.findall(r'^--- !u!', scene, re.M)), 'duplicate scene file ID'
controller = blocks['2140000001']
for reference in re.findall(r'\{fileID: (\d+)\}', controller):
    assert reference == '0' or reference in blocks, f'missing local binding: {reference}'
for field in ['decisionPanel', 'titleText', 'bodyText', 'levelText', 'dangerText', 'reticle', 'promptPanel', 'menuPanel', 'menuButton', 'resumeButton', 'restartButton', 'mainMenuButton', 'playerTransform', 'playerMovement', 'movementGameManager', 'secondDecisionLocation', 'kitchenFollowUpLocation', 'thirdDecisionLocation']:
    assert re.search(rf'^  {field}: \{{fileID: [1-9]\d*\}}$', controller, re.M), f'unbound: {field}'
for file_id in ['848959993', '848959998', '593878084', '745399940', '745399942']:
    assert 'm_Enabled: 0' in blocks[file_id], f'legacy behavior not disabled: {file_id}'
assert 'm_LocalScale: {x: 1, y: 1, z: 1}' in blocks['848959997']
for button_id in ['169307887', '1693259705', '1687251304', '485194006']:
    assert f'fileID: {button_id}' in controller and 'm_Interactable: 1' in blocks[button_id]
settings = (PROJECT / 'ProjectSettings/EditorBuildSettings.asset').read_text()
for scene_path in re.findall(r'path: (.+)', settings):
    assert (PROJECT / scene_path).is_file(), f'missing build scene: {scene_path}'
assert 'enabled: 1\n    path: Assets/Scenes/Level_2.unity' in settings
assert 'enabled: 1\n    path: Assets/Scenes/Level.unity' in settings
for filename in ['EvacuationSession.cs', 'EvacuationLevelController.cs', 'EvacuationRouteGate.cs']:
    assert (PROJECT / 'Assets/FireSafety/Scripts/Scenario' / (filename + '.meta')).is_file()
assert 'secondDecisionLocation: {fileID: 243462388}' in controller
floor = blocks['243462387']
assert 'value: Floor_16\n' in floor
for axis, coordinate in [('x', '-3.125'), ('y', '2.5'), ('z', '5.125')]:
    assert f'propertyPath: m_LocalPosition.{axis}\n      value: {coordinate}\n' in floor
level_one = (PROJECT / 'Assets/Scenes/Level.unity').read_text()
for original, copied in [('1410014664', '2140000011'), ('1410014663', '2140000012'), ('1410014662', '2140000013')]:
    source = re.search(r'--- !u!114 &' + original + r'\n.*?(?=--- !u!|\Z)', level_one, re.S)[0]
    target = blocks[copied]
    assert re.search(r'm_Script: (.+)', source)[1] == re.search(r'm_Script: (.+)', target)[1], 'different locomotion script'
    def tuning(block):
        return dict(re.findall(r'^  (\w+): (-?[\d.]+)$', block, re.M))
    expected = tuning(source)
    actual = tuning(target)
    expected.pop('m_Enabled', None)
    actual.pop('m_Enabled', None)
    assert expected == actual, f'Level 1 movement tuning mismatch: {copied}'
    assert 'gameManager: {fileID: 2140000010}' in target
    for reference in re.findall(r'\{fileID: (\d+)\}', target):
        assert reference == '0' or reference in blocks, f'missing locomotion reference: {reference}'
assert 'initialState: 2' in blocks['2140000010']
assert 'm_LocalPosition: {x: 0, y: 1.06, z: 0}' in blocks['139235999']
assert 'm_Enabled: 0' in blocks['593878087']
print('PASS: Level 2 reuses the three active Level 1 locomotion scripts with identical tuning and valid references.')
assert 'kitchenFollowUpLocation: {fileID: 1804536684}' in controller
furniture = blocks['1804536683']
assert 'value: RoomFurniture_03\n' in furniture
assert 'm_TransformParent: {fileID: 1486781259}' in furniture
assert 'm_LocalPosition: {x: 0, y: -0.1, z: 0}' in blocks['1486781259']
for axis, coordinate in [('x', '3.4770002'), ('y', '0.20918305'), ('z', '5.363')]:
    assert f'propertyPath: m_LocalPosition.{axis}\n      value: {coordinate}\n' in furniture
print('PASS: RoomFurniture_03 kitchen follow-up target matches screenshot and includes parent offset.')
assert 'thirdDecisionLocation: {fileID: 692819273}' in controller
carpet = blocks['692819272']
assert 'value: Catpet_04\n' in carpet
assert 'm_TransformParent: {fileID: 1486781259}' in carpet
for axis, coordinate in [('x', '3.223'), ('y', '0.223'), ('z', '4.509')]:
    assert f'propertyPath: m_LocalPosition.{axis}\n      value: {coordinate}\n' in carpet
assert 'thirdArrivalRadius: 0.45' in controller
print('PASS: third decision uses the original Catpet_04 position with no X override.')
print('PASS: Floor_16 movement target matches the requested screenshot.')
print('PASS: level 2 bindings, enabled build scenes, metadata, duplicate IDs and legacy interaction isolation.')
print('NOT RUN: Unity Editor import/Console, Play Mode clicks, Chinese glyph/layout and first-level gameplay regression.')
