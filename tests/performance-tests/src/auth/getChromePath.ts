import os from 'os';

export default function getChromePath(): string {
  const platform = os.platform();

  switch (platform) {
    case 'win32':
      return 'C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe';

    case 'darwin':
      return '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';

    case 'linux':
      return '/usr/bin/google-chrome';

    default:
      throw new Error(`Unsupported platform: ${platform}`);
  }
}
