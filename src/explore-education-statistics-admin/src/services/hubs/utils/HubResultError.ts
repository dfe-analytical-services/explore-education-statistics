import { HubResult } from '@admin/services/hubs/utils/isHubResult';

export default class HubResultError extends Error {
  constructor(
    public readonly result: HubResult<unknown>,
    public readonly methodName: string,
  ) {
    super(
      result.message ||
        `Hub method '${methodName}' responded with status ${result.status}`,
    );
  }
}
