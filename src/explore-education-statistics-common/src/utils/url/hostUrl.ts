let hostUrl: URL;

export function setHostUrl(value: string): void {
  hostUrl = new URL(value);
}

export function getHostUrl(): URL {
  if (!hostUrl) {
    throw new Error(
      'hostUrl must be set. This is done via config in admin or _app in public. If testing, have you mocked out getHostUrl properly?',
    );
  }
  return hostUrl;
}
