import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import findWebAppFailure, { webAppServices } from './webAppHealth';

/**
 * The log lines here are real ones, copied from data/dashboard-logs after an
 * admin and a frontend that had both been left with a stale install. That
 * matters more than usual for this module: everything it does is pattern
 * matching against output nothing in this repo controls, so a test written
 * against invented lines would only prove the patterns match themselves.
 */

// Admin's SPA middleware logs the dev server's output through ILogger, which
// indents every line under the category - so the patterns have to match
// mid-line, not just at the start.
const adminLogs = [
  'info: Microsoft.AspNetCore.SpaServices[0]',
  '      > explore-education-statistics-admin@0.1.0 start',
  'info: Microsoft.AspNetCore.SpaServices[0]',
  '      > node scripts/start.js',
  'info: Microsoft.AspNetCore.SpaServices[0]',
  "      Cannot find module 'swc-loader'",
  'info: Microsoft.AspNetCore.SpaServices[0]',
  '      Require stack:',
  'fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]',
  "      System.AggregateException: One or more errors occurred. (The npm script 'start' exited without indicating that the create-react-app server was listening for requests. The error output was: )",
];

const frontendLogs = [
  'Server started on http://localhost:3000',
  ' ○ Compiling /middleware ...',
  'No typescript errors found.',
  "[Error: ENOENT: no such file or directory, open '/home/x/ees/src/explore-education-statistics-frontend/.next/required-server-files.json'] {",
  '  errno: -2,',
  ' GET /education-in-numbers 500 in 7ms',
];

describe('findWebAppFailure', () => {
  it('names the missing package behind an admin SPA failure', () => {
    const failure = findWebAppFailure('admin', adminLogs);

    assert.ok(failure);
    assert.match(failure.cause, /'swc-loader' package isn't installed/);
    assert.match(failure.message, /pnpm clean && pnpm i/);
  });

  it('names whichever package is missing, not just the one we first hit', () => {
    // Nothing here knows what this checkout's dependencies are, so a new
    // failure in six months' time should be reported just as precisely.
    [
      "Cannot find module '@swc/core'",
      "Cannot find package 'sass' imported from /home/x/ees/next.config.js",
      "Module not found: Can't resolve 'govuk-frontend'",
    ].forEach(line => {
      const failure = findWebAppFailure('frontend', [line]);

      assert.ok(failure, `nothing detected in ${line}`);
      assert.match(failure.cause, /'(@swc\/core|sass|govuk-frontend)'/);
    });
  });

  it('reports an ESM resolution failure that names nothing at all', () => {
    const failure = findWebAppFailure('frontend', [
      '  code: ERR_MODULE_NOT_FOUND',
    ]);

    assert.ok(failure);
    assert.match(failure.cause, /isn't installed/);
  });

  it('prefers the cause over the symptom it produces', () => {
    // The exception admin actually shows you says only that the dev server
    // didn't come up. Reporting that back would be no help at all, and it's
    // the line most likely to survive in the buffer - so the ordering that
    // keeps the cause winning is worth pinning down.
    const failure = findWebAppFailure('admin', adminLogs);

    assert.ok(failure);
    assert.doesNotMatch(failure.cause, /exited instead of starting up/);
  });

  it('falls back to the symptom when the cause has scrolled away', () => {
    const failure = findWebAppFailure(
      'admin',
      adminLogs.filter(line => !line.includes('Cannot find module')),
    );

    assert.ok(failure);
    assert.match(failure.cause, /exited instead of starting up/);
  });

  it("reports a Next build output the frontend's own dev server never wrote", () => {
    const failure = findWebAppFailure('frontend', frontendLogs);

    assert.ok(failure);
    assert.match(failure.cause, /build output is missing/);
    assert.match(failure.cause, /required-server-files\.json/);

    // The whole of that log line is the path the cause already names, so it
    // shouldn't also be quoted - the banner is one line, and it would appear
    // twice, the second time truncated mid-path.
    assert.equal(
      failure.message.match(/required-server-files\.json/g)?.length,
      1,
    );
  });

  it('applies to the frontend in either mode', () => {
    assert.ok(findWebAppFailure('frontendProd', frontendLogs));
  });

  it('ignores services with no JavaScript app of their own', () => {
    // The .NET APIs and Function hosts have nothing that a reinstall would
    // fix, and 'Cannot find module' in their output means something else.
    assert.equal(
      findWebAppFailure('content', ["Cannot find module 'swc-loader'"]),
      undefined,
    );
  });

  it('says nothing about a healthy service', () => {
    assert.equal(
      findWebAppFailure('frontend', [
        'Server started on http://localhost:3000',
        'No typescript errors found.',
      ]),
      undefined,
    );
  });

  it('leaves an unresolved relative import alone', () => {
    // A source problem - a typo'd path, a file not written yet - which
    // reinstalling dependencies would do nothing for.
    [
      "Cannot find module './middleware/chain'",
      "Module not found: Can't resolve '../foo'",
    ].forEach(line => {
      assert.equal(
        findWebAppFailure('frontend', [line]),
        undefined,
        `${line} should not be read as a broken install`,
      );
    });
  });

  it('trims the indentation and length off the line it quotes', () => {
    const failure = findWebAppFailure('frontend', [
      `      Cannot find module 'swc-loader' ${'x'.repeat(500)}`,
    ]);

    assert.ok(failure);
    assert.doesNotMatch(failure.message, /\("\s/);
    assert.ok(
      failure.message.length < 500,
      `banner message was ${failure.message.length} characters`,
    );
  });

  it('covers exactly the services that run a JavaScript app', () => {
    assert.deepEqual(webAppServices, ['admin', 'frontend', 'frontendProd']);
  });
});
