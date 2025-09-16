import {
  internalHosts,
  trustedExternalHosts,
} from '@common/utils/url/allowedHosts';

export default function getUrlAttributes(
  urlString: string,
): { isExternal: boolean; isTrusted: boolean } | undefined {
  try {
    const url = new URL(urlString);
    const isExternal =
      !internalHosts.includes(url.host) &&
      (url.protocol === 'http:' || url.protocol === 'https:');
    const isTrusted =
      isExternal && trustedExternalHosts.some(host => url.host.endsWith(host));

    return {
      isExternal,
      isTrusted,
    };
  } catch {
    return undefined;
  }
}
