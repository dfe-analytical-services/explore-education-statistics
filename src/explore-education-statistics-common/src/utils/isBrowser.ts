type ExpectedBrowser = 'IE';

export default function isBrowser(browser: ExpectedBrowser): boolean {
  switch (browser) {
    case 'IE':
      // @ts-ignore
      return !!document.documentMode;
    default:
      throw new Error('Invalid browser argument');
  }
}
