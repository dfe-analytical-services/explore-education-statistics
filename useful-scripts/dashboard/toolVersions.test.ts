import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { MIN_CORE_TOOLS_VERSION } from './functionHostHealth';
import {
  describeCoreToolsIssue,
  describeDotnetIssue,
  describeNodeIssue,
  describePythonIssue,
} from './toolVersions';
import {
  isOlderVersion,
  parseVersion,
  versionSatisfiesPin,
} from './utils/versions';

describe('parseVersion', () => {
  it('reads the version out of the formats the tools actually print', () => {
    // `func --version` prints the bare version, python prefixes it, .nvmrc
    // may carry a "v" and a trailing newline.
    assert.equal(parseVersion('4.14.0\n'), '4.14.0');
    assert.equal(parseVersion('Python 3.14.6'), '3.14.6');
    assert.equal(parseVersion('v22.23.1\n'), '22.23.1');
    assert.equal(parseVersion('10.0.302'), '10.0.302');
  });

  it('returns undefined when there is no version to read', () => {
    assert.equal(parseVersion(''), undefined);
    assert.equal(parseVersion('command not found: func'), undefined);
  });
});

describe('isOlderVersion', () => {
  it('compares numerically, not lexically', () => {
    // Lexically '4.9.0' > '4.13.0', which is exactly the bug this guards.
    assert.equal(isOlderVersion('4.9.0', '4.13.0'), true);
    assert.equal(isOlderVersion('4.13.0', '4.9.0'), false);
  });

  it('treats missing parts as zero', () => {
    assert.equal(isOlderVersion('4.13', '4.13.0'), false);
    assert.equal(isOlderVersion('4.13', '4.13.1'), true);
  });

  it('is false for equal versions', () => {
    assert.equal(isOlderVersion('4.14.0', '4.14.0'), false);
  });
});

describe('versionSatisfiesPin', () => {
  it('matches to the precision the pin specifies', () => {
    assert.equal(versionSatisfiesPin('22.23.1', '22'), true);
    assert.equal(versionSatisfiesPin('22.23.1', '22.23'), true);
    assert.equal(versionSatisfiesPin('22.23.1', '22.23.1'), true);
    assert.equal(versionSatisfiesPin('22.22.0', '22.23.1'), false);
    assert.equal(versionSatisfiesPin('23.0.0', '22'), false);
  });
});

describe('describeCoreToolsIssue', () => {
  it('reports a missing func as a warning that names the minimum', () => {
    const issue = describeCoreToolsIssue(undefined, '4.14.0');

    assert.ok(issue);
    assert.equal(issue.severity, 'warning');
    assert.match(issue.message, /isn't on PATH/);
    assert.match(issue.message, new RegExp(MIN_CORE_TOOLS_VERSION));
  });

  it('reports a version below the minimum as an error, before any host has faulted', () => {
    const issue = describeCoreToolsIssue('4.9.0', '4.14.0');

    assert.ok(issue);
    assert.equal(issue.severity, 'error');
    assert.match(issue.message, /net10\.0/);
    assert.match(issue.message, new RegExp(MIN_CORE_TOOLS_VERSION));
  });

  it('nudges about a newer release, naming both versions', () => {
    const issue = describeCoreToolsIssue('4.13.0', '4.14.0');

    assert.ok(issue);
    assert.equal(issue.severity, 'warning');
    assert.match(issue.message, /4\.13\.0 installed, 4\.14\.0 latest/);
  });

  it('says nothing when up to date', () => {
    assert.equal(describeCoreToolsIssue('4.14.0', '4.14.0'), undefined);
  });

  it('says nothing about staleness when the latest release is unknown', () => {
    // Offline is a normal state for a laptop - a good-enough version with no
    // latest to compare against isn't worth a banner.
    assert.equal(describeCoreToolsIssue('4.13.0', undefined), undefined);
  });

  it("doesn't report a version newer than the latest release as behind it", () => {
    assert.equal(describeCoreToolsIssue('4.15.0', '4.14.0'), undefined);
  });
});

describe('describeDotnetIssue', () => {
  it('reports an unsatisfied global.json as an error naming the pin', () => {
    const issue = describeDotnetIssue(undefined, {
      version: '10.0.302',
      rollForward: 'latestMinor',
    });

    assert.ok(issue);
    assert.equal(issue.severity, 'error');
    assert.match(issue.message, /10\.0\.302/);
    assert.match(issue.message, /latestMinor/);
  });

  it("says nothing when 'dotnet --version' resolves", () => {
    // The resolved version already satisfies global.json by construction -
    // dotnet applies the pin and rollForward itself, run from the project
    // root - so there's no comparison left for the dashboard to second-guess.
    assert.equal(
      describeDotnetIssue('10.1.100', { version: '10.0.302' }),
      undefined,
    );
  });
});

describe('describeNodeIssue', () => {
  it('warns when the running Node differs from the .nvmrc pin', () => {
    const issue = describeNodeIssue('22.22.0', '22.23.1');

    assert.ok(issue);
    assert.equal(issue.severity, 'warning');
    assert.match(issue.message, /22\.22\.0/);
    assert.match(issue.message, /22\.23\.1/);
    assert.match(issue.message, /restart the dashboard/);
  });

  it('says nothing when the pin is satisfied', () => {
    assert.equal(describeNodeIssue('22.23.1', '22.23.1'), undefined);
  });
});

describe('describePythonIssue', () => {
  it('warns when python3 is missing but the repo pins a version', () => {
    const issue = describePythonIssue(undefined, '3.14.6');

    assert.ok(issue);
    assert.equal(issue.severity, 'warning');
    assert.match(issue.message, /isn't on PATH/);
  });

  it('warns when python3 differs from the .python-version pin', () => {
    const issue = describePythonIssue('3.12.4', '3.14.6');

    assert.ok(issue);
    assert.equal(issue.severity, 'warning');
    assert.match(issue.message, /3\.12\.4/);
    assert.match(issue.message, /3\.14\.6/);
  });

  it('says nothing when the pin is satisfied', () => {
    assert.equal(describePythonIssue('3.14.6', '3.14.6'), undefined);
  });
});
