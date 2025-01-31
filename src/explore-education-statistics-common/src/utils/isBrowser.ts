type ExpectedBrowser = 'IE';

export default function isBrowser(browser: ExpectedBrowser): boolean {
  switch (browser) {
    case 'IE':
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      return !!document.documentMode;
    default:
      throw new Error('Invalid browser argument');
  }
}
