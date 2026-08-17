import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import {
  allowedServiceNames,
  resolveDockerServices,
  resolvePublicDataDbAvailability,
  resolveServiceDependencies,
  ServiceName,
} from './services';

/**
 * These cover the dependency resolution in services.ts, which both the `start`
 * CLI and the dashboard rely on to decide what a single named service actually
 * needs running alongside it. It's the kind of logic that fails quietly - you
 * get a service that starts and then misbehaves, rather than an error - so it's
 * worth pinning down.
 *
 * Deliberately not covered: readLayeredAppSetting and getServicePort. Both read
 * files out of the real checkout, including a gitignored appsettings.Local.json
 * that developers set differently, so any assertion about them would pass or
 * fail depending on whose machine it ran on.
 */

const withPublicData = { env: { PublicDataDbExists: 'true' } };
const withoutPublicData = { env: { PublicDataDbExists: 'false' } };

describe('resolveServiceDependencies', () => {
  it('resolves services a service talks to directly over HTTP', () => {
    // The frontend calls the content and data APIs itself, so neither is
    // optional for it.
    assert.deepEqual(resolveServiceDependencies('frontend'), [
      'content',
      'data',
    ]);
  });

  it('never includes the service itself', () => {
    allowedServiceNames.forEach(service => {
      assert.ok(
        !resolveServiceDependencies(service, withPublicData).includes(service),
        `${service} resolved itself as its own dependency`,
      );
    });
  });

  it('lists dependencies before the services that need them', () => {
    // Callers start these in order, so anything appearing after its dependent
    // would be started too late to be of use.
    const resolved = resolveServiceDependencies('admin', withPublicData);

    assert.ok(
      resolved.indexOf('content') < resolved.indexOf('publicData'),
      `content should precede publicData, got ${resolved.join(', ')}`,
    );
  });

  it('pulls in the public API services only when admin uses them', () => {
    assert.deepEqual(resolveServiceDependencies('admin', withoutPublicData), [
      'processor',
      'publisher',
    ]);

    const withApi = resolveServiceDependencies('admin', withPublicData);

    assert.ok(withApi.includes('publicProcessor'));
    assert.ok(withApi.includes('publicData'));
  });

  it('resolves transitively without repeating a shared dependency', () => {
    // Both publicData and searchFunctionApp depend on content.
    const resolved = resolveServiceDependencies('admin', withPublicData);

    assert.equal(
      resolved.filter(service => service === 'content').length,
      1,
      `content appeared more than once in ${resolved.join(', ')}`,
    );
  });

  it('resolves every service without throwing', () => {
    allowedServiceNames.forEach(service => {
      assert.doesNotThrow(() =>
        resolveServiceDependencies(service, withPublicData),
      );
    });
  });
});

describe('resolveDockerServices', () => {
  it("includes Docker services needed by a service's own dependencies", () => {
    // The frontend declares no Docker services itself; it needs these only
    // because content and data do.
    const resolved = resolveDockerServices('frontend', {});

    assert.ok(resolved.includes('db'));
    assert.ok(resolved.includes('data-storage'));
  });

  it('resolves a Docker service to itself', () => {
    assert.deepEqual(resolveDockerServices('db', {}), ['db']);
  });

  it('brings up public-api-db only when the public API is in play', () => {
    assert.ok(
      !resolveDockerServices('admin', withoutPublicData).includes(
        'public-api-db',
      ),
    );
    assert.ok(
      resolveDockerServices('admin', withPublicData).includes('public-api-db'),
    );
  });

  it('does not repeat a Docker service needed by several dependencies', () => {
    const resolved = resolveDockerServices('admin', withPublicData);

    assert.equal(
      resolved.length,
      new Set(resolved).size,
      `duplicates in ${resolved.join(', ')}`,
    );
  });
});

describe('resolvePublicDataDbAvailability', () => {
  it('does not depend on the order services are given in', () => {
    // The whole reason this function exists. Resolved per-service instead,
    // whether publisher gets public-api-db would depend on whether admin
    // happened to be listed before or after it.
    const pairs: [ServiceName, ServiceName][] = [
      ['admin', 'publisher'],
      ['publicData', 'notifier'],
      ['publicApiDb', 'content'],
      ['publicProcessor', 'frontend'],
    ];

    pairs.forEach(([first, second]) => {
      assert.equal(
        resolvePublicDataDbAvailability([first, second]),
        resolvePublicDataDbAvailability([second, first]),
        `${first} + ${second} resolved differently when reversed`,
      );
    });
  });

  it('is true when any service brings public-api-db up', () => {
    assert.equal(resolvePublicDataDbAvailability(['publicData']), true);
    assert.equal(resolvePublicDataDbAvailability(['publicProcessor']), true);
    // Asked for directly, as a Docker service.
    assert.equal(resolvePublicDataDbAvailability(['publicApiDb']), true);
  });

  it('is false when nothing involved uses it', () => {
    assert.equal(
      resolvePublicDataDbAvailability(['content', 'data', 'frontend']),
      false,
    );
  });

  it('lets an explicit override win either way', () => {
    // Even when nothing would have pulled it in...
    assert.equal(
      resolvePublicDataDbAvailability(['content'], withPublicData),
      true,
    );

    // ...and even when something would have.
    assert.equal(
      resolvePublicDataDbAvailability(['publicData'], withoutPublicData),
      false,
    );
  });

  it('is false for no services at all', () => {
    assert.equal(resolvePublicDataDbAvailability([]), false);
  });
});
