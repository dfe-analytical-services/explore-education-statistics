import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import { describeExitedContainer, readableLogLines } from './dockerManager';

/**
 * These cover how a Docker service that fails to start is reported, which is
 * the one part of dockerManager.ts that doesn't need Docker: `compose up -d`
 * exits 0 as soon as a container has been started, so a container that dies
 * immediately used to leave nothing on screen but a card saying 'Stopped'.
 *
 * Deliberately not covered: everything that shells out (getComposePsEntries,
 * startDockerServices and the grace period it watches). Those need a real
 * container stack to mean anything, and a test that mocked execa would only
 * assert the command strings back at themselves.
 */

// Verbatim from `docker logs ees-idp` after a boot was interrupted part-way
// through, leaving an admin user in keycloak-add-user.json that the server
// never got to consume. Every later start dies on this one line, before
// Keycloak itself is invoked - so it's the only thing there is to report.
const idpLogs =
  "\u001B[0mUser with username 'admin' already added to '/opt/jboss/keycloak/standalone/configuration/keycloak-add-user.json'\n";

describe('readableLogLines', () => {
  it('strips the escape codes services colour their output with', () => {
    assert.deepEqual(readableLogLines(idpLogs), [
      "User with username 'admin' already added to '/opt/jboss/keycloak/standalone/configuration/keycloak-add-user.json'",
    ]);
  });

  it('drops blank lines rather than padding the message out with them', () => {
    assert.deepEqual(readableLogLines('first\n\n\nsecond\n\n'), [
      'first',
      'second',
    ]);
  });

  it('keeps the last lines, which is where the failure is', () => {
    const lines = readableLogLines(
      Array.from({ length: 100 }, (_, i) => `line ${i}`).join('\n'),
    );

    assert.equal(lines.at(-1), 'line 99');
    assert.ok(lines.length < 100, `kept ${lines.length} lines`);
  });

  it('truncates a line long enough to bury the rest of the message', () => {
    const [line] = readableLogLines('x'.repeat(5000));

    assert.ok(line.length < 500, `line was ${line.length} characters`);
    assert.match(line, /\.\.\.$/);
  });
});

describe('describeExitedContainer', () => {
  it('reports the exit code and what the container last logged', () => {
    const { detail } = describeExitedContainer(
      'idp',
      { exitCode: 1 },
      readableLogLines(idpLogs),
    );

    assert.match(detail, /'idp' didn't stay running/);
    assert.match(detail, /exited with code 1/);
    assert.match(detail, /already added to/);
  });

  it('summarises it in one line for the service card', () => {
    // The card renders this as a single line of text, so a summary carrying
    // the log tail would come out as one long unreadable run of it.
    const { summary } = describeExitedContainer(
      'idp',
      { exitCode: 1 },
      readableLogLines(idpLogs),
    );

    assert.doesNotMatch(summary, /\n/);
    assert.match(summary, /exited with code 1/);
    assert.match(summary, /logs/);
  });

  it('says so when there is no log to quote', () => {
    const { detail } = describeExitedContainer('db', { exitCode: 137 }, []);

    assert.match(detail, /exited with code 137/);
    assert.match(detail, /log is empty/);
  });

  it('distinguishes a container that was never created from one that exited', () => {
    // `compose up` failing to build an image leaves no container at all, and
    // 'exited with code undefined' would be a worse than useless thing to say
    // about it.
    const { detail } = describeExitedContainer('data-screener', undefined, []);

    assert.match(detail, /no container was created/);
    assert.doesNotMatch(detail, /undefined/);
  });

  it('names the service it is about', () => {
    // Starting admin brings up several containers at once, so a message that
    // didn't say which one had failed would leave the whole set to check.
    const { service, detail, summary } = describeExitedContainer(
      'public-api-db',
      { exitCode: 1 },
      [],
    );

    assert.equal(service, 'public-api-db');
    assert.match(detail, /'public-api-db'/);
    assert.doesNotMatch(summary, /undefined/);
  });
});
