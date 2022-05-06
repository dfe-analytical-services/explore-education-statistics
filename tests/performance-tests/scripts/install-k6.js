const os = require('os');
const execa = require('execa');

const main = async () => {
  const platform = os.platform();

  switch (platform) {
    case 'darwin':
      await execa('brew', ['install', 'k6']);
      break;

    case 'win32':
      console.log('open CMD as administrator & run "choco install k6"');
      break;

    case 'linux':
      await execa('sudo gpg', [
        '--no-default-keyring',
        '--keyring',
        '/usr/share/keyrings/k6-archive-keyring.gpg',
        '--keyserver',
        'hkp://keyserver.ubuntu.com:80',
        '--recv-keys',
        'C5AD17C747E3415A3642D57D77C6C491D6AC1D69',
      ]);
      await execa('echo', [
        '"deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list',
      ]);
      await execa('sudo', ['apt-get', 'update']);
      await execa('sudo', ['apt-get', 'install', 'k6']);
      break;

    default:
      throw new Error('Unsupported platform');
  }
};

main().catch(e => console.error(e));
